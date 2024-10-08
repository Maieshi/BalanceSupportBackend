using Balance_Support.DataClasses.Messages;

namespace Balance_Support.Scripts.WebSockets.Interfaces;

public interface IMessageSender
{
    Task<MessageSendResult> SendMessage(string userId, IMessage message);
}