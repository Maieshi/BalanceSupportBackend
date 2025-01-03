using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Balance_Support.Scripts.WebSockets.ConnectionManager
{
    public class ConnectionManager : IConnectionManager
    {
        private readonly ConcurrentDictionary<(Type hubType, string userId), string> _connections = new();

        public void AddConnection<T>(string userId, string connectionId) where T : Hub
        {
            var key = (typeof(T), userId);
            _connections[key] = connectionId;
            Log.Information("Added connection for user {UserId} in hub {HubType} with connection ID {ConnectionId}.",
                userId, typeof(T).Name, connectionId);
        }

        public void RemoveConnection<T>(string userId) where T : Hub
        {
            var key = (typeof(T), userId);
            if (_connections.TryRemove(key, out var connectionId))
            {
                Log.Information(
                    "Removed connection for user {UserId} in hub {HubType} with connection ID {ConnectionId}.", userId,
                    typeof(T).Name, connectionId);
            }
            else
            {
                Log.Information("No connection found for user {UserId} in hub {HubType} to remove.", userId,
                    typeof(T).Name);
            }
        }

        public string? GetConnectionId<T>(string userId) where T : Hub
        {
            var key = (typeof(T), userId);
            Log.Information($"Connection for user {userId} in hub {typeof(T)}");

            return _connections.GetValueOrDefault(key);
        }

        public string Test()
        {
            string str = "//";
            foreach (var connection in _connections)
            {
                str +=
                    $"User:{connection.Key.userId} Hub:{connection.Key.hubType} Connection ID:{connection.Value}/r/n";
            }

            Log.Information(str);
            return str;
        }
    }
}