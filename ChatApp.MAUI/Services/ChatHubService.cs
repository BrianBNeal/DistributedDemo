using ChatApp.MAUI.SignalR;
using ChatApp.Shared.Configuration;
using ChatApp.Shared.Constants;
using ChatApp.Shared.Enums;
using ChatApp.Shared.Models;
using ChatApp.Shared.DTOs;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace ChatApp.MAUI.Services;

public class ChatHubService : IChatHubService
{
    private readonly ChatClientOptions _options;
    private readonly ILogger<ChatHubService>? _logger;
    private HubConnection? _connection;
    private ChatHubProxy? _proxy;

    public event Action<ChatMessage>? MessageReceived;
    public event Action<User>? UserJoined;
    public event Action<string>? UserLeft;
    public event Action<ChatHistoryResponse>? HistoryLoaded;
    public event Action<string>? ConnectionError;
    public event Action<ChatConnectionState>? ConnectionStateChanged;

    public ChatConnectionState State { get; private set; } = ChatConnectionState.Disconnected;
    public bool IsConnected => State == ChatConnectionState.Connected;

    public ChatHubService(IOptions<ChatClientOptions> options, ILogger<ChatHubService>? logger = null)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<bool> ConnectAsync(string userName, CancellationToken ct = default)
    {
        if (IsConnected) return true;
        SetState(ChatConnectionState.Connecting);

        var hubUrl = _options.GetHubUrl();
        _connection = new HubConnectionBuilder()
            .WithUrl(hubUrl, cfg =>
            {
                cfg.HttpMessageHandlerFactory = handler =>
                {
#if ANDROID || IOS
                    if (handler is HttpClientHandler httpClientHandler)
                    {
                        httpClientHandler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true; // dev only
                    }
#endif
                    return handler!;
                };
            })
            .WithAutomaticReconnect()
            .Build();

        _proxy = new ChatHubProxy(_connection);
        RegisterHandlers();
        RegisterConnectionLifecycle();

        try
        {
            await _connection.StartAsync(ct);
            await _proxy.JoinChat(userName); // server will trigger history send; no need to call GetChatHistory explicitly
            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to connect to hub at {HubUrl}", hubUrl);
            ConnectionError?.Invoke(ex.Message);
            SetState(ChatConnectionState.Disconnected);
            return false;
        }
    }

    private void RegisterHandlers()
    {
        if (_connection == null) return;
        _connection.On<ChatMessage>(HubMethods.Client.ReceiveMessage, msg => Dispatch(() => MessageReceived?.Invoke(msg)));
        _connection.On<User>(HubMethods.Client.UserJoined, user => Dispatch(() => UserJoined?.Invoke(user)));
        _connection.On<string>(HubMethods.Client.UserLeft, name => Dispatch(() => UserLeft?.Invoke(name)));
        _connection.On<ChatHistoryResponse>(HubMethods.Client.ChatHistoryLoaded, history => Dispatch(() => HistoryLoaded?.Invoke(history)));
        _connection.On<string>(HubMethods.Client.ConnectionError, err => Dispatch(() => ConnectionError?.Invoke(err)));
    }

    private void RegisterConnectionLifecycle()
    {
        if (_connection == null) return;
        _connection.Reconnecting += ex =>
        {
            _logger?.LogWarning(ex, "Reconnecting to chat hub...");
            SetState(ChatConnectionState.Reconnecting);
            return Task.CompletedTask;
        };
        _connection.Reconnected += id =>
        {
            _logger?.LogInformation("Reconnected. New connection id {Id}", id);
            SetState(ChatConnectionState.Connected);
            return Task.CompletedTask;
        };
        _connection.Closed += ex =>
        {
            _logger?.LogWarning(ex, "Connection closed");
            SetState(ChatConnectionState.Disconnected);
            return Task.CompletedTask;
        };
    }

    public async Task SendMessageAsync(string content, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(content) || content.Length > Shared.Constants.ChatConstants.MaxMessageLength)
            return;
        if (!IsConnected || _proxy == null) return;
        try
        {
            await _proxy.SendMessage(content.Trim());
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error sending message");
            ConnectionError?.Invoke("Failed to send message");
        }
    }

    public async Task DisconnectAsync(CancellationToken ct = default)
    {
        if (_connection != null && _proxy != null)
        {
            try
            {
                SetState(ChatConnectionState.Closing);
                await _proxy.LeaveChat();
                await _connection.StopAsync(ct);
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Error during disconnect");
            }
            finally
            {
                await _connection.DisposeAsync();
                _connection = null;
                _proxy = null;
                SetState(ChatConnectionState.Disconnected);
            }
        }
    }

    private void SetState(ChatConnectionState newState)
    {
        State = newState;
        Dispatch(() => ConnectionStateChanged?.Invoke(State));
    }

    private static void Dispatch(Action action)
    {
        if (MainThread.IsMainThread) action();
        else MainThread.BeginInvokeOnMainThread(action);
    }

    public async ValueTask DisposeAsync()
    {
        await DisconnectAsync();
    }
}
