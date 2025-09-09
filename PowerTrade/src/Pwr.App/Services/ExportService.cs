using CsvHelper.Configuration;
using CsvHelper;
using Microsoft.Extensions.Logging;
using Pwr.App.Models;
using System.Globalization;
using System.Text;

namespace Pwr.App.Services;

public class ExportService
{
    private const string CsvDelimiter = ";";
    private const string FilePrefix = "PowerPosition";    
    private const string FileFormat = "yyyyMMdd_yyyyMMddHHmm";

    private readonly ILogger<ExportService> _logger;

    public ExportService(ILogger<ExportService> logger)
    {
        _logger = logger;
    }

    public bool GenerateReport(DateTime requestedUtc, List<OutputItemDto> rows)
    {
        if (rows == null || rows.Count == 0)
        {
            _logger.LogWarning("No data available to generate report for date {RequestedDate}", requestedUtc);
            return false;
        }

        var datePart = requestedUtc.ToString(FileFormat);
        var fileName = $"{FilePrefix}_{datePart}.csv";
        var basePath = $"C:\\Users\\DanielUrdanetaOropez\\source";
        _logger.LogInformation("Starting to write CSV report for date {RequestedDate}", requestedUtc);

        try
        {
            //TODO create output folder if does not exists
            //Delimiter as constant
            //Read from config
            //implement interface
            
            var config = new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = CsvDelimiter, Encoding = Encoding.UTF8 };

            using (var writer = new StreamWriter($"{basePath}\\{fileName}"))
            using (var csv = new CsvWriter(writer, config))
            {
                csv.WriteRecords(rows);
            }

            _logger.LogInformation("CSV report {FileName} created successfully", fileName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while generating CSV report for date {RequestedDate}", requestedUtc);
            return false;
        }
    }
}
