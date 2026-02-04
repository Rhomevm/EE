using System;
using System.Diagnostics;
using System.IO;
using EELanLauncher.Utils;

namespace EELanLauncher.Services;

public class GameLauncherService
{
    public void Launch(string? exePath)
    {
        if (string.IsNullOrEmpty(exePath) || !File.Exists(exePath)) throw new FileNotFoundException("EmpireEarth.exe not found.");
        var psi = new ProcessStartInfo
        {
            FileName = exePath,
            WorkingDirectory = Path.GetDirectoryName(exePath)!,
            UseShellExecute = true
        };
        Process.Start(psi);
        Logger.Log("Launched game: " + exePath);
    }
}
