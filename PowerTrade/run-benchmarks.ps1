# PowerTrade Simple Performance Benchmarking Script
# This script provides an easy way to run simplified performance benchmarks

param(
    [string]$Configuration = "Release",
    [switch]$Verbose,
    [switch]$Help
)

if ($Help) {
    Write-Host "PowerTrade Simple Performance Benchmarking Script" -ForegroundColor Green
    Write-Host "=================================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "Usage: .\run-benchmarks.ps1 [options]" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Options:" -ForegroundColor Yellow
    Write-Host "  -Configuration <cfg>  Build configuration (Debug, Release)" -ForegroundColor White
    Write-Host "  -Verbose             Enable verbose logging" -ForegroundColor White
    Write-Host "  -Help                Show this help message" -ForegroundColor White
    Write-Host ""
    Write-Host "Examples:" -ForegroundColor Yellow
    Write-Host "  .\run-benchmarks.ps1                    # Run simplified benchmarks" -ForegroundColor White
    Write-Host "  .\run-benchmarks.ps1 -Configuration Debug  # Run in debug mode" -ForegroundColor White
    Write-Host "  .\run-benchmarks.ps1 -Verbose          # Run with verbose output" -ForegroundColor White
    Write-Host ""
    Write-Host "This simplified benchmark suite tests:" -ForegroundColor Cyan
    Write-Host "  • Data generation performance" -ForegroundColor White
    Write-Host "  • Data processing operations" -ForegroundColor White
    Write-Host "  • CSV content generation" -ForegroundColor White
    Write-Host "  • DateTime operations" -ForegroundColor White
    Write-Host "  • String operations" -ForegroundColor White
    Write-Host "  • Validation operations" -ForegroundColor White
    exit 0
}

# Set error action preference
$ErrorActionPreference = "Stop"

# Colors for output
$SuccessColor = "Green"
$InfoColor = "Cyan"
$WarningColor = "Yellow"
$ErrorColor = "Red"

Write-Host "PowerTrade Simple Performance Benchmarking" -ForegroundColor $SuccessColor
Write-Host "===========================================" -ForegroundColor $SuccessColor
Write-Host ""

# Set working directory
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$BenchmarkDir = Join-Path $ScriptDir "test\Pwr.Benchmarks"

if (-not (Test-Path $BenchmarkDir)) {
    Write-Host "Error: Benchmark directory not found: $BenchmarkDir" -ForegroundColor $ErrorColor
    exit 1
}

Set-Location $BenchmarkDir

Write-Host "Working Directory: $BenchmarkDir" -ForegroundColor $InfoColor
Write-Host "Configuration: $Configuration" -ForegroundColor $InfoColor
Write-Host ""

try {
    # Restore packages
    Write-Host "📦 Restoring NuGet packages..." -ForegroundColor $InfoColor
    dotnet restore
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to restore packages"
    }
    Write-Host "✅ Packages restored successfully" -ForegroundColor $SuccessColor
    Write-Host ""

    # Build project
    Write-Host "🔨 Building project..." -ForegroundColor $InfoColor
    dotnet build --configuration $Configuration --no-restore
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to build project"
    }
    Write-Host "✅ Project built successfully" -ForegroundColor $SuccessColor
    Write-Host ""

    # Prepare run arguments
    $RunArgs = @()
    if ($Verbose) {
        $RunArgs += "--verbose"
    }

    # Run simplified benchmarks
    Write-Host "🚀 Starting simplified benchmark execution..." -ForegroundColor $InfoColor
    Write-Host "This will test core data processing operations." -ForegroundColor $InfoColor
    Write-Host ""

    $StartTime = Get-Date
    dotnet run --configuration $Configuration @RunArgs
    $EndTime = Get-Date
    $Duration = $EndTime - $StartTime

    if ($LASTEXITCODE -ne 0) {
        throw "Benchmark execution failed"
    }

    Write-Host ""
    Write-Host "✅ Simplified benchmarks completed successfully!" -ForegroundColor $SuccessColor
    Write-Host "⏱️  Total execution time: $($Duration.ToString('hh\:mm\:ss'))" -ForegroundColor $InfoColor
    Write-Host ""

    # Check for BenchmarkDotNet artifacts
    $ArtifactsDir = Join-Path $BenchmarkDir "BenchmarkDotNet.Artifacts"
    if (Test-Path $ArtifactsDir) {
        Write-Host "📊 Generated Reports:" -ForegroundColor $InfoColor
        $ReportFiles = Get-ChildItem $ArtifactsDir -File -Recurse | Where-Object { $_.Extension -in @('.html', '.md', '.csv', '.json') }
        if ($ReportFiles.Count -gt 0) {
            foreach ($file in $ReportFiles) {
                $fileSize = [math]::Round($file.Length / 1KB, 2)
                Write-Host "  📄 $($file.Name) ($fileSize KB)" -ForegroundColor $InfoColor
            }
        }
        Write-Host ""
        Write-Host "📁 Reports location: $ArtifactsDir" -ForegroundColor $InfoColor
    }

    Write-Host ""
    Write-Host "🎉 Simple performance benchmarking completed successfully!" -ForegroundColor $SuccessColor

} catch {
    Write-Host ""
    Write-Host "❌ Error: $($_.Exception.Message)" -ForegroundColor $ErrorColor
    Write-Host ""
    Write-Host "Troubleshooting tips:" -ForegroundColor $WarningColor
    Write-Host "1. Ensure .NET 8.0 SDK is installed" -ForegroundColor $WarningColor
    Write-Host "2. Check that all project dependencies are available" -ForegroundColor $WarningColor
    Write-Host "3. Verify sufficient system resources (memory, disk space)" -ForegroundColor $WarningColor
    Write-Host "4. Try running with -Configuration Debug for more detailed error information" -ForegroundColor $WarningColor
    exit 1
} finally {
    # Return to original directory
    Set-Location $ScriptDir
}
