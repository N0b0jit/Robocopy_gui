namespace NexusCopy.Shell;

using Microsoft.Win32;
using NexusCopy.Core.Models;

/// <summary>
/// Installs and uninstalls Windows Explorer context menu entries for Nexus Copy.
/// </summary>
public static class ContextMenuInstaller
{
    private const string AppExePath = @"C:\Program Files\NexusCopy\NexusCopy.exe";
    private const string ParentKeyName = "NexusCopy";

    /// <summary>
    /// Installs the context menu entries.
    /// </summary>
    public static void Install()
    {
        try
        {
            // Install for right-clicking on folder icons
            InstallUnderRoot(@"Directory\shell");

            // Install for right-clicking inside folders (background)
            InstallUnderRoot(@"Directory\Background\shell");

            Console.WriteLine("Context menu installed successfully.");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to install context menu.", ex);
        }
    }

    /// <summary>
    /// Uninstalls the context menu entries.
    /// </summary>
    public static void Uninstall()
    {
        try
        {
            // Remove from both locations
            Registry.ClassesRoot.DeleteSubKeyTree(@"Directory\shell\" + ParentKeyName, throwOnMissingSubKey: false);
            Registry.ClassesRoot.DeleteSubKeyTree(@"Directory\Background\shell\" + ParentKeyName, throwOnMissingSubKey: false);

            Console.WriteLine("Context menu uninstalled successfully.");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to uninstall context menu.", ex);
        }
    }

    private static void InstallUnderRoot(string rootPath)
    {
        // Parent key
        var parentKey = $@"{rootPath}\{ParentKeyName}";
        Registry.SetValue($@"HKEY_CLASSES_ROOT\{parentKey}", "", "Nexus Copy Here →");
        Registry.SetValue($@"HKEY_CLASSES_ROOT\{parentKey}", "Icon", $"{AppExePath},0");
        Registry.SetValue($@"HKEY_CLASSES_ROOT\{parentKey}", "SubCommands", "");

        // Sub-commands
        RegisterSubCommand(rootPath, "CopyTo", "📋 Copy To...", "--mode copy --source \"%1\"");
        RegisterSubCommand(rootPath, "MoveTo", "✂️ Move To...", "--mode move --source \"%1\"");
        RegisterSubCommand(rootPath, "MirrorTo", "🔁 Mirror To...", "--mode mirror --source \"%1\"");
    }

    private static void RegisterSubCommand(string rootPath, string name, string label, string argTemplate)
    {
        var subKey = $@"{rootPath}\{ParentKeyName}\shell\{name}";
        Registry.SetValue($@"HKEY_CLASSES_ROOT\{subKey}", "", label);
        Registry.SetValue($@"HKEY_CLASSES_ROOT\{subKey}", "Icon", $"{AppExePath},1");
        Registry.SetValue($@"HKEY_CLASSES_ROOT\{subKey}\command", "", $"\"{AppExePath}\" {argTemplate}");
    }

    /// <summary>
    /// Checks if the context menu is installed.
    /// </summary>
    /// <returns>True if installed, otherwise false.</returns>
    public static bool IsInstalled()
    {
        try
        {
            var parentKey = $@"HKEY_CLASSES_ROOT\Directory\shell\{ParentKeyName}";
            var value = Registry.GetValue(parentKey, "", null);
            return value != null;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Gets the actual application executable path from the current assembly location.
    /// </summary>
    /// <returns>The full path to the executable.</returns>
    public static string GetApplicationPath()
    {
        var assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
        var directory = Path.GetDirectoryName(assemblyLocation);
        var exeName = Path.GetFileNameWithoutExtension(assemblyLocation) + ".exe";
        return Path.Combine(directory ?? "", exeName);
    }
}
