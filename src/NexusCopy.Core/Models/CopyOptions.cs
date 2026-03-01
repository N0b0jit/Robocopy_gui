namespace NexusCopy.Core.Models;

/// <summary>
/// Represents all configurable options for a copy operation.
/// </summary>
public record CopyOptions
{
    /// <summary>
    /// Gets or sets the source directory path.
    /// </summary>
    public required string Source { get; init; }
    
    /// <summary>
    /// Gets or sets the destination directory path.
    /// </summary>
    public required string Destination { get; init; }
    
    /// <summary>
    /// Gets or sets the copy operation mode.
    /// </summary>
    public CopyMode Mode { get; init; } = CopyMode.Copy;
    
    /// <summary>
    /// Gets or sets whether to include subdirectories.
    /// </summary>
    public bool IncludeSubdirectories { get; init; } = true;
    
    /// <summary>
    /// Gets or sets whether to include empty subdirectories.
    /// </summary>
    public bool IncludeEmpty { get; init; } = false;
    
    /// <summary>
    /// Gets or sets whether to use restartable mode.
    /// </summary>
    public bool RestartableMode { get; init; } = true;
    
    /// <summary>
    /// Gets or sets whether to use backup mode.
    /// </summary>
    public bool BackupMode { get; init; } = false;
    
    /// <summary>
    /// Gets or sets whether to use multi-threaded copying.
    /// </summary>
    public bool MultiThreaded { get; init; } = true;
    
    /// <summary>
    /// Gets or sets the number of threads to use for multi-threaded copying (1-128).
    /// </summary>
    public int ThreadCount { get; init; } = 16;
    
    /// <summary>
    /// Gets or sets whether to skip newer files in destination.
    /// </summary>
    public bool SkipNewerFiles { get; init; } = false;
    
    /// <summary>
    /// Gets or sets whether to exclude hidden files.
    /// </summary>
    public bool ExcludeHidden { get; init; } = false;
    
    /// <summary>
    /// Gets or sets whether to exclude system files.
    /// </summary>
    public bool ExcludeSystem { get; init; } = false;
    
    /// <summary>
    /// Gets or sets the list of file patterns to exclude.
    /// </summary>
    public string[] ExcludeFiles { get; init; } = [];
    
    /// <summary>
    /// Gets or sets the list of directory patterns to exclude.
    /// </summary>
    public string[] ExcludeDirectories { get; init; } = [];
    
    /// <summary>
    /// Gets or sets the path to the log file.
    /// </summary>
    public required string LogFilePath { get; init; }
    
    /// <summary>
    /// Gets or sets whether to mirror the directory (overwrite destination).
    /// </summary>
    public bool MirrorMode { get; init; } = false;
    
    /// <summary>
    /// Gets or sets whether to move files (delete from source after copy).
    /// </summary>
    public bool MoveFiles { get; init; } = false;
}
