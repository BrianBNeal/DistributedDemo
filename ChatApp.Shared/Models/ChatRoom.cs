namespace ChatApp.Shared.Models;

public record ChatRoom(
    string Id,
    string Name,
    DateTime CreatedAt,
    List<string> ParticipantIds
);