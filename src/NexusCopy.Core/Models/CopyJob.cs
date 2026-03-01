namespace NexusCopy.Core.Models;

/// <summary>
/// Represents a complete copy job with its configuration and execution state.
/// </summary>
public record CopyJob
{
    /// <summary>
    /// Gets or sets the unique identifier for this job.
    /// </summary>
    public required Guid Id { get; init; }
    
    /// <summary>
    /// Gets or sets the copy options for this job.
    /// </summary>
    public required CopyOptions Options { get; init; }
    
    /// <summary>
    /// Gets or sets the time when the job started.
    /// </summary>
    public DateTime StartedAt { get; set; }
    
    /// <summary>
    /// Gets or sets the time when the job completed.
    /// </summary>
    public DateTime? CompletedAt { get; set; }
    
    /// <summary>
    /// Gets or sets the current status of the job.
    /// </summary>
    public JobStatus Status { get; set; } = JobStatus.Idle;
    
    /// <summary>
    /// Gets or sets the current progress of the job.
    /// </summary>
    public CopyProgress Progress { get; set; } = new CopyProgress();
    
    /// <summary>
    /// Gets or sets the path to the log file for this job.
    /// </summary>
    public required string LogFilePath { get; set; }
    
    /// <summary>
    /// Gets or sets the robocopy exit code when the job completes.
    /// </summary>
    public int ExitCode { get; set; }
    
    /// <summary>
    /// Gets or sets the error message if the job failed.
    /// </summary>
    public string? ErrorMessage { get; set; }
}
