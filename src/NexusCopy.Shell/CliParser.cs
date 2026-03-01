namespace NexusCopy.Shell;

using NexusCopy.Core.Models;

/// <summary>
/// Parses command line arguments for Nexus Copy.
/// </summary>
public static class CliParser
{
    /// <summary>
    /// Parses command line arguments into launch options.
    /// </summary>
    /// <param name="args">The command line arguments.</param>
    /// <returns>The parsed launch options.</returns>
    public static LaunchOptions Parse(string[] args)
    {
        var options = new LaunchOptions();

        for (int i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            
            switch (arg.ToLowerInvariant())
            {
                case "--source":
                case "-s":
                    if (i + 1 < args.Length)
                    {
                        options.Source = args[++i];
                    }
                    break;

                case "--destination":
                case "-d":
                    if (i + 1 < args.Length)
                    {
                        options.Destination = args[++i];
                    }
                    break;

                case "--mode":
                case "-m":
                    if (i + 1 < args.Length)
                    {
                        var modeText = args[++i];
                        if (Enum.TryParse<CopyMode>(modeText, ignoreCase: true, out var mode))
                        {
                            options.Mode = mode;
                        }
                    }
                    break;

                case "--help":
                case "-h":
                case "-?":
                    options.ShowHelp = true;
                    break;

                case "--version":
                case "-v":
                    options.ShowVersion = true;
                    break;

                default:
                    // Handle arguments without -- prefix (legacy support)
                    if (arg.StartsWith("--") && !arg.Contains(" "))
                    {
                        // Skip unknown arguments
                        continue;
                    }
                    break;
            }
        }

        return options;
    }

    /// <summary>
    /// Generates help text for the command line interface.
    /// </summary>
    /// <returns>The help text.</returns>
    public static string GetHelpText()
    {
        return """
Nexus Copy - Fast File Copy Manager

Usage:
  NexusCopy.exe [options]

Options:
  --source <path>        Pre-fill the source folder path
  --destination <path>   Pre-fill the destination folder path
  --mode <mode>          Pre-select copy mode (Copy, Move, Mirror, Sync)
  --help, -h             Show this help message
  --version, -v          Show version information

Examples:
  NexusCopy.exe --source "C:\MyFolder" --mode copy
  NexusCopy.exe --source "C:\Data" --destination "D:\Backup" --mode mirror

Context Menu Integration:
  Right-click any folder in Windows Explorer to see "Nexus Copy Here →"
  with options for Copy To, Move To, and Mirror To operations.
""";
    }

    /// <summary>
    /// Gets the version information.
    /// </summary>
    /// <returns>The version text.</returns>
    public static string GetVersionText()
    {
        var version = System.Reflection.Assembly.GetExecutingAssembly()
            .GetName().Version?.ToString() ?? "1.0.0";
        
        return $"Nexus Copy v{version}";
    }
}

/// <summary>
/// Represents parsed command line launch options.
/// </summary>
public class LaunchOptions
{
    /// <summary>
    /// Gets or sets the source folder path.
    /// </summary>
    public string? Source { get; set; }

    /// <summary>
    /// Gets or sets the destination folder path.
    /// </summary>
    public string? Destination { get; set; }

    /// <summary>
    /// Gets or sets the copy mode.
    /// </summary>
    public CopyMode Mode { get; set; } = CopyMode.Copy;

    /// <summary>
    /// Gets or sets a value indicating whether to show help.
    /// </summary>
    public bool ShowHelp { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to show version.
    /// </summary>
    public bool ShowVersion { get; set; }

    /// <summary>
    /// Gets a value indicating whether any launch options were provided.
    /// </summary>
    public bool HasOptions => !string.IsNullOrEmpty(Source) || 
                             !string.IsNullOrEmpty(Destination) || 
                             ShowHelp || 
                             ShowVersion;
}
