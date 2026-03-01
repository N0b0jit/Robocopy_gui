namespace NexusCopy.App.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using NexusCopy.Core.Interfaces;
using NexusCopy.Core.Models;
using NexusCopy.Services;
using System.Diagnostics;
using System.IO;
using System.Windows;

/// <summary>
/// ViewModel for managing copy job operations.
/// </summary>
public partial class CopyJobViewModel : ObservableObject
{
    private readonly IRobocopyService _robocopyService;
    private readonly ILogService _logService;
    private CancellationTokenSource? _cancellationTokenSource;

    [ObservableProperty]
    private string _sourcePath = string.Empty;

    [ObservableProperty]
    private string _destinationPath = string.Empty;

    [ObservableProperty]
    private string _statusMessage = "Ready to copy";

    [ObservableProperty]
    private bool _isRunning;

    [ObservableProperty]
    private bool _isPaused;

    [ObservableProperty]
    private bool _canStart;

    [ObservableProperty]
    private bool _canPause;

    [ObservableProperty]
    private bool _canCancel;

    [ObservableProperty]
    private string _commandPreview = string.Empty;

    [ObservableProperty]
    private string _logOutput = string.Empty;

    [ObservableProperty]
    private bool _autoScrollLog = true;

    public CopyOptionsViewModel OptionsViewModel { get; }
    public CopyProgressViewModel ProgressViewModel { get; }

    /// <summary>
    /// Initializes a new instance of the CopyJobViewModel class.
    /// </summary>
    /// <param name="robocopyService">The robocopy service.</param>
    /// <param name="logService">The log service.</param>
    public CopyJobViewModel(IRobocopyService robocopyService, ILogService logService)
    {
        _robocopyService = robocopyService;
        _logService = logService;

        OptionsViewModel = new CopyOptionsViewModel();
        ProgressViewModel = new CopyProgressViewModel();

        // Subscribe to service events
        _robocopyService.ProgressUpdated += OnProgressUpdated;
        _robocopyService.LogLineReceived += OnLogLineReceived;
        _robocopyService.Completed += OnCompleted;

        // Update command states
        UpdateCommandStates();
        UpdateCommandPreview();
    }

    /// <summary>
    /// Sets the source path and updates the UI accordingly.
    /// </summary>
    /// <param name="path">The source path.</param>
    public void SetSourcePath(string path)
    {
        SourcePath = path;
        UpdateCommandStates();
        UpdateCommandPreview();

        if (!string.IsNullOrEmpty(path))
        {
            StatusMessage = "Ready to copy! Select destination and click Start.";
        }
    }

    /// <summary>
    /// Sets the destination path and updates the UI accordingly.
    /// </summary>
    /// <param name="path">The destination path.</param>
    public void SetDestinationPath(string path)
    {
        DestinationPath = path;
        UpdateCommandStates();
        UpdateCommandPreview();
    }

    /// <summary>
    /// Starts the copy operation.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanStartCopy))]
    private async Task StartCopyAsync()
    {
        try
        {
            // Validate paths
            if (!Directory.Exists(SourcePath))
            {
                StatusMessage = "Source directory does not exist.";
                return;
            }

            if (!Directory.Exists(Path.GetDirectoryName(DestinationPath)))
            {
                StatusMessage = "Destination parent directory does not exist.";
                return;
            }

            // Create destination if it doesn't exist
            if (!Directory.Exists(DestinationPath))
            {
                Directory.CreateDirectory(DestinationPath);
            }

            // Create cancellation token
            _cancellationTokenSource = new CancellationTokenSource();

            // Create log file path
            var jobId = Guid.NewGuid();
            var logFilePath = _logService.CreateLogFilePath(jobId);

            // Build copy options
            var options = OptionsViewModel.GetCopyOptions(SourcePath, DestinationPath, logFilePath);

            // Create job
            var job = new CopyJob
            {
                Id = jobId,
                Options = options,
                StartedAt = DateTime.UtcNow,
                Status = JobStatus.Running,
                LogFilePath = logFilePath
            };

            // Update UI state
            IsRunning = true;
            IsPaused = false;
            StatusMessage = "Starting copy operation...";
            ProgressViewModel.Reset();
            LogOutput = string.Empty;
            UpdateCommandStates();

            // Execute copy
            await _robocopyService.ExecuteAsync(options, _cancellationTokenSource.Token);

            StatusMessage = "Copy operation completed.";
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Copy operation was cancelled.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsRunning = false;
            IsPaused = false;
            UpdateCommandStates();
        }
    }

