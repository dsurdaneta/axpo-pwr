using Axpo;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pwr.Application.Interfaces;
using Pwr.Application.Options;
using Pwr.Application.Services;

namespace Pwr.Application;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ExtractTradesOptions>(configuration.GetSection(ExtractTradesOptions.SectionName));

        // Register external service
        services.AddScoped<IPowerService, PowerService>();

        // Register application services
        services.AddScoped<IExtractService, ExtractService>();
        services.AddScoped<IRetryService, RetryService>();
        services.AddScoped<ITimerService, TimerService>();
        
        // Register the background service
        services.AddHostedService<ScheduledExtractService>();
        services.AddScoped<IScheduledExtractService, ScheduledExtractService>();

        return services;
    }
}
