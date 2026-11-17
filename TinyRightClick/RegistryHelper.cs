using Microsoft.Win32;

namespace TinyRightClick;

/// <summary>
/// Manages Windows registry entries for the right-click context menu.
/// Uses the .NET Registry API instead of reg.exe to avoid quoting/escaping issues.
/// </summary>
public static class RegistryHelper
{
    // Register under SystemFileAssociations\image and specific extensions for 100% compatibility
    private static readonly string[] AssociationKeys =
    [
        @"Software\Classes\SystemFileAssociations\image\shell\TinyRC",
        @"Software\Classes\SystemFileAssociations\.png\shell\TinyRC",
        @"Software\Classes\SystemFileAssociations\.jpg\shell\TinyRC",
        @"Software\Classes\SystemFileAssociations\.jpeg\shell\TinyRC",
        @"Software\Classes\SystemFileAssociations\.webp\shell\TinyRC"
    ];

    public static bool IsInstalled()
    {
        using var key = Registry.CurrentUser.OpenSubKey(AssociationKeys[0]);
        return key != null;
    }

    public static void Install(string exePath)
    {
        foreach (var keyPath in AssociationKeys)
        {
            using var shellKey = Registry.CurrentUser.CreateSubKey(keyPath);
            shellKey.SetValue("", "TinyRC");                 // Context menu item text
            shellKey.SetValue("Icon", $"\"{exePath}\",0");   // Panda icon from executable
            shellKey.SetValue("MultiSelectModel", "Player"); // Multi-select support

            using var cmdKey = shellKey.CreateSubKey("command");
            cmdKey.SetValue("", $"\"{exePath}\" --compress \"%1\"");
        }
    }

    public static void Uninstall()
    {
        foreach (var keyPath in AssociationKeys)
        {
            try
            {
                Registry.CurrentUser.DeleteSubKeyTree(keyPath, throwOnMissingSubKey: false);
            }
            catch (ArgumentException)
            {
                // Key doesn't exist
            }
        }
    }
}
