using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ChatApp.MAUI.Services;
using ChatApp.Shared.Models;
using ChatApp.Shared.DTOs;

namespace ChatApp.MAUI.ViewModels;

public partial class ChatViewModel : ObservableObject
{
    private readonly IChatHubService _chatHubService;

    [ObservableProperty]
    private ObservableCollection<ChatMessage> messages = new();

    [ObservableProperty]
    private ObservableCollection<User> onlineUsers = new();

    [ObservableProperty]
    private string currentMessage = string.Empty;

    [ObservableProperty]
    private bool isConnected;

    [ObservableProperty]
    private string connectionStatus = "Disconnected";

    public ChatViewModel(IChatHubService chatHubService)
    {
        _chatHubService = chatHubService;
        _chatHubService.MessageReceived += OnMessageReceived;
        _chatHubService.UserJoined += OnUserJoined;
        _chatHubService.UserLeft += OnUserLeft;
        _chatHubService.HistoryLoaded += OnHistoryLoaded;
        _chatHubService.ConnectionError += err => ConnectionStatus = $"Error: {err}";
        IsConnected = _chatHubService.IsConnected;
        ConnectionStatus = IsConnected ? "Connected" : "Disconnected";
    }

    private void OnMessageReceived(ChatMessage message)
    {
        Messages.Add(message);
    }

    private void OnUserJoined(User user)
    {
        if (!OnlineUsers.Any(u => u.ConnectionId == user.ConnectionId))
            OnlineUsers.Add(user);
        Messages.Add(new ChatMessage(Guid.NewGuid().ToString(), user.Name, $"{user.Name} joined", DateTime.UtcNow, MessageType.Join));
    }

    private void OnUserLeft(string userName)
    {
        var existing = OnlineUsers.FirstOrDefault(u => u.Name == userName);
        if (existing != null)
            OnlineUsers.Remove(existing);
        Messages.Add(new ChatMessage(Guid.NewGuid().ToString(), userName, $"{userName} left", DateTime.UtcNow, MessageType.Leave));
    }

    private void OnHistoryLoaded(ChatHistoryResponse history)
    {
        Messages.Clear();
        foreach (var m in history.Messages.OrderBy(m => m.Timestamp))
            Messages.Add(m);
        OnlineUsers.Clear();
        foreach (var u in history.OnlineUsers)
            OnlineUsers.Add(u);
        ConnectionStatus = "Connected";
        IsConnected = true;
    }

    [RelayCommand]
    private async Task SendMessageAsync()
    {
        if (string.IsNullOrWhiteSpace(CurrentMessage)) return;
        await _chatHubService.SendMessageAsync(CurrentMessage.Trim());
        CurrentMessage = string.Empty;
    }

    [RelayCommand]
    private async Task DisconnectAsync()
    {
        await _chatHubService.DisconnectAsync();
        IsConnected = false;
        ConnectionStatus = "Disconnected";
    }
}
