namespace NexusCopy.Services.Tests;

using FluentAssertions;
using NexusCopy.Core.Models;
using Xunit;

/// <summary>
/// Unit tests for CommandBuilder.
/// </summary>
public class CommandBuilderTests
{
    [Fact]
    public void Build_WithBasicCopyOptions_ShouldGenerateCorrectCommand()
    {
        // Arrange
        var options = new CopyOptions
        {
            Source = @"C:\Source",
            Destination = @"D:\Destination",
            LogFilePath = @"C:\Logs\test.log",
            Mode = CopyMode.Copy,
            IncludeSubdirectories = true,
            RestartableMode = true,
            MultiThreaded = true,
            ThreadCount = 16
        };

        // Act
        var result = CommandBuilder.Build(options);

        // Assert
        result.Should().Contain("\"C:\\Source\" \"D:\\Destination\"");
        result.Should().Contain("/s");
        result.Should().Contain("/z");
        result.Should().Contain("/mt:16");
        result.Should().Contain("/log+:\"C:\\Logs\\test.log\"");
        result.Should().Contain("/tee");
    }

    [Fact]
    public void Build_WithMoveMode_ShouldIncludeMoveFlag()
    {
        // Arrange
        var options = new CopyOptions
        {
            Source = @"C:\Source",
            Destination = @"D:\Destination",
            LogFilePath = @"C:\Logs\test.log",
            Mode = CopyMode.Move
        };

        // Act
        var result = CommandBuilder.Build(options);

        // Assert
        result.Should().Contain("/move");
    }

    [Fact]
    public void Build_WithMirrorMode_ShouldIncludeMirrorFlag()
    {
        // Arrange
        var options = new CopyOptions
        {
            Source = @"C:\Source",
            Destination = @"D:\Destination",
            LogFilePath = @"C:\Logs\test.log",
            Mode = CopyMode.Mirror
        };

        // Act
        var result = CommandBuilder.Build(options);

        // Assert
        result.Should().Contain("/mir");
    }

    [Fact]
    public void Build_WithSyncMode_ShouldIncludeEmptyFlag()
    {
        // Arrange
        var options = new CopyOptions
        {
            Source = @"C:\Source",
            Destination = @"D:\Destination",
            LogFilePath = @"C:\Logs\test.log",
            Mode = CopyMode.Sync,
            IncludeSubdirectories = true,
            IncludeEmpty = true
        };

        // Act
        var result = CommandBuilder.Build(options);

        // Assert
        result.Should().Contain("/e");
    }

    [Fact]
    public void Build_WithIncludeEmpty_ShouldUseEFlag()
    {
        // Arrange
        var options = new CopyOptions
        {
            Source = @"C:\Source",
            Destination = @"D:\Destination",
            LogFilePath = @"C:\Logs\test.log",
            IncludeSubdirectories = true,
            IncludeEmpty = true
        };

        // Act
        var result = CommandBuilder.Build(options);

        // Assert
        result.Should().Contain("/e");
        result.Should().NotContain("/s");
    }

    [Fact]
    public void Build_WithoutIncludeEmpty_ShouldUseSFlag()
    {
        // Arrange
        var options = new CopyOptions
        {
            Source = @"C:\Source",
            Destination = @"D:\Destination",
            LogFilePath = @"C:\Logs\test.log",
            IncludeSubdirectories = true,
            IncludeEmpty = false
        };

        // Act
        var result = CommandBuilder.Build(options);

        // Assert
        result.Should().Contain("/s");
        result.Should().NotContain("/e");
    }

    [Fact]
    public void Build_WithExcludeFiles_ShouldIncludeXfFlags()
    {
        // Arrange
        var options = new CopyOptions
        {
            Source = @"C:\Source",
            Destination = @"D:\Destination",
            LogFilePath = @"C:\Logs\test.log",
            ExcludeFiles = new[] { "*.tmp", "*.log", "temp.txt" }
        };

        // Act
        var result = CommandBuilder.Build(options);

        // Assert
        result.Should().Contain("/xf \"*.tmp\"");
        result.Should().Contain("/xf \"*.log\"");
        result.Should().Contain("/xf \"temp.txt\"");
    }

