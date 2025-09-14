using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pwr.Application;
using Pwr.Application.Interfaces;
using Pwr.Infrastructure;

namespace Pwr.ConsoleApp;

internal class Program
{
    static async Task Main(string[] args)
    {        
        Console.WriteLine("Starting PowerTrade application...");
        
        // Setup Host
        var host = CreateHostBuilder(args).Build();
        
        try
        {
            Console.WriteLine("Starting background services...");
            
            await host.StartAsync();
            var scheduled = host.Services.GetRequiredService<IScheduledExtractService>();
            await scheduled.StartAsync(default);

            Console.WriteLine("Application started successfully!");
            Console.WriteLine("Background services are running:");
            Console.WriteLine("- ScheduledExtractService: Automated data extraction");
            Console.WriteLine("- TimerService: Scheduling management");
            Console.WriteLine();
            Console.WriteLine("Press Ctrl+C to stop the application...");
            
            // Keep the application running
            await host.WaitForShutdownAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Application failed to start: {ex.Message}");
            Console.WriteLine("Press any key to exit...");
            Console.ReadLine();
        }
        finally
        {
            Console.WriteLine("Shutting down application...");
            await host.StopAsync();
            host.Dispose();
        }
    }

    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                config.SetBasePath(Directory.GetCurrentDirectory())
                      .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                      .AddEnvironmentVariables();
            }).ConfigureServices((context, services) =>
            {
                services.AddLogging(builder =>
                {
                    //On prod we can set a different log configuration
                    builder.AddConsole();
                    builder.SetMinimumLevel(LogLevel.Debug);
                });

                //register solution services
                services.AddInfrastructureServices(context.Configuration)
                        .AddApplicationServices(context.Configuration);
            });
}
