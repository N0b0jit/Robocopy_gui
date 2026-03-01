namespace NexusCopy.App;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NexusCopy.App.ViewModels;
using NexusCopy.Core.Interfaces;
using NexusCopy.Services;
using NexusCopy.Shell;
using System.Windows;
using Views;

/// <summary>
/// The main application class.
/// </summary>
public partial class App : Application
{
    private IHost? _host;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Setup dependency injection
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

        // Create and show main window
        var mainWindow = new MainWindow();
        mainWindow.HandleStartup(e.Args);
        mainWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _host?.Dispose();
        base.OnExit(e);
    }
}
