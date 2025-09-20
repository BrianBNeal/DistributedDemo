# Real-Time Chat Application - MAUI Client Implementation

## ?? Project Goal
Build a **cross-platform real-time chat demonstration application** using .NET 10, MAUI, SignalR, Redis, and Aspire to showcase:
- Real-time messaging across devices
- Cross-platform UI with MAUI (Windows, Android, iOS)
- Modern distributed .NET architecture orchestrated by Aspire
- Redis-based scalable persistence
- Extensible foundation for future push notifications

## ?? Demo Scenario
- Windows desktop and mobile (Android/iOS) clients
- Users join with a username, exchange live messages
- Persisted history sourced from Redis
- Presence notifications (join/leave + system messages)

## ?? Current Implementation Status

### ? Backend Core (Complete – Enhancements Pending)
Implemented:
- **ChatApp.Shared**: Models, DTOs, interfaces, constants
- **ChatApp.Server**: SignalR hub (join, leave, history, broadcast)
- **Redis Integration**: Message history + online user tracking
- **Aspire AppHost**: Orchestration + Redis resource
- **Health/OpenAPI**: Basic endpoints
- **System Messages**: Join/leave persisted as system entries

Pending Enhancements:
- Message retention policy (constant `MessageRetentionDays` unused)
- Decide on using `MessageType.Join` / `MessageType.Leave` vs generic system messages
- Broaden CORS or dynamic hub endpoint resolution for device testing
- Optional metrics/tracing surfacing (OpenTelemetry exporter config)
- Authentication / identity (deferred)
- Load / scalability validation
- Improved transient error recovery logic

### ?? MAUI Client (Not Yet Implemented)
To be created: project wiring, pages, view models, SignalR client service, navigation, styling, state management.

### ?? Future (Phase 2)
Push notifications (background delivery + platform channels)

## ?? Architecture Overview
```
????????????????????????????????????????????????????????????????
?                        Aspire AppHost                       ?
?  ??????????????????   ????????????????????????????????????  ?
?  ?   Redis Cache  ?   ?          ChatApp.Server          ?  ?
?  ? (Messages/User)?   ? (SignalR Hub + Services + API)   ?  ?
?  ??????????????????   ????????????????????????????????????  ?
????????????????????????????????????????????????????????????????
                            ? SignalR
????????????????????????????????????????????????????????????????
?                       ChatApp.MAUI                          ?
?  Windows (Desktop) | Android (Mobile) | iOS (Mobile)         ?
????????????????????????????????????????????????????????????????
```

## ?? MAUI Implementation Plan

### 1. Project Structure (Planned)
```
ChatApp.MAUI/
  Views/
    LoginPage.xaml
    ChatPage.xaml
  ViewModels/
    LoginViewModel.cs
    ChatViewModel.cs
  Services/
    IChatHubService.cs
    ChatHubService.cs
    INavigationService.cs
  Converters/
    MessageTypeConverter.cs
  Platforms/
```

### 2. ViewModels (Planned)
LoginViewModel:
```
public partial class LoginViewModel : ObservableObject
{
    [ObservableProperty] private string userName = string.Empty;
    [ObservableProperty] private bool isConnecting;
    [ObservableProperty] private string errorMessage = string.Empty;

    [RelayCommand] private Task ConnectAsync();
    [RelayCommand] private Task ValidateUserNameAsync();
}
```
ChatViewModel:
```
public partial class ChatViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<ChatMessage> messages = new();
    [ObservableProperty] private ObservableCollection<User> onlineUsers = new();
    [ObservableProperty] private string currentMessage = string.Empty;
    [ObservableProperty] private bool isConnected;
    [ObservableProperty] private string connectionStatus = "Disconnected";

    [RelayCommand] private Task SendMessageAsync();
    [RelayCommand] private Task DisconnectAsync();
}
```

### 3. Required NuGet Packages
```
<PackageReference Include="Microsoft.AspNetCore.SignalR.Client" />
<PackageReference Include="CommunityToolkit.Mvvm" />
<PackageReference Include="Microsoft.Extensions.Logging" />
<PackageReference Include="Microsoft.Extensions.DependencyInjection" />
```

### 4. Platform Configurations
- Android: Min API 21, Target 34, Internet permission
- iOS: Min 11.0, Target latest, NSAppTransportSecurity allowances (local dev)
- Windows: Network loopback / local dev configuration

## ?? Backend API Summary
SignalR Hub Path: `/chathub` (see `ChatConstants.ChatHubPath`)

Hub Methods (`IChatHub`):
```
JoinChat(string userName)
SendMessage(string content)
LeaveChat()
GetChatHistory()
```

Client Callbacks (`IChatClient`):
```
ReceiveMessage(ChatMessage message)
UserJoined(User user)
UserLeft(string userName)
ChatHistoryLoaded(ChatHistoryResponse history)
ConnectionError(string error)
```

Note: Join/leave events are currently surfaced as:
- Explicit callbacks (`UserJoined`, `UserLeft`)
- Plus persisted system messages (`MessageType.System`)
`MessageType.Join` / `MessageType.Leave` are defined but not emitted yet.

## ?? UI / UX Goals
- Consistent shared XAML with adaptive layout
- Clear connection states: Connecting / Connected / Reconnecting / Disconnected
- System vs user message styling
- Scroll preservation and incremental history trimming
- (Optional) Typing indicators (not implemented server-side yet)

## ?? Development Workflow (Adjusted)

### 1. Scaffold MAUI Project
```
dotnet new maui -n ChatApp.MAUI -f net10.0
dotnet add ChatApp.MAUI reference ChatApp.Shared
dotnet sln add ChatApp.MAUI/ChatApp.MAUI.csproj
```

### 2. Add Required Packages & Services
- Configure dependency injection
- Register `IChatHubService`, `INavigationService`
- Setup MVVM (CommunityToolkit)

### 3. Implement Core Screens
- `LoginPage` with username validation + navigation
- `ChatPage` with messages + users list + input bar

### 4. Add SignalR Integration
- `HubConnection` with `WithAutomaticReconnect()`
- Connection state events ? ViewModel state
- Main-thread marshaling for collection updates
- Retry strategy on initial connect (exponential backoff)

### 5. Cross-Platform Testing
- Windows (local)
- Android emulator/device
- iOS simulator/device

## ? / ?? Success Criteria (Updated)

Functional:
- [ ] Users can join chat with username (MAUI client)
- [ ] Real-time message sending/receiving (MAUI client)
- [x] Message history persistence (server)
- [x] User presence (server events)
- [ ] Cross-platform deployment (pending client)

Technical:
- [x] Shared contracts
- [x] SignalR hub operational
- [ ] MVVM bindings in MAUI
- [ ] Reconnection UX
- [ ] Client-side validation & error surfacing

Demo:
- [ ] Multi-device simultaneous session
- [ ] Polished UI/UX pass
- [ ] Performance sanity checks
- [ ] Presentation readiness

## ? Quick Start (Backend)
```
dotnet run --project DistributedDemo.AppHost
# Dashboard: (example) https://localhost:17261
curl https://localhost:{port}/health
curl https://localhost:{port}/test-redis
```

## ?? Implementation Notes
Patterns:
- MVVM + DI + event-driven SignalR callbacks
- Immutable record models simplify serialization & diffing

Performance / UX:
- Server returns up to 100 messages; client should optionally trim older ones
- Consider future lazy history loading if expanding beyond cap

Retention:
- `MessageRetentionDays` currently unused (choose: implement pruning or remove constant)

Join/Leave Semantics:
- Currently persisted as `MessageType.System` + separate callbacks
- Option: emit distinct `MessageType.Join/Leave` for richer UI styling

Resilience:
- Add retry backoff for connect
- Surface reconnect states (Connecting / Connected / Reconnecting / Disconnected)

Security (Deferred):
- No auth; username uniqueness enforced via Redis set

Planned Enhancements:
- Typing indicators (would require new hub methods + ephemeral broadcast)
- Push notifications (Phase 2)
- Optional structured tracing + correlation IDs

## ?? Pending / Technical Debt
- Clarify message type usage for join/leave
- Implement or drop retention policy
- Expand CORS / dynamic hub URL for real devices
- Decide on removal of unused constants if not implemented

## ?? Next Concrete Actions
1. Create `ChatApp.MAUI` project & add references (already present – wire up services)
2. Add packages + register services in `MauiProgram`
3. Implement `IChatHubService` with automatic reconnect
4. Build `LoginPage` + navigation to `ChatPage`
5. Bind collections with thread-safe updates
6. Style system vs user messages
7. Implement connection state indicators
8. Test on Windows ? Android ? iOS
9. Add retry/backoff + error surfacing
10. Decide on join/leave message enum usage

---

**Next Action**: Scaffold and implement `ChatApp.MAUI` client (services + pages + view models).

_Last Updated: 2025-09-20_