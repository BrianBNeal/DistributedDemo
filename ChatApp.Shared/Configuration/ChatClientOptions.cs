using Microsoft.Extensions.Options;

namespace ChatApp.Shared.Configuration;

/// <summary>
/// Options controlling how the MAUI client connects to the chat backend.
/// </summary>
public class ChatClientOptions
{
    /// <summary>Base URL of the backend (scheme + host + optional port) e.g. https://localhost:7000</summary>
    public string? BaseUrl { get; set; }

    /// <summary>Optional override for hub path (defaults to ChatConstants.ChatHubPath)</summary>
    public string? HubPath { get; set; }

    public string GetHubUrl()
    {
        if (string.IsNullOrWhiteSpace(BaseUrl))
            throw new InvalidOperationException("ChatClientOptions.BaseUrl must be configured");
        var path = string.IsNullOrWhiteSpace(HubPath) ? Constants.ChatConstants.ChatHubPath : HubPath!;
        return BaseUrl!.TrimEnd('/') + path;
    }
}

/// <summary>
/// Validates <see cref="ChatClientOptions"/> at startup.
/// </summary>
public class ChatClientOptionsValidator : IValidateOptions<ChatClientOptions>
{
    public ValidateOptionsResult Validate(string? name, ChatClientOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.BaseUrl))
            return ValidateOptionsResult.Fail("BaseUrl must be provided for ChatClientOptions");
        if (!Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out var uri) || (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp))
            return ValidateOptionsResult.Fail("BaseUrl must be an absolute http/https URL");
        return ValidateOptionsResult.Success;
    }
}
