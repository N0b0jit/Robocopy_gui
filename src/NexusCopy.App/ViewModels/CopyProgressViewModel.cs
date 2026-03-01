namespace NexusCopy.App.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using NexusCopy.Core.Models;

/// <summary>
/// ViewModel for copy operation progress tracking.
/// </summary>
public partial class CopyProgressViewModel : ObservableObject
{
    [ObservableProperty]
    private string _currentFileName = string.Empty;

    [ObservableProperty]
    private double _currentFilePercent;

    [ObservableProperty]
    private double _overallPercent;

    [ObservableProperty]
    private long _bytesPerSecond;

    [ObservableProperty]
    private TimeSpan _estimatedTimeRemaining;

    [ObservableProperty]
    private long _filesCopied;

    [ObservableProperty]
    private long _totalFiles;

    [ObservableProperty]
    private long _filesSkipped;

    [ObservableProperty]
    private long _bytesCopied;

    [ObservableProperty]
    private long _totalBytes;

    [ObservableProperty]
    private string _statusText = string.Empty;

    [ObservableProperty]
    private bool _isIndeterminate = true;

    [ObservableProperty]
    private string _speedText = "0 MB/s";

    [ObservableProperty]
    private string _etaText = "Calculating...";

    [ObservableProperty]
    private string _filesText = "0 / 0 files";

    /// <summary>
    /// Updates the progress from a CopyProgress object.
    /// </summary>
    /// <param name="progress">The progress information.</param>
    public void UpdateProgress(CopyProgress progress)
    {
        CurrentFileName = progress.CurrentFileName;
        CurrentFilePercent = progress.CurrentFilePercent;
        OverallPercent = progress.OverallPercent;
        BytesPerSecond = progress.BytesPerSecond;
        EstimatedTimeRemaining = progress.EstimatedTimeRemaining;
        FilesCopied = progress.FilesCopied;
        TotalFiles = progress.TotalFiles;
        FilesSkipped = progress.FilesSkipped;
        BytesCopied = progress.BytesCopied;
        TotalBytes = progress.TotalBytes;
        StatusText = progress.StatusText;

        // Update derived properties
        UpdateDerivedProperties();
    }

    /// <summary>
    /// Resets all progress values to their initial state.
    /// </summary>
    public void Reset()
    {
        CurrentFileName = string.Empty;
        CurrentFilePercent = 0;
        OverallPercent = 0;
        BytesPerSecond = 0;
        EstimatedTimeRemaining = TimeSpan.Zero;
        FilesCopied = 0;
        TotalFiles = 0;
        FilesSkipped = 0;
        BytesCopied = 0;
        TotalBytes = 0;
        StatusText = string.Empty;
        IsIndeterminate = true;
        SpeedText = "0 MB/s";
        EtaText = "Calculating...";
        FilesText = "0 / 0 files";
    }

    /// <summary>
    /// Sets the total counts when starting a copy operation.
    /// </summary>
    /// <param name="totalFiles">The total number of files.</param>
    /// <param name="totalBytes">The total number of bytes.</param>
    public void SetTotals(long totalFiles, long totalBytes)
    {
        TotalFiles = totalFiles;
        TotalBytes = totalBytes;
        IsIndeterminate = totalFiles == 0;
        UpdateDerivedProperties();
    }

    private void UpdateDerivedProperties()
    {
        // Update speed text
        var mbps = BytesPerSecond / (1024.0 * 1024.0);
        SpeedText = mbps >= 1.0 ? $"{mbps:F1} MB/s" : $"{BytesPerSecond / 1024.0:F0} KB/s";

        // Update ETA text
        if (EstimatedTimeRemaining == TimeSpan.Zero || BytesPerSecond == 0)
        {
            EtaText = "Calculating...";
        }
        else
        {
            EtaText = EstimatedTimeRemaining.TotalHours >= 1.0
                ? $"{(int)EstimatedTimeRemaining.TotalHours}h {EstimatedTimeRemaining.Minutes}m"
                : EstimatedTimeRemaining.TotalMinutes >= 1.0
                    ? $"{(int)EstimatedTimeRemaining.TotalMinutes}m {EstimatedTimeRemaining.Seconds}s"
                    : $"{EstimatedTimeRemaining.Seconds}s";
        }

        // Update files text
        FilesText = TotalFiles > 0
            ? $"{FilesCopied:N0} / {TotalFiles:N0} files"
            : $"{FilesCopied:N0} files";
    }
}
