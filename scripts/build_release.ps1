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

    # Try to find WiX tools (heat/candle/light) via PATH or common install locations
    $heatCmd = (Get-Command heat.exe -ErrorAction SilentlyContinue)
    $candleCmd = (Get-Command candle.exe -ErrorAction SilentlyContinue)
    $lightCmd = (Get-Command light.exe -ErrorAction SilentlyContinue)

    if (-not $heatCmd -or -not $candleCmd -or -not $lightCmd)
    {
        # Try common Program Files locations
        $possible = Get-ChildItem 'C:\Program Files (x86)\' -Directory -ErrorAction SilentlyContinue | Where-Object { $_.Name -like 'WiX*' }
        foreach ($d in $possible)
        {
            if (-not $heatCmd -and (Test-Path (Join-Path $d.FullName 'bin\heat.exe'))) { $heatCmd = @{ Source = (Join-Path $d.FullName 'bin\heat.exe') } }
            if (-not $candleCmd -and (Test-Path (Join-Path $d.FullName 'bin\candle.exe'))) { $candleCmd = @{ Source = (Join-Path $d.FullName 'bin\candle.exe') } }
            if (-not $lightCmd -and (Test-Path (Join-Path $d.FullName 'bin\light.exe'))) { $lightCmd = @{ Source = (Join-Path $d.FullName 'bin\light.exe') } }
        }
    }

    if ($heatCmd -and $candleCmd -and $lightCmd)
    {
        Write-Host "Found WiX tools: $($heatCmd.Source), $($candleCmd.Source), $($lightCmd.Source)"
        & $heatCmd.Source dir $launcherDir -cg LAUNCHER_CONTENT -dr INSTALLFOLDER -ag -sfrag -srd -o $harvestOut

        # Copy a simple product wxs template
        $template = "$PSScriptRoot\..\installer\wix\installer.wxs"
        if (-Not (Test-Path $template)) { Write-Error "WiX template missing: $template"; exit 1 }

        # Compile
        Push-Location $OutDir
        & $candleCmd.Source -nologo ..\installer\wix\installer.wxs components.wxs
        & $lightCmd.Source -nologo -ext WixUIExtension -cultures:en-us -out ee-lan-launcher.msi installer.wixobj components.wixobj
        Pop-Location

        if (Test-Path (Join-Path $OutDir 'ee-lan-launcher.msi')) { Write-Host "Built MSI: $OutDir\ee-lan-launcher.msi" }
        else { Write-Host "WiX tools ran but MSI not found; check heat/candle/light output." }
    }
    else
    {
        Write-Host "WiX tools not found; skipping MSI build. Expected heat/candle/light on PATH or under C:\\Program Files (x86)\\WiX*";
    }
}
else
{
    Write-Host "WiX not requested — skipping MSI build. Set -WixInstalled to build MSI (or ensure WiX is available on PATH)."
}
