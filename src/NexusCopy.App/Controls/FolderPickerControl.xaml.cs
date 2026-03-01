namespace NexusCopy.App.Controls;

using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows;
using System.Windows.Controls;

/// <summary>
/// A reusable folder picker control with drag-and-drop support.
/// </summary>
public partial class FolderPickerControl : UserControl
{
    /// <summary>
    /// Gets or sets the selected folder path.
    /// </summary>
    public string Path
    {
        get => (string)GetValue(PathProperty);
        set => SetValue(PathProperty, value);
    }

    /// <summary>
    /// Gets or sets the placeholder text for the text box.
    /// </summary>
    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the text box is read-only.
    /// </summary>
    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    /// <summary>
    /// Occurs when the selected path changes.
    /// </summary>
    public event EventHandler<string>? PathChanged;

    public static readonly DependencyProperty PathProperty =
        DependencyProperty.Register(nameof(Path), typeof(string), typeof(FolderPickerControl),
            new PropertyMetadata(string.Empty, OnPathChanged));

    public static readonly DependencyProperty PlaceholderProperty =
        DependencyProperty.Register(nameof(Placeholder), typeof(string), typeof(FolderPickerControl),
            new PropertyMetadata("Select folder..."));

    public static readonly DependencyProperty IsReadOnlyProperty =
        DependencyProperty.Register(nameof(IsReadOnly), typeof(bool), typeof(FolderPickerControl),
            new PropertyMetadata(false));

    public FolderPickerControl()
    {
        InitializeComponent();
        UpdatePlaceholder();
    }

    private static void OnPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FolderPickerControl control)
        {
            control.UpdatePlaceholder();
            control.PathChanged?.Invoke(control, (string)e.NewValue);
        }
    }

    private void UpdatePlaceholder()
    {
        if (string.IsNullOrEmpty(Path))
        {
            PathTextBox.Text = Placeholder;
            PathTextBox.Foreground = System.Windows.Media.Brushes.Gray;
        }
        else
        {
            PathTextBox.Text = Path;
            PathTextBox.Foreground = System.Windows.Media.Brushes.Black;
        }
    }

    private void BrowseButton_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        var dialog = new System.Windows.Forms.FolderBrowserDialog
        {
            Description = $"Select {Placeholder.ToLowerInvariant()}",
            ShowNewFolderButton = true
        };

        // Set initial directory if current path exists
        if (!string.IsNullOrEmpty(Path) && System.IO.Directory.Exists(Path))
        {
            dialog.SelectedPath = Path;
        }

        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            Path = dialog.SelectedPath;
        }
    }

    private void PathTextBox_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length > 0)
            {
                var path = files[0];
                if (System.IO.Directory.Exists(path))
                {
                    Path = path;
                    e.Handled = true;
                }
            }
        }
    }

    private void PathTextBox_DragEnter(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length > 0 && System.IO.Directory.Exists(files[0]))
            {
                PathTextBox.Background = System.Windows.Media.Brushes.LightBlue;
                e.Handled = true;
            }
        }
    }

    private void PathTextBox_DragLeave(object sender, DragEventArgs e)
    {
        PathTextBox.Background = System.Windows.Media.Brushes.Transparent;
        e.Handled = true;
    }

    protected override void OnRenderSizeChanged(System.Windows.SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);
        UpdatePlaceholder();
    }
}
