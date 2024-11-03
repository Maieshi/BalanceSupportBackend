using System.Security.Claims;
using Balance_Support.DataClasses.Messages;
using Balance_Support.Scripts.WebSockets.ConnectionManager;
using Balance_Support.Scripts.WebSockets.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace Balance_Support.Scripts.WebSockets;

public class BaseHub : Hub, IMessageSender
{
    private readonly IConnectionManager connectionManager;
    private readonly IHubContext<BaseHub> hubContext;

    public BaseHub(IConnectionManager connectionManager, IHubContext<BaseHub> hubContext)
    {
        this.connectionManager = connectionManager;
        this.hubContext = hubContext;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        if (userId != null)
        {
            connectionManager.AddConnection<BaseHub>(userId, Context.ConnectionId);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        if (userId != null)
        {
            connectionManager.RemoveConnection<BaseHub>(userId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    protected string? GetConnection(string UserId)
    {
        return connectionManager.GetConnectionId<BaseHub>(UserId);
    }


    public async Task<MessageSendResult> SendMessage(string userId, IMessage message)
    {
        var connectionId = GetConnection(userId);
        var messageTypeName = message.GetType().Name;
        if (connectionId == null)
        {
            return MessageSendResult.UserNotFound(
                $"Receiver not found: Connection ID is null for UserID: {userId} and Type: {messageTypeName}");
        }

        try
        {
            var client = hubContext.Clients.Client(connectionId);

            await client.SendAsync(messageTypeName, message);
            return MessageSendResult.Success(
                $"Message successfully sent. UserID: {userId}.  Connection ID: {connectionId}. MessageType: {messageTypeName}");
        }
        catch (Exception e)
        {
            return MessageSendResult.SendingError(
                $"Error sending message. UserID: {userId}  Connection ID: {connectionId}. MessageType: {messageTypeName}. Error message:{e.Message}. Stack trace: {e.StackTrace}");
        }
    }
}