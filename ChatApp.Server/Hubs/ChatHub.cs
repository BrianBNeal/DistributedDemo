using ChatApp.Shared.Interfaces;
using ChatApp.Shared.Models;
using ChatApp.Shared.DTOs;
using ChatApp.Shared.Constants;
using ChatApp.Server.Services;
using Microsoft.AspNetCore.SignalR;

namespace ChatApp.Server.Hubs;

public class ChatHub : Hub<IChatClient>, IChatHub
{
    private readonly IChatService _chatService;
    private readonly IUserService _userService;
    private readonly ILogger<ChatHub> _logger;

    public ChatHub(IChatService chatService, IUserService userService, ILogger<ChatHub> logger)
    {
        _chatService = chatService;
        _userService = userService;
        _logger = logger;
    }

    public async Task JoinChat(string userName)
    {
        try
        {
            // Validate username
            if (string.IsNullOrWhiteSpace(userName) || userName.Length > ChatConstants.MaxUsernameLength)
            {
                await Clients.Caller.ConnectionError(ChatConstants.InvalidUsernameError);
                return;
            }

            // Check if username is already taken
            if (await _userService.IsUserNameTakenAsync(userName))
            {
                await Clients.Caller.ConnectionError(ChatConstants.DuplicateUsernameError);
                return;
            }

            // Create user object
            var user = new User(
                ConnectionId: Context.ConnectionId,
                Name: userName,
                JoinedAt: DateTime.UtcNow,
                IsOnline: true
            );

            // Add user to service
            await _userService.AddUserAsync(user);

            // Create and save join message
            var joinMessage = string.Format(ChatConstants.UserJoinedMessageTemplate, userName);
            var systemMessage = await _chatService.CreateSystemMessageAsync(joinMessage);

            // Notify all clients about new user
            await Clients.All.UserJoined(user);
            await Clients.All.ReceiveMessage(systemMessage);

            // Send chat history to the joining user
            await GetChatHistory();

            _logger.LogInformation("User {UserName} joined chat with connection {ConnectionId}", userName, Context.ConnectionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user join for {UserName}", userName);
            await Clients.Caller.ConnectionError("Failed to join chat. Please try again.");
        }
    }

    public async Task SendMessage(string content)
    {
        try
        {
            // Validate message content
            if (string.IsNullOrWhiteSpace(content) || content.Length > ChatConstants.MaxMessageLength)
            {
                await Clients.Caller.ConnectionError(ChatConstants.InvalidMessageError);
                return;
            }

            // Get user info
            var user = await _userService.GetUserByConnectionIdAsync(Context.ConnectionId);
            if (user == null)
            {
                await Clients.Caller.ConnectionError("User not found. Please rejoin the chat.");
                return;
            }

            // Create chat message
            var message = new ChatMessage(
                Id: Guid.NewGuid().ToString(),
                UserName: user.Name,
                Content: content.Trim(),
                Timestamp: DateTime.UtcNow,
                Type: MessageType.User
            );

            // Save message
            await _chatService.SaveMessageAsync(message);

            // Broadcast to all clients
            await Clients.All.ReceiveMessage(message);

            _logger.LogDebug("Message sent by {UserName}: {Content}", user.Name, content);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message from connection {ConnectionId}", Context.ConnectionId);
            await Clients.Caller.ConnectionError("Failed to send message. Please try again.");
        }
    }

    public async Task LeaveChat()
    {
        try
        {
            await HandleUserDisconnection(Context.ConnectionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user leave for connection {ConnectionId}", Context.ConnectionId);
        }
    }

    public async Task GetChatHistory()
    {
        try
        {
            var messages = await _chatService.GetChatHistoryAsync();
            var onlineUsers = await _userService.GetOnlineUsersAsync();

            var response = new ChatHistoryResponse(messages, onlineUsers);
            await Clients.Caller.ChatHistoryLoaded(response);

            _logger.LogDebug("Chat history sent to connection {ConnectionId}", Context.ConnectionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving chat history for connection {ConnectionId}", Context.ConnectionId);
            await Clients.Caller.ConnectionError("Failed to load chat history.");
        }
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("New connection established: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        try
        {
            await HandleUserDisconnection(Context.ConnectionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling disconnection for {ConnectionId}", Context.ConnectionId);
        }

        if (exception != null)
        {
            _logger.LogWarning(exception, "Connection {ConnectionId} disconnected with error", Context.ConnectionId);
        }
        else
        {
            _logger.LogInformation("Connection {ConnectionId} disconnected", Context.ConnectionId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    private async Task HandleUserDisconnection(string connectionId)
    {
        // Get user before removing
        var user = await _userService.GetUserByConnectionIdAsync(connectionId);
        if (user != null)
        {
            // Remove user from service
            await _userService.RemoveUserAsync(connectionId);

            // Create and save leave message
            var leaveMessage = string.Format(ChatConstants.UserLeftMessageTemplate, user.Name);
            var systemMessage = await _chatService.CreateSystemMessageAsync(leaveMessage);

            // Notify other clients
            await Clients.Others.UserLeft(user.Name);
            await Clients.Others.ReceiveMessage(systemMessage);

            _logger.LogInformation("User {UserName} left chat", user.Name);
        }
    }
}