using ChatApp.Shared.Models;
using ChatApp.Shared.DTOs;

namespace ChatApp.MAUI.Services;

public interface IChatHubService
{
    event Action<ChatMessage>? MessageReceived;
    event Action<User>? UserJoined;
    event Action<string>? UserLeft;
    event Action<ChatHistoryResponse>? HistoryLoaded;
    event Action<string>? ConnectionError;
    bool IsConnected { get; }
    Task ConnectAsync(string userName, CancellationToken ct = default);
    Task SendMessageAsync(string content, CancellationToken ct = default);
    Task DisconnectAsync(CancellationToken ct = default);
}
