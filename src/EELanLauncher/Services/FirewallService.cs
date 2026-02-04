using System;
using System.IO;
using System.Threading.Tasks;
using EELanLauncher.Utils;

namespace EELanLauncher.Services;

public class FirewallService
{
    public static string RulePrefix = "EE-LAN-Launcher";

    public async Task<bool> AddRulesForExecutableAsync(string exePath)
    {
        if (string.IsNullOrEmpty(exePath) || !File.Exists(exePath)) return false;
        var name = Path.GetFileNameWithoutExtension(exePath);
        var ruleNameIn = $"{RulePrefix}-{name}-in";
        var ruleNameOut = $"{RulePrefix}-{name}-out";
        try
        {
            // Idempotent: delete if exists then add
            await RunPowershellAsync($"If (Get-NetFirewallRule -DisplayName '{ruleNameIn}' -ErrorAction SilentlyContinue) {{ Remove-NetFirewallRule -DisplayName '{ruleNameIn}' }}; New-NetFirewallRule -DisplayName '{ruleNameIn}' -Direction Inbound -Action Allow -Program '{exePath}' -Profile Private");
            await RunPowershellAsync($"If (Get-NetFirewallRule -DisplayName '{ruleNameOut}' -ErrorAction SilentlyContinue) {{ Remove-NetFirewallRule -DisplayName '{ruleNameOut}' }}; New-NetFirewallRule -DisplayName '{ruleNameOut}' -Direction Outbound -Action Allow -Program '{exePath}' -Profile Private");
            Utils.Logger.Log("Firewall rules added for " + exePath);
            return true;
        }
        catch (Exception ex)
        {
            Utils.Logger.Log("AddRulesForExecutableAsync failed: " + ex.Message);
            return false;
        }
    }

    private static Task<string> RunPowershellAsync(string cmd) => RunHelper.RunProcessAsync("powershell", $"-Command \"{cmd}\"");
}
