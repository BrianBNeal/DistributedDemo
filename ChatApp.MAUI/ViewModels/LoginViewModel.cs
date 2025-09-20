using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ChatApp.MAUI.Services;

namespace ChatApp.MAUI.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly IChatHubService _chatHubService;
    private readonly INavigationService _navigation;

    [ObservableProperty]
    private string userName = string.Empty;

    [ObservableProperty]
    private bool isConnecting;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    public LoginViewModel(IChatHubService chatHubService, INavigationService navigation)
    {
        _chatHubService = chatHubService;
        _navigation = navigation;
    }

    [RelayCommand]
    private async Task ConnectAsync()
    {
        if (string.IsNullOrWhiteSpace(UserName))
        {
            ErrorMessage = "Username required";
            return;
        }
        try
        {
            IsConnecting = true;
            ErrorMessage = string.Empty;
            await _chatHubService.ConnectAsync(UserName.Trim());
            await _navigation.GoToAsync("///ChatPage");
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsConnecting = false;
        }
    }
}
