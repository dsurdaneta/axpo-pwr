using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Pwr.Application.Models;
using Pwr.Domain.Models;

namespace Pwr.Tests.Benchmarks;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80)]
[MinColumn, MaxColumn, MeanColumn, MedianColumn]
public class SimpleBenchmarks
{
    private List<InputItemDto> _testData = null!;
    private DateTime _testDate;

    [GlobalSetup]
    public void Setup()
    {
        _testData = GenerateTestData(1000);
        _testDate = DateTime.UtcNow.AddDays(1);
    }

    // Data Generation Performance
    [Benchmark]
    [Arguments(100)]
    [Arguments(1000)]
    [Arguments(10000)]
    public List<InputItemDto> GenerateTestData_Performance(int count)
    {
        return GenerateTestData(count);
    }

    // Data Processing Performance
    [Benchmark]
    [Arguments(100)]
    [Arguments(1000)]
    [Arguments(10000)]
    public double CalculateTotalVolume_Performance(int count)
    {
        var data = GenerateTestData(count);
        return data.Sum(item => item.Volume);
    }

    [Benchmark]
    [Arguments(100)]
    [Arguments(1000)]
    [Arguments(10000)]
    public int CountValidItems_Performance(int count)
    {
        var data = GenerateTestData(count);
        return data.Count(item => item.Volume != 0);
    }

    [Benchmark]
    [Arguments(100)]
    [Arguments(1000)]
    [Arguments(10000)]
    public List<InputItemDto> FilterPositiveVolumes_Performance(int count)
    {
        var data = GenerateTestData(count);
        return data.Where(item => item.Volume > 0).ToList();
    }

    // Data Conversion Performance
    [Benchmark]
    [Arguments(100)]
    [Arguments(1000)]
    [Arguments(10000)]
    public List<OutputItemDto> ConvertToOutputItems_Performance(int count)
    {
        var data = GenerateTestData(count);
        return data.Select(OutputItemDto.FromInputItemDto).ToList();
    }

    // CSV Generation Performance
    [Benchmark]
    [Arguments(100)]
    [Arguments(1000)]
    [Arguments(10000)]
    public string GenerateCsvContent_Performance(int count)
    {
        var data = GenerateTestData(count);
        var outputItems = data.Select(OutputItemDto.FromInputItemDto).ToList();
        
        var csvContent = new System.Text.StringBuilder();
        csvContent.AppendLine("Local Time;Volume");
        
        foreach (var item in outputItems)
        {
            csvContent.AppendLine($"{item.DateTime};{item.Volume}");
        }
        
        return csvContent.ToString();
    }

    // Context Creation Performance
    [Benchmark]
    public ExtractContext CreateExtractContext_Performance()
    {
        return new ExtractContext
        {
            RequestedUtc = DateTime.UtcNow.AddDays(1),
            MaxRetryAttempts = 3,
            RetryDelaySeconds = 5
        };
    }

    // Result Creation Performance
    [Benchmark]
    public ExtractResult CreateSuccessResult_Performance()
    {
        return ExtractResult.Success(1);
    }

    [Benchmark]
    public ExtractResult CreateFailureResult_Performance()
    {
        return ExtractResult.Failure(1, "Test failure");
    }

    // DateTime Operations Performance
    [Benchmark]
    [Arguments(100)]
    [Arguments(1000)]
    [Arguments(10000)]
    public List<InputItemDto> SortByDateTime_Performance(int count)
    {
        var data = GenerateTestData(count);
        return data.OrderBy(item => item.DateTime).ToList();
    }

    [Benchmark]
    [Arguments(100)]
    [Arguments(1000)]
    [Arguments(10000)]
    public Dictionary<int, double> GroupByHour_Performance(int count)
    {
        var data = GenerateTestData(count);
        return data.GroupBy(item => item.DateTime.Hour)
                   .ToDictionary(g => g.Key, g => g.Sum(item => item.Volume));
    }

    // String Operations Performance
    [Benchmark]
    public string GenerateFileName_Performance()
    {
        var datePart = _testDate.ToString("yyyyMMdd_yyyyMMddHHmm");
        return $"PowerPosition_{datePart}.csv";
    }

    [Benchmark]
    public string GenerateCorrelationId_Performance()
    {
        return Guid.NewGuid().ToString("N")[..8];
    }

    // Validation Performance
    [Benchmark]
    [Arguments(100)]
    [Arguments(1000)]
    [Arguments(10000)]
    public bool ValidateData_Performance(int count)
    {
        var data = GenerateTestData(count);
        return data != null && data.Count > 0;
    }

    [Benchmark]
    [Arguments(0)]
    [Arguments(1)]
    [Arguments(60)]
    [Arguments(1440)]
    public bool ValidateInterval_Performance(int intervalMinutes)
    {
        return intervalMinutes >= 0 && intervalMinutes <= 10080; // Max 1 week
    }

    private static List<InputItemDto> GenerateTestData(int count)
    {
        var random = new Random(42); // Fixed seed for consistent results
        var data = new List<InputItemDto>();
        var baseDate = DateTime.UtcNow.AddDays(1).Date;

        for (int i = 0; i < count; i++)
        {
            data.Add(new InputItemDto
            {
                DateTime = baseDate.AddHours(i),
                Volume = random.NextDouble() * 1000 - 500 // Random volume between -500 and 500
            });
        }

        return data;
    }
}
