namespace ChatApp.MAUI;

public partial class App : Application
{
    public App(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        MainPage = new AppShell();
#if WINDOWS
        // Fire and forget backend startup service if registered
        if (serviceProvider.GetService(typeof(ChatApp.MAUI.Services.IBackendStartupService)) is ChatApp.MAUI.Services.IBackendStartupService starter)
        {
            _ = starter.EnsureBackendRunningAsync();
        }
#endif
    }
}