    [Fact]
    public void Build_WithExcludeDirectories_ShouldIncludeXdFlags()
    {
        // Arrange
        var options = new CopyOptions
        {
            Source = @"C:\Source",
            Destination = @"D:\Destination",
            LogFilePath = @"C:\Logs\test.log",
            ExcludeDirectories = new[] { ".git", "node_modules", "bin" }
        };

        // Act
        var result = CommandBuilder.Build(options);

        // Assert
        result.Should().Contain("/xd \".git\"");
        result.Should().Contain("/xd \"node_modules\"");
        result.Should().Contain("/xd \"bin\"");
    }

    [Fact]
    public void Build_WithBackupMode_ShouldIncludeBFlag()
    {
        // Arrange
        var options = new CopyOptions
        {
            Source = @"C:\Source",
            Destination = @"D:\Destination",
            LogFilePath = @"C:\Logs\test.log",
            BackupMode = true
        };

        // Act
        var result = CommandBuilder.Build(options);

        // Assert
        result.Should().Contain("/b");
    }

    [Fact]
    public void Build_WithSkipNewerFiles_ShouldIncludeXoFlag()
    {
        // Arrange
        var options = new CopyOptions
        {
            Source = @"C:\Source",
            Destination = @"D:\Destination",
            LogFilePath = @"C:\Logs\test.log",
            SkipNewerFiles = true
        };

        // Act
        var result = CommandBuilder.Build(options);

        // Assert
        result.Should().Contain("/xo");
    }

    [Fact]
    public void Build_WithExcludeHidden_ShouldIncludeXaHFlag()
    {
        // Arrange
        var options = new CopyOptions
        {
            Source = @"C:\Source",
            Destination = @"D:\Destination",
            LogFilePath = @"C:\Logs\test.log",
            ExcludeHidden = true
        };

        // Act
        var result = CommandBuilder.Build(options);

        // Assert
        result.Should().Contain("/xa:H");
    }

    [Fact]
    public void Build_WithThreadCount_ShouldClampToValidRange()
    {
        // Arrange
        var options = new CopyOptions
        {
            Source = @"C:\Source",
            Destination = @"D:\Destination",
            LogFilePath = @"C:\Logs\test.log",
            MultiThreaded = true,
            ThreadCount = 256 // Above maximum
        };

        // Act
        var result = CommandBuilder.Build(options);

        // Assert
        result.Should().Contain("/mt:128"); // Should be clamped to 128
    }

    [Fact]
    public void Build_WithEmptyExcludeArrays_ShouldNotIncludeFlags()
    {
        // Arrange
        var options = new CopyOptions
        {
            Source = @"C:\Source",
            Destination = @"D:\Destination",
            LogFilePath = @"C:\Logs\test.log",
            ExcludeFiles = Array.Empty<string>(),
            ExcludeDirectories = Array.Empty<string>()
        };

        // Act
        var result = CommandBuilder.Build(options);

        // Assert
        result.Should().NotContain("/xf");
        result.Should().NotContain("/xd");
    }

    [Fact]
    public void Build_WithNullExcludeArrays_ShouldNotIncludeFlags()
    {
        // Arrange
        var options = new CopyOptions
        {
            Source = @"C:\Source",
            Destination = @"D:\Destination",
            LogFilePath = @"C:\Logs\test.log",
            ExcludeFiles = null!,
            ExcludeDirectories = null!
        };

        // Act
        var result = CommandBuilder.Build(options);

        // Assert
        result.Should().NotContain("/xf");
        result.Should().NotContain("/xd");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(8)]
    [InlineData(16)]
    [InlineData(32)]
    [InlineData(64)]
    [InlineData(128)]
    public void Build_WithValidThreadCounts_ShouldIncludeMtFlag(int threadCount)
    {
        // Arrange
        var options = new CopyOptions
        {
            Source = @"C:\Source",
            Destination = @"D:\Destination",
            LogFilePath = @"C:\Logs\test.log",
            MultiThreaded = true,
            ThreadCount = threadCount
        };

        // Act
        var result = CommandBuilder.Build(options);

        // Assert
        result.Should().Contain($"/mt:{threadCount}");
    }

    [Fact]
    public void Build_WithPathsContainingSpaces_ShouldQuotePaths()
    {
        // Arrange
        var options = new CopyOptions
        {
            Source = @"C:\My Source Folder",
            Destination = @"D:\My Destination Folder",
            LogFilePath = @"C:\Logs\test.log"
        };

        // Act
        var result = CommandBuilder.Build(options);

        // Assert
        result.Should().Contain("\"C:\\My Source Folder\" \"D:\\My Destination Folder\"");
    }
}
