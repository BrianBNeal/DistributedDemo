using ChatApp.Shared.Models;
using ChatApp.Shared.Constants;
using StackExchange.Redis;
using System.Text.Json;

namespace ChatApp.Server.Services;

public class ChatService : IChatService
{
    private readonly IDatabase _database;
    private readonly ILogger<ChatService> _logger;

    public ChatService(IConnectionMultiplexer redis, ILogger<ChatService> logger)
    {
        _database = redis.GetDatabase();
        _logger = logger;
    }

    public async Task<List<ChatMessage>> GetChatHistoryAsync()
    {
        try
        {
            var messages = await _database.ListRangeAsync(
                ChatConstants.ChatMessagesKey, 
                0, 
                ChatConstants.MaxMessagesInHistory - 1);

            var chatMessages = new List<ChatMessage>();
            foreach (var message in messages)
            {
                if (message.HasValue)
                {
                    var chatMessage = JsonSerializer.Deserialize<ChatMessage>(message.ToString());
                    if (chatMessage != null)
                    {
                        chatMessages.Add(chatMessage);
                    }
                }
            }

            // Redis LRANGE returns in order, but we want oldest first
            chatMessages.Reverse();
            
            _logger.LogInformation("Retrieved {Count} messages from chat history", chatMessages.Count);
            return chatMessages;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving chat history");
            return new List<ChatMessage>();
        }
    }

    public async Task SaveMessageAsync(ChatMessage message)
    {
        try
        {
            var serializedMessage = JsonSerializer.Serialize(message);
            
            // Add to front of list (newest first in Redis)
            await _database.ListLeftPushAsync(ChatConstants.ChatMessagesKey, serializedMessage);
            
            // Trim list to maintain maximum size
            await _database.ListTrimAsync(
                ChatConstants.ChatMessagesKey, 
                0, 
                ChatConstants.MaxMessagesInHistory - 1);

            _logger.LogDebug("Saved message from {UserName}: {Content}", message.UserName, message.Content);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving message from {UserName}", message.UserName);
            throw;
        }
    }

    public async Task<ChatMessage> CreateSystemMessageAsync(string content)
    {
        var systemMessage = new ChatMessage(
            Id: Guid.NewGuid().ToString(),
            UserName: ChatConstants.SystemUserName,
            Content: content,
            Timestamp: DateTime.UtcNow,
            Type: MessageType.System
        );

        await SaveMessageAsync(systemMessage);
        return systemMessage;
    }
}