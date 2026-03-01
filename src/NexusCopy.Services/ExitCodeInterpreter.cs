namespace NexusCopy.Services;

/// <summary>
/// Interprets robocopy exit codes and provides user-friendly messages.
/// </summary>
public static class ExitCodeInterpreter
{
    /// <summary>
    /// Interprets a robocopy exit code and returns a user-friendly message.
    /// </summary>
    /// <param name="exitCode">The robocopy exit code.</param>
    /// <returns>A tuple containing the message type and message text.</returns>
    public static (MessageType Type, string Message) Interpret(int exitCode)
    {
        // Robocopy exit codes are bitwise flags
        // 0 = No files copied, no errors
        // 1 = Files copied successfully
        // 2 = Extra files detected in destination
        // 4 = Mismatched files found
        // 8 = Some files not copied
        // 16 = Fatal error
        
        var messages = new List<(MessageType Type, string Message)>();

        // Check for fatal error first (highest priority)
        if ((exitCode & 16) != 0)
        {
            messages.Add((MessageType.Error, "Fatal error occurred during copy operation."));
        }

        // Check for files not copied
        if ((exitCode & 8) != 0)
        {
            messages.Add((MessageType.Warning, "Some files failed to copy."));
        }

        // Check for mismatched files
        if ((exitCode & 4) != 0)
        {
            messages.Add((MessageType.Warning, "Mismatched files found between source and destination."));
        }

        // Check for extra files
        if ((exitCode & 2) != 0)
        {
            messages.Add((MessageType.Success, "Extra files found in destination."));
        }

        // Check for successful copy
        if ((exitCode & 1) != 0)
        {
            messages.Add((MessageType.Success, "Files copied successfully."));
        }

        // Handle special case: exit code 0 (nothing to copy)
        if (exitCode == 0)
        {
            return (MessageType.Success, "Nothing to copy - destination is already up to date.");
        }

        // If we have any messages, combine them appropriately
        if (messages.Count > 0)
        {
            var hasError = messages.Any(m => m.Type == MessageType.Error);
            var hasWarning = messages.Any(m => m.Type == MessageType.Warning);
            var hasSuccess = messages.Any(m => m.Type == MessageType.Success);

            // Determine overall message type
            MessageType overallType;
            if (hasError) overallType = MessageType.Error;
            else if (hasWarning) overallType = MessageType.Warning;
            else overallType = MessageType.Success;

            // Build combined message
            var messageParts = messages.Select(m => m.Message).ToList();
            var combinedMessage = string.Join(" ", messageParts);

            return (overallType, combinedMessage);
        }

        // Unknown exit code
        return (MessageType.Error, $"Unknown exit code: {exitCode}");
    }
}

/// <summary>
/// Defines the type of message for UI display.
/// </summary>
public enum MessageType
{
    /// <summary>
    /// Information message.
    /// </summary>
    Info,
    
    /// <summary>
    /// Success message.
    /// </summary>
    Success,
    
    /// <summary>
    /// Warning message.
    /// </summary>
    Warning,
    
    /// <summary>
    /// Error message.
    /// </summary>
    Error
}
