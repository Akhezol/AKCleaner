# AKCleaner Release Guide

## Build
- `dotnet restore AKCleaner.sln`
- `dotnet build AKCleaner.sln`
- `dotnet publish ConsoleApp2/ConsoleApp2.csproj -c Release -r win-x64 --self-contained true -o artifacts/app`
- Yerelde otomasyon: `.\build-windows.ps1`

## CI
- Etiket `v*` itibarıyla GitHub Actions derlemesi: `.github/workflows/release.yml`

## Dağıtım
- Kararlı sürümler: Git etiketleri `vX.Y.Z`
- Yayın çıktısı: `artifacts/app` (publish klasörü) veya workflow artifact’ı
