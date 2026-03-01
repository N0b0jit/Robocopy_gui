namespace NexusCopy.App.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.IO;
using System.Windows;

/// <summary>
/// ViewModel for application settings.
/// </summary>
public partial class SettingsViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _darkTheme = true;

    [ObservableProperty]
    private bool _autoStartCopy = false;

    [ObservableProperty]
    private bool _confirmBeforeCancel = true;

    [ObservableProperty]
    private bool _showNotifications = true;

    [ObservableProperty]
    private bool _minimizeToTray = false;

    [ObservableProperty]
    private int _defaultThreadCount = 16;

    [ObservableProperty]
    private bool _restartableModeDefault = true;

    [ObservableProperty]
    private bool _includeSubdirsDefault = true;

    [ObservableProperty]
    private string _defaultExcludeFiles = "*.tmp;*.log;*.cache";

    [ObservableProperty]
    private string _defaultExcludeDirectories = ".git;node_modules;bin;obj";

    [ObservableProperty]
    private int _maxHistoryItems = 100;

    [ObservableProperty]
    private bool _autoSaveProfiles = true;

    [ObservableProperty]
    private string _appVersion = "1.0.0";

    /// <summary>
    /// Initializes a new instance of the SettingsViewModel class.
    /// </summary>
    public SettingsViewModel()
    {
        LoadSettings();
        AppVersion = GetAppVersion();
    }

    /// <summary>
    /// Saves the current settings.
    /// </summary>
    [RelayCommand]
    private void SaveSettings()
    {
        try
        {
            var settings = new AppSettings
            {
                DarkTheme = DarkTheme,
                AutoStartCopy = AutoStartCopy,
                ConfirmBeforeCancel = ConfirmBeforeCancel,
                ShowNotifications = ShowNotifications,
                MinimizeToTray = MinimizeToTray,
                DefaultThreadCount = DefaultThreadCount,
                RestartableModeDefault = RestartableModeDefault,
                IncludeSubdirsDefault = IncludeSubdirsDefault,
                DefaultExcludeFiles = DefaultExcludeFiles,
                DefaultExcludeDirectories = DefaultExcludeDirectories,
                MaxHistoryItems = MaxHistoryItems,
                AutoSaveProfiles = AutoSaveProfiles
            };

            SaveSettingsToFile(settings);
            MessageBox.Show("Settings saved successfully!", "Settings", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error saving settings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Resets all settings to their default values.
    /// </summary>
    [RelayCommand]
    private void ResetToDefaults()
    {
        try
        {
            var result = MessageBox.Show(
                "Are you sure you want to reset all settings to their default values? This action cannot be undone.",
                "Reset Settings",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                DarkTheme = true;
                AutoStartCopy = false;
                ConfirmBeforeCancel = true;
                ShowNotifications = true;
                MinimizeToTray = false;
                DefaultThreadCount = 16;
                RestartableModeDefault = true;
                IncludeSubdirsDefault = true;
                DefaultExcludeFiles = "*.tmp;*.log;*.cache";
                DefaultExcludeDirectories = ".git;node_modules;bin;obj";
                MaxHistoryItems = 100;
                AutoSaveProfiles = true;

                SaveSettings();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error resetting settings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Opens the application data folder.
    /// </summary>
    [RelayCommand]
    private void OpenDataFolder()
    {
        try
        {
            var dataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "NexusCopy");

            if (!Directory.Exists(dataPath))
            {
                Directory.CreateDirectory(dataPath);
            }

            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = dataPath,
                    UseShellExecute = true
                }
            };
            process.Start();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error opening data folder: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Shows information about the application.
    /// </summary>
    [RelayCommand]
    private void ShowAbout()
    {
        var aboutText = """
Nexus Copy v1.0.0

Fast. Powerful. Simple. File copying reimagined.

Built with:
• C# 12 / .NET 8
• WPF with Modern UI
• Windows robocopy engine

© 2026 Nexus Copy
""";

        MessageBox.Show(aboutText, "About Nexus Copy", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void LoadSettings()
    {
        try
        {
            var settings = LoadSettingsFromFile();
            if (settings != null)
            {
                DarkTheme = settings.DarkTheme;
                AutoStartCopy = settings.AutoStartCopy;
                ConfirmBeforeCancel = settings.ConfirmBeforeCancel;
                ShowNotifications = settings.ShowNotifications;
                MinimizeToTray = settings.MinimizeToTray;
                DefaultThreadCount = settings.DefaultThreadCount;
                RestartableModeDefault = settings.RestartableModeDefault;
                IncludeSubdirsDefault = settings.IncludeSubdirsDefault;
                DefaultExcludeFiles = settings.DefaultExcludeFiles;
                DefaultExcludeDirectories = settings.DefaultExcludeDirectories;
                MaxHistoryItems = settings.MaxHistoryItems;
                AutoSaveProfiles = settings.AutoSaveProfiles;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading settings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private AppSettings? LoadSettingsFromFile()
    {
        var settingsPath = GetSettingsFilePath();
        if (!File.Exists(settingsPath))
            return null;

        try
        {
            var json = File.ReadAllText(settingsPath);
            return System.Text.Json.JsonSerializer.Deserialize<AppSettings>(json, new System.Text.Json.JsonSerializerOptions
            {
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
            });
        }
        catch
        {
            return null;
        }
    }

    private void SaveSettingsToFile(AppSettings settings)
    {
        var settingsPath = GetSettingsFilePath();
        var directory = Path.GetDirectoryName(settingsPath);
        
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = System.Text.Json.JsonSerializer.Serialize(settings, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
        });

        File.WriteAllText(settingsPath, json);
    }

    private static string GetSettingsFilePath()
    {
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "NexusCopy",
            "settings.json");
    }

    private static string GetAppVersion()
    {
        return System.Reflection.Assembly.GetExecutingAssembly()
            .GetName().Version?.ToString() ?? "1.0.0";
    }
}

/// <summary>
/// Represents application settings.
/// </summary>
internal class AppSettings
{
    public bool DarkTheme { get; set; } = true;
    public bool AutoStartCopy { get; set; } = false;
    public bool ConfirmBeforeCancel { get; set; } = true;
    public bool ShowNotifications { get; set; } = true;
    public bool MinimizeToTray { get; set; } = false;
    public int DefaultThreadCount { get; set; } = 16;
    public bool RestartableModeDefault { get; set; } = true;
    public bool IncludeSubdirsDefault { get; set; } = true;
    public string DefaultExcludeFiles { get; set; } = "*.tmp;*.log;*.cache";
    public string DefaultExcludeDirectories { get; set; } = ".git;node_modules;bin;obj";
    public int MaxHistoryItems { get; set; } = 100;
    public bool AutoSaveProfiles { get; set; } = true;
}
