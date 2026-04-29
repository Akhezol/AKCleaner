# AKCleaner (Windows v1)

AKCleaner is a Windows cleaner product built with an Electron desktop UI and a C# cleanup engine.

## Project Structure
- `src/AKCleaner.Core`: scan/delete engine, safety rules, audit logging.
- `src/AKCleaner.Agent`: IPC bridge between Electron and cleaner core.
- `desktop-ui`: Electron application (`home`, `quick scan`, `advanced`, `settings`, `history`).
- `tests/AKCleaner.Core.Tests`: unit tests for core behavior.

## Local Development
1. Build backend:
   - `dotnet build AKCleaner.sln`
2. Run tests:
   - `dotnet test AKCleaner.sln`
3. Start desktop UI:
   - `cd desktop-ui`
   - `./dev.sh`

## Safety Features
- Scan and cleanup are separate operations.
- Optional recycle bin deletion mode.
- Protected system path guard + include/exclude path filters.
- Action audit logs under `%LOCALAPPDATA%\\AKCleaner\\logs`.
