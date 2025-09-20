using ChatApp.Shared.Models;
using ChatApp.Shared.DTOs;

namespace ChatApp.Shared.Interfaces;

public interface IChatClient
{
    Task ReceiveMessage(ChatMessage message);
    Task UserJoined(User user);
    Task UserLeft(string userName);
    Task ChatHistoryLoaded(ChatHistoryResponse history);
    Task ConnectionError(string error);
}