namespace NexusCopy.App.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using NexusCopy.Core.Interfaces;
using NexusCopy.Core.Models;
using NexusCopy.Shell;
using System.Windows;

/// <summary>
/// Main application ViewModel.
/// </summary>
public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private string _windowTitle = "Nexus Copy";

    [ObservableProperty]
    private int _selectedTabIndex;

    [ObservableProperty]
    private string _statusMessage = "Ready";

    public CopyJobViewModel CopyJobVm { get; }
    public HistoryViewModel HistoryVm { get; }
    public ProfileManagerViewModel ProfileManagerVm { get; }
    public SettingsViewModel SettingsVm { get; }

    /// <summary>
    /// Initializes a new instance of the MainViewModel class.
    /// </summary>
    /// <param name="robocopyService">The robocopy service.</param>
    /// <param name="logService">The log service.</param>
    /// <param name="profileService">The profile service.</param>
    public MainViewModel(IRobocopyService robocopyService, ILogService logService, IProfileService profileService)
    {
        CopyJobVm = new CopyJobViewModel(robocopyService, logService);
        HistoryVm = new HistoryViewModel(logService);
        ProfileManagerVm = new ProfileManagerViewModel(profileService);
        SettingsVm = new SettingsViewModel();

        // Subscribe to view model events
        CopyJobVm.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(CopyJobViewModel.StatusMessage))
            {
                StatusMessage = CopyJobVm.StatusMessage;
            }
        };
    }

    /// <summary>
    /// Handles application startup with command line arguments.
    /// </summary>
    /// <param name="args">Command line arguments.</param>
    public void HandleStartup(string[] args)
    {
        try
        {
            var launchOptions = CliParser.Parse(args);

            if (launchOptions.ShowHelp)
            {
                MessageBox.Show(CliParser.GetHelpText(), "Nexus Copy - Help", MessageBoxButton.OK, MessageBoxImage.Information);
                Application.Current.Shutdown();
                return;
            }

            if (launchOptions.ShowVersion)
            {
                MessageBox.Show(CliParser.GetVersionText(), "Nexus Copy - Version", MessageBoxButton.OK, MessageBoxImage.Information);
                Application.Current.Shutdown();
                return;
            }

            // Pre-fill UI with command line arguments
            if (!string.IsNullOrEmpty(launchOptions.Source))
            {
                CopyJobVm.SetSourcePath(launchOptions.Source);
            }

            if (!string.IsNullOrEmpty(launchOptions.Destination))
            {
                CopyJobVm.SetDestinationPath(launchOptions.Destination);
            }

            if (launchOptions.Mode != CopyMode.Copy)
            {
                CopyJobVm.OptionsViewModel.Mode = launchOptions.Mode;
            }

            if (launchOptions.HasOptions)
            {
                WindowTitle = "Nexus Copy - Launched from context menu";
                StatusMessage = "Ready to copy! Select destination and click Start.";
                SelectedTabIndex = 0; // Focus on Copy Job tab
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error processing command line: {ex.Message}";
        }
    }

    /// <summary>
    /// Shows the about dialog.
    /// </summary>
    [RelayCommand]
    private void ShowAbout()
    {
        SettingsVm.ShowAboutCommand.Execute(null);
    }

    /// <summary>
    /// Exits the application.
    /// </summary>
    [RelayCommand]
    private void Exit()
    {
        Application.Current.Shutdown();
    }

    /// <summary>
    /// Handles tab selection changes.
    /// </summary>
    partial void OnSelectedTabIndexChanged(int oldValue, int newValue)
    {
        switch (newValue)
        {
            case 0: // Copy Job
                StatusMessage = CopyJobVm.StatusMessage;
                break;
            case 1: // History
                StatusMessage = HistoryVm.StatusMessage;
                break;
            case 2: // Profiles
                StatusMessage = ProfileManagerVm.StatusMessage;
                break;
            case 3: // Settings
                StatusMessage = "Configure application settings";
                break;
        }
    }
}
