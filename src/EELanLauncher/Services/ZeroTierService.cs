using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.ServiceProcess;
using EELanLauncher.Utils;

namespace EELanLauncher.Services;

public class ZeroTierService
{
    public enum NetworkState { NotInstalled, NotJoined, Joining, WaitingForAuth, Online }

    private string? _cliPath;

    public ZeroTierService()
    {
        _cliPath = FindCli();
    }

    public bool IsInstalled()
    {
        if (ServiceExists("ZeroTier One")) return true;
        return !string.IsNullOrEmpty(_cliPath);
    }

    private static bool ServiceExists(string name)
    {
        try
        {
            var sc = new ServiceController(name);
            var status = sc.Status; // may throw
            return true;
        }
        catch
        {
            return false;
        }
    }

    private string? FindCli()
    {
        var candidates = new[] {
            "zerotier-cli.exe",
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "ZeroTier", "One", "zerotier-cli.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "ZeroTier", "One", "zerotier-cli.exe")
        };
        foreach (var c in candidates)
        {
            try
            {
                if (File.Exists(c)) return c;
                // check PATH
                var which = RunHelper.RunAndReadOutput("where", c);
                if (!string.IsNullOrEmpty(which)) return which.Split(new[] {'\r','\n'}, StringSplitOptions.RemoveEmptyEntries)[0];
            }
            catch { }
        }
        return null;
    }

    public async Task<NetworkState> GetNetworkStateAsync(string networkId)
    {
        _cliPath ??= FindCli();
        if (string.IsNullOrEmpty(_cliPath)) return NetworkState.NotInstalled;

        try
        {
            // listnetworks
            var outp = await RunHelper.RunProcessAsync(_cliPath, "listnetworks");
            Utils.Logger.Log("zerotier-cli listnetworks: " + outp);

            // parse lines for network id
            var lines = outp.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var found = lines.Select(l => l.Trim()).FirstOrDefault(l => l.StartsWith(networkId));
            if (found == null) return NetworkState.NotJoined;

            // format: <netid> <status> <type> <name>
            // status may be OK, ACCESS_DENIED, REQUESTING_CONFIG, NOT_FOUND
            var parts = Regex.Split(found, "\s+");
            if (parts.Length < 2) return NetworkState.NotJoined;
            var status = parts[1];

            if (status == "ACCESS_DENIED") return NetworkState.WaitingForAuth;
            if (status == "OK")
            {
                // Check if an IPv4 is assigned by running "zerotier-cli info" or checking adapters elsewhere
                return NetworkState.Online;
            }

            return NetworkState.Joining;
        }
        catch (Exception ex)
        {
            Utils.Logger.Log("ZeroTier check failed: " + ex);
            return NetworkState.NotInstalled;
        }
    }

    public async Task<bool> JoinNetworkAsync(string networkId)
    {
        _cliPath ??= FindCli();
        if (string.IsNullOrEmpty(_cliPath)) return false;
        var outp = await RunHelper.RunProcessAsync(_cliPath, $"join {networkId}");
        Utils.Logger.Log("zerotier-cli join: " + outp);
        return true;
    }
}
