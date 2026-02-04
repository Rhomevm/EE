using System.Windows;

namespace EELanLauncher;

public partial class PlayConfirmWindow : Window
{
    public PlayConfirmWindow()
    {
        InitializeComponent();
    }

    private void Ok_Click(object sender, RoutedEventArgs e)
    {
        // Save don't show again
        var config = Models.Config.Load();
        if (DontShowAgainCheck.IsChecked == true) config.HidePlayConfirmation = true;
        else config.HidePlayConfirmation = false;
        config.Save();
        DialogResult = true;
        Close();
    }
}
