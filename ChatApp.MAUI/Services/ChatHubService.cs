using ChatApp.Shared.Models;
using ChatApp.Shared.DTOs;
using Microsoft.AspNetCore.SignalR.Client;

namespace ChatApp.MAUI.Services;

public class ChatHubService : IChatHubService
{
    private HubConnection? _connection;
    private string? _userName;

    public event Action<ChatMessage>? MessageReceived;
    public event Action<User>? UserJoined;
    public event Action<string>? UserLeft;
    public event Action<ChatHistoryResponse>? HistoryLoaded;
    public event Action<string>? ConnectionError;

    public bool IsConnected => _connection?.State == HubConnectionState.Connected;

    private static string HubUrl => "https://localhost:7000/chathub"; // TODO: Use Aspire discovery

    public async Task ConnectAsync(string userName, CancellationToken ct = default)
    {
        if (IsConnected) return;
        _userName = userName;

        _connection = new HubConnectionBuilder()
            .WithUrl(HubUrl, options =>
            {
                options.HttpMessageHandlerFactory = handler =>
                {
#if ANDROID || IOS
                    if (handler is HttpClientHandler httpClientHandler)
                    {
                        httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true; // dev only
                    }
#endif
                    return handler!;
                };
            })
            .WithAutomaticReconnect()
            .Build();

        RegisterHandlers();

        try
        {
            await _connection.StartAsync(ct);
            await _connection.InvokeAsync("JoinChat", userName, ct);
            await _connection.InvokeAsync("GetChatHistory", cancellationToken: ct);
        }
        catch (Exception ex)
        {
            ConnectionError?.Invoke(ex.Message);
            throw;
        }
    }

    private void RegisterHandlers()
    {
        if (_connection == null) return;
        _connection.On<ChatMessage>("ReceiveMessage", msg => MainThread.BeginInvokeOnMainThread(() => MessageReceived?.Invoke(msg)));
        _connection.On<User>("UserJoined", user => MainThread.BeginInvokeOnMainThread(() => UserJoined?.Invoke(user)));
        _connection.On<string>("UserLeft", name => MainThread.BeginInvokeOnMainThread(() => UserLeft?.Invoke(name)));
        _connection.On<ChatHistoryResponse>("ChatHistoryLoaded", history => MainThread.BeginInvokeOnMainThread(() => HistoryLoaded?.Invoke(history)));
        _connection.On<string>("ConnectionError", err => MainThread.BeginInvokeOnMainThread(() => ConnectionError?.Invoke(err)));
    }

    public async Task SendMessageAsync(string content, CancellationToken ct = default)
    {
        if (!IsConnected || _connection == null) return;
        await _connection.InvokeAsync("SendMessage", content, ct);
    }

    public async Task DisconnectAsync(CancellationToken ct = default)
    {
        if (_connection != null)
        {
            try
            {
                await _connection.InvokeAsync("LeaveChat", cancellationToken: ct);
                await _connection.StopAsync(ct);
            }
            catch { }
            finally
            {
                await _connection.DisposeAsync();
                _connection = null;
            }
        }
    }
}
