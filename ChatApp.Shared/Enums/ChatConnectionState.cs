namespace ChatApp.Shared.Enums;

/// <summary>
/// Represents the lifecycle state of the client SignalR connection.
/// </summary>
public enum ChatConnectionState
{
    Disconnected = 0,
    Connecting = 1,
    Connected = 2,
    Reconnecting = 3,
    Closing = 4
}
