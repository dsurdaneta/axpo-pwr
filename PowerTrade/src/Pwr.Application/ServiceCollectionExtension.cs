﻿using Axpo;
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
        services.Configure<CsvOptions>(configuration.GetSection(CsvOptions.SectionName));

        services.AddScoped<IPowerService, PowerService>();
        services.AddScoped<IForcastCallingService, ForcastCallingService>();
        services.AddScoped<IExportService, ExportService>();

        return services;
    }
}
