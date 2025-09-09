using Pwr.App.Models;

namespace Pwr.App.Services;

internal interface IExportService
{
    bool GenerateReport(DateTime requestedUtc, List<OutputItemDto> rows);
}
