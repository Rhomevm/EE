using System;
using System.IO;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;

namespace EELanLauncher;

public partial class MainWindow : Window
{
    private readonly Services.ZeroTierService _zt = new();
    private readonly Services.AdapterService _adapter = new();
    private readonly Services.FirewallService _firewall = new();
    private readonly Services.GameLauncherService _gameLauncher = new();
    private Models.Config _config = Models.Config.Load();

    public MainWindow()
    {
        InitializeComponent();
        NetworkIdText.Text = _config.ZeroTierNetworkId ?? "f3797ba7a8ab2c2b";
        GamePathText.Text = _config.GamePath ?? "";
        ShaText.Text = "";
        UpdateUi();
        _ = RefreshStatusAsync();
    }

    private void UpdateUi()
    {
        InstallZtButton.Visibility = _zt.IsInstalled() ? Visibility.Collapsed : Visibility.Visible;
        RecheckZtButton.IsEnabled = true;
        CopyIpButton.IsEnabled = !string.IsNullOrEmpty(_adapter.LanIp);
        GamePathText.Text = _config.GamePath ?? "";
        ShaText.Text = !string.IsNullOrEmpty(_config.GamePath) && File.Exists(_config.GamePath) ? Utils.HashUtil.Sha256OfFile(_config.GamePath) : "";

        // Disable PLAY until ZeroTier is installed and an IP is available (blocking UI as required)
        PlayButton.IsEnabled = _zt.IsInstalled() && !string.IsNullOrEmpty(_adapter.LanIp);
    }

    private async Task RefreshStatusAsync()
    {
        // ZeroTier
        if (!_zt.IsInstalled())
        {
            ZtStatusText.Text = "ZeroTier is required for LAN multiplayer.";
            LanStatusText.Text = "";
            InstallZtButton.Visibility = Visibility.Visible;
            PlayButton.IsEnabled = false;
            return;
        }

        InstallZtButton.Visibility = Visibility.Collapsed;
        var joinState = await _zt.GetNetworkStateAsync(NetworkIdText.Text);

        if (joinState == Services.ZeroTierService.NetworkState.NotJoined)
        {
            // Attempt to join automatically
            ZtStatusText.Text = "Joining network...";
            await _zt.JoinNetworkAsync(NetworkIdText.Text);

            // Poll until state changes
            for (int i = 0; i < 12; i++)
            {
                await Task.Delay(1500);
                joinState = await _zt.GetNetworkStateAsync(NetworkIdText.Text);
                if (joinState != Services.ZeroTierService.NetworkState.NotJoined && joinState != Services.ZeroTierService.NetworkState.Joining) break;
            }
        }

        switch (joinState)
        {
            case Services.ZeroTierService.NetworkState.NotJoined:
                ZtStatusText.Text = "Installed but not joined";
                break;
            case Services.ZeroTierService.NetworkState.Joining:
                ZtStatusText.Text = "Joining network...";
                break;
            case Services.ZeroTierService.NetworkState.WaitingForAuth:
                ZtStatusText.Text = "Waiting for network authorization. Ask the host to approve your device in ZeroTier Central.";
                break;
            case Services.ZeroTierService.NetworkState.Online:
                ZtStatusText.Text = "ZeroTier online";
                break;
        }

        // Adapter
        await _adapter.RefreshAsync();
        if (!string.IsNullOrEmpty(_adapter.LanIp))
        {
            LanStatusText.Text = $"LAN network active. Your LAN IP: {_adapter.LanIp}";
            CopyIpButton.IsEnabled = true;
        }
        else
        {
            LanStatusText.Text = "No ZeroTier IPv4 address detected.";
            CopyIpButton.IsEnabled = false;
        }

        // Network profile: best-effort
        if (!_adapter.IsProfilePrivate())
        {
            if (IsAdministrator())
            {
                var res = await _adapter.TrySetProfilePrivateAsync();
                if (!res)
                {
                    Utils.Logger.Log("Failed to set network profile to Private.");
                }
            }
            else
            {
                Utils.Logger.Log("Not admin: can't set profile private.");
            }
        }

        UpdateUi();
    }

