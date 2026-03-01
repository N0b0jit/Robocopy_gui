namespace NexusCopy.Core.Interfaces;

using NexusCopy.Core.Models;

/// <summary>
/// Defines the contract for managing saved profiles.
/// </summary>
public interface IProfileService
{
    /// <summary>
    /// Gets all saved profiles.
    /// </summary>
    /// <returns>A collection of saved profiles.</returns>
    Task<IReadOnlyList<SavedProfile>> GetProfilesAsync();
    
    /// <summary>
    /// Gets a profile by its identifier.
    /// </summary>
    /// <param name="id">The profile identifier.</param>
    /// <returns>The saved profile, or null if not found.</returns>
    Task<SavedProfile?> GetProfileAsync(Guid id);
    
    /// <summary>
    /// Saves a profile.
    /// </summary>
    /// <param name="profile">The profile to save.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task SaveProfileAsync(SavedProfile profile);
    
    /// <summary>
    /// Deletes a profile.
    /// </summary>
    /// <param name="id">The profile identifier.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task DeleteProfileAsync(Guid id);
    
    /// <summary>
    /// Updates an existing profile.
    /// </summary>
    /// <param name="profile">The profile to update.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task UpdateProfileAsync(SavedProfile profile);
}
