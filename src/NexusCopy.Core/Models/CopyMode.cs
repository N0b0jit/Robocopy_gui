namespace NexusCopy.Core.Models;

/// <summary>
/// Defines the copy operation mode.
/// </summary>
public enum CopyMode
{
    /// <summary>
    /// Copy files from source to destination.
    /// </summary>
    Copy,
    
    /// <summary>
    /// Move files from source to destination (deletes from source after copy).
    /// </summary>
    Move,
    
    /// <summary>
    /// Mirror source to destination (deletes extra files in destination).
    /// </summary>
    Mirror,
    
    /// <summary>
    /// Synchronize source and destination (copy newer files both ways).
    /// </summary>
    Sync
}
