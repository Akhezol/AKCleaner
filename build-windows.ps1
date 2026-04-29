param(
  [string]$Configuration = "Release",
  [string]$Runtime = "win-x64"
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$solution = Join-Path $repoRoot "AKCleaner.sln"
$agentProject = Join-Path $repoRoot "src\AKCleaner.Agent\AKCleaner.Agent.csproj"
$uiRoot = Join-Path $repoRoot "desktop-ui"
$agentOut = Join-Path $uiRoot "agent"

Write-Host "==> Building: $solution ($Configuration)"
dotnet build $solution -c $Configuration | Out-Host

Write-Host "==> Publishing agent to: $agentOut"
if (Test-Path $agentOut) { Remove-Item -Recurse -Force $agentOut }
New-Item -ItemType Directory -Path $agentOut | Out-Null

dotnet publish $agentProject -c $Configuration -r $Runtime --self-contained true -o $agentOut | Out-Host

Write-Host "==> Building desktop UI"
Push-Location $uiRoot

if (!(Test-Path (Join-Path $uiRoot "node_modules"))) {
  Write-Host "node_modules yok; npm install çalıştırılıyor..."
  npm install | Out-Host
}

npm run build | Out-Host
Pop-Location

Write-Host "==> Done."
