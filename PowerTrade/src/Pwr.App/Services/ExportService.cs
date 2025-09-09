using CsvHelper.Configuration;
using CsvHelper;
using Microsoft.Extensions.Logging;
using Pwr.App.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pwr.App.Services;

public class ExportService
{
    private readonly ILogger<ExportService> _logger;

    public ExportService(ILogger<ExportService> logger)
    {
        _logger = logger;
    }

    public void GenerateReport(DateTime requestedUtc, List<OutputItemDto> rows)
    {
        try
        {
            //TODO create output folder if does not exists
            //Delimiter as constant
            //Read from config
            //implement interface
            _logger.LogInformation("Starting to write CSV report for date {RequestedDate}", requestedUtc);
            var config = new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = ";", Encoding = Encoding.UTF8 };
            var datePart = requestedUtc.ToString(Constants.FileFormat);
            var fileName = $"{Constants.FilePrefix}_{datePart}.csv";
            var basePath = $"C:\\Users\\DanielUrdanetaOropez\\source";
            using (var writer = new StreamWriter($"{basePath}\\{fileName}"))
            using (var csv = new CsvWriter(writer, config))
            {
                csv.WriteRecords(rows);
            }
            _logger.LogInformation("CSV report {FileName} created successfully", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while generating CSV report for date {RequestedDate}", requestedUtc);
        }
    }
}
