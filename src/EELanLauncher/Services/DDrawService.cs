using System;
using System.IO;
using System.Threading.Tasks;
using EELanLauncher.Utils;

namespace EELanLauncher.Services;

public static class DDrawService
{
    public static string ToolsPath => Path.Combine(AppContext.BaseDirectory, "tools", "ddrawcompat");

    public static async Task<bool> InstallAsync(string gameExePath, bool enable)
    {
        var gameDir = Path.GetDirectoryName(gameExePath) ?? throw new InvalidOperationException("Game dir invalid");
        var src = Path.Combine(ToolsPath, "ddraw.dll");
        if (!File.Exists(src)) throw new FileNotFoundException("DDrawCompat not found in tools/ddrawcompat/");

        var dest = Path.Combine(gameDir, "ddraw.dll");
        var bak = Path.Combine(gameDir, "ddraw.dll.bak");

        await Task.Run(() =>
        {
            if (enable)
            {
                if (File.Exists(dest))
                {
                    if (!File.Exists(bak)) File.Move(dest, bak);
                    else File.Delete(dest);
                }
                File.Copy(src, dest, true);
                Logger.Log("DDrawCompat installed to " + gameDir);
            }
            else
            {
                if (File.Exists(bak))
                {
                    File.Copy(bak, dest, true);
                    File.Delete(bak);
                }
                else if (File.Exists(dest))
                {
                    File.Delete(dest);
                }
            }
        });

        return true;
    }
}
