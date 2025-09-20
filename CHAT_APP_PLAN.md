# ChatApp Plan (MAUI Real-Time Chat)

## TL;DR
**Goal**: Cross?platform (.NET MAUI) real?time chat using SignalR + Redis orchestrated by Aspire.  
**Backend**: Core complete (hub, persistence, presence, system messages).  
**Client**: Service + connection lifecycle implemented; UI pages pending.  
**Next 3 Tasks**: (1) Implement LoginPage / ChatPage UI (2) Username validation + connect flow (3) Style message list (system vs user) + trimming.  
**Key Decisions**: Retention via capped Redis list; join/leave represented as system messages + presence callbacks; strongly typed hub methods.  
**Open Decisions**: Distinct MessageType.Join/Leave usage; reconnect backoff policy; extended history pagination.

---
## 1. Project Goal
Demonstration of a modern distributed .NET architecture: MAUI client (Windows / Android / iOS) consuming a SignalR hub backed by Redis for short?term message history and presence state. Extensible for telemetry, auth, and push notifications.

## 2. Core Scenario
1. User enters a username and joins chat.
2. Client receives last N messages (bounded) + current online users.
3. Messages broadcast in real time; join/leave events surfaced via presence callbacks + persisted system messages.
4. Client handles reconnect transparently.

## 3. Current Status (Tagged)
- [DONE] Shared contracts (`IChatHub`, `IChatClient`, models, DTOs)
- [DONE] Strongly typed hub method names (`HubMethods`)
- [DONE] Connection lifecycle enum (`ChatConnectionState`)
- [DONE] Chat + user services with Redis persistence
- [DONE] System join/leave messages persisted
- [DONE] MAUI `ChatHubService` (auto reconnect, state events)
- [IN PROGRESS] MAUI ViewModels (Chat implemented; Login pending)
- [TODO] Pages (LoginPage.xaml, ChatPage.xaml)
- [TODO] Username validation UX & error surfacing
- [TODO] Styling (system vs user, theming, scroll behavior)
- [TODO] Client-side trimming (keep last 200 messages)
- [TODO] Enhanced reconnect backoff + user feedback
- [TODO] Metrics / tracing (OpenTelemetry wiring)
- [DECIDE] Distinct MessageType.Join/Leave vs system composite
- [DECIDE] CORS & dynamic hub URL discovery for device testing
- [FUTURE] Typing indicators
- [FUTURE] Push notifications
- [FUTURE] Authentication / identity

## 4. Architecture (Text Diagram)
```
+------------------+   SignalR   +-------------------+
|  MAUI Client     | <---------> |  ChatApp.Server   |
|  (ChatApp.MAUI)  |             |  (SignalR Hub)    |
+---------+--------+             +----+--------------+
          | Redis (messages + presence) |
          +-----------------------------+
                  Orchestrated by Aspire
```

## 5. Shared Contracts Snapshot
Hub (server ? invoked by client) `IChatHub`:
- JoinChat(string userName)
- SendMessage(string content)
- LeaveChat()
- GetChatHistory()

Callbacks (server ? client) `IChatClient`:
- ReceiveMessage(ChatMessage message)
- UserJoined(User user)
- UserLeft(string userName)
- ChatHistoryLoaded(ChatHistoryResponse history)
- ConnectionError(string error)

Strong Names: `HubMethods.Server.*`, `HubMethods.Client.*`.

Models:
- `ChatMessage(Id, UserName, Content, Timestamp, Type)`
- `User(ConnectionId, Name, JoinedAt, IsOnline)`
- `ChatHistoryResponse(Messages, OnlineUsers)`

## 6. Connection Lifecycle
Enum `ChatConnectionState`: Disconnected, Connecting, Connected, Reconnecting, Closing.  
`ChatHubService` emits `ConnectionStateChanged` + error events; automatic reconnect enabled via SignalR default policy (future: custom exponential backoff + circuit breaker).

## 7. Configuration
- `ChatClientOptions` (+ validation) supplies base URL & hub path.
- Environment override: `CHATAPP_BASEURL` (Android default uses 10.0.2.2).
- Constants: `ChatConstants` (limits, hub path, system messages, error templates).

## 8. Retention Strategy
- Server retains at most `MaxMessagesInHistory` (100) in Redis list (newest at head). Day?based retention constant removed to avoid unused config noise.
- Client will additionally trim local collection to a target (planned 200) to prevent UI bloat if history expands later.

## 9. Join/Leave Semantics
Current: Presence callbacks (`UserJoined`, `UserLeft`) + persisted system messages for audit/history.  
Pending Decision: Introduce distinct `MessageType.Join/Leave` for specialized styling OR continue with `System` type only. (Impact: UI template branching vs simplicity.)

