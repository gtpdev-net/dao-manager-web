# DAO Manager - Test Coverage Report
# PowerShell version for cross-platform support

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  DAO Manager - Test Coverage Report" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

# Clean previous results
Write-Host "Cleaning previous test results..." -ForegroundColor Blue
Get-ChildItem -Path . -Recurse -Directory -Filter "TestResults" | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path "./coveragereport" -Recurse -Force -ErrorAction SilentlyContinue

# Restore packages
Write-Host "Restoring packages..." -ForegroundColor Blue
dotnet restore

# Run tests with coverage
Write-Host "Running tests with coverage collection..." -ForegroundColor Blue
dotnet test --no-restore --settings coverageSettings.runsettings

# Check if reportgenerator is installed
$reportgen = Get-Command reportgenerator -ErrorAction SilentlyContinue
if (-not $reportgen) {
    Write-Host "ReportGenerator not found. Installing as global tool..." -ForegroundColor Yellow
    dotnet tool install -g dotnet-reportgenerator-globaltool

    # Refresh PATH
    $env:PATH = [System.Environment]::GetEnvironmentVariable("PATH", "Machine") + ";" + [System.Environment]::GetEnvironmentVariable("PATH", "User")
}

# Find coverage files
$coverageFiles = Get-ChildItem -Path "./tests" -Recurse -Filter "coverage.cobertura.xml" | Select-Object -First 1

if (-not $coverageFiles) {
    Write-Host "Warning: No coverage files found. Make sure tests ran successfully." -ForegroundColor Yellow
    exit 1
}

# Generate HTML report
Write-Host "Generating HTML coverage report..." -ForegroundColor Blue
reportgenerator `
    -reports:"./tests/**/TestResults/**/coverage.cobertura.xml" `
    -targetdir:"./coveragereport" `
    -reporttypes:"Html;HtmlSummary;Badges;TextSummary" `
    -title:"DAO Manager Coverage Report" `
    -verbosity:"Warning"

# Display summary
Write-Host ""
Write-Host "============================================" -ForegroundColor Green
Write-Host "  Coverage Report Generated Successfully!" -ForegroundColor Green
Write-Host "============================================" -ForegroundColor Green
Write-Host ""

# Show text summary if available
if (Test-Path "./coveragereport/Summary.txt") {
    Get-Content "./coveragereport/Summary.txt"
    Write-Host ""
}

Write-Host "HTML Report Location: " -NoNewline -ForegroundColor Blue
Write-Host "./coveragereport/index.html"
Write-Host ""
Write-Host "To view the report:" -ForegroundColor Yellow
Write-Host "  - Open: ./coveragereport/index.html"
Write-Host "  - Or run: Invoke-Item ./coveragereport/index.html"
Write-Host ""

# Optionally open in browser
if ($args -contains "--open" -or $args -contains "-o") {
    Write-Host "Opening report in browser..." -ForegroundColor Blue
    Invoke-Item "./coveragereport/index.html"
}
