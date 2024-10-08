using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;

namespace Balance_Support.Scripts.WebSockets;
public class ConnectionManager : IConnectionManager
{
    private readonly ConcurrentDictionary<(Type hubType, string userId), string> _connections = new();

    public void AddConnection<T>(string userId, string connectionId) where T : Hub
    {
        var key = (typeof(T), userId);
        _connections[key] = connectionId;
    }

    public void RemoveConnection<T>(string userId) where T : Hub
    {
        var key = (typeof(T), userId);
        _connections.TryRemove(key, out _);
    }

    public string? GetConnectionId<T>( string userId) where T : Hub
    {
        var key = (typeof(T), userId);
        return _connections.GetValueOrDefault(key);
    }
}
