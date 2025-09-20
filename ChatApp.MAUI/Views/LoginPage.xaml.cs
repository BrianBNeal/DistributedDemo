using ChatApp.MAUI.ViewModels;

namespace ChatApp.MAUI.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage(LoginViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
