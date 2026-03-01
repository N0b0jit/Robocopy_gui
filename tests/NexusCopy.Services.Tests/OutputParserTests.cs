namespace NexusCopy.Services.Tests;

using FluentAssertions;
using Xunit;

/// <summary>
/// Unit tests for OutputParser.
/// </summary>
public class OutputParserTests
{
    [Fact]
    public void TryParse_WithValidProgressLine_ShouldReturnProgressUpdate()
    {
        // Arrange
        var line = "	100%	New File 		   2.5 m	Documents\report.pdf";

        // Act
        var result = OutputParser.TryParse(line);

        // Assert
        result.Should().NotBeNull();
        result!.FilePercent.Should().Be(100.0);
        result.Status.Should().Be("New File");
        result.FileName.Should().Be(@"Documents\report.pdf");
        result.SizeText.Should().Be("2.5 m");
    }

    [Fact]
    public void TryParse_WithPartialProgressLine_ShouldReturnProgressUpdate()
    {
        // Arrange
        var line = "	67.5%	Newer 		   1.2 m	Documents\budget.xlsx";

        // Act
        var result = OutputParser.TryParse(line);

        // Assert
        result.Should().NotBeNull();
        result!.FilePercent.Should().Be(67.5);
        result.Status.Should().Be("Newer");
        result.FileName.Should().Be(@"Documents\budget.xlsx");
        result.SizeText.Should().Be("1.2 m");
    }

    [Fact]
    public void TryParse_WithZeroPercent_ShouldReturnProgressUpdate()
    {
        // Arrange
        var line = "	0%	New File 		   800 k	Documents\notes.txt";

        // Act
        var result = OutputParser.TryParse(line);

        // Assert
        result.Should().NotBeNull();
        result!.FilePercent.Should().Be(0.0);
        result.Status.Should().Be("New File");
        result.FileName.Should().Be(@"Documents\notes.txt");
        result.SizeText.Should().Be("800 k");
    }

    [Fact]
    public void TryParse_WithDifferentStatusTypes_ShouldParseCorrectly()
    {
        // Arrange & Act & Assert
        var testCases = new[]
        {
            ("	100%	New File 		   2.5 m	Documents\\report.pdf", "New File"),
            ("	100%	Newer 		   1.2 m	Documents\\budget.xlsx", "Newer"),
            ("	100%	Same 		   800 k	Documents\\notes.txt", "Same"),
            ("	100%	Changed 		   1.5 g	Videos\\movie.mp4", "Changed"),
            ("	100%	Touched 		   200 k	Config\\settings.ini", "Touched")
        };

        foreach (var (line, expectedStatus) in testCases)
        {
            var result = OutputParser.TryParse(line);
            result.Should().NotBeNull();
            result!.Status.Should().Be(expectedStatus);
        }
    }

    [Fact]
    public void TryParse_WithDifferentSizeFormats_ShouldParseCorrectly()
    {
        // Arrange & Act & Assert
        var testCases = new[]
        {
            ("	100%	New File 		   800 b	Documents\\small.txt", "800 b"),
            ("	100%	New File 		   1.2 k	Documents\\medium.txt", "1.2 k"),
            ("	100%	New File 		   2.5 m	Documents\\large.txt", "2.5 m"),
            ("	100%	New File 		   1.5 g	Videos\\movie.mp4", "1.5 g")
        };

        foreach (var (line, expectedSize) in testCases)
        {
            var result = OutputParser.TryParse(line);
            result.Should().NotBeNull();
            result!.SizeText.Should().Be(expectedSize);
        }
    }

    [Fact]
    public void TryParse_WithInvalidLine_ShouldReturnNull()
    {
        // Arrange
        var invalidLines = new[]
        {
            string.Empty,
            "   ",
            "Invalid line without progress",
            "Total    Copied   Skipped  Mismatch    FAILED    Extras",
            "Dirs :    123    456    0    0    0",
            "----------",
            "               0    0    0    0    0"
        };

        foreach (var line in invalidLines)
        {
            // Act
            var result = OutputParser.TryParse(line);

            // Assert
            result.Should().BeNull();
        }
    }

