using System.IO;
using System.Windows;

namespace TinyRightClick;

public partial class App : System.Windows.Application
{
    protected override void OnStartup(System.Windows.StartupEventArgs e)
    {
        base.OnStartup(e);

        DispatcherUnhandledException += (s, args) =>
        {
            System.Windows.MessageBox.Show(
                $"An error occurred: {args.Exception.Message}\n\n{args.Exception}",
                "Tiny Right Click Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            args.Handled = true;
        };

        AppDomain.CurrentDomain.UnhandledException += (s, args) =>
        {
            if (args.ExceptionObject is Exception ex)
            {
                System.Windows.MessageBox.Show(
                    $"Fatal error: {ex.Message}\n\n{ex}",
                    "Tiny Right Click Fatal Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        };

        var args = e.Args;
        if (args.Length >= 1 && args[0] == "--compress")
        {
            // Silent compression mode invoked by right-click menu
            ShutdownMode = ShutdownMode.OnExplicitShutdown;
            var filePaths = args.Skip(1).ToArray();
            _ = CompressAndExit(filePaths);
            return;
        }

        // GUI mode: show main window
        ShutdownMode = ShutdownMode.OnMainWindowClose;
        var mainWindow = new MainWindow();
        MainWindow = mainWindow;
        mainWindow.Show();
    }

    private async Task CompressAndExit(string[] filePaths)
    {
        try
        {
            var config = ConfigManager.Load();
            if (string.IsNullOrWhiteSpace(config.ApiKey))
            {
                ToastNotifier.Show("TinyRC", "API key not set. Open Tiny Right Click to configure.");
                await Task.Delay(2000);
                Shutdown();
                return;
            }

            foreach (var filePath in filePaths)
            {
                if (!System.IO.File.Exists(filePath))
                    continue;

                var ext = System.IO.Path.GetExtension(filePath).ToLowerInvariant();
                if (ext is not (".png" or ".jpg" or ".jpeg" or ".webp"))
                    continue;

                try
                {
                    var result = await TinifyApi.CompressAsync(filePath, config.ApiKey);
                    var name = System.IO.Path.GetFileName(filePath);
                    ToastNotifier.Show("TinyRC ✓",
                        $"{name} — saved {result.SavedPercent:F1}% ({FormatSize(result.OriginalSize)} → {FormatSize(result.CompressedSize)})");
                }
                catch (Exception ex)
                {
                    var name = System.IO.Path.GetFileName(filePath);
                    ToastNotifier.Show("TinyRC ✗", $"{name}: {ex.Message}");
                }
            }

            // Allow the notification to be seen before the background process terminates
            await Task.Delay(2500);
        }
        catch (Exception ex)
        {
            ToastNotifier.Show("TinyRC ✗", $"Error: {ex.Message}");
            await Task.Delay(2500);
        }
        finally
        {
            Shutdown();
        }
    }

    private static string FormatSize(long bytes)
    {
        if (bytes < 1024) return $"{bytes} B";
        if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
        return $"{bytes / (1024.0 * 1024.0):F1} MB";
    }
}
