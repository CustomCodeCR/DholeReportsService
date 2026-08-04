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
builder.Services.AddCors(options => options.AddPolicy(CorsPolicyName, policy => policy
    .WithOrigins(
        "http://localhost:5173",
        "http://127.0.0.1:5173",
        "http://192.168.1.193:5173",
        "http://192.168.0.219:5173")
    .AllowAnyHeader()
    .AllowAnyMethod()));

builder.Services.AddApplication();
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();
app.UseCustomCodeApi();
app.UseCors(CorsPolicyName);

if (app.Environment.IsDevelopment()) app.UseCustomCodeSwagger();

app.MapGet(
        "/health",
        async (ServiceDbContext dbContext, CancellationToken cancellationToken) =>
        {
            var databaseHealthy = false;

            try
            {
                databaseHealthy = await dbContext.Database.CanConnectAsync(cancellationToken);
            }
            catch
            {
                databaseHealthy = false;
            }

            var statusCode = databaseHealthy
                ? StatusCodes.Status200OK
                : StatusCodes.Status503ServiceUnavailable;

            return Results.Json(
                new
                {
                    service = "DholeReportsService",
                    status = databaseHealthy ? "Healthy" : "Unhealthy",
                    database = databaseHealthy ? "Connected" : "Unavailable",
                    timestamp = DateTimeOffset.UtcNow,
                },
                statusCode: statusCode
            );
        }
    )
    .AllowAnonymous();

app.UseAuthentication();
app.UseAuthorization();
app.MapReportTemplateEndpoints();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ServiceDbContext>();
    await dbContext.Database.MigrateAsync();
}

app.Run();
