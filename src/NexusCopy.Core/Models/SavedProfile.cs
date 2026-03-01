namespace NexusCopy.Core.Models;

/// <summary>
/// Represents a saved profile containing copy options.
/// </summary>
public record SavedProfile
{
    /// <summary>
    /// Gets or sets the unique identifier for this profile.
    /// </summary>
    public required Guid Id { get; init; }
    
    /// <summary>
    /// Gets or sets the display name for this profile.
    /// </summary>
    public required string Name { get; init; }
    
    /// <summary>
    /// Gets or sets the time when this profile was created.
    /// </summary>
    public DateTime CreatedAt { get; init; }
    
    /// <summary>
    /// Gets or sets the time when this profile was last modified.
    /// </summary>
    public DateTime ModifiedAt { get; set; }
    
    /// <summary>
    /// Gets or sets the copy options stored in this profile.
    /// </summary>
    public required CopyOptions Options { get; init; }
    
    /// <summary>
    /// Gets or sets the description for this profile.
    /// </summary>
    public string? Description { get; init; }
}
