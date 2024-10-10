namespace Balance_Support.Scripts.WebSockets.Interfaces;

public class MessageSendResult
{
    public readonly bool IsSuccess;

    public ErrorType ErrorType;
    
    public readonly string? ErrorDetails;

    private MessageSendResult(bool isSuccess, string? errorDetails = null)
    {
        IsSuccess = isSuccess;
        ErrorDetails = errorDetails;
    }

    
    private MessageSendResult(bool isSuccess,ErrorType errorType ,string? errorDetails = null)
    {
        IsSuccess = isSuccess;
        ErrorDetails = errorDetails;
        ErrorType = errorType;
    }
    // Factory methods to create success or failure results
    public static MessageSendResult Success() => new MessageSendResult(true);
    public static MessageSendResult SendingError(string? errorDetails = null) 
        => new MessageSendResult(false, ErrorType.SendingError,errorDetails);
    public static MessageSendResult UserNotFound(string? errorDetails = null) 
        => new MessageSendResult(false, ErrorType.UserNotFound,errorDetails);
}

public enum ErrorType
{
    UserNotFound,
    SendingError
}