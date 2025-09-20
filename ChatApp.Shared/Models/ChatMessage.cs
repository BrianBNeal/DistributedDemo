namespace ChatApp.Shared.Models;

public record ChatMessage(
    string Id,
    string UserName,
    string Content,
    DateTime Timestamp,
    MessageType Type = MessageType.User
);

public enum MessageType
{
    User,
    System,
    Join,
    Leave
}