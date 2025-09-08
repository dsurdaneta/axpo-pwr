using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pwr.App.Options;

public class CsvOptions
{
    public const string SectionName = "CsvOptions";
    public const string FilePrefix = "PowerPosition";
    public const string FileExtension = "csv";
    public const string FileFormat = "YYYYMMDD_YYYYMMDDHHMM";

    public string FolderPath { get; set; } = string.Empty;
    public int ExtractIntervalMinutes { get; set; } = 15;
}
