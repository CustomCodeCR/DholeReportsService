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
app.MapTabularReportEndpoints();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ServiceDbContext>();
    await dbContext.Database.MigrateAsync();
}

// Dhole API Manager: authenticated runtime inventory, not a public proxy.
app.MapGet("/api/internal/third-party-route-catalog", (HttpContext http) =>
{
    var allowed = http.User.Identity?.IsAuthenticated == true &&
        http.User.Claims.Where(c => c.Type is "scope" or "scp" or "scopes")
            .SelectMany(c => c.Value.Split([' ', ',', ';'], StringSplitOptions.RemoveEmptyEntries))
            .Contains("integrations.thirdparty.manage", StringComparer.OrdinalIgnoreCase);
    if (!allowed) return (IResult)Results.StatusCode(StatusCodes.Status403Forbidden);
    var routes = new List<object>();
    foreach (var source in ((Microsoft.AspNetCore.Routing.IEndpointRouteBuilder)app).DataSources)
        foreach (var endpoint in source.Endpoints.OfType<Microsoft.AspNetCore.Routing.RouteEndpoint>())
        {
            var methods = endpoint.Metadata.GetMetadata<Microsoft.AspNetCore.Routing.HttpMethodMetadata>()?.HttpMethods;
            if (methods is null || endpoint.RoutePattern.RawText is not { } path) continue;
            foreach (var method in methods)
                if (method is "GET" or "POST" or "PUT" or "PATCH" or "DELETE")
                    routes.Add(new { method, path = "/" + path.TrimStart('/') });
        }
    return (IResult)Results.Ok(routes);
}).ExcludeFromDescription();

app.Run();
