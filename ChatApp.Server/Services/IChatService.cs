using ChatApp.Shared.Models;

namespace ChatApp.Server.Services;

public interface IChatService
{
    Task<List<ChatMessage>> GetChatHistoryAsync();
    Task SaveMessageAsync(ChatMessage message);
    Task<ChatMessage> CreateSystemMessageAsync(string content);
}