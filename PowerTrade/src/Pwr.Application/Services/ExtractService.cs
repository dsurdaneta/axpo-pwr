using Microsoft.Extensions.Logging;
using Pwr.Application.Interfaces;
using Pwr.Application.Models;
using Pwr.Domain.Models;
using Pwr.Infrastructure.Interfaces;

namespace Pwr.Application.Services;

public class ExtractService(
    ILogger<ExtractService> logger,
    IForcastCallingService forcastCallingService,
    IExportService exportService) : IExtractService
{
    private readonly ILogger<ExtractService> _logger = logger;
    private readonly IForcastCallingService _forcastCallingService = forcastCallingService;
    private readonly IExportService _exportService = exportService;

    public async Task<ExtractResult> PerformExtractAsync(ExtractContext context, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Starting extract for {RequestedUtc} [CorrelationId: {CorrelationId}]",
            context.RequestedUtc, context.CorrelationId);

        try
        {
            // Get forecast data
            var forecastList = await _forcastCallingService.GetForcastAsync(context.RequestedUtc);

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
