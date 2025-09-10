using CsvHelper.Configuration;
using CsvHelper;
using Microsoft.Extensions.Logging;
using Pwr.Domain.Models;
using System.Globalization;
using System.Text;
using Pwr.Application.Interfaces;

namespace Pwr.Application.Services;

public class ExportService(ILogger<ExportService> logger) : IExportService
{
    private const string CsvDelimiter = ";";
    private const string FilePrefix = "PowerPosition";
    private const string FileFormat = "yyyyMMdd_yyyyMMddHHmm";

    public bool GenerateReport(DateTime requestedUtc, List<OutputItemDto> rows)
    {
        if (rows == null || rows.Count == 0)
        {
            logger.LogWarning("No data available to generate report for date {RequestedDate}", requestedUtc);
            return false;
        }
        
        var basePath = $"C:\\Users\\DanielUrdanetaOropez\\source";
        logger.LogInformation("Starting to write CSV report for date {RequestedDate}", requestedUtc);

        try
        {
            //TODO
            //create output folder if does not exists
            //Read from config
            //if config valu is null use constant defalt
            var datePart = requestedUtc.ToString(FileFormat);
            var fileName = $"{FilePrefix}_{datePart}.csv";

            var config = new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = CsvDelimiter, Encoding = Encoding.UTF8 };

            using (var writer = new StreamWriter($"{basePath}\\{fileName}"))
            using (var csv = new CsvWriter(writer, config))
            {
                csv.WriteRecords(rows);
            }

            logger.LogInformation("CSV report {FileName} created successfully", fileName);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while generating CSV report for date {RequestedDate}", requestedUtc);
            return false;
        }
    }
}
