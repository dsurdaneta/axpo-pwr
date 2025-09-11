using Pwr.Domain.Models;

namespace Pwr.Infrastructure.Interfaces;

public interface IExportService
{
    bool GenerateReport(DateTime requestedUtc, List<InputItemDto> rows);
}
