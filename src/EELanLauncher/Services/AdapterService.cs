using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Net.Sockets;
using EELanLauncher.Utils;
using System.Diagnostics;

namespace EELanLauncher.Services;

public class AdapterService
{
    public string? LanIp { get; private set; }
    public string? AdapterName { get; private set; }

    public async Task RefreshAsync()
    {
        await Task.Run(() =>
        {
            LanIp = null;
            AdapterName = null;
            var ifs = NetworkInterface.GetAllNetworkInterfaces();
            foreach (var ni in ifs)
            {
                try
                {
                    if (!ni.Name.Contains("ZeroTier One")) continue;
                    var props = ni.GetIPProperties();
                    var v4 = props.UnicastAddresses.FirstOrDefault(a => a.Address.AddressFamily == AddressFamily.InterNetwork && a.Address.ToString().StartsWith("10."));
                    if (v4 != null)
                    {
                        LanIp = v4.Address.ToString();
                        AdapterName = ni.Name;
                        Utils.Logger.Log($"Found ZeroTier adapter {AdapterName} with ip {LanIp}");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Utils.Logger.Log("AdapterService exception: " + ex.Message);
                }
            }
        });
    }

    public bool IsProfilePrivate()
    {
        try
        {
            if (string.IsNullOrEmpty(AdapterName)) return false;
            var outp = RunHelper.RunAndReadOutput("powershell", $"-Command \"Get-NetConnectionProfile -InterfaceAlias '{AdapterName}' | Select -ExpandProperty NetworkCategory\" ");
            return outp?.Trim().Equals("Private", StringComparison.OrdinalIgnoreCase) ?? false;
        }
        catch (Exception ex)
        {
            Utils.Logger.Log("IsProfilePrivate failed: " + ex.Message);
            return false;
        }
    }

    public async Task<bool> TrySetProfilePrivateAsync()
    {
        try
        {
            if (string.IsNullOrEmpty(AdapterName)) return false;
            var cmd = $"Set-NetConnectionProfile -InterfaceAlias '{AdapterName}' -NetworkCategory Private";
            var outp = await RunHelper.RunProcessAsync("powershell", $"-Command \"{cmd}\"");
            Utils.Logger.Log("Set-NetConnectionProfile: " + outp);
            return true;
        }
        catch (Exception ex)
        {
            Utils.Logger.Log("TrySetProfilePrivateAsync failed: " + ex.Message);
            return false;
        }
    }
}
