using System.Windows.Forms;

namespace TinyRightClick;

/// <summary>
/// Shows a Windows system tray balloon notification.
/// Used in silent --compress mode (no GUI window).
/// </summary>
public static class ToastNotifier
{
    public static void Show(string title, string message)
    {
        try
        {
            var notify = new NotifyIcon
            {
                Icon = System.Drawing.SystemIcons.Information,
                BalloonTipTitle = title,
                BalloonTipText = message,
                Visible = true
            };

            notify.ShowBalloonTip(5000);

            // Clean up after the balloon closes
            notify.BalloonTipClosed += (s, e) =>
            {
                notify.Visible = false;
                notify.Dispose();
            };

            // Also clean up after a timeout
            var timer = new System.Threading.Timer(_ =>
            {
                notify.Visible = false;
                notify.Dispose();
            }, null, 6000, System.Threading.Timeout.Infinite);
        }
        catch
        {
            // Silently fail if notification can't be shown
        }
    }
}
