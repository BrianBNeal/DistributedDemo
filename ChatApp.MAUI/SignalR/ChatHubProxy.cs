using ChatApp.Shared.Constants;
using ChatApp.Shared.Interfaces;
using Microsoft.AspNetCore.SignalR.Client;

namespace ChatApp.MAUI.SignalR;

/// <summary>
/// Strongly typed proxy for invoking server hub methods implementing <see cref="IChatHub"/>.
/// </summary>
public class ChatHubProxy : IChatHub
{
    private readonly HubConnection _connection;

    public ChatHubProxy(HubConnection connection) => _connection = connection;

    public Task JoinChat(string userName) => _connection.InvokeAsync(HubMethods.Server.JoinChat, userName);
    public Task SendMessage(string content) => _connection.InvokeAsync(HubMethods.Server.SendMessage, content);
    public Task LeaveChat() => _connection.InvokeAsync(HubMethods.Server.LeaveChat);
    public Task GetChatHistory() => _connection.InvokeAsync(HubMethods.Server.GetChatHistory);
}
