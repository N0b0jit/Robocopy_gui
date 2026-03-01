namespace NexusCopy.Core.Models;

/// <summary>
/// Represents the current status of a copy job.
/// </summary>
public enum JobStatus
{
    /// <summary>
    /// Job has not started yet.
    /// </summary>
    Idle,
    
    /// <summary>
    /// Job is currently validating paths and options.
    /// </summary>
    Validating,
    
    /// <summary>
    /// Job is actively copying files.
    /// </summary>
    Running,
    
    /// <summary>
    /// Job has been paused by the user.
    /// </summary>
    Paused,
    
    /// <summary>
    /// Job completed successfully.
    /// </summary>
    Completed,
    
    /// <summary>
    /// Job failed due to an error.
    /// </summary>
    Failed,
    
    /// <summary>
    /// Job was cancelled by the user.
    /// </summary>
    Cancelled
}
