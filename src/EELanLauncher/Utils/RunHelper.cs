using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace EELanLauncher.Utils;

public static class RunHelper
{
    public static string RunAndReadOutput(string fileName, string args)
    {
        try
        {
            var p = new ProcessStartInfo(fileName, args)
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var proc = Process.Start(p) ?? throw new InvalidOperationException("process start failed");
            var outp = proc.StandardOutput.ReadToEnd();
            proc.WaitForExit(2000);
            return outp;
        }
        catch (Exception ex)
        {
            return "";
        }
    }

    public static Task<string> RunProcessAsync(string fileName, string args)
    {
        return Task.Run(() => RunAndReadOutput(fileName, args));
    }
}
