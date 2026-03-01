namespace NexusCopy.App.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using NexusCopy.Core.Interfaces;
using NexusCopy.Core.Models;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;

/// <summary>
/// ViewModel for managing saved profiles.
/// </summary>
public partial class ProfileManagerViewModel : ObservableObject
{
    private readonly IProfileService _profileService;

    [ObservableProperty]
    private ObservableCollection<SavedProfile> _profiles = new();

    [ObservableProperty]
    private SavedProfile? _selectedProfile;

    [ObservableProperty]
    private string _profileName = string.Empty;

    [ObservableProperty]
    private string _profileDescription = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _statusMessage = "Loading profiles...";

    [ObservableProperty]
    private bool _isEditing;

    /// <summary>
    /// Gets the copy options for the current profile being edited.
    /// </summary>
    public CopyOptionsViewModel OptionsViewModel { get; }

    /// <summary>
    /// Initializes a new instance of the ProfileManagerViewModel class.
    /// </summary>
    /// <param name="profileService">The profile service.</param>
    public ProfileManagerViewModel(IProfileService profileService)
    {
        _profileService = profileService;
        OptionsViewModel = new CopyOptionsViewModel();
        _ = LoadProfilesAsync();
    }

    /// <summary>
    /// Loads all profiles asynchronously.
    /// </summary>
    [RelayCommand]
    private async Task LoadProfilesAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = "Loading profiles...";

            var profiles = await _profileService.GetProfilesAsync();
            
            Profiles.Clear();
            foreach (var profile in profiles.OrderBy(p => p.Name))
            {
                Profiles.Add(profile);
            }

            StatusMessage = Profiles.Count == 0 
                ? "No profiles found" 
                : $"Loaded {Profiles.Count} profile{(Profiles.Count == 1 ? "" : "s")}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading profiles: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Creates a new profile.
    /// </summary>
    [RelayCommand]
    private void NewProfile()
    {
        ClearEditForm();
        IsEditing = true;
        StatusMessage = "Creating new profile - enter details and save";
    }

    /// <summary>
    /// Edits the selected profile.
    /// </summary>
    [RelayCommand]
    private void EditProfile()
    {
        if (SelectedProfile == null) return;

        ProfileName = SelectedProfile.Name;
        ProfileDescription = SelectedProfile.Description ?? string.Empty;
        OptionsViewModel.SetCopyOptions(SelectedProfile.Options);
        IsEditing = true;
        StatusMessage = $"Editing profile: {SelectedProfile.Name}";
    }

    /// <summary>
    /// Saves the current profile.
    /// </summary>
    [RelayCommand]
    private async Task SaveProfileAsync()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(ProfileName))
            {
                StatusMessage = "Please enter a profile name";
                return;
            }

            // Create a temporary log path for building options
            var tempLogPath = Path.GetTempFileName();
            try
            {
                var options = OptionsViewModel.GetCopyOptions("C:\\temp", "C:\\temp", tempLogPath);

                var profile = new SavedProfile
                {
                    Id = SelectedProfile?.Id ?? Guid.NewGuid(),
                    Name = ProfileName.Trim(),
                    Description = string.IsNullOrWhiteSpace(ProfileDescription) ? null : ProfileDescription.Trim(),
                    Options = options,
                    CreatedAt = SelectedProfile?.CreatedAt ?? DateTime.UtcNow,
                    ModifiedAt = DateTime.UtcNow
                };

                await _profileService.SaveProfileAsync(profile);
                
                await LoadProfilesAsync();
                ClearEditForm();
                
                StatusMessage = $"Profile '{profile.Name}' saved successfully";
            }
            finally
            {
                try { File.Delete(tempLogPath); } catch { }
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error saving profile: {ex.Message}";
        }
    }

    /// <summary>
    /// Deletes the selected profile.
    /// </summary>
    [RelayCommand]
    private async Task DeleteProfileAsync()
    {
        if (SelectedProfile == null) return;

        try
        {
            var result = MessageBox.Show(
                $"Are you sure you want to delete the profile '{SelectedProfile.Name}'? This action cannot be undone.",
                "Delete Profile",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                await _profileService.DeleteProfileAsync(SelectedProfile.Id);
                await LoadProfilesAsync();
                
                if (SelectedProfile?.Id == SelectedProfile?.Id)
                {
                    SelectedProfile = null;
                }
                
                StatusMessage = $"Profile '{SelectedProfile?.Name}' deleted";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error deleting profile: {ex.Message}";
        }
    }

    /// <summary>
    /// Cancels the current edit operation.
    /// </summary>
    [RelayCommand]
    private void CancelEdit()
    {
        ClearEditForm();
        StatusMessage = "Edit cancelled";
    }

    /// <summary>
    /// Loads the selected profile into the edit form.
    /// </summary>
    [RelayCommand]
    private void LoadProfile()
    {
        if (SelectedProfile == null) return;

        ProfileName = SelectedProfile.Name;
        ProfileDescription = SelectedProfile.Description ?? string.Empty;
        OptionsViewModel.SetCopyOptions(SelectedProfile.Options);
        StatusMessage = $"Loaded profile: {SelectedProfile.Name}";
    }

    /// <summary>
    /// Exports the selected profile to a file.
    /// </summary>
    [RelayCommand]
    private async Task ExportProfileAsync()
    {
        if (SelectedProfile == null) return;

        try
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Nexus Copy Profile (*.ncprofile)|*.ncprofile|All files (*.*)|*.*",
                FileName = $"{SelectedProfile.Name}.ncprofile",
                Title = "Export Profile"
            };

            if (dialog.ShowDialog() == true)
            {
                var json = System.Text.Json.JsonSerializer.Serialize(SelectedProfile, new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
                });

                await File.WriteAllTextAsync(dialog.FileName, json);
                StatusMessage = $"Profile exported to {dialog.FileName}";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error exporting profile: {ex.Message}";
        }
    }

    /// <summary>
    /// Imports a profile from a file.
    /// </summary>
    [RelayCommand]
    private async Task ImportProfileAsync()
    {
        try
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Nexus Copy Profile (*.ncprofile)|*.ncprofile|All files (*.*)|*.*",
                Title = "Import Profile"
            };

            if (dialog.ShowDialog() == true)
            {
                var json = await File.ReadAllTextAsync(dialog.FileName);
                var profile = System.Text.Json.JsonSerializer.Deserialize<SavedProfile>(json, new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
                });

                if (profile != null)
                {
                    // Generate new ID to avoid conflicts
                    profile = profile with { Id = Guid.NewGuid(), CreatedAt = DateTime.UtcNow, ModifiedAt = DateTime.UtcNow };
                    await _profileService.SaveProfileAsync(profile);
                    await LoadProfilesAsync();
                    StatusMessage = $"Profile '{profile.Name}' imported successfully";
                }
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error importing profile: {ex.Message}";
        }
    }

    private void ClearEditForm()
    {
        ProfileName = string.Empty;
        ProfileDescription = string.Empty;
        OptionsViewModel.ResetToDefaults();
        IsEditing = false;
        SelectedProfile = null;
    }

    partial void OnSelectedProfileChanged(SavedProfile? oldValue, SavedProfile? newValue)
    {
        if (newValue != null && !IsEditing)
        {
            LoadProfile();
        }
    }
}
