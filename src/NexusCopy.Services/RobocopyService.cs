namespace NexusCopy.Services;

using NexusCopy.Core.Interfaces;
using NexusCopy.Core.Models;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

/// <summary>
/// Service for executing robocopy operations with progress tracking.
/// </summary>
public class RobocopyService : IRobocopyService, IDisposable
{
    private Process? _process;
    private CancellationTokenSource? _cancellationTokenSource;
    private readonly object _lockObject = new();
    private bool _disposed;

    // P/Invoke declarations for pause/resume functionality
    [DllImport("ntdll.dll", SetLastError = true)]
    private static extern uint NtSuspendProcess(IntPtr processHandle);

    [DllImport("ntdll.dll", SetLastError = true)]
    private static extern uint NtResumeProcess(IntPtr processHandle);

    /// <inheritdoc />
    public event Action<CopyProgress>? ProgressUpdated;

    /// <inheritdoc />
    public event Action<string>? LogLineReceived;

    /// <inheritdoc />
    public event Action<int>? Completed;

    /// <inheritdoc />
    public async Task ExecuteAsync(CopyOptions options, CancellationToken cancellationToken)
    {
        lock (_lockObject)
        {
            if (_process != null && !_process.HasExited)
            {
                throw new InvalidOperationException("A copy operation is already in progress.");
            }

            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        }

        try
        {
            var args = CommandBuilder.Build(options);

            _process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "robocopy.exe",
                    Arguments = args,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8
                },
                EnableRaisingEvents = true
            };

            // Set up event handlers
            _process.OutputDataReceived += OnOutputDataReceived;
            _process.ErrorDataReceived += OnErrorDataReceived;
            _process.Exited += OnProcessExited;
            _process.Disposed += OnProcessDisposed;

            // Start the process
            _process.Start();
            _process.BeginOutputReadLine();
            _process.BeginErrorReadLine();

            // Wait for process to exit
            await _process.WaitForExitAsync(_cancellationTokenSource.Token);

            // Fire completion event
            Completed?.Invoke(_process.ExitCode);
        }
        catch (OperationCanceledException)
        {
            // Process was cancelled
            Cancel();
            throw;
        }
        finally
        {
            CleanupProcess();
        }
    }

    /// <inheritdoc />
    public void Pause()
    {
        lock (_lockObject)
        {
            if (_process != null && !_process.HasExited)
            {
                try
                {
                    var result = NtSuspendProcess(_process.Handle);
                    if (result != 0)
                    {
                        throw new InvalidOperationException($"Failed to pause process. Error code: {result}");
                    }
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("Failed to pause copy operation.", ex);
                }
            }
        }
    }

    /// <inheritdoc />
    public void Resume()
    {
        lock (_lockObject)
        {
            if (_process != null && !_process.HasExited)
            {
                try
                {
                    var result = NtResumeProcess(_process.Handle);
                    if (result != 0)
                    {
                        throw new InvalidOperationException($"Failed to resume process. Error code: {result}");
                    }
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("Failed to resume copy operation.", ex);
                }
            }
        }
    }

    /// <inheritdoc />
    public void Cancel()
    {
        lock (_lockObject)
        {
            if (_process != null && !_process.HasExited)
            {
                try
                {
                    _cancellationTokenSource?.Cancel();
                    _process.Kill(entireProcessTree: true);
                }
                catch
                {
                    // Ignore exceptions during cancellation
                }
            }
        }
    }

    private void OnOutputDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (e.Data == null) return;

        // Fire log line event
        LogLineReceived?.Invoke(e.Data);

        // Try to parse progress information
        var progressUpdate = OutputParser.TryParse(e.Data);
        if (progressUpdate != null)
        {
            var progress = new CopyProgress
            {
                CurrentFileName = progressUpdate.FileName,
                CurrentFilePercent = progressUpdate.FilePercent,
                StatusText = progressUpdate.Status
            };

            ProgressUpdated?.Invoke(progress);
        }
    }

    private void OnErrorDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (e.Data != null)
        {
            LogLineReceived?.Invoke($"ERROR: {e.Data}");
        }
    }

    private void OnProcessExited(object? sender, EventArgs e)
    {
        // Process has exited, cleanup will happen in the finally block
    }

    private void OnProcessDisposed(object? sender, EventArgs e)
    {
        // Process has been disposed
    }

    private void CleanupProcess()
    {
        lock (_lockObject)
        {
            if (_process != null)
            {
                _process.OutputDataReceived -= OnOutputDataReceived;
                _process.ErrorDataReceived -= OnErrorDataReceived;
                _process.Exited -= OnProcessExited;
                _process.Disposed -= OnProcessDisposed;

                if (!_process.HasExited)
                {
                    try
                    {
                        _process.Kill(entireProcessTree: true);
                    }
                    catch
                    {
                        // Ignore exceptions during cleanup
                    }
                }

                _process.Dispose();
                _process = null;
            }

            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }
    }

    /// <summary>
    /// Disposes the service and cleans up resources.
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            Cancel();
            CleanupProcess();
            _disposed = true;
        }
    }
}
