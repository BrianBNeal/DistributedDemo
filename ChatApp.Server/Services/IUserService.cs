using ChatApp.Shared.Models;

namespace ChatApp.Server.Services;

public interface IUserService
{
    Task<List<User>> GetOnlineUsersAsync();
    Task AddUserAsync(User user);
    Task RemoveUserAsync(string connectionId);
    Task<User?> GetUserByConnectionIdAsync(string connectionId);
    Task<bool> IsUserNameTakenAsync(string userName);
}