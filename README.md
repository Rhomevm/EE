# Empire Earth LAN Launcher 🔧

A small Windows launcher to make Empire Earth (original, Patch 1.0.4.0) multiplayer reliable over WiFi by forcing LAN play over a ZeroTier virtual LAN.

⚠️ Important: The launcher does NOT modify or patch `EmpireEarth.exe`.

## Features ✅
- Forces players to use LAN mode only
- Uses ZeroTier to create a stable virtual LAN
- Avoids port forwarding, router configuration, and Direct IP
- Works on Windows 10 and Windows 11
- Detects ZeroTier installation and guides users to install it if missing
- Best-effort: sets ZeroTier adapter to Private profile and adds firewall rules (Admin required)
- Optional: installs `ddraw.dll` (DDrawCompat) into the game folder (backup is kept)
- Computes and displays SHA-256 of `EmpireEarth.exe` for version matching
- Logs to `%AppData%\EE-LAN-Launcher\logs\`

## Quick Start for users 🕹️
1. Install ZeroTier: https://www.zerotier.com/download/
2. Join network: `f3797ba7a8ab2c2b` (Private network). Wait for authorization in ZeroTier Central.
3. Open Launcher, locate your `EmpireEarth.exe` (one time)
4. Always use LAN in-game: Host -> Create LAN game. Join -> Refresh LAN list.

## Build (developer) 🔧
- Requires .NET 8 SDK on Windows
- Build: `dotnet build src/EELanLauncher/EELanLauncher.csproj -c Release`
- To create a release layout and optional MSI installer locally, run:
  - `dotnet publish src/EELanLauncher/EELanLauncher.csproj -c Release -r win-x64 --self-contained false -o publish`
  - `pwsh scripts/build_release.ps1 -PublishDir publish -OutDir release -WixInstalled`

## Release layout (what to ship) 📦
```
Launcher.exe
config.json
/tools/ddrawcompat/
/docs/README.txt
```
Note: `tools/ddrawcompat/ddraw.dll` should be provided by the distributor or placed by the user (see `/tools/ddrawcompat/README.txt`).

## Packaging / CI
- A GitHub Actions workflow is included at `.github/workflows/release.yml` which builds and uploads the release layout when you run it manually or push a `v*` tag.
- If WiX is available in the build environment the workflow will also generate an `ee-lan-launcher.msi` using the WiX Toolset (harvests the release folder via `heat.exe`).

## Logs & Config 🗂️
- Config: `%AppData%\EE-LAN-Launcher\config.json`
- Logs: `%AppData%\EE-LAN-Launcher\logs\`

## Security / Legal
- Do NOT distribute game files
- Do NOT modify or patch `EmpireEarth.exe`
- Do NOT include piracy instructions

## Tests / QA ✅
See `/docs/TEST_PLAN.md` for the test matrix and steps.

---

For more details see `/docs/README.txt` and the `docs/` folder.
