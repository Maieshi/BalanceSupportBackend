using System.Security.Claims;
using Balance_Support.DataClasses.Messages;
using Balance_Support.Scripts.WebSockets.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace Balance_Support.Scripts.WebSockets;
public  class BaseHub : Hub, IMessageSender
{
    private readonly IConnectionManager connectionManager;

    public BaseHub(IConnectionManager connectionManager)
    {
        this.connectionManager = connectionManager;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
        if (userId != null)
        {
            connectionManager.AddConnection<BaseHub>(userId, Context.ConnectionId);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
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
        if (connectionId != null)
        {
            try
            {
                await Clients.Client(connectionId).SendAsync(nameof(message), message);
                return MessageSendResult.Success();
            }
            catch (Exception e)
            {
                return MessageSendResult.SendingError(e.Message);
            }
        }
        return MessageSendResult.UserNotFound("Receiver not found");

    }
}
