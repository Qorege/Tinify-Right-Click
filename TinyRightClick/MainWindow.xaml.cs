using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Color = System.Windows.Media.Color;

namespace TinyRightClick;

public partial class MainWindow : Window
{
    private bool _keyVisible;
    private string _apiKeyValue = "";

    public MainWindow()
    {
        InitializeComponent();

        try
        {
            var iconUri = new Uri("pack://application:,,,/TinyRightClick;component/icon.ico", UriKind.Absolute);
            Icon = System.Windows.Media.Imaging.BitmapFrame.Create(iconUri);
        }
        catch
        {
            // Fallback to default application icon
        }
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        RefreshStatus();
    }

    private void RefreshStatus()
    {
        var config = ConfigManager.Load();
        bool installed = RegistryHelper.IsInstalled();

        // Update status badge
        if (installed)
        {
            StatusBadge.Background = new SolidColorBrush(Color.FromArgb(0x26, 0x4C, 0xAF, 0x50));
            StatusBadge.BorderBrush = new SolidColorBrush(Color.FromArgb(0x40, 0x4C, 0xAF, 0x50));
            StatusBadge.BorderThickness = new Thickness(1);
            StatusDot.Fill = new SolidColorBrush(Color.FromRgb(0x4C, 0xAF, 0x50));
            StatusText.Text = "Installed";
            StatusText.Foreground = new SolidColorBrush(Color.FromRgb(0x81, 0xC7, 0x84));

            InstallBtn.IsEnabled = false;
            UninstallBtn.IsEnabled = true;
        }
        else
        {
            StatusBadge.Background = new SolidColorBrush(Color.FromArgb(0x1E, 0xFF, 0x98, 0x00));
            StatusBadge.BorderBrush = new SolidColorBrush(Color.FromArgb(0x33, 0xFF, 0x98, 0x00));
            StatusBadge.BorderThickness = new Thickness(1);
            StatusDot.Fill = new SolidColorBrush(Color.FromRgb(0xFF, 0x98, 0x00));
            StatusText.Text = "Not Installed";
            StatusText.Foreground = new SolidColorBrush(Color.FromRgb(0xFF, 0xB7, 0x4D));

            InstallBtn.IsEnabled = true;
            UninstallBtn.IsEnabled = false;
        }

        // Load API key
        if (!string.IsNullOrEmpty(config.ApiKey))
        {
            _apiKeyValue = config.ApiKey;
            if (_keyVisible)
                ApiKeyTextBox.Text = _apiKeyValue;
            else
                ApiKeyPasswordBox.Password = _apiKeyValue;
        }
    }

    private void ToggleKeyVisibility_Click(object sender, RoutedEventArgs e)
    {
        _keyVisible = !_keyVisible;

        if (_keyVisible)
        {
            // Show plain text
            ApiKeyTextBox.Text = _apiKeyValue;
            ApiKeyTextBox.Visibility = Visibility.Visible;
            ApiKeyPasswordBox.Visibility = Visibility.Collapsed;
            EyeIcon.Text = "🔒";
        }
        else
        {
            // Show password
            ApiKeyPasswordBox.Password = _apiKeyValue;
            ApiKeyPasswordBox.Visibility = Visibility.Visible;
            ApiKeyTextBox.Visibility = Visibility.Collapsed;
            EyeIcon.Text = "👁";
        }
    }

    private void ApiKeyPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        _apiKeyValue = ApiKeyPasswordBox.Password;
    }

    private void ApiKeyTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        _apiKeyValue = ApiKeyTextBox.Text;
    }

    private void SaveApiKey_Click(object sender, RoutedEventArgs e)
    {
        var key = _apiKeyValue.Trim();
        if (string.IsNullOrEmpty(key))
        {
            ShowToast("Please enter an API key", ToastType.Error);
            return;
        }

        var config = ConfigManager.Load();
        config.ApiKey = key;
        ConfigManager.Save(config);
        ShowToast("API key saved successfully", ToastType.Success);
        RefreshStatus();
    }

    private void Install_Click(object sender, RoutedEventArgs e)
    {
        var config = ConfigManager.Load();
        if (string.IsNullOrWhiteSpace(config.ApiKey))
        {
            ShowToast("Please set your Tinify API key first", ToastType.Error);
            return;
        }

        InstallBtn.IsEnabled = false;

        try
        {
            // Get the path to this executable
            var exePath = Process.GetCurrentProcess().MainModule?.FileName
                ?? Environment.ProcessPath
                ?? throw new Exception("Cannot determine executable path");

            RegistryHelper.Install(exePath);
            ShowToast("Context menu installed! Right-click any image to compress.", ToastType.Success);
        }
        catch (Exception ex)
        {
            ShowToast($"Install failed: {ex.Message}", ToastType.Error);
        }

        RefreshStatus();
    }

    private void Uninstall_Click(object sender, RoutedEventArgs e)
    {
        UninstallBtn.IsEnabled = false;

        try
        {
            RegistryHelper.Uninstall();
            ShowToast("Context menu removed successfully", ToastType.Success);
        }
        catch (Exception ex)
        {
            ShowToast($"Uninstall failed: {ex.Message}", ToastType.Error);
        }

        RefreshStatus();
    }

    private void TinifyLink_Click(object sender, RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo("https://tinypng.com/developers") { UseShellExecute = true });
    }

    // ── Toast Notification ──────────────────────────────────────
    private enum ToastType { Success, Error, Info }

    private void ShowToast(string message, ToastType type)
    {
        ToastText.Text = message;

        ToastBorder.Background = type switch
        {
            ToastType.Success => new SolidColorBrush(Color.FromArgb(0xE6, 0x4C, 0xAF, 0x50)),
            ToastType.Error => new SolidColorBrush(Color.FromArgb(0xE6, 0xE5, 0x39, 0x35)),
            _ => new SolidColorBrush(Color.FromArgb(0xE6, 0x6C, 0x63, 0xFF))
        };

        // Animate in
        var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(250));
        var moveIn = new ThicknessAnimation(
            new Thickness(0, 0, 0, -50),
            new Thickness(0, 60, 0, 0),
            TimeSpan.FromMilliseconds(250))
        { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } };

        ToastBorder.BeginAnimation(OpacityProperty, fadeIn);
        ToastBorder.BeginAnimation(MarginProperty, moveIn);

        // Animate out after delay
        var timer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
        timer.Tick += (s, _) =>
        {
            timer.Stop();
            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(300));
            ToastBorder.BeginAnimation(OpacityProperty, fadeOut);
        };
        timer.Start();
    }
}
