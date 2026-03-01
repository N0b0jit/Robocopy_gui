namespace NexusCopy.App.Controls;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;
using System.Windows.Controls;

/// <summary>
/// A control for displaying the generated robocopy command.
/// </summary>
public partial class CommandPreviewControl : UserControl
{
    /// <summary>
    /// Gets or sets the command preview text.
    /// </summary>
    public string CommandPreview
    {
        get => (string)GetValue(CommandPreviewProperty);
        set => SetValue(CommandPreviewProperty, value);
    }

    public static readonly DependencyProperty CommandPreviewProperty =
        DependencyProperty.Register(nameof(CommandPreview), typeof(string), typeof(CommandPreviewControl),
            new PropertyMetadata(string.Empty));

    public CommandPreviewControl()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Copies the command to clipboard.
    /// </summary>
    [RelayCommand]
    private void CopyCommand()
    {
        try
        {
            if (!string.IsNullOrEmpty(CommandPreview))
            {
                System.Windows.Clipboard.SetText(CommandPreview);
            }
        }
        catch
        {
            // Clipboard operations can fail, ignore silently
        }
    }
}
