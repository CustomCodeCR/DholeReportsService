using System.Text.Json;
using CustomCodeFramework.Messaging.Outbox;
using CustomCodeFramework.Redis.Streams.Abstractions;
using CustomCodeFramework.Redis.Streams.Messages;
using Dhole.Reports.Persistence.DbContexts;
using Dhole.Reports.Persistence.Messaging;
using Microsoft.EntityFrameworkCore;

namespace Dhole.Reports.Workers.Realtime;

/// <summary>
/// F6.06: publishes only committed Reports Outbox messages. Redis failures never
/// happen inside an report business transaction; EventId allows deduplication.
/// Existing Reports stream consumers and persistent history are unchanged.
/// </summary>
public sealed class ReportsRealtimeOutboxPublisher(
    IServiceScopeFactory scopeFactory, IConfiguration config,
    ILogger<ReportsRealtimeOutboxPublisher> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!ReportsRealtimeInvalidation.IsEnabled(config))
            return;
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var db = scope.ServiceProvider.GetRequiredService<ServiceDbContext>();
                var publisher = scope.ServiceProvider.GetRequiredService<IRedisStreamPublisher>();
                var messages = await db.OutboxMessages
                    .Where(m => m.EventName == ReportsRealtimeInvalidation.Type &&
                        (m.Status == OutboxMessageStatus.Pending ||
                         (m.Status == OutboxMessageStatus.Failed && m.RetryCount < 10)))
                    .OrderBy(m => m.CreatedAtUtc)
                    .Take(25)
                    .ToListAsync(stoppingToken);
                foreach (var message in messages)
                {
                    stoppingToken.ThrowIfCancellationRequested();
                    try
                    {
                        var json = JsonSerializer.Deserialize<JsonElement>(message.PayloadJson);
                        await publisher.PublishAsync(new RedisStreamMessage
                        {
                            StreamName = ReportsRealtimeInvalidation.Stream,
                            MessageType = message.EventName,
                            Payload = json,
                            Headers = new Dictionary<string, string>
                            {
                                ["event_id"] = message.EventId.ToString("D"),
                                ["source_service"] = "DholeAuditLogsService",
                                ["correlation_id"] = message.CorrelationId ?? message.EventId.ToString("D")
                            }
                        }, stoppingToken);
                        message.Status = OutboxMessageStatus.Processed;
                        message.ProcessedAtUtc = DateTime.UtcNow;
                        message.ErrorMessage = null;
                    }
                    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                    {
                        throw;
                    }
                    catch (Exception failure)
                    {
                        message.RetryCount++;
                        message.Status = OutboxMessageStatus.Failed;
                        message.ErrorMessage = failure.GetType().Name;
                        logger.LogWarning("Reports realtime publish failed for {EventId}, attempt {Count}",
                            message.EventId, message.RetryCount);
                    }
                }
                await db.SaveChangesAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception failure)
            {
                logger.LogWarning("Reports realtime publisher retry after {FailureType}",
                    failure.GetType().Name);
            }
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}
