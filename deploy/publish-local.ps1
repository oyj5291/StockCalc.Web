param(
    [string]$Output = ".\publish"
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot
$project = Join-Path $repoRoot "StockCalc.Web\StockCalc.Web.csproj"
$outputPath = Join-Path $repoRoot $Output

dotnet publish $project -c Release -o $outputPath

Write-Host "Published to $outputPath"