    [Fact]
    public void TryParse_WithNullLine_ShouldReturnNull()
    {
        // Arrange
        string? line = null;

        // Act
        var result = OutputParser.TryParse(line!);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void TryParse_WithMalformedProgressLine_ShouldReturnNull()
    {
        // Arrange
        var malformedLines = new[]
        {
            "100%	New File", // Missing size and filename
            "	New File 		   2.5 m	Documents\\report.pdf", // Missing percent
            "	100%	New File 		   Documents\\report.pdf", // Missing size
            "	abc	New File 		   2.5 m	Documents\\report.pdf", // Invalid percent
            "	100%	2.5 m	Documents\\report.pdf", // Missing status
        };

        foreach (var line in malformedLines)
        {
            // Act
            var result = OutputParser.TryParse(line);

            // Assert
            result.Should().BeNull();
        }
    }

    [Fact]
    public void TryParseFileCount_WithValidDirectoryLine_ShouldReturnFileCountInfo()
    {
        // Arrange
        var line = "	Dirs :	  123	  456	   0	   0	   0";

        // Act
        var result = OutputParser.TryParseFileCount(line);

        // Assert
        result.Should().NotBeNull();
        result!.Type.Should().Be("Dirs");
        result.Total.Should().Be(123);
        result.Copied.Should().Be(456);
        result.Skipped.Should().Be(0);
        result.Mismatch.Should().Be(0);
        result.Failed.Should().Be(0);
    }

    [Fact]
    public void TryParseFileCount_WithValidFilesLine_ShouldReturnFileCountInfo()
    {
        // Arrange
        var line = "	Files :	 12345	  6789	   123	   45	   6";

        // Act
        var result = OutputParser.TryParseFileCount(line);

        // Assert
        result.Should().NotBeNull();
        result!.Type.Should().Be("Files");
        result.Total.Should().Be(12345);
        result.Copied.Should().Be(6789);
        result.Skipped.Should().Be(123);
        result.Mismatch.Should().Be(45);
        result.Failed.Should().Be(6);
    }

    [Fact]
    public void TryParseFileCount_WithInvalidLine_ShouldReturnNull()
    {
        // Arrange
        var invalidLines = new[]
        {
            string.Empty,
            "   ",
            "Invalid line without file count",
            "Total    Copied   Skipped  Mismatch    FAILED    Extras",
            "----------"
        };

        foreach (var line in invalidLines)
        {
            // Act
            var result = OutputParser.TryParseFileCount(line);

            // Assert
            result.Should().BeNull();
        }
    }

    [Fact]
    public void IsSummaryHeader_WithValidHeader_ShouldReturnTrue()
    {
        // Arrange
        var headerLines = new[]
        {
            "Total    Copied   Skipped  Mismatch    FAILED    Extras",
            "               Copied   Skipped  Mismatch    FAILED    Extras",
            "TOTAL    COPIED   SKIPPED  MISMATCH    FAILED    EXTRAS"
        };

        foreach (var line in headerLines)
        {
            // Act
            var result = OutputParser.IsSummaryHeader(line);

            // Assert
            result.Should().BeTrue();
        }
    }

    [Fact]
    public void IsSummaryHeader_WithInvalidLine_ShouldReturnFalse()
    {
        // Arrange
        var invalidLines = new[]
        {
            string.Empty,
            "   ",
            "	100%	New File 		   2.5 m	Documents\report.pdf",
            "	Dirs :	  123	  456	   0	   0	   0",
            "----------"
        };

        foreach (var line in invalidLines)
        {
            // Act
            var result = OutputParser.IsSummaryHeader(line);

            // Assert
            result.Should().BeFalse();
        }
    }

    [Fact]
    public void TryParse_WithSpecialCharactersInFilename_ShouldParseCorrectly()
    {
        // Arrange
        var line = "	100%	New File 		   2.5 m	Documents\\report (v2).pdf";

        // Act
        var result = OutputParser.TryParse(line);

        // Assert
        result.Should().NotBeNull();
        result!.FileName.Should().Be(@"Documents\report (v2).pdf");
    }

    [Fact]
    public void TryParse_WithDeepPath_ShouldParseCorrectly()
    {
        // Arrange
        var line = "	100%	New File 		   1.2 m	Very\\Deep\\Nested\\Path\\To\\File.txt";

        // Act
        var result = OutputParser.TryParse(line);

        // Assert
        result.Should().NotBeNull();
        result!.FileName.Should().Be(@"Very\Deep\Nested\Path\To\File.txt");
    }
}
