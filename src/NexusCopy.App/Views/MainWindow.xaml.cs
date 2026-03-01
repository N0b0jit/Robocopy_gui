namespace NexusCopy.App.Views;

using System.Windows;
using NexusCopy.App.ViewModels;

/// <summary>
/// The main application window.
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
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
}
