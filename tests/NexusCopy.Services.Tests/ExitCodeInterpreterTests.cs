namespace NexusCopy.Services.Tests;

using FluentAssertions;
using Xunit;

/// <summary>
/// Unit tests for ExitCodeInterpreter.
/// </summary>
public class ExitCodeInterpreterTests
{
    [Fact]
    public void Interpret_WithExitCode0_ShouldReturnSuccessMessage()
    {
        // Arrange
        var exitCode = 0;

        // Act
        var (messageType, message) = ExitCodeInterpreter.Interpret(exitCode);

        // Assert
        messageType.Should().Be(MessageType.Success);
        message.Should().Be("Nothing to copy - destination is already up to date.");
    }

    [Fact]
    public void Interpret_WithExitCode1_ShouldReturnSuccessMessage()
    {
        // Arrange
        var exitCode = 1;

        // Act
        var (messageType, message) = ExitCodeInterpreter.Interpret(exitCode);

        // Assert
        messageType.Should().Be(MessageType.Success);
        message.Should().Be("Files copied successfully.");
    }

    [Fact]
    public void Interpret_WithExitCode2_ShouldReturnSuccessWithExtrasMessage()
    {
        // Arrange
        var exitCode = 2;

        // Act
        var (messageType, message) = ExitCodeInterpreter.Interpret(exitCode);

        // Assert
        messageType.Should().Be(MessageType.Success);
        message.Should().Be("Extra files found in destination.");
    }

    [Fact]
    public void Interpret_WithExitCode4_ShouldReturnWarningMessage()
    {
        // Arrange
        var exitCode = 4;

        // Act
        var (messageType, message) = ExitCodeInterpreter.Interpret(exitCode);

        // Assert
        messageType.Should().Be(MessageType.Warning);
        message.Should().Be("Mismatched files found between source and destination.");
    }

    [Fact]
    public void Interpret_WithExitCode8_ShouldReturnWarningMessage()
    {
        // Arrange
        var exitCode = 8;

        // Act
        var (messageType, message) = ExitCodeInterpreter.Interpret(exitCode);

        // Assert
        messageType.Should().Be(MessageType.Warning);
        message.Should().Be("Some files failed to copy.");
    }

    [Fact]
    public void Interpret_WithExitCode16_ShouldReturnErrorMessage()
    {
        // Arrange
        var exitCode = 16;

        // Act
        var (messageType, message) = ExitCodeInterpreter.Interpret(exitCode);

        // Assert
        messageType.Should().Be(MessageType.Error);
        message.Should().Be("Fatal error occurred during copy operation.");
    }

    [Fact]
    public void Interpret_WithCombinedExitCodes_ShouldCombineMessages()
    {
        // Arrange & Act & Assert
        var testCases = new[]
        {
            (3, MessageType.Success, "Files copied successfully. Extra files found in destination."),
            (5, MessageType.Warning, "Files copied successfully. Mismatched files found between source and destination."),
            (9, MessageType.Warning, "Files copied successfully. Some files failed to copy."),
            (12, MessageType.Warning, "Extra files found in destination. Mismatched files found between source and destination."),
            (20, MessageType.Error, "Fatal error occurred during copy operation. Extra files found in destination."),
            (31, MessageType.Error, "Fatal error occurred during copy operation. Some files failed to copy. Mismatched files found between source and destination. Extra files found in destination. Files copied successfully.")
        };

        foreach (var (exitCode, expectedType, expectedMessage) in testCases)
        {
            var (messageType, message) = ExitCodeInterpreter.Interpret(exitCode);
            messageType.Should().Be(expectedType, $"Exit code {exitCode} should return {expectedType}");
            message.Should().Be(expectedMessage, $"Exit code {exitCode} should return correct message");
        }
    }

    [Theory]
    [InlineData(1, 2, 3)] // 1 + 2 = 3
    [InlineData(1, 4, 5)] // 1 + 4 = 5
    [InlineData(2, 4, 6)] // 2 + 4 = 6
    [InlineData(1, 2, 4, 7)] // 1 + 2 + 4 = 7
    [InlineData(1, 8, 9)] // 1 + 8 = 9
    [InlineData(16, 1, 17)] // 16 + 1 = 17
    public void Interpret_WithBitwiseCombinations_ShouldCombineCorrectMessages(params int[] flags)
    {
        // Arrange
        var exitCode = flags.Aggregate(0, (acc, flag) => acc | flag);

        // Act
        var (messageType, message) = ExitCodeInterpreter.Interpret(exitCode);

        // Assert
        messageType.Should().NotBe(MessageType.Info);
        message.Should().NotBeNullOrEmpty();

        // Verify all expected messages are present
        if (flags.Contains(16))
            messageType.Should().Be(MessageType.Error, "Should be error when fatal error flag is set");

        if (flags.Contains(8) && !flags.Contains(16))
            message.Should().Contain("Some files failed to copy");

        if (flags.Contains(4) && !flags.Contains(16))
            message.Should().Contain("Mismatched files found");

        if (flags.Contains(2) && !flags.Contains(16))
            message.Should().Contain("Extra files found");

        if (flags.Contains(1) && !flags.Contains(16))
            message.Should().Contain("Files copied successfully");
    }

    [Fact]
    public void Interpret_WithAllFlagsSet_ShouldReturnErrorWithAllMessages()
    {
        // Arrange
        var exitCode = 31; // 1 + 2 + 4 + 8 + 16

        // Act
        var (messageType, message) = ExitCodeInterpreter.Interpret(exitCode);

        // Assert
        messageType.Should().Be(MessageType.Error);
        message.Should().Contain("Fatal error occurred during copy operation");
        message.Should().Contain("Some files failed to copy");
        message.Should().Contain("Mismatched files found between source and destination");
        message.Should().Contain("Extra files found in destination");
        message.Should().Contain("Files copied successfully");
    }

    [Fact]
    public void Interpret_WithUnknownExitCode_ShouldReturnErrorMessage()
    {
        // Arrange
        var exitCode = 32; // Unknown exit code

        // Act
        var (messageType, message) = ExitCodeInterpreter.Interpret(exitCode);

        // Assert
        messageType.Should().Be(MessageType.Error);
        message.Should().Be($"Unknown exit code: {exitCode}");
    }

    [Fact]
    public void Interpret_WithNegativeExitCode_ShouldReturnErrorMessage()
    {
        // Arrange
        var exitCode = -1;

        // Act
        var (messageType, message) = ExitCodeInterpreter.Interpret(exitCode);

        // Assert
        messageType.Should().Be(MessageType.Error);
        message.Should().Be($"Unknown exit code: {exitCode}");
    }

    [Fact]
    public void Interpret_WithZeroExitCode_ShouldReturnSpecificMessage()
    {
        // Arrange
        var exitCode = 0;

        // Act
        var (messageType, message) = ExitCodeInterpreter.Interpret(exitCode);

        // Assert
        messageType.Should().Be(MessageType.Success);
        message.Should().Be("Nothing to copy - destination is already up to date.");
        message.Should().NotContain("Files copied successfully");
    }

    [Fact]
    public void Interpret_WithOnlySuccessFlag_ShouldNotIncludeOtherMessages()
    {
        // Arrange
        var exitCode = 1;

        // Act
        var (messageType, message) = ExitCodeInterpreter.Interpret(exitCode);

        // Assert
        messageType.Should().Be(MessageType.Success);
        message.Should().Be("Files copied successfully.");
        message.Should().NotContain("Extra files");
        message.Should().NotContain("Mismatched");
        message.Should().NotContain("failed");
        message.Should().NotContain("Fatal error");
    }
}
