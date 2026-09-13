using CustomCodeFramework.Api.DependencyInjection;
using CustomCodeFramework.Api.Swagger;
using CustomCodeFramework.Core.Abstractions;
using Dhole.Reports.Api.Endpoints;
using Dhole.Reports.Application.DependencyInjection;
using Dhole.Reports.Infrastructure.DependencyInjection;
using Dhole.Reports.Infrastructure.Time;
using Dhole.Reports.Persistence.DbContexts;
using Dhole.Reports.Persistence.DependencyInjection;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
const string CorsPolicyName = "DholeWebCors";
builder.Services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
builder.Services.AddCustomCodeApiWithSwagger(title: "Dhole Reports Service", version: "v1");
builder.Services.AddCors(options => options.AddPolicy(CorsPolicyName, policy => policy.WithOrigins("http://localhost:5173", "http://127.0.0.1:5173", "http://192.168.1.193:5173", "http://192.168.0.219:5173").AllowAnyHeader().AllowAnyMethod()));
builder.Services.AddApplication();
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);
var app = builder.Build();
app.UseCustomCodeApi();
app.UseCors(CorsPolicyName);
if (app.Environment.IsDevelopment()) app.UseCustomCodeSwagger();
app.MapGet("/health", async (ServiceDbContext db, CancellationToken ct) =>
{
    var healthy = false; try { healthy = await db.Database.CanConnectAsync(ct); } catch { }
    return Results.Json(new { service = "DholeReportsService", status = healthy ? "Healthy" : "Unhealthy", database = healthy ? "Connected" : "Unavailable", timestamp = DateTimeOffset.UtcNow }, statusCode: healthy ? 200 : 503);
}).AllowAnonymous();
app.UseAuthentication();
app.UseAuthorization();
app.MapReportTemplateEndpoints();
app.MapTabularReportEndpoints();
app.MapContentAnalyticsEndpoints();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ServiceDbContext>();
    await db.Database.MigrateAsync();
}
app.Run();
