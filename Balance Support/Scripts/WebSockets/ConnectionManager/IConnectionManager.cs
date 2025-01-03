using Microsoft.AspNetCore.SignalR;

namespace Balance_Support.Scripts.WebSockets.ConnectionManager;
public interface IConnectionManager
{
    void AddConnection<T>(string userId, string connectionId) where T : Hub;
    void RemoveConnection<T>(string userId) where T : Hub;
    string GetConnectionId<T>(string userId) where T : Hub;

    string Test();
}
