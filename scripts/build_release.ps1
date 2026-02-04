param(
    [string]$PublishDir = "publish",
    [string]$OutDir = "release",
    [switch]$WixInstalled
)

Write-Host "PublishDir: $PublishDir"
Write-Host "OutDir: $OutDir"

if (-Not (Test-Path $PublishDir)) { Write-Error "Publish dir not found: $PublishDir"; exit 1 }

# Create release layout
$launcherDir = Join-Path $OutDir "launcher"
Remove-Item -Recurse -Force $launcherDir -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Path $launcherDir | Out-Null

# Copy published files
Copy-Item -Path (Join-Path $PublishDir '*') -Destination $launcherDir -Recurse -Force

# Copy docs and tools
Copy-Item -Path docs -Destination $OutDir -Recurse -Force
if (Test-Path tools) { Copy-Item -Path tools -Destination $OutDir -Recurse -Force }

# Ensure config.json template exists
$configPath = Join-Path $launcherDir 'config.json'
if (-Not (Test-Path $configPath)) { '{ "ZeroTierNetworkId": "f3797ba7a8ab2c2b" }' | Out-File -FilePath $configPath -Encoding utf8 }

# Create zip
$zipPath = Join-Path $OutDir 'ee-lan-launcher.zip'
if (Test-Path $zipPath) { Remove-Item $zipPath }
Add-Type -AssemblyName System.IO.Compression.FileSystem
[System.IO.Compression.ZipFile]::CreateFromDirectory($launcherDir, $zipPath)
Write-Host "Created $zipPath"

# Build MSI if WiX installed
if ($WixInstalled)
{
    # Harvest the launcher directory into a component fragment
    $harvestOut = Join-Path $OutDir 'components.wxs'
    & "$env:ProgramFiles(x86)\WiX Toolset v3.11\bin\heat.exe" dir $launcherDir -cg LAUNCHER_CONTENT -dr INSTALLFOLDER -ag -sfrag -srd -o $harvestOut

    # Copy a simple product wxs template
    $template = "$PSScriptRoot\..\installer\wix\installer.wxs"
    if (-Not (Test-Path $template)) { Write-Error "WiX template missing: $template"; exit 1 }

    # Compile
    Push-Location $OutDir
    & "$env:ProgramFiles(x86)\WiX Toolset v3.11\bin\candle.exe" -nologo ..\installer\wix\installer.wxs components.wxs
    & "$env:ProgramFiles(x86)\WiX Toolset v3.11\bin\light.exe" -nologo -ext WixUIExtension -cultures:en-us -out ee-lan-launcher.msi installer.wixobj components.wixobj
    Pop-Location

    Write-Host "Built MSI: $OutDir\ee-lan-launcher.msi"
}
else
{
    Write-Host "WiX not installed — skipping MSI build. Set -WixInstalled to build MSI (or ensure WiX is available on PATH)."
}
