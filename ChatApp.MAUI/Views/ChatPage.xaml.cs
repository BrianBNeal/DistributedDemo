using ChatApp.MAUI.ViewModels;

namespace ChatApp.MAUI.Views;

public partial class ChatPage : ContentPage
{
    public ChatPage(ChatViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
