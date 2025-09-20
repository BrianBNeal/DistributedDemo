namespace ChatApp.Shared.Models;

public record User(
    string ConnectionId,
    string Name,
    DateTime JoinedAt,
    bool IsOnline = true
);