# How to Run the PowerTrade Application

## Prerequisites

1. **.NET 8.0 SDK** - Make sure you have .NET 8.0 installed
2. **Visual Studio 2022** or **VS Code** (optional)
3. **Windows OS** - Required for the CSV file paths

## Running the Application

### Method 1: Using Visual Studio
1. Open the solution file `PowerTrade.sln` in Visual Studio 2022
2. Set `Pwr.App` as the startup project (right-click → Set as Startup Project)
3. Press **F5** or click the **Start** button
4. The console application will start and show the background services running

### Method 2: Using Command Line
1. Open Command Prompt or PowerShell
2. Navigate to the project directory:
   ```bash
   cd "C:\Users\DanielUrdanetaOropez\source\repos\Daniel_Urdaneta\axpo-pwr\PowerTrade\src\Pwr.App"
   ```
3. Run the application:
   ```bash
   dotnet run
   ```

### Method 3: Using .NET CLI from Solution Root
1. Navigate to the solution root:
   ```bash
   cd "C:\Users\DanielUrdanetaOropez\source\repos\Daniel_Urdaneta\axpo-pwr\PowerTrade"
   ```
2. Run the application:
   ```bash
   dotnet run --project src\Pwr.App\Pwr.ConsoleApp.csproj
   ```

## What You Should See

When the application starts successfully, you should see output like this:

```
Starting PowerTrade application...
Starting background services...
Application started successfully!
Background services are running:
- ScheduledExtractService: Automated data extraction
- TimerService: Scheduling management

Press Ctrl+C to stop the application...
```

## Background Service Behavior

The `ScheduledExtractService` will:

1. **Start automatically** when the application launches
2. **Run every 3 minutes** (configurable in `appsettings.json`)
3. **Extract power trading data** for the next day
4. **Generate CSV reports** in the configured folder
5. **Log all activities** to the console

## Configuration

You can modify the behavior by editing `src\Pwr.App\appsettings.json`:

```json
{
  "ExtractTradesOptions": {
    "ExtractIntervalMinutes": 3,    // Change this to run more/less frequently
    "MaxRetryAttempts": 3,          // Number of retry attempts
    "RetryDelaySeconds": 30         // Delay between retries
  },
  "CsvOptions": {
    "FolderPath": "C:\\CsvFiles\\"  // Change this to your desired output folder
  }
}
```

## Stopping the Application

- **In Visual Studio**: Press **Shift+F5** or click the **Stop** button
- **In Command Line**: Press **Ctrl+C**
- The application will gracefully shut down all background services

## Troubleshooting

### Common Issues

1. **"Project not found" error**:
   - Make sure you're in the correct directory
   - Run `dotnet restore` first

2. **"PowerService.dll not found" error**:
   - Ensure the PowerService.dll is in the `assets\PowerServiceDll\` folder
   - Check that the reference path in the project file is correct

3. **"Configuration not found" error**:
   - Make sure `appsettings.json` exists in the `src\Pwr.App` folder
   - Check that the file has valid JSON syntax

4. **"Permission denied" for CSV folder**:
   - Make sure the folder path in `CsvOptions.FolderPath` exists
   - Check that you have write permissions to that folder

### Logs

The application provides detailed logging. Look for:
- **Information**: Normal operation flow
- **Warning**: Non-critical issues (e.g., no data available)
- **Error**: Retryable failures
- **Critical**: Unrecoverable failures

## Expected Output Files

The application will create CSV files in the configured folder with names like:
- `PowerPosition_20241201_202412011200.csv`
- `PowerPosition_20241201_202412011500.csv`

Each file contains power trading data for specific time periods.

## Next Steps

Once the application is running:
1. **Monitor the console output** for any errors or warnings
2. **Check the CSV output folder** for generated files
3. **Adjust configuration** as needed for your requirements
4. **Review logs** for any issues or performance insights
