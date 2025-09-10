using Pwr.Domain.Models;

namespace Pwr.Application.Interfaces;

public interface IExportService
{
    bool GenerateReport(DateTime requestedUtc, List<OutputItemDto> rows);
}
