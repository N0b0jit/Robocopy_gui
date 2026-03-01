namespace NexusCopy.Services;

using NexusCopy.Core.Models;
using System.Text;

/// <summary>
/// Builds robocopy command line arguments from CopyOptions.
/// </summary>
public static class CommandBuilder
{
    /// <summary>
    /// Builds the complete robocopy command line arguments string.
    /// </summary>
    /// <param name="options">The copy options to convert.</param>
    /// <returns>The command line arguments string.</returns>
    public static string Build(CopyOptions options)
    {
        var args = new StringBuilder();

        // Source and destination (always quoted)
        args.Append($"\"{options.Source}\" \"{options.Destination}\"");

        // Copy flags based on mode
        switch (options.Mode)
        {
            case CopyMode.Move:
                args.Append(" /move");
                break;
            case CopyMode.Mirror:
                args.Append(" /mir");
                break;
            case CopyMode.Sync:
                args.Append(" /e");
                break;
            case CopyMode.Copy:
                if (options.IncludeSubdirectories)
                {
                    args.Append(options.IncludeEmpty ? " /e" : " /s");
                }
                break;
        }

        // Additional options
        if (options.RestartableMode) args.Append(" /z");
        if (options.BackupMode) args.Append(" /b");
        if (options.MirrorMode) args.Append(" /mir");
        if (options.MoveFiles) args.Append(" /move");

        // Multi-threading
        if (options.MultiThreaded)
        {
            var threadCount = Math.Clamp(options.ThreadCount, 1, 128);
            args.Append($" /mt:{threadCount}");
        }

        // File selection options
        if (options.SkipNewerFiles) args.Append(" /xo");
        if (options.ExcludeHidden) args.Append(" /xa:H");
        if (options.ExcludeSystem) args.Append(" /xa:S");

        // Exclude files
        foreach (var file in options.ExcludeFiles.Where(f => !string.IsNullOrEmpty(f)))
        {
            args.Append($" /xf \"{file}\"");
        }

        // Exclude directories
        foreach (var dir in options.ExcludeDirectories.Where(d => !string.IsNullOrEmpty(d)))
        {
            args.Append($" /xd \"{dir}\"");
        }

        // Logging (always log to file and to stdout)
        args.Append($" /log+:\"{options.LogFilePath}\"");
        args.Append(" /tee");

        return args.ToString().Trim();
    }
}
