using ChatApp.Shared.Constants;

namespace ChatApp.Server.Extensions;

public static class ValidationExtensions
{
    public static bool IsValidUserName(this string userName)
    {
        return !string.IsNullOrWhiteSpace(userName) && 
               userName.Length <= ChatConstants.MaxUsernameLength &&
               userName.All(c => char.IsLetterOrDigit(c) || c == ' ' || c == '_' || c == '-');
    }

    public static bool IsValidMessage(this string message)
    {
        return !string.IsNullOrWhiteSpace(message) && 
               message.Length <= ChatConstants.MaxMessageLength;
    }

    public static string SanitizeInput(this string input)
    {
        return string.IsNullOrWhiteSpace(input) ? string.Empty : input.Trim();
    }
}