## 10. Planned MAUI Structure
```
ChatApp.MAUI/
  Views/ (LoginPage.xaml, ChatPage.xaml)
  ViewModels/ (LoginViewModel, ChatViewModel)
  Services/ (ChatHubService, NavigationService, BackendStartupService WIN only)
  SignalR/ (ChatHubProxy)
  Converters/ (MessageTypeConverter planned)
```

## 11. UX / UI Requirements
- Adaptive layout for desktop vs mobile (single column vs side pane user list).
- System vs user message templates (color + subtle icon / prefix).
- Auto-scroll only when user is at bottom; preserve position on historical review.
- Connection state banner or status indicator.
- Inline validation: disable Send on empty/oversized messages; show username error before connect.

## 12. Error & Resilience Plan
Categories: Validation | Connectivity | Unexpected.  
Surface: Non-blocking banner / toast for transient connectivity, inline field errors for validation.  
Future Enhancements: Explicit backoff (e.g., 0s, 2s, 5s, 10s, 30s max), offline outbound queue (bounded) flush on reconnect, metrics for reconnect attempt counts.

## 13. Telemetry (Deferred)
Add OpenTelemetry (traces + metrics) hook in AppHost; instrument hub methods (`JoinChat`, `SendMessage`) and Redis operations. Provide optional exporter config via environment variables.

## 14. Open Questions
1. Adopt distinct join/leave message types? (Affects styling + filtering.)
2. Need pagination / lazy load beyond 100 messages? (Scroll boundary trigger.)
3. Implement outbound queue for sends during Reconnecting state?
4. Should username uniqueness collisions prompt auto-suffix suggestion?
5. Add optimistic local echo before server ack? (Currently immediate broadcast anyway.)

## 15. Decisions Log
- (Retention) Use capped Redis list only; removed day-based retention constant.
- (Method Names) Centralized into `HubMethods` nested classes to avoid string drift.
- (Join/Leave) Represented as system messages + presence callbacks (avoid doubling timeline entries).
- (Config) `CHATAPP_BASEURL` environment override introduced for device/emulator flexibility.

## 16. Technical Debt / Risks
- No UI yet for login/chat -> blocks full end-to-end demo.
- No message trimming on client -> potential memory/UI performance growth (low risk near term).
- Reconnect strategy minimal (no user feedback detail or custom timing).
- No telemetry -> limited observability under load.
- No auth -> username spoofing possible (acceptable for demo).

## 17. Immediate Next Actions (Detailed)
1. Scaffold `LoginPage` + validation (length, non-empty) -> call `ConnectAsync`.
2. Implement `ChatPage` with message list (CollectionView), user list (BindableLayout or side pane), input bar.
3. Add message DataTemplateSelector or simple converter for system vs user message styling.
4. Add client-side trim after each append (`if (Messages.Count > 200) remove oldest`).
5. Add reconnect visual indicator (state color map + subtle toast on transitions).
6. Basic error banner control (bind to last error string, auto clear after timeout).

## 18. Longer-Term Enhancements
- Typing indicators (transient hub broadcast, non-persisted).
- Push notifications for background delivery (platform channels + hub ? push integration).
- Auth integration (JWT / ephemeral tokens) enabling identity & moderation features.
- Distributed scaling validation (multiple hub instances + backplane if required).

## 19. Directory (Key Only)
```
ChatApp.Shared/ (Contracts, Constants, Enums, Options)
ChatApp.Server/ (Hubs, Services)
ChatApp.MAUI/ (Services, SignalR, ViewModels, Views [pending])
DistributedDemo.AppHost/ (Aspire orchestration)
```

## 20. Glossary
- **Hub Proxy**: Client wrapper implementing `IChatHub` for strongly typed invocation.
- **System Message**: Persisted informational event (join/leave) not authored by a user.
- **Presence Callback**: Real-time notification of user join/leave (not necessarily stored).
- **Connection State**: Runtime status of SignalR client connection.

## 21. Known Limitations
- No authentication / authorization.
- History limited to 100 messages (no pagination).
- No offline queue for messages during reconnect.
- Minimal telemetry / diagnostics.
- No UI virtualization optimization beyond basic list.

## 22. Quick Backend Commands
```
# Run orchestrated backend
DOTNET_ENVIRONMENT=Development dotnet run --project DistributedDemo.AppHost
```
Optionally set: `CHATAPP_BASEURL` for client if not default.

## 23. Update Log
- 2025-09-20: Added connection state enum, hub method constants, options validation; removed retention days; expanded plan structure.

_Last Updated: 2025-09-20_