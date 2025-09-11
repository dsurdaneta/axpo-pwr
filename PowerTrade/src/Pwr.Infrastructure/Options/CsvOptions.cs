using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pwr.Infrastructure.Options;

public class CsvOptions
{
    public const string SectionName = "CsvOptions";

    public string FolderPath { get; set; } = string.Empty;
}
