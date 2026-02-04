# Building Release & MSI Installer

This document explains how to create the Release layout and an MSI installer for the launcher.

Requirements (Windows)
- .NET 8 SDK
- WiX Toolset (if you want to build MSI)

Quick manual build
1. dotnet publish src/EELanLauncher/EELanLauncher.csproj -c Release -r win-x64 --self-contained false -o publish
2. pwsh scripts/build_release.ps1 -PublishDir publish -OutDir release -WixInstalled
   - The script will copy published files to `release/launcher/`, add docs and tools, create `ee-lan-launcher.zip` and build `ee-lan-launcher.msi` (if WiX is installed).

CI
- A GitHub Actions workflow is added at `.github/workflows/release.yml` that builds on `workflow_dispatch` or tag pushes and uploads the release artifact.

Notes
- The WiX step uses `heat.exe` to harvest the release folder and `candle.exe`/`light.exe` to compile an MSI. The WiX template is in `installer/wix/installer.wxs`.
- Ensure you set a stable `UpgradeCode` in the WiX template if you plan to publish updates.
