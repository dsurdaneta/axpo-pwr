using Pwr.Domain.Models;

namespace Pwr.Infrastructure.Interfaces;

/// <summary>
/// Handles the generation of CSV reports.
/// </summary>
public interface IExportService
{
    /// <summary>
    /// Generates a CSV report based on the requested UTC date and a list of input items with power volumes.
    /// </summary>
    /// <param name="requestedUtc">the requested UTC date</param>
    /// <param name="rows">the list of power volumes retrieved from the external service</param>
    /// <returns>bool indicating whether the report was generated</returns>
    bool GenerateReport(DateTime requestedUtc, List<InputItemDto> rows);
}
