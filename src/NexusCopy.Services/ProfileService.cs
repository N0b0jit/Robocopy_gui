namespace NexusCopy.Services;

using NexusCopy.Core.Interfaces;
using NexusCopy.Core.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Service for managing saved profiles using JSON storage.
/// </summary>
public class ProfileService : IProfileService
{
    private readonly string _profilesFilePath;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    /// <summary>
    /// Initializes a new instance of the ProfileService class.
    /// </summary>
    /// <param name="appDataPath">The application data directory path.</param>
    public ProfileService(string? appDataPath = null)
    {
        var dataPath = appDataPath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "NexusCopy");

        // Ensure directory exists
        Directory.CreateDirectory(dataPath);

        _profilesFilePath = Path.Combine(dataPath, "profiles.json");

        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters = { new JsonStringEnumConverter() }
        };
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<SavedProfile>> GetProfilesAsync()
    {
        await _semaphore.WaitAsync();
        try
        {
            if (!File.Exists(_profilesFilePath))
            {
                return Array.Empty<SavedProfile>();
            }

            var json = await File.ReadAllTextAsync(_profilesFilePath);
            var data = JsonSerializer.Deserialize<ProfileData>(json, _jsonOptions);
            return data?.Profiles ?? Array.Empty<SavedProfile>();
        }
        catch (Exception ex)
        {
            // Log error and return empty list
            Console.WriteLine($"Error loading profiles: {ex.Message}");
            return Array.Empty<SavedProfile>();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <inheritdoc />
    public async Task<SavedProfile?> GetProfileAsync(Guid id)
    {
        var profiles = await GetProfilesAsync();
        return profiles.FirstOrDefault(p => p.Id == id);
    }

    /// <inheritdoc />
    public async Task SaveProfileAsync(SavedProfile profile)
    {
        await _semaphore.WaitAsync();
        try
        {
            var profilesList = await GetProfilesAsync();
            var existingIndex = profilesList.ToList().FindIndex(p => p.Id == profile.Id);

            var updatedProfile = profile with 
            { 
                ModifiedAt = DateTime.UtcNow,
                CreatedAt = existingIndex >= 0 ? profile.CreatedAt : DateTime.UtcNow
            };

            IReadOnlyList<SavedProfile> profiles;
            if (existingIndex >= 0)
            {
                var mutableProfiles = profilesList.ToList();
                mutableProfiles[existingIndex] = updatedProfile;
                profiles = mutableProfiles;
            }
            else
            {
                profiles = profilesList.Append(updatedProfile).ToList();
            }

            await SaveProfilesToFile(profiles);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <inheritdoc />
    public async Task DeleteProfileAsync(Guid id)
    {
        await _semaphore.WaitAsync();
        try
        {
            var profiles = await GetProfilesAsync();
            var filteredProfiles = profiles.Where(p => p.Id != id).ToList();
            await SaveProfilesToFile(filteredProfiles);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <inheritdoc />
    public async Task UpdateProfileAsync(SavedProfile profile)
    {
        await SaveProfileAsync(profile);
    }

    private async Task SaveProfilesToFile(IReadOnlyList<SavedProfile> profiles)
    {
        var data = new ProfileData { Profiles = profiles };
        var json = JsonSerializer.Serialize(data, _jsonOptions);
        await File.WriteAllTextAsync(_profilesFilePath, json);
    }

    /// <summary>
    /// Disposes the service.
    /// </summary>
    public void Dispose()
    {
        _semaphore.Dispose();
    }
}

/// <summary>
/// JSON data structure for profiles file.
/// </summary>
internal class ProfileData
{
    public IReadOnlyList<SavedProfile> Profiles { get; set; } = Array.Empty<SavedProfile>();
}
