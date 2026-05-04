param(
  [string]$Configuration = "Release",
  [string]$Runtime = "win-x64"
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$solution = Join-Path $repoRoot "AKCleaner.sln"
$guiProject = Join-Path $repoRoot "ConsoleApp2\ConsoleApp2.csproj"
$outDir = Join-Path $repoRoot "artifacts\app"

Write-Host "==> Building: $solution ($Configuration)"
dotnet build $solution -c $Configuration | Out-Host

Write-Host "==> Publishing WPF app to: $outDir"
if (Test-Path $outDir) { Remove-Item -Recurse -Force $outDir }
New-Item -ItemType Directory -Path $outDir | Out-Null

dotnet publish $guiProject -c $Configuration -r $Runtime --self-contained true -o $outDir | Out-Host

Write-Host "==> Done."
