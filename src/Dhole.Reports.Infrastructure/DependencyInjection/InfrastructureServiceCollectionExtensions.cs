using CustomCodeFramework.Auth.DependencyInjection;
using Dhole.Reports.Application.Abstractions.Generation;
using Dhole.Reports.Infrastructure.Generation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dhole.Reports.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddCustomCodeAuth(configuration);
        services.PostConfigure<AuthenticationOptions>(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        });

        services.Configure<ReportGenerationOptions>(configuration.GetSection("Reports:Generation"));
        services.AddScoped<IReportDocumentGenerator, ReportDocumentGenerator>();
        return services;
    }
}