    private void InstallZtButton_Click(object sender, RoutedEventArgs e)
    {
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        {
            FileName = "https://www.zerotier.com/download/",
            UseShellExecute = true
        });
    }

    private async void RecheckZtButton_Click(object sender, RoutedEventArgs e)
    {
        RecheckZtButton.IsEnabled = false;
        await RefreshStatusAsync();
        RecheckZtButton.IsEnabled = true;
    }

    private void CopyIpButton_Click(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(_adapter.LanIp))
        {
            Clipboard.SetText(_adapter.LanIp);
        }
    }

    private void LocateGameButton_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new OpenFileDialog();
        dlg.Filter = "Empire Earth|EmpireEarth.exe";
        if (dlg.ShowDialog() == true)
        {
            _config.GamePath = dlg.FileName;
            _config.Save();
            UpdateUi();
        }
    }

    private void ManageDdrawButton_Click(object sender, RoutedEventArgs e)
    {
        var dlg = MessageBox.Show("Enable DDrawCompat will copy the bundled ddraw.dll into the game folder and backup any existing ddraw.dll. Proceed?", "DDrawCompat", MessageBoxButton.YesNo);
        if (dlg == MessageBoxResult.Yes && !string.IsNullOrEmpty(_config.GamePath))
        {
            try
            {
                _ = Services.DDrawService.InstallAsync(_config.GamePath, enable: true);
                MessageBox.Show("DDrawCompat installed.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed: " + ex.Message);
            }
        }
    }

    private async void PlayButton_Click(object sender, RoutedEventArgs e)
    {
        // Confirmation modal (respect saved setting)
        if (!_config.HidePlayConfirmation)
        {
            var modal = new PlayConfirmWindow();
            if (modal.ShowDialog() != true)
                return;
        }

        // Validate
        if (!_zt.IsInstalled())
        {
            MessageBox.Show("ZeroTier is required for LAN multiplayer.");
            return;
        }

        var state = await _zt.GetNetworkStateAsync(NetworkIdText.Text);
        if (state != Services.ZeroTierService.NetworkState.Online)
        {
            MessageBox.Show("ZeroTier must be online with an IPv4 address.");
            return;
        }

        await _adapter.RefreshAsync();
        if (string.IsNullOrEmpty(_adapter.LanIp))
        {
            MessageBox.Show("No ZeroTier IPv4 address detected.");
            return;
        }

        // Best-effort firewall/profile
        if (!IsAdministrator())
        {
            var res = MessageBox.Show("Setting firewall rules and network profile requires Administrator. Continue without them?", "Admin required", MessageBoxButton.YesNo);
            if (res != MessageBoxResult.Yes) return;
        }
        else
        {
            await _adapter.TrySetProfilePrivateAsync();
            await _firewall.AddRulesForExecutableAsync(_config.GamePath);
        }

        // DDraw
        if (EnableDdrawCheck.IsChecked == true && !string.IsNullOrEmpty(_config.GamePath))
        {
            await Services.DDrawService.InstallAsync(_config.GamePath, enable: true);
        }

        // Launch game
        try
        {
            _gameLauncher.Launch(_config.GamePath);
            MessageBox.Show("Launched. In Multiplayer: Choose LAN, Host: Create LAN game, Join: Refresh LAN list. Do NOT use Internet or Direct IP.");
        }
        catch (Exception ex)
        {
            MessageBox.Show("Failed to launch: " + ex.Message);
        }
    }

    private void SaveSettingsButton_Click(object sender, RoutedEventArgs e)
    {
        _config.ZeroTierNetworkId = NetworkIdText.Text.Trim();
        _config.Save();
        MessageBox.Show("Settings saved.");
    }

    private void OpenLogsButton_Click(object sender, RoutedEventArgs e)
    {
        var dir = Utils.Logger.LogsPath;
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = dir, UseShellExecute = true });
    }

    private void CopyHashButton_Click(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(ShaText.Text)) Clipboard.SetText(ShaText.Text);
    }

    private static bool IsAdministrator()
    {
        var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }
}
