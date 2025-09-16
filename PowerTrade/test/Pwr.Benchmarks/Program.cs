using BenchmarkDotNet.Running;

namespace Pwr.Tests.Benchmarks;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("PowerTrade Simple Performance Benchmarking");
        Console.WriteLine("===========================================");
        Console.WriteLine();

        if (args.Length > 0 && args[0].ToLowerInvariant() == "help")
        {
            ShowHelp();
            return;
        }

        Console.WriteLine("Running simplified performance benchmarks...");
        Console.WriteLine("This will test core data processing operations.");
        Console.WriteLine();

        // Run the simple benchmarks
        var summary = BenchmarkDotNet.Running.BenchmarkRunner.Run<SimpleBenchmarks>();
        
        Console.WriteLine();
        Console.WriteLine("✅ Benchmarking completed successfully!");
        Console.WriteLine("Check the 'BenchmarkDotNet.Artifacts' folder for detailed results.");
    }

    private static void ShowHelp()
    {
        Console.WriteLine("PowerTrade Simple Performance Benchmarking");
        Console.WriteLine("===========================================");
        Console.WriteLine();
        Console.WriteLine("Usage: dotnet run [help]");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  help    Show this help message");
        Console.WriteLine();
        Console.WriteLine("This simplified benchmark suite tests:");
        Console.WriteLine("  • Data generation performance");
        Console.WriteLine("  • Data processing operations");
        Console.WriteLine("  • CSV content generation");
        Console.WriteLine("  • DateTime operations");
        Console.WriteLine("  • String operations");
        Console.WriteLine("  • Validation operations");
        Console.WriteLine();
        Console.WriteLine("Results are saved to the 'BenchmarkDotNet.Artifacts' folder.");
    }
}
