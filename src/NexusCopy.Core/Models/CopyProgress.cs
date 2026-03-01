namespace NexusCopy.Core.Models;

/// <summary>
/// Represents the current progress of a copy operation.
/// </summary>
public record CopyProgress
{
    /// <summary>
    /// Gets or sets the current file being processed.
    /// </summary>
    public string CurrentFileName { get; init; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the progress percentage of the current file (0-100).
    /// </summary>
    public double CurrentFilePercent { get; init; }
    
    /// <summary>
    /// Gets or sets the overall progress percentage (0-100).
    /// </summary>
    public double OverallPercent { get; init; }
    
    /// <summary>
    /// Gets or sets the current copy speed in bytes per second.
    /// </summary>
    public long BytesPerSecond { get; init; }
    
    /// <summary>
    /// Gets or sets the estimated time remaining in seconds.
    /// </summary>
    public TimeSpan EstimatedTimeRemaining { get; init; }
    
    /// <summary>
    /// Gets or sets the number of files copied so far.
    /// </summary>
    public long FilesCopied { get; init; }
    
    /// <summary>
    /// Gets or sets the total number of files to copy.
    /// </summary>
    public long TotalFiles { get; init; }
    
    /// <summary>
    /// Gets or sets the number of files skipped.
    /// </summary>
    public long FilesSkipped { get; init; }
    
    /// <summary>
    /// Gets or sets the number of bytes copied so far.
    /// </summary>
    public long BytesCopied { get; init; }
    
    /// <summary>
    /// Gets or sets the total bytes to copy.
    /// </summary>
    public long TotalBytes { get; init; }
    
    /// <summary>
    /// Gets or sets the current status text.
    /// </summary>
    public string StatusText { get; init; } = string.Empty;
}
