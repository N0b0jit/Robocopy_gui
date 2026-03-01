namespace NexusCopy.App.Views;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NexusCopy.App.ViewModels;
using NexusCopy.Core.Interfaces;
using NexusCopy.Services;
using NexusCopy.Shell;
using System.Windows;

/// <summary>
/// The main application window.
/// </summary>
public partial class MainWindow : Window
{
    private IHost? _host;

    public MainWindow()
    {
        InitializeComponent();
        SetupServices();
        DataContext = _host?.Services.GetRequiredService<MainViewModel>();
    }

    /// <summary>
    /// Handles application startup with command line arguments.
    /// </summary>
    /// <param name="args">Command line arguments.</param>
    public void HandleStartup(string[] args)
    {
        if (DataContext is MainViewModel mainViewModel)
        {
            mainViewModel.HandleStartup(args);
        }
    }

    private void SetupServices()
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                // Core services
                services.AddSingleton<IRobocopyService, RobocopyService>();
                services.AddSingleton<ILogService, LogService>();
                services.AddSingleton<IProfileService, ProfileService>();

                // ViewModels
                services.AddTransient<MainViewModel>();
                services.AddTransient<CopyJobViewModel>();
                services.AddTransient<HistoryViewModel>();
                services.AddTransient<ProfileManagerViewModel>();
                services.AddTransient<SettingsViewModel>();
            })
            .Build();
    }

    protected override void OnClosed(EventArgs e)
    {
        _host?.Dispose();
        base.OnClosed(e);
    }
}
