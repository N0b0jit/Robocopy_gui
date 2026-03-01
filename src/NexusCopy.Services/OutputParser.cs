namespace NexusCopy.Services;

using NexusCopy.Core.Models;
using System.Text.RegularExpressions;

/// <summary>
/// Parses robocopy output lines to extract progress information.
/// </summary>
public static class OutputParser
{
    // Regex pattern for progress lines: "100%        New File           1.2 m        Documents\report.pdf"
    private static readonly Regex ProgressLineRegex = new(
        @"^\s*(\d+\.?\d*)%\s+(\w[\w\s]*?)\s+([\d.]+\s*[kmgKMG]?)\s+(.+)$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    // Regex pattern for summary lines: "Total    Copied   Skipped  Mismatch    FAILED    Extras"
    private static readonly Regex SummaryLineRegex = new(
        @"^\s*Total\s+Copied\s+Skipped\s+Mismatch\s+FAILED\s+Extras",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    // Regex pattern for file count summary: "Dirs :    123    456    0    0    0"
    private static readonly Regex FileCountRegex = new(
        @"^\s*(Dirs|Files)\s*:\s*(\d+)\s+(\d+)\s+(\d+)\s+(\d+)\s+(\d+)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    /// <summary>
    /// Attempts to parse a robocopy output line as a progress update.
    /// </summary>
    /// <param name="line">The output line to parse.</param>
    /// <returns>A progress update if the line contains progress information, otherwise null.</returns>
    public static CopyProgressUpdate? TryParse(string line)
    {
        if (string.IsNullOrWhiteSpace(line))
            return null;

        var match = ProgressLineRegex.Match(line);
        if (!match.Success)
            return null;

        try
        {
            var percentText = match.Groups[1].Value;
            var status = match.Groups[2].Value.Trim();
            var sizeText = match.Groups[3].Value.Trim();
            var fileName = match.Groups[4].Value.Trim();

            if (double.TryParse(percentText, out var percent))
            {
                return new CopyProgressUpdate(
                    FilePercent: percent,
                    Status: status,
                    FileName: fileName,
                    SizeText: sizeText
                );
            }
        }
        catch
        {
            // Ignore parsing errors and return null
        }

        return null;
    }

    /// <summary>
    /// Attempts to parse file count information from summary lines.
    /// </summary>
    /// <param name="line">The output line to parse.</param>
    /// <returns>File count information if available, otherwise null.</returns>
    public static FileCountInfo? TryParseFileCount(string line)
    {
        if (string.IsNullOrWhiteSpace(line))
            return null;

        var match = FileCountRegex.Match(line);
        if (!match.Success)
            return null;

        try
        {
            var type = match.Groups[1].Value;
            var total = long.Parse(match.Groups[2].Value);
            var copied = long.Parse(match.Groups[3].Value);
            var skipped = long.Parse(match.Groups[4].Value);
            var mismatch = long.Parse(match.Groups[5].Value);
            var failed = long.Parse(match.Groups[6].Value);

            return new FileCountInfo(
                Type: type,
                Total: total,
                Copied: copied,
                Skipped: skipped,
                Mismatch: mismatch,
                Failed: failed
            );
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Checks if a line indicates the start of the summary section.
    /// </summary>
    /// <param name="line">The output line to check.</param>
    /// <returns>True if the line is a summary header, otherwise false.</returns>
    public static bool IsSummaryHeader(string line)
    {
        return !string.IsNullOrWhiteSpace(line) && SummaryLineRegex.IsMatch(line);
    }
}

/// <summary>
/// Represents a progress update from robocopy output.
/// </summary>
public record CopyProgressUpdate(
    double FilePercent,
    string Status,
    string FileName,
    string SizeText
);

/// <summary>
/// Represents file count information from robocopy summary.
/// </summary>
public record FileCountInfo(
    string Type,
    long Total,
    long Copied,
    long Skipped,
    long Mismatch,
    long Failed
);
