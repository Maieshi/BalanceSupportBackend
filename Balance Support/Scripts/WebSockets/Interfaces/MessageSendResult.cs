namespace Balance_Support.Scripts.WebSockets.Interfaces;

public class MessageSendResult
{
    public  bool IsSuccess {get; }

    public MessageStatus MessageStatus{get; set; }
    
    public  string? StatusDetails{get; set; }

    private MessageSendResult(bool isSuccess, string? statusDetails = null)
    {
        IsSuccess = isSuccess;
        StatusDetails = statusDetails;
    }

    
    private MessageSendResult(bool isSuccess,MessageStatus messageStatus ,string? statusDetails = null)
    {
        IsSuccess = isSuccess;
        StatusDetails = statusDetails;
        MessageStatus = messageStatus;
    }
    // Factory methods to create success or failure results
    public static MessageSendResult Success(string? message = null) => new MessageSendResult(true,MessageStatus.Success,message);
    
    // public static MessageSendResult Success() => new MessageSendResult(true);
    public static MessageSendResult SendingError(string? errorDetails = null) 
        => new MessageSendResult(false, MessageStatus.SendingError,errorDetails);
    public static MessageSendResult UserNotFound(string? errorDetails = null) 
        => new MessageSendResult(false, MessageStatus.UserNotFound,errorDetails);
}

public enum MessageStatus
{
    Success,
    UserNotFound,
    SendingError
}