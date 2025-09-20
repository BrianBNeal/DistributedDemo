# Real-Time Chat App Implementation Plan

## Overview
Building a cross-platform real-time chat application using .NET 10, MAUI, SignalR, Redis, and Aspire for demonstration purposes.

### Demo Scenario
- **PC Client**: MAUI Windows app
- **Mobile Client**: MAUI mobile app (Android/iOS)
- **Server**: ASP.NET Core with SignalR Hub
- **Storage**: Redis for chat history and SignalR backplane
- **Orchestration**: Aspire for service management

## Architecture

```
DistributedDemo/
??? DistributedDemo.AppHost/              ? (Existing - Aspire orchestration)
??? DistributedDemo.ServiceDefaults/      ? (Existing - Shared Aspire config)
??? ChatApp.Server/                       ?? (ASP.NET Core Web API + SignalR)
??? ChatApp.MAUI/                         ?? (Cross-platform MAUI client)
??? ChatApp.Shared/                       ?? (Shared models/DTOs)
??? ChatApp.Tests/                        ?? (Unit tests - optional)
```

## Phase 1: Core Real-Time Chat

### 1. Project Setup Tasks

#### ChatApp.Shared (.NET 10 Class Library)
**Purpose**: Shared data models and interfaces across all projects

**Files to create**:
- `Models/ChatMessage.cs` - Core message model
- `Models/User.cs` - User representation
- `Models/ChatRoom.cs` - Chat room model (future expansion)
- `DTOs/JoinChatRequest.cs` - Request for joining chat
- `DTOs/SendMessageRequest.cs` - Request for sending message
- `DTOs/ChatHistoryResponse.cs` - Response with chat history
- `Interfaces/IChatHub.cs` - SignalR hub contract
- `Interfaces/IChatClient.cs` - SignalR client contract
- `Constants/ChatConstants.cs` - Application constants

#### ChatApp.Server (ASP.NET Core Web API)
**Purpose**: Backend API and SignalR hub for real-time communication

**Key Components**:
- `Hubs/ChatHub.cs` - SignalR hub implementation
- `Services/ChatService.cs` - Business logic for chat operations
- `Services/UserService.cs` - User management
- `Controllers/ChatController.cs` - REST API endpoints
- `Program.cs` - Application configuration
- `appsettings.json` - Configuration settings

**Dependencies**:
- Microsoft.AspNetCore.SignalR
- StackExchange.Redis
- Aspire.StackExchange.Redis

#### ChatApp.MAUI (.NET 10 Multi-platform App)
**Purpose**: Cross-platform client application

**Project Structure**:
- `Views/LoadingPage.xaml` - Name entry screen
- `Views/ChatPage.xaml` - Main chat interface
- `ViewModels/LoadingViewModel.cs` - Loading screen logic
- `ViewModels/ChatViewModel.cs` - Chat screen logic
- `Services/ChatHubService.cs` - SignalR client wrapper
- `Services/INavigationService.cs` - Navigation abstraction
- `Platforms/` - Platform-specific configurations

**Dependencies**:
- Microsoft.AspNetCore.SignalR.Client
- CommunityToolkit.Mvvm
- Microsoft.Extensions.Logging

### 2. Data Models

```csharp
// ChatApp.Shared/Models/ChatMessage.cs
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

// ChatApp.Shared/Models/User.cs
public record User(
    string ConnectionId,
    string Name,
    DateTime JoinedAt,
    bool IsOnline = true
);

// ChatApp.Shared/DTOs/JoinChatRequest.cs
public record JoinChatRequest(string UserName);

// ChatApp.Shared/DTOs/SendMessageRequest.cs
public record SendMessageRequest(string Content);

// ChatApp.Shared/DTOs/ChatHistoryResponse.cs
public record ChatHistoryResponse(
    List<ChatMessage> Messages,
    List<User> OnlineUsers
);
```

### 3. SignalR Hub Contract

```csharp
// ChatApp.Shared/Interfaces/IChatClient.cs
public interface IChatClient
{
    Task ReceiveMessage(ChatMessage message);
    Task UserJoined(User user);
    Task UserLeft(string userName);
    Task ChatHistoryLoaded(ChatHistoryResponse history);
    Task ConnectionError(string error);
}

// ChatApp.Shared/Interfaces/IChatHub.cs
public interface IChatHub
{
    Task JoinChat(string userName);
    Task SendMessage(string content);
    Task LeaveChat();
    Task GetChatHistory();
}
```

