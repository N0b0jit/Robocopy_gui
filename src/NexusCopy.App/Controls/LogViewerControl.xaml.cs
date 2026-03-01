namespace NexusCopy.App.Controls;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;
using System.Windows.Controls;

/// <summary>
/// A control for displaying log output with auto-scroll functionality.
/// </summary>
public partial class LogViewerControl : UserControl
{
    /// <summary>
    /// Gets or sets the log output text.
    /// </summary>
    public string LogOutput
    {
        get => (string)GetValue(LogOutputProperty);
        set => SetValue(LogOutputProperty, value);
    }

    /// <summary>
    /// Gets or sets whether to auto-scroll to the bottom.
    /// </summary>
    public bool AutoScroll
    {
        get => (bool)GetValue(AutoScrollProperty);
        set => SetValue(AutoScrollProperty, value);
    }

    public static readonly DependencyProperty LogOutputProperty =
        DependencyProperty.Register(nameof(LogOutput), typeof(string), typeof(LogViewerControl),
            new PropertyMetadata(string.Empty, OnLogOutputChanged));

    public static readonly DependencyProperty AutoScrollProperty =
        DependencyProperty.Register(nameof(AutoScroll), typeof(bool), typeof(LogViewerControl),
            new PropertyMetadata(true));

    private bool _userScrolled = false;
    private ScrollViewer? _scrollViewer;

    public LogViewerControl()
    {
        InitializeComponent();
        Loaded += LogViewerControl_Loaded;
    }

    private void LogViewerControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        // Find the ScrollViewer in the visual tree
        _scrollViewer = FindChild<ScrollViewer>(LogScrollViewer);
        if (_scrollViewer != null)
        {
            _scrollViewer.ScrollChanged += ScrollViewer_ScrollChanged;
        }
    }

    private static void OnLogOutputChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is LogViewerControl control)
        {
            control.UpdateScrollPosition();
        }
    }

    private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        if (_scrollViewer == null) return;

        // Check if user has manually scrolled away from bottom
        var isAtBottom = Math.Abs(_scrollViewer.VerticalOffset - _scrollViewer.ScrollableHeight) < 1.0;
        _userScrolled = !isAtBottom && e.VerticalChange != 0;
    }

    private void UpdateScrollPosition()
    {
        if (AutoScroll && !_userScrolled && _scrollViewer != null)
        {
            // Use Dispatcher to ensure UI is updated before scrolling
            Dispatcher.BeginInvoke(() =>
            {
                _scrollViewer?.ScrollToBottom();
            });
        }
    }

    /// <summary>
    /// Clears the log output.
    /// </summary>
    [RelayCommand]
    private void ClearLog()
    {
        LogOutput = string.Empty;
        _userScrolled = false;
    }

    /// <summary>
    /// Finds a child element of the specified type in the visual tree.
    /// </summary>
    private static T? FindChild<T>(DependencyObject parent) where T : DependencyObject
    {
        if (parent == null) return null;

        for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
            if (child is T result)
                return result;

            var childOfChild = FindChild<T>(child);
            if (childOfChild != null)
                return childOfChild;
        }

        return null;
    }
}
