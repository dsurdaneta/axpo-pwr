using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pwr.Domain;

public static class DateTimeExtensions
{
    public static string ToUniversalIso8601(this DateTime dateTime) =>
        dateTime.ToUniversalTime().ToString("u").Replace(" ", "T");
}
