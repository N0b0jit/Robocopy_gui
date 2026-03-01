namespace NexusCopy.Core.Interfaces;

using NexusCopy.Core.Models;

/// <summary>
/// Defines the contract for logging copy job history and managing log files.
/// </summary>
public interface ILogService
{
    /// <summary>
    /// Saves a completed copy job to the history.
    /// </summary>
    /// <param name="job">The completed job to save.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task SaveJobAsync(CopyJob job);
    
    /// <summary>
    /// Gets the history of copy jobs.
    /// </summary>
    /// <param name="maxCount">The maximum number of jobs to retrieve.</param>
    /// <returns>A collection of copy jobs.</returns>
    Task<IReadOnlyList<CopyJob>> GetJobHistoryAsync(int maxCount = 100);
    
    /// <summary>
    /// Gets a specific job by its identifier.
    /// </summary>
    /// <param name="id">The job identifier.</param>
    /// <returns>The copy job, or null if not found.</returns>
    Task<CopyJob?> GetJobAsync(Guid id);
    
    /// <summary>
    /// Clears the job history.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task ClearHistoryAsync();
    
    /// <summary>
    /// Creates a new log file path for a job.
    /// </summary>
    /// <param name="jobId">The job identifier.</param>
    /// <returns>The path to the log file.</returns>
    string CreateLogFilePath(Guid jobId);
}
