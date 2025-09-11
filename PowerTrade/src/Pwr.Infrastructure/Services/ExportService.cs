using CsvHelper.Configuration;
using CsvHelper;
using Microsoft.Extensions.Logging;
using Pwr.Domain.Models;
using System.Globalization;
using System.Text;
using Microsoft.Extensions.Options;
using Pwr.Infrastructure.Interfaces;
using Pwr.Infrastructure.Options;

namespace Pwr.Infrastructure.Services;

public class ExportService(ILogger<ExportService> logger, IOptionsMonitor<CsvOptions> options) : IExportService
{
    internal const string CsvDelimiter = ";";
    internal const string FilePrefix = "PowerPosition";
    internal const string FileFormat = "yyyyMMdd_yyyyMMddHHmm";
    internal const string DefaultFolderPath = "C:\\CsvFiles\\";

    public bool GenerateReport(DateTime requestedUtc, List<InputItemDto> inputItems)
    {
        if (inputItems == null || inputItems.Count == 0)
        {
            logger.LogWarning("No data available to generate report for date {RequestedDate}", requestedUtc);
            return false;
        }

        var basePath = options.CurrentValue.FolderPath ?? DefaultFolderPath;
        logger.LogInformation("Starting to write CSV report for date {RequestedDate}", requestedUtc);

        try
        {
            var datePart = requestedUtc.ToString(FileFormat);
            var fileName = $"{FilePrefix}_{datePart}.csv";

            // Try to create the directory.
            logger.LogInformation("Ensuring the output directory exists at path: {BasePath}", basePath);
            _ = Directory.CreateDirectory(basePath);

            var rows = inputItems.Select(OutputItemDto.FromInputItemDto).ToList();

            var config = new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = CsvDelimiter, Encoding = Encoding.UTF8 };

            using (var writer = new StreamWriter($"{basePath}\\{fileName}"))
            using (var csv = new CsvWriter(writer, config))
            {
                csv.WriteRecords(rows);
            }

            logger.LogInformation("CSV report {FileName} created successfully", fileName);
            return true;
        }
        catch (UnauthorizedAccessException e)
        {
            logger.LogError(e, "Permission denied when trying to create directory or file at path: {BasePath}", basePath);
            return false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while generating CSV report for date {RequestedDate}", requestedUtc);
            return false;
        }
    }
}