### 4. Server Implementation Details

#### ChatHub.cs
```csharp
public class ChatHub : Hub<IChatClient>, IChatHub
{
    private readonly IChatService _chatService;
    private readonly IUserService _userService;

    // Key methods to implement:
    // - JoinChat(string userName)
    // - SendMessage(string content)  
    // - LeaveChat()
    // - GetChatHistory()
    // - OnConnectedAsync()
    // - OnDisconnectedAsync(Exception exception)
}
```

#### ChatService.cs
```csharp
public class ChatService : IChatService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _database;

    // Key methods to implement:
    // - Task<List<ChatMessage>> GetChatHistoryAsync()
    // - Task SaveMessageAsync(ChatMessage message)
    // - Task<List<User>> GetOnlineUsersAsync()
    // - Task AddUserAsync(User user)
    // - Task RemoveUserAsync(string connectionId)
}
```

### 5. MAUI Client Implementation

#### ChatViewModel.cs (Primary client logic)
```csharp
public partial class ChatViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<ChatMessage> messages = new();
    
    [ObservableProperty]
    private ObservableCollection<User> onlineUsers = new();
    
    [ObservableProperty]
    private string currentMessage = string.Empty;
    
    [ObservableProperty]
    private bool isConnected = false;

    // Key methods to implement:
    // - Task ConnectAsync(string userName)
    // - Task DisconnectAsync()
    // - [RelayCommand] Task SendMessageAsync()
    // - Task HandleIncomingMessage(ChatMessage message)
    // - Task HandleUserJoined(User user)
    // - Task HandleUserLeft(string userName)
}
```

### 6. Redis Configuration

#### Data Structure:
- **Chat Messages**: `chat:messages` (Redis List)
- **Online Users**: `chat:users:online` (Redis Set)
- **User Details**: `chat:user:{connectionId}` (Redis Hash)
- **SignalR Backplane**: Automatic via Aspire integration

#### Sample Redis Operations:
```csharp
// Store message
await _database.ListLeftPushAsync("chat:messages", JsonSerializer.Serialize(message));

// Get recent messages (last 50)
var messages = await _database.ListRangeAsync("chat:messages", 0, 49);

// Track online user
await _database.SetAddAsync("chat:users:online", userName);
```

### 7. Aspire Integration

#### AppHost.cs Updates:
```csharp
var builder = DistributedApplication.CreateBuilder(args);

// Add Redis for chat storage and SignalR backplane
var redis = builder.AddRedis("redis")
    .WithRedisInsight(); // Optional: Redis management UI

// Add chat server with Redis dependency
var chatServer = builder.AddProject<Projects.ChatApp_Server>("chatserver")
    .WithReference(redis)
    .WithExternalHttpEndpoints();

// Optional: Add health checks dashboard
builder.AddProject<Projects.ChatApp_HealthDashboard>("healthdashboard")
    .WithReference(chatServer);

builder.Build().Run();
```

## Phase 2: Push Notifications

### Platform-Specific Notification Services

#### Interface Definition:
```csharp
public interface INotificationService
{
    Task InitializeAsync();
    Task<bool> RequestPermissionAsync();
    Task ShowNotificationAsync(string title, string message);
    Task RegisterForBackgroundUpdatesAsync();
}
```

#### Implementation Strategy:
- **Android**: Firebase Cloud Messaging + Local Notifications
- **iOS**: Apple Push Notification Service + User Notifications
- **Windows**: Windows Notification Service + Toast Notifications

### Background Service for Message Monitoring:
```csharp
public class BackgroundChatMonitor : BackgroundService
{
    // Monitor Redis for new messages when app is backgrounded
    // Trigger platform-specific notifications
    // Handle notification taps to return to chat
}
```

## Development Milestones

### Milestone 1: Basic Infrastructure (Week 1)
- [ ] Create all project structures
- [ ] Implement shared models and interfaces
- [ ] Set up basic SignalR hub
- [ ] Configure Redis integration
- [ ] Update Aspire orchestration

