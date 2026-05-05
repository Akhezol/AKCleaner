# AKCleaner -> https://github.com/Akhezol/AKCleaner
# Calistir:  powershell -ExecutionPolicy Bypass -File .\sync-github.ps1
# Zorla uzerine yaz:  .\sync-github.ps1 -Force

param(
    [switch]$Force
)

$ErrorActionPreference = "Stop"
Set-Location $PSScriptRoot

Write-Host "==> Klasor: $PSScriptRoot" -ForegroundColor Cyan

# Bos legacy klasorleri temizle
$dead = @(
    'desktop-ui\electron', 'desktop-ui\scripts', 'desktop-ui\src', 'desktop-ui',
    'src\AKCleaner.Core', 'src\AKCleaner.Agent', 'src',
    'tests\AKCleaner.Core.Tests', 'tests'
)
foreach ($rel in $dead) {
    $p = Join-Path $PSScriptRoot $rel
    if (Test-Path -LiteralPath $p) {
        Write-Host "Siliniyor: $rel" -ForegroundColor DarkYellow
        Remove-Item -LiteralPath $p -Recurse -Force
    }
}

if (-not (Get-Command git -ErrorAction SilentlyContinue)) {
    Write-Host "HATA: git yuklu degil. Git for Windows kurun." -ForegroundColor Red
    exit 1
}

if (-not (Test-Path -LiteralPath ".git")) {
    Write-Host "==> git init" -ForegroundColor Cyan
    git init
}

Write-Host "==> remote origin" -ForegroundColor Cyan
git remote remove origin 2>$null
git remote add origin "https://github.com/Akhezol/AKCleaner.git"

Write-Host "==> git add / commit" -ForegroundColor Cyan
git add -A
git status

$msg = "refactor: WPF masaustu, eski Electron/Core kaldirildi"
git diff --cached --quiet
if ($LASTEXITCODE -eq 0) {
    Write-Host "Degisiklik yok (commit atlandi)." -ForegroundColor Yellow
} else {
    git commit -m $msg
}

git branch -M main

Write-Host "==> git push" -ForegroundColor Cyan
if ($Force) {
    git push -u origin main --force-with-lease
} else {
    git push -u origin main
    if ($LASTEXITCODE -ne 0) {
        Write-Host "`nIlk push reddedildi. --force-with-lease deneniyor (uzak gecmisi ezilebilir)..." -ForegroundColor Yellow
        git push -u origin main --force-with-lease
    }
}

if ($LASTEXITCODE -eq 0) {
    Write-Host "`nTamam: https://github.com/Akhezol/AKCleaner" -ForegroundColor Green
} else {
    Write-Host "`nPush basarisiz. Deneyin:" -ForegroundColor Red
    Write-Host "  gh auth login" -ForegroundColor White
    Write-Host "  veya Git Credential Manager ile GitHub PAT" -ForegroundColor White
    Write-Host "  veya: .\sync-github.ps1 -Force" -ForegroundColor White
    exit 1
}
