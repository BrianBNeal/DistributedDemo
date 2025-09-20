using ChatApp.Shared.DTOs;
using ChatApp.Shared.Enums;
using ChatApp.Shared.Models;

namespace ChatApp.MAUI.Services;

/// <summary>
/// Abstraction over the SignalR chat hub connection for the MAUI client.
/// Provides events for inbound messages and connection lifecycle, plus methods to send/receive.
/// </summary>
public interface IChatHubService : IAsyncDisposable
{
    event Action<ChatMessage>? MessageReceived;
    event Action<User>? UserJoined;
    event Action<string>? UserLeft;
    event Action<ChatHistoryResponse>? HistoryLoaded;
    event Action<string>? ConnectionError;
    event Action<ChatConnectionState>? ConnectionStateChanged;

    ChatConnectionState State { get; }
    bool IsConnected { get; }

    Task<bool> ConnectAsync(string userName, CancellationToken ct = default);
    Task SendMessageAsync(string content, CancellationToken ct = default);
    Task DisconnectAsync(CancellationToken ct = default);
}
