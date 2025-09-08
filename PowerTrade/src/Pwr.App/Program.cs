using Axpo;

namespace Pwr.App
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                IPowerService service = new PowerService();
                var requestedUtc = DateTime.UtcNow;
                var trades = await service.GetTradesAsync(requestedUtc);
                Console.WriteLine($"Date Requested in UTC: {requestedUtc}");
                Console.WriteLine($"Trades count: {trades.Count()}");

                var firstTrade = trades.FirstOrDefault();
                var periodsCount = firstTrade.Periods.Count();
                Console.WriteLine($"Periods count: {periodsCount}");

                for (int i = 0; i < firstTrade.Periods.Length; i++)
                {
                    var calculatedVolume = trades.Sum(t => t.Periods[i].Volume);
                    var periodCounter = i + 1;
                    var periodAsDateTime = new DateTime(requestedUtc.Year, requestedUtc.Month, requestedUtc.Day, requestedUtc.Hour + periodCounter, 0, 0);
                    Console.WriteLine($"DateTime {periodAsDateTime} - Volume {calculatedVolume}");
                }
            }
            catch (Axpo.PowerServiceException ex)
            {
                Console.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            Console.WriteLine("Press any key");
            Console.ReadLine();
        }
    }
}
