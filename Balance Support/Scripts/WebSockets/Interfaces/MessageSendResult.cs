namespace Balance_Support.Scripts.WebSockets.Interfaces;

public class MessageSendResult
{
    public readonly bool Success;
    
    public readonly string? ErrorDetails;

    private MessageSendResult(bool success, string? errorDetails = null)
    {
        Success = success;
        ErrorDetails = errorDetails;
    }

    // Factory methods to create success or failure results
    public static MessageSendResult SuccessResult() => new MessageSendResult(true);
    public static MessageSendResult FailureResult(string? errorDetails = null) 
        => new MessageSendResult(false, errorDetails);
}