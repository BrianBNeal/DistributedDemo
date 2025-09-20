namespace ChatApp.Shared.Constants;

public static class ChatConstants
{
    // Redis Keys
    public const string ChatMessagesKey = "chat:messages";
    public const string OnlineUsersKey = "chat:users:online";
    public const string UserDetailsKeyPrefix = "chat:user:";
    
    // SignalR Hub Names
    public const string ChatHubPath = "/chathub";
    
    // Message Limits
    public const int MaxUsernameLength = 50;
    public const int MaxMessageLength = 1000;
    public const int MaxMessagesInHistory = 100;
    
    // System Messages
    public const string SystemUserName = "System";
    public const string UserJoinedMessageTemplate = "{0} joined the chat";
    public const string UserLeftMessageTemplate = "{0} left the chat";
    
    // Error Messages
    public const string InvalidUsernameError = "Username must be between 1 and 50 characters";
    public const string InvalidMessageError = "Message cannot be empty or exceed 1000 characters";
    public const string ConnectionFailedError = "Failed to connect to chat server";
    public const string DuplicateUsernameError = "Username is already taken";
}