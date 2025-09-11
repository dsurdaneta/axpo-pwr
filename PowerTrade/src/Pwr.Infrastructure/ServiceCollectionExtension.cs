using Axpo;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pwr.Infrastructure.Interfaces;
using Pwr.Infrastructure.Options;
using Pwr.Infrastructure.Services;

namespace Pwr.Infrastructure;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<CsvOptions>(configuration.GetSection(CsvOptions.SectionName));

        // Register external service
        services.AddScoped<IPowerService, PowerService>();

        // Register Infrastructure services
        services.AddScoped<IForecastCallingService, ForecastCallingService>();
        services.AddScoped<IExportService, ExportService>();

        return services;
    }
}
