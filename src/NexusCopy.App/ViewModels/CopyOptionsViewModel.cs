namespace NexusCopy.App.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NexusCopy.Core.Models;

/// <summary>
/// ViewModel for copy options configuration.
/// </summary>
public partial class CopyOptionsViewModel : ObservableObject
{
    [ObservableProperty]
    private CopyMode _mode = CopyMode.Copy;

    [ObservableProperty]
    private bool _includeSubdirectories = true;

    [ObservableProperty]
    private bool _includeEmpty = false;

    [ObservableProperty]
    private bool _restartableMode = true;

    [ObservableProperty]
    private bool _backupMode = false;

    [ObservableProperty]
    private bool _multiThreaded = true;

    [ObservableProperty]
    private int _threadCount = 16;

    [ObservableProperty]
    private bool _skipNewerFiles = false;

    [ObservableProperty]
    private bool _excludeHidden = false;

    [ObservableProperty]
    private bool _excludeSystem = false;

    [ObservableProperty]
    private string _excludeFiles = string.Empty;

    [ObservableProperty]
    private string _excludeDirectories = string.Empty;

    /// <summary>
    /// Gets the copy options configured by this ViewModel.
    /// </summary>
    /// <param name="source">The source path.</param>
    /// <param name="destination">The destination path.</param>
    /// <param name="logFilePath">The log file path.</param>
    /// <returns>A configured CopyOptions object.</returns>
    public CopyOptions GetCopyOptions(string source, string destination, string logFilePath)
    {
        var excludeFiles = ExcludeFiles
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToArray();

        var excludeDirectories = ExcludeDirectories
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToArray();

        return new CopyOptions
        {
            Source = source,
            Destination = destination,
            Mode = Mode,
            IncludeSubdirectories = IncludeSubdirectories,
            IncludeEmpty = IncludeEmpty,
            RestartableMode = RestartableMode,
            BackupMode = BackupMode,
            MultiThreaded = MultiThreaded,
            ThreadCount = ThreadCount,
            SkipNewerFiles = SkipNewerFiles,
            ExcludeHidden = ExcludeHidden,
            ExcludeSystem = ExcludeSystem,
            ExcludeFiles = excludeFiles,
            ExcludeDirectories = excludeDirectories,
            LogFilePath = logFilePath,
            MirrorMode = Mode == CopyMode.Mirror,
            MoveFiles = Mode == CopyMode.Move
        };
    }

    /// <summary>
    /// Sets the copy options from a CopyOptions object.
    /// </summary>
    /// <param name="options">The CopyOptions to load.</param>
    public void SetCopyOptions(CopyOptions options)
    {
        Mode = options.Mode;
        IncludeSubdirectories = options.IncludeSubdirectories;
        IncludeEmpty = options.IncludeEmpty;
        RestartableMode = options.RestartableMode;
        BackupMode = options.BackupMode;
        MultiThreaded = options.MultiThreaded;
        ThreadCount = options.ThreadCount;
        SkipNewerFiles = options.SkipNewerFiles;
        ExcludeHidden = options.ExcludeHidden;
        ExcludeSystem = options.ExcludeSystem;
        ExcludeFiles = string.Join("; ", options.ExcludeFiles);
        ExcludeDirectories = string.Join("; ", options.ExcludeDirectories);
    }

    /// <summary>
    /// Resets all options to their default values.
    /// </summary>
    [RelayCommand]
    public void ResetToDefaults()
    {
        Mode = CopyMode.Copy;
        IncludeSubdirectories = true;
        IncludeEmpty = false;
        RestartableMode = true;
        BackupMode = false;
        MultiThreaded = true;
        ThreadCount = 16;
        SkipNewerFiles = false;
        ExcludeHidden = false;
        ExcludeSystem = false;
        ExcludeFiles = string.Empty;
        ExcludeDirectories = string.Empty;
    }
}
