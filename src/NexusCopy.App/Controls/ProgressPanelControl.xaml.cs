namespace NexusCopy.App.Controls;

using System.Windows.Controls;

/// <summary>
/// A control for displaying copy operation progress.
/// </summary>
public partial class ProgressPanelControl : UserControl
{
    /// <summary>
    /// Gets or sets the progress view model.
    /// </summary>
    public new object? DataContext
    {
        get => base.DataContext;
        set => base.DataContext = value;
    }

    public ProgressPanelControl()
    {
        InitializeComponent();
    }
}
