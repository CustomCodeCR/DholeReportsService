using System.Text.Json;
using Dhole.Reports.Domain.Templates.Entities;
using Dhole.Reports.Persistence.DbContexts;
using Dhole.Reports.Persistence.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Dhole.Reports.IntegrationTests;

[TestClass]
public sealed class F6ReportsRealtimeTests
{
    private static IConfiguration Flags(bool approved = true)
        => new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?>
        {
            ["Features:UniversalRealtimeEnabled"]="true",
            ["Realtime:Outbox:Enabled"]="true",
            ["Realtime:BatchApproved"]=approved.ToString(),
            ["Realtime:ReleaseId"]=ReportsRealtimeInvalidation.ReleaseId
        }).Build();

    [TestMethod]
    public void Reports_realtime_gates_are_off_without_batch_approval()
    {
        Assert.IsFalse(ReportsRealtimeInvalidation.IsEnabled(null));
        Assert.IsFalse(ReportsRealtimeInvalidation.IsEnabled(Flags(false)));
        var flags = Flags();
        Assert.IsTrue(ReportsRealtimeInvalidation.IsEnabled(flags));
        flags["Realtime:ReleaseId"] = "invalid";
        Assert.IsFalse(ReportsRealtimeInvalidation.IsEnabled(flags));
    }

    [TestMethod]
    public void Template_event_never_exposes_HTML_PDF_or_quotation_data()
    {
        var eventId = Guid.NewGuid();
        var templateId = Guid.NewGuid();
        var data = ReportsRealtimeInvalidation.Serialize(eventId,templateId,
            ReportsRealtimeInvalidation.NextVersionHint(),DateTimeOffset.UtcNow,"test");
        using var json = JsonDocument.Parse(data);
        var root = json.RootElement;
        Assert.AreEqual(ReportsRealtimeInvalidation.Type, root.GetProperty("eventType").GetString());
        Assert.AreEqual($"reports:template:{templateId:D}", root.GetProperty("audienceRef").GetString());
        CollectionAssert.AreEquivalent(new[] {"changedFields"},
            root.GetProperty("payload").EnumerateObject().Select(x=>x.Name).ToArray());
        foreach(var banned in new[] {"htmlContent","previewPdf","clientName","saleAmount","designerJson"})
            Assert.IsFalse(data.Contains(banned,StringComparison.OrdinalIgnoreCase),banned);
    }

    [TestMethod]
    public async Task Real_PostgreSQL_insert_commits_template_and_minimal_Outbox_together()
    {
        var pg = Environment.GetEnvironmentVariable("REPORTS_F6_POSTGRES")
            ?? throw new InvalidOperationException("REPORTS_F6_POSTGRES required");
        var options = new DbContextOptionsBuilder<ServiceDbContext>().UseNpgsql(pg).Options;
        await using var db = new ServiceDbContext(options, Flags());
        await db.Database.MigrateAsync();
        var item = ReportTemplate.Create("F6 Test "+Guid.NewGuid().ToString("N"), null,
            "<html><body>test</body></html>","{}","A4","Portrait", new byte[] {37,80,68,70});
        db.ReportTemplates.Add(item);
        await db.SaveChangesAsync();
        var outbox = (await db.OutboxMessages.Where(x =>
            x.EventName == ReportsRealtimeInvalidation.Type).ToListAsync())
            .Single(x => x.PayloadJson.Contains(item.Id.ToString("D"), StringComparison.Ordinal));
        Assert.AreEqual(1, await db.ReportTemplates.CountAsync(x=>x.Id==item.Id));
        using var json = JsonDocument.Parse(outbox.PayloadJson);
        Assert.AreEqual(item.Id, json.RootElement.GetProperty("aggregateId").GetGuid());
        Assert.AreEqual(1,await db.OutboxMessages.CountAsync(x=>x.EventId==outbox.EventId));

        await using var disabled = new ServiceDbContext(options, Flags(false));
        var second = ReportTemplate.Create("F6 Off "+Guid.NewGuid().ToString("N"),null,
            "<html>off</html>","{}","A4","Portrait",new byte[]{37,80,68,70});
        disabled.ReportTemplates.Add(second);
        await disabled.SaveChangesAsync();
        var pending = await disabled.OutboxMessages.Where(x =>
            x.EventName==ReportsRealtimeInvalidation.Type).ToListAsync();
        Assert.AreEqual(0,pending.Count(x=>
            x.PayloadJson.Contains(second.Id.ToString("D"),StringComparison.Ordinal)));
    }
}
