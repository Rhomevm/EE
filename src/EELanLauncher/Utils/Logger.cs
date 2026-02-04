using System;
using System.IO;

namespace EELanLauncher.Utils;

public static class Logger
{
    public static string AppDataPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EE-LAN-Launcher");
    public static string LogsPath => Path.Combine(AppDataPath, "logs");

    public static void Log(string s)
    {
        try
        {
            var dir = LogsPath;
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            var f = Path.Combine(dir, DateTime.UtcNow.ToString("yyyyMMdd") + ".log");
            File.AppendAllText(f, DateTime.UtcNow.ToString("o") + " - " + s + Environment.NewLine);
        }
        catch { }
    }
}
