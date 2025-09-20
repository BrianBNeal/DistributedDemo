namespace ChatApp.Shared.Interfaces;

public interface IChatHub
{
    Task JoinChat(string userName);
    Task SendMessage(string content);
    Task LeaveChat();
    Task GetChatHistory();
}