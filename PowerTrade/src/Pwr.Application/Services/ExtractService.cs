using Microsoft.Extensions.Logging;
using Pwr.Application.Interfaces;
using Pwr.Application.Models;
using Pwr.Infrastructure.Interfaces;

namespace Pwr.Application.Services;

/// <summary>
/// Service responsible for performing the extract operation. Handles the actual extract logic
/// </summary>
/// <param name="logger"></param>
/// <param name="forcastCallingService">External service to obtain forcasted power trades</param>
/// <param name="exportService">Service to generate reports</param>
public class ExtractService(
    ILogger<ExtractService> logger,
    IForecastCallingService forcastCallingService,
    IExportService exportService) : IExtractService
{
    private readonly ILogger<ExtractService> _logger = logger;
    private readonly IForecastCallingService _forcastCallingService = forcastCallingService;
    private readonly IExportService _exportService = exportService;

    /// <summary>
    /// Extracts forecast data generating a report.
    /// </summary>
    /// <param name="context">Context information for data extraction operations</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<ExtractResult> PerformExtractAsync(ExtractContext context, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Starting extract for {RequestedUtc} [CorrelationId: {CorrelationId}]",
            context.RequestedUtc, context.CorrelationId);

        try
        {
            // Get forecast data
            var forecastList = await _forcastCallingService.GetForecastAsync(context.RequestedUtc);

            if (forecastList.Count == 0)
            {
                _logger.LogWarning(
                    "No forecast data retrieved for {RequestedUtc} [CorrelationId: {CorrelationId}]",
                    context.RequestedUtc, context.CorrelationId);
                return ExtractResult.Failure(1, "No forecast data available");
            }

            // Generate report
            var reportGenerated = _exportService.GenerateReport(context.RequestedUtc, forecastList);

            if (reportGenerated)
            {
                _logger.LogInformation(
                    "Extract completed successfully for {RequestedUtc} [CorrelationId: {CorrelationId}]",
                    context.RequestedUtc, context.CorrelationId);
                return ExtractResult.Success(1);
            }

            return ExtractResult.Failure(1, "Failed to generate report");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error occurred during extract for {RequestedUtc} [CorrelationId: {CorrelationId}]",
                context.RequestedUtc, context.CorrelationId);
            return ExtractResult.Failure(1, "Extract operation failed", ex);
        }
    }
}