    /// <summary>
    /// Pauses the copy operation.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanPauseCopy))]
    private void PauseCopy()
    {
        try
        {
            _robocopyService.Pause();
            IsPaused = true;
            StatusMessage = "Copy operation paused.";
            UpdateCommandStates();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to pause: {ex.Message}";
        }
    }

    /// <summary>
    /// Resumes the copy operation.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanResumeCopy))]
    private void ResumeCopy()
    {
        try
        {
            _robocopyService.Resume();
            IsPaused = false;
            StatusMessage = "Copy operation resumed.";
            UpdateCommandStates();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to resume: {ex.Message}";
        }
    }

    /// <summary>
    /// Cancels the copy operation.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanCancelCopy))]
    private void CancelCopy()
    {
        try
        {
            _cancellationTokenSource?.Cancel();
            _robocopyService.Cancel();
            StatusMessage = "Copy operation cancelled.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to cancel: {ex.Message}";
        }
    }

    /// <summary>
    /// Clears the log output.
    /// </summary>
    [RelayCommand]
    private void ClearLog()
    {
        LogOutput = string.Empty;
    }

    /// <summary>
    /// Copies the command preview to clipboard.
    /// </summary>
    [RelayCommand]
    private void CopyCommandToClipboard()
    {
        try
        {
            Clipboard.SetText(CommandPreview);
            StatusMessage = "Command copied to clipboard.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to copy command: {ex.Message}";
        }
    }

    /// <summary>
    /// Browses for a source folder.
    /// </summary>
    [RelayCommand]
    private void BrowseSource()
    {
        var dialog = new System.Windows.Forms.FolderBrowserDialog
        {
            Description = "Select source folder",
            ShowNewFolderButton = false
        };

        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            SetSourcePath(dialog.SelectedPath);
        }
    }

    /// <summary>
    /// Browses for a destination folder.
    /// </summary>
    [RelayCommand]
    private void BrowseDestination()
    {
        var dialog = new System.Windows.Forms.FolderBrowserDialog
        {
            Description = "Select destination folder",
            ShowNewFolderButton = true
        };

        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            SetDestinationPath(dialog.SelectedPath);
        }
    }

    private bool CanStartCopy() => !IsRunning && !string.IsNullOrEmpty(SourcePath) && !string.IsNullOrEmpty(DestinationPath);
    private bool CanPauseCopy() => IsRunning && !IsPaused;
    private bool CanResumeCopy() => IsRunning && IsPaused;
    private bool CanCancelCopy() => IsRunning;

    private void UpdateCommandStates()
    {
        StartCopyCommand.NotifyCanExecuteChanged();
        PauseCopyCommand.NotifyCanExecuteChanged();
        ResumeCopyCommand.NotifyCanExecuteChanged();
        CancelCopyCommand.NotifyCanExecuteChanged();
    }

    private void UpdateCommandPreview()
    {
        if (string.IsNullOrEmpty(SourcePath) || string.IsNullOrEmpty(DestinationPath))
        {
            CommandPreview = string.Empty;
            return;
        }

        try
        {
            var tempLogPath = Path.GetTempFileName();
            var options = OptionsViewModel.GetCopyOptions(SourcePath, DestinationPath, tempLogPath);
            CommandPreview = $"robocopy.exe {CommandBuilder.Build(options)}";
            
            try { File.Delete(tempLogPath); } catch { }
        }
        catch
        {
            CommandPreview = "Invalid configuration";
        }
    }

    private void OnProgressUpdated(CopyProgress? progress)
    {
        if (progress != null)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                ProgressViewModel.UpdateProgress(progress);
                StatusMessage = $"Copying: {progress.CurrentFileName}";
            });
        }
    }

    private void OnLogLineReceived(string? logLine)
    {
        if (logLine != null)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                LogOutput += logLine + Environment.NewLine;
                
                // Auto-scroll if enabled
                if (AutoScrollLog && LogOutput.Length > 0)
                {
                    // This would be handled by the UI's ScrollViewer
                }
            });
        }
    }

    private void OnCompleted(int exitCode)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            var (messageType, message) = ExitCodeInterpreter.Interpret(exitCode);
            StatusMessage = message;
            
            // Save job to history
            // This would be implemented with the actual job object
        });
    }

    partial void OnSourcePathChanged(string? oldValue, string newValue)
    {
        UpdateCommandStates();
        UpdateCommandPreview();
    }

    partial void OnDestinationPathChanged(string? oldValue, string newValue)
    {
        UpdateCommandStates();
        UpdateCommandPreview();
    }
}
