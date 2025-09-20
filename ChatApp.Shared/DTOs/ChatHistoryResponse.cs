using ChatApp.Shared.Models;

namespace ChatApp.Shared.DTOs;

public record ChatHistoryResponse(
    List<ChatMessage> Messages,
    List<User> OnlineUsers
);