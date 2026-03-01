namespace NexusCopy.Core.Interfaces;

using NexusCopy.Core.Models;

/// <summary>
/// Defines the contract for the robocopy service that executes copy operations.
/// </summary>
public interface IRobocopyService
{
    /// <summary>
    /// Occurs when progress is updated during a copy operation.
    /// </summary>
    event Action<CopyProgress>? ProgressUpdated;
    
    /// <summary>
    /// Occurs when a log line is received from robocopy.
    /// </summary>
    event Action<string>? LogLineReceived;
    
    /// <summary>
    /// Occurs when the copy operation completes.
    /// </summary>
    event Action<int>? Completed;
    
    /// <summary>
    /// Executes a copy operation asynchronously.
    /// </summary>
    /// <param name="options">The copy options to use.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task ExecuteAsync(CopyOptions options, CancellationToken cancellationToken);
    
    /// <summary>
    /// Pauses the current copy operation.
    /// </summary>
    void Pause();
    
    /// <summary>
    /// Resumes the paused copy operation.
    /// </summary>
    void Resume();
    
    /// <summary>
    /// Cancels the current copy operation.
    /// </summary>
    void Cancel();
}
