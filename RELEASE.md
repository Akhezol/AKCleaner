# AKCleaner Release Guide

## Build
- `dotnet restore AKCleaner.sln`
- `dotnet build AKCleaner.sln`
- `dotnet publish ConsoleApp2/ConsoleApp2.csproj -c Release -r win-x64 --self-contained true -o artifacts/app`

## Signing
- Configure `WIN_CSC_LINK` and `WIN_CSC_KEY_PASSWORD` secrets in CI.
- Sign published binaries and installer artifacts.

## Distribution
- Stable releases use Git tags `vX.Y.Z`.
- Beta channel can be published from `beta/*` branches.
- Upload `msi` and `nsis` installer outputs together with `SHA256SUMS.txt`.
