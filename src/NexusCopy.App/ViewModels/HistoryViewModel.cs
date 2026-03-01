namespace NexusCopy.App.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using NexusCopy.Core.Interfaces;
using NexusCopy.Core.Models;
using NexusCopy.Services;
using System.Diagnostics;
using System.IO;
using System.Collections.ObjectModel;
using System.Windows;

/// <summary>
/// ViewModel for managing copy job history.
/// </summary>
public partial class HistoryViewModel : ObservableObject
{
    private readonly ILogService _logService;

    [ObservableProperty]
    private ObservableCollection<CopyJob> _jobs = new();

    [ObservableProperty]
    private CopyJob? _selectedJob;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _statusMessage = "Loading job history...";

    /// <summary>
    /// Initializes a new instance of the HistoryViewModel class.
    /// </summary>
    /// <param name="logService">The log service.</param>
    public HistoryViewModel(ILogService logService)
    {
        _logService = logService;
        _ = LoadJobsAsync();
    }

    /// <summary>
    /// Loads the job history asynchronously.
    /// </summary>
    [RelayCommand]
    private async Task LoadJobsAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = "Loading job history...";

            var jobs = await _logService.GetJobHistoryAsync();
            
            Jobs.Clear();
            foreach (var job in jobs.OrderByDescending(j => j.StartedAt))
            {
                Jobs.Add(job);
            }

            StatusMessage = Jobs.Count == 0 
                ? "No copy jobs found" 
                : $"Loaded {Jobs.Count} job{(Jobs.Count == 1 ? "" : "s")}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading history: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Clears the job history.
    /// </summary>
    [RelayCommand]
    private async Task ClearHistoryAsync()
    {
        try
        {
            var result = MessageBox.Show(
                "Are you sure you want to clear all job history? This action cannot be undone.",
                "Clear History",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                await _logService.ClearHistoryAsync();
                Jobs.Clear();
                StatusMessage = "Job history cleared";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error clearing history: {ex.Message}";
        }
    }

    /// <summary>
    /// Opens the log file for the selected job.
    /// </summary>
    [RelayCommand]
    private void OpenLogFile()
    {
        if (SelectedJob == null) return;

        try
        {
            if (File.Exists(SelectedJob.LogFilePath))
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = SelectedJob.LogFilePath,
                        UseShellExecute = true
                    }
                };
                process.Start();
            }
            else
            {
                MessageBox.Show(
                    "Log file not found.",
                    "File Not Found",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Error opening log file: {ex.Message}",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Copies the selected job's command to clipboard.
    /// </summary>
    [RelayCommand]
    private void CopyCommandToClipboard()
    {
        if (SelectedJob == null) return;

        try
        {
            var command = $"robocopy.exe {CommandBuilder.Build(SelectedJob.Options)}";
            Clipboard.SetText(command);
            StatusMessage = "Command copied to clipboard";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error copying command: {ex.Message}";
        }
    }

    /// <summary>
    /// Refreshes the job history.
    /// </summary>
    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadJobsAsync();
    }

    /// <summary>
    /// Filters jobs based on search text.
    /// </summary>
    partial void OnSearchTextChanged(string? oldValue, string newValue)
    {
        FilterJobs();
    }

    private async void FilterJobs()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            await LoadJobsAsync();
            return;
        }

        try
        {
            var allJobs = await _logService.GetJobHistoryAsync();
            var filtered = allJobs.Where(job => 
                job.Options.Source.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                job.Options.Destination.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                job.Status.ToString().Contains(SearchText, StringComparison.OrdinalIgnoreCase));

            Jobs.Clear();
            foreach (var job in filtered.OrderByDescending(j => j.StartedAt))
            {
                Jobs.Add(job);
            }

            StatusMessage = $"Found {Jobs.Count} matching job{(Jobs.Count == 1 ? "" : "s")}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error filtering jobs: {ex.Message}";
        }
    }

    /// <summary>
    /// Gets a formatted status text for a job.
    /// </summary>
    /// <param name="job">The job to format.</param>
    /// <returns>Formatted status text.</returns>
    public static string GetJobStatusText(CopyJob job)
    {
        var duration = job.CompletedAt?.Subtract(job.StartedAt) ?? DateTime.UtcNow.Subtract(job.StartedAt);
        var durationText = duration.TotalHours >= 1
            ? $"{(int)duration.TotalHours}h {duration.Minutes}m"
            : duration.TotalMinutes >= 1
                ? $"{(int)duration.TotalMinutes}m {duration.Seconds}s"
                : $"{duration.Seconds}s";

        return $"{job.Status} - {durationText}";
    }

    /// <summary>
    /// Gets a formatted file count text for a job.
    /// </summary>
    /// <param name="job">The job to format.</param>
    /// <returns>Formatted file count text.</returns>
    public static string GetFileCountText(CopyJob job)
    {
        return $"{job.Progress.FilesCopied:N0} files";
    }

    /// <summary>
    /// Gets a formatted size text for a job.
    /// </summary>
    /// <param name="job">The job to format.</param>
    /// <returns>Formatted size text.</returns>
    public static string GetSizeText(CopyJob job)
    {
        var bytes = job.Progress.BytesCopied;
        if (bytes >= 1024 * 1024 * 1024)
            return $"{bytes / (1024.0 * 1024.0 * 1024.0):F1} GB";
        if (bytes >= 1024 * 1024)
            return $"{bytes / (1024.0 * 1024.0):F1} MB";
        if (bytes >= 1024)
            return $"{bytes / 1024.0:F1} KB";
        return $"{bytes} B";
    }
}
