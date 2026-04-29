# AKCleaner Release Guide

## Build
- `dotnet restore AKCleaner.sln`
- `dotnet test AKCleaner.sln`
- `dotnet publish src/AKCleaner.Agent/AKCleaner.Agent.csproj -c Release -r win-x64 --self-contained true`
- `cd desktop-ui && npm ci && npm run build`

## Signing
- Configure `WIN_CSC_LINK` and `WIN_CSC_KEY_PASSWORD` secrets in CI.
- Sign both agent binary and installer artifacts.

## Distribution
- Stable releases use Git tags `vX.Y.Z`.
- Beta channel can be published from `beta/*` branches.
- Upload `msi` and `nsis` installer outputs together with `SHA256SUMS.txt`.
