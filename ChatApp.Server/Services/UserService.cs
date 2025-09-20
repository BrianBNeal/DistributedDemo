using ChatApp.Shared.Models;
using ChatApp.Shared.Constants;
using StackExchange.Redis;
using System.Text.Json;

namespace ChatApp.Server.Services;

public class UserService : IUserService
{
    private readonly IDatabase _database;
    private readonly ILogger<UserService> _logger;

    public UserService(IConnectionMultiplexer redis, ILogger<UserService> logger)
    {
        _database = redis.GetDatabase();
        _logger = logger;
    }

    public async Task<List<User>> GetOnlineUsersAsync()
    {
        try
        {
            var userNames = await _database.SetMembersAsync(ChatConstants.OnlineUsersKey);
            var users = new List<User>();

            foreach (var userName in userNames)
            {
                if (userName.HasValue)
                {
                    // Get user details from hash
                    var userHash = await _database.HashGetAllAsync($"{ChatConstants.UserDetailsKeyPrefix}{userName}");
                    if (userHash.Any())
                    {
                        var userDict = userHash.ToDictionary(h => h.Name.ToString(), h => h.Value.ToString());
                        
                        if (userDict.TryGetValue("ConnectionId", out var connectionId) &&
                            userDict.TryGetValue("Name", out var name) &&
                            userDict.TryGetValue("JoinedAt", out var joinedAtStr) &&
                            DateTime.TryParse(joinedAtStr, out var joinedAt))
                        {
                            var user = new User(connectionId, name, joinedAt, true);
                            users.Add(user);
                        }
                    }
                }
            }

            _logger.LogDebug("Retrieved {Count} online users", users.Count);
            return users.OrderBy(u => u.JoinedAt).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving online users");
            return new List<User>();
        }
    }

    public async Task AddUserAsync(User user)
    {
        try
        {
            // Add username to online users set
            await _database.SetAddAsync(ChatConstants.OnlineUsersKey, user.Name);

            // Store user details in hash
            var userKey = $"{ChatConstants.UserDetailsKeyPrefix}{user.Name}";
            var userHash = new HashEntry[]
            {
                new("ConnectionId", user.ConnectionId),
                new("Name", user.Name),
                new("JoinedAt", user.JoinedAt.ToString("O")), // ISO 8601 format
                new("IsOnline", user.IsOnline.ToString())
            };

            await _database.HashSetAsync(userKey, userHash);

            // Set expiration for user details (cleanup in case of unexpected disconnections)
            await _database.KeyExpireAsync(userKey, TimeSpan.FromHours(24));

            _logger.LogInformation("Added user {UserName} with connection {ConnectionId}", user.Name, user.ConnectionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding user {UserName}", user.Name);
            throw;
        }
    }

    public async Task RemoveUserAsync(string connectionId)
    {
        try
        {
            // Find user by connection ID
            var user = await GetUserByConnectionIdAsync(connectionId);
            if (user == null)
            {
                _logger.LogWarning("Attempted to remove user with connection {ConnectionId} but user not found", connectionId);
                return;
            }

            // Remove from online users set
            await _database.SetRemoveAsync(ChatConstants.OnlineUsersKey, user.Name);

            // Remove user details hash
            var userKey = $"{ChatConstants.UserDetailsKeyPrefix}{user.Name}";
            await _database.KeyDeleteAsync(userKey);

            _logger.LogInformation("Removed user {UserName} with connection {ConnectionId}", user.Name, connectionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing user with connection {ConnectionId}", connectionId);
            throw;
        }
    }

    public async Task<User?> GetUserByConnectionIdAsync(string connectionId)
    {
        try
        {
            var userNames = await _database.SetMembersAsync(ChatConstants.OnlineUsersKey);

            foreach (var userName in userNames)
            {
                if (userName.HasValue)
                {
                    var userKey = $"{ChatConstants.UserDetailsKeyPrefix}{userName}";
                    var storedConnectionId = await _database.HashGetAsync(userKey, "ConnectionId");
                    
                    if (storedConnectionId.HasValue && storedConnectionId == connectionId)
                    {
                        var userHash = await _database.HashGetAllAsync(userKey);
                        var userDict = userHash.ToDictionary(h => h.Name.ToString(), h => h.Value.ToString());
                        
                        if (userDict.TryGetValue("Name", out var name) &&
                            userDict.TryGetValue("JoinedAt", out var joinedAtStr) &&
                            DateTime.TryParse(joinedAtStr, out var joinedAt))
                        {
                            return new User(connectionId, name, joinedAt, true);
                        }
                    }
                }
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding user by connection {ConnectionId}", connectionId);
            return null;
        }
    }

    public async Task<bool> IsUserNameTakenAsync(string userName)
    {
        try
        {
            return await _database.SetContainsAsync(ChatConstants.OnlineUsersKey, userName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if username {UserName} is taken", userName);
            return true; // Err on the side of caution
        }
    }
}