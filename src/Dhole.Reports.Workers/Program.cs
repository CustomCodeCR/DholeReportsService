using CustomCodeFramework.Redis.DependencyInjection;
using CustomCodeFramework.Redis.Streams.DependencyInjection;
using Dhole.Reports.Persistence.DependencyInjection;
using Dhole.Reports.Workers.Streams;

var contentRoot = Path.Combine(Directory.GetCurrentDirectory(), "src", "Dhole.Reports.Workers");
if (!Directory.Exists(contentRoot)) contentRoot = Directory.GetCurrentDirectory();
var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings { Args = args, ContentRootPath = contentRoot });
builder.Configuration.Sources.Clear();
builder.Configuration.SetBasePath(contentRoot)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddCustomCodeRedis(builder.Configuration);
builder.Services.AddCustomCodeRedisStreams(builder.Configuration);
builder.Services.AddCustomCodeRedisStreamConsumerBackgroundService();
builder.Services.AddCustomCodeRedisStreamHandler<PageViewedStreamHandler>();
builder.Services.AddCustomCodeRedisStreamHandler<FormSubmittedStreamHandler>();
builder.Services.AddCustomCodeRedisStreamHandler<InteractionClickedStreamHandler>();
builder.Services.AddCustomCodeRedisStreamHandler<MeetingRequestedStreamHandler>();

var host = builder.Build();
await host.RunAsync();
