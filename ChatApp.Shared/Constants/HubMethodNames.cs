namespace ChatApp.Shared.Constants;

/// <summary>
/// Centralized hub method name constants (server + client callback) to avoid magic strings.
/// Grouped for clarity. Use these instead of raw literals when invoking or registering handlers.
/// </summary>
public static class HubMethods
{
    public static class Server
    {
        public const string JoinChat = nameof(Interfaces.IChatHub.JoinChat);
        public const string SendMessage = nameof(Interfaces.IChatHub.SendMessage);
        public const string LeaveChat = nameof(Interfaces.IChatHub.LeaveChat);
        public const string GetChatHistory = nameof(Interfaces.IChatHub.GetChatHistory);
    }

    public static class Client
    {
        public const string ReceiveMessage = nameof(Interfaces.IChatClient.ReceiveMessage);
        public const string UserJoined = nameof(Interfaces.IChatClient.UserJoined);
        public const string UserLeft = nameof(Interfaces.IChatClient.UserLeft);
        public const string ChatHistoryLoaded = nameof(Interfaces.IChatClient.ChatHistoryLoaded);
        public const string ConnectionError = nameof(Interfaces.IChatClient.ConnectionError);
    }
}
