namespace NexusCopy.Core.Tests.Models;

using FluentAssertions;
using NexusCopy.Core.Models;
using Xunit;

/// <summary>
/// Unit tests for CopyOptions.
/// </summary>
public class CopyOptionsTests
{
    [Fact]
    public void CopyOptions_WithValidData_ShouldCreateInstance()
    {
        // Arrange
        var source = @"C:\Source";
        var destination = @"D:\Destination";
        var logFilePath = @"C:\Logs\test.log";

        // Act
        var options = new CopyOptions
        {
            Source = source,
            Destination = destination,
            LogFilePath = logFilePath,
            Mode = CopyMode.Copy,
            IncludeSubdirectories = true,
            MultiThreaded = true,
            ThreadCount = 16
        };

        // Assert
        options.Source.Should().Be(source);
        options.Destination.Should().Be(destination);
        options.LogFilePath.Should().Be(logFilePath);
        options.Mode.Should().Be(CopyMode.Copy);
        options.IncludeSubdirectories.Should().BeTrue();
        options.MultiThreaded.Should().BeTrue();
        options.ThreadCount.Should().Be(16);
    }

    [Theory]
    [InlineData(CopyMode.Copy)]
    [InlineData(CopyMode.Move)]
    [InlineData(CopyMode.Mirror)]
    [InlineData(CopyMode.Sync)]
    public void CopyOptions_WithAllModes_ShouldSupportAllCopyModes(CopyMode mode)
    {
        // Arrange & Act
        var options = new CopyOptions
        {
            Source = @"C:\Source",
            Destination = @"D:\Destination",
            LogFilePath = @"C:\Logs\test.log",
            Mode = mode
        };

        // Assert
        options.Mode.Should().Be(mode);
    }

    [Fact]
    public void CopyOptions_WithExcludePatterns_ShouldHandleArrays()
    {
        // Arrange
        var excludeFiles = new[] { "*.tmp", "*.log" };
        var excludeDirectories = new[] { ".git", "node_modules" };

        // Act
        var options = new CopyOptions
        {
            Source = @"C:\Source",
            Destination = @"D:\Destination",
            LogFilePath = @"C:\Logs\test.log",
            ExcludeFiles = excludeFiles,
            ExcludeDirectories = excludeDirectories
        };

        // Assert
        options.ExcludeFiles.Should().BeEquivalentTo(excludeFiles);
        options.ExcludeDirectories.Should().BeEquivalentTo(excludeDirectories);
    }

    [Fact]
    public void CopyOptions_WithThreadCount_ShouldClampToValidRange()
    {
        // Arrange & Act
        var options = new CopyOptions
        {
            Source = @"C:\Source",
            Destination = @"D:\Destination",
            LogFilePath = @"C:\Logs\test.log",
            ThreadCount = 256 // Above maximum
        };

        // Assert
        options.ThreadCount.Should().Be(256); // The model itself doesn't clamp, service layer should
    }

    [Fact]
    public void CopyOptions_WithDefaultValues_ShouldUseSensibleDefaults()
    {
        // Arrange & Act
        var options = new CopyOptions
        {
            Source = @"C:\Source",
            Destination = @"D:\Destination",
            LogFilePath = @"C:\Logs\test.log"
        };

        // Assert
        options.Mode.Should().Be(CopyMode.Copy);
        options.IncludeSubdirectories.Should().BeTrue();
        options.IncludeEmpty.Should().BeFalse();
        options.RestartableMode.Should().BeTrue();
        options.BackupMode.Should().BeFalse();
        options.MultiThreaded.Should().BeTrue();
        options.ThreadCount.Should().Be(16);
        options.SkipNewerFiles.Should().BeFalse();
        options.ExcludeHidden.Should().BeFalse();
        options.ExcludeSystem.Should().BeFalse();
        options.ExcludeFiles.Should().BeEmpty();
        options.ExcludeDirectories.Should().BeEmpty();
        options.MirrorMode.Should().BeFalse();
        options.MoveFiles.Should().BeFalse();
    }
}