### Milestone 2: Core Chat Functionality (Week 2)
- [ ] Complete server-side chat logic
- [ ] Implement MAUI client UI
- [ ] SignalR client-server communication
- [ ] Message sending and receiving
- [ ] User join/leave functionality

### Milestone 3: Polish and Reliability (Week 3)
- [ ] Error handling and resilience
- [ ] Connection state management
- [ ] UI improvements for mobile
- [ ] Chat history persistence
- [ ] Cross-platform testing

### Milestone 4: Push Notifications (Week 4)
- [ ] Platform-specific notification setup
- [ ] Background message monitoring
- [ ] Notification permission handling
- [ ] End-to-end notification testing

## Demo Flow Script

### Setup:
1. Start Aspire AppHost (shows services in dashboard)
2. Launch MAUI app on Windows PC
3. Launch MAUI app on mobile device (Android/iOS)

### Demo Steps:
1. **Show Aspire Dashboard**: Display running services, health checks, logs
2. **PC App - Join Chat**: Enter name "Alice", show loading then chat interface
3. **Mobile App - Join Chat**: Enter name "Bob", see chat history with Alice's join message
4. **Real-time Messaging**: Send messages between PC and mobile, show instant sync
5. **User Management**: Show online users list, demonstrate join/leave notifications
6. **Background App**: Send message from PC while mobile app is backgrounded
7. **Push Notification**: Show notification on mobile, tap to return to chat

### Technical Highlights for Demo:
- Cross-platform native performance
- Real-time synchronization
- Persistent chat history
- Scalable Redis backend
- Aspire service orchestration
- Modern .NET 10 features

## Configuration Settings

### ChatApp.Server/appsettings.json:
```json
{
  "ConnectionStrings": {
    "Redis": "localhost:6379"
  },
  "ChatSettings": {
    "MaxMessagesInHistory": 100,
    "MessageRetentionDays": 30,
    "MaxUsernameLength": 50,
    "MaxMessageLength": 1000
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore.SignalR": "Debug"
    }
  }
}
```

### ChatApp.MAUI/Platforms Configuration:
- **Android**: Minimum API 21, Target API 34
- **iOS**: Minimum iOS 11.0, Target iOS 17.0  
- **Windows**: Windows 10 version 1809+

## Security Considerations

### For Production (Future):
- Input validation and sanitization
- Rate limiting for messages
- User authentication/authorization
- Message encryption in transit
- Redis security configuration
- CORS policy configuration
- Content Security Policy

### For Demo:
- Basic input validation
- Simple username-based identification
- Rate limiting to prevent spam
- Basic error handling

## Testing Strategy

### Unit Tests:
- ChatService Redis operations
- SignalR hub methods
- Message validation logic
- User management operations

### Integration Tests:
- SignalR client-server communication
- Redis data persistence
- Cross-platform MAUI functionality

### Manual Testing Scenarios:
- Multiple users joining/leaving
- Network interruption recovery
- App backgrounding/foregrounding
- Cross-platform message sync
- Notification delivery and handling

---

## Quick Reference Commands

### Create Projects:
```bash
# From solution root
dotnet new classlib -n ChatApp.Shared -f net10.0
dotnet new webapi -n ChatApp.Server -f net10.0
dotnet new maui -n ChatApp.MAUI -f net10.0

# Add to solution
dotnet sln add ChatApp.Shared/ChatApp.Shared.csproj
dotnet sln add ChatApp.Server/ChatApp.Server.csproj  
dotnet sln add ChatApp.MAUI/ChatApp.MAUI.csproj
```

### Add Package References:
```bash
# ChatApp.Server
dotnet add ChatApp.Server package Microsoft.AspNetCore.SignalR
dotnet add ChatApp.Server package StackExchange.Redis
dotnet add ChatApp.Server package Aspire.StackExchange.Redis

# ChatApp.MAUI
dotnet add ChatApp.MAUI package Microsoft.AspNetCore.SignalR.Client
dotnet add ChatApp.MAUI package CommunityToolkit.Mvvm
```

### Run Commands:
```bash
# Start Aspire (from AppHost directory)
dotnet run --project DistributedDemo.AppHost

# Build entire solution
dotnet build

# Run tests
dotnet test
```

---

*This plan serves as the complete reference for implementing the real-time chat application. Each section provides specific implementation details and can be used as context for development tasks.*