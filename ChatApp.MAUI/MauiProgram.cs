using Microsoft.Extensions.Logging;
using ChatApp.MAUI.Services;
using ChatApp.MAUI.ViewModels;
using ChatApp.MAUI.Views;

namespace ChatApp.MAUI;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif
		// Register services
		builder.Services.AddSingleton<IChatHubService, ChatHubService>();
		builder.Services.AddSingleton<INavigationService, NavigationService>();
#if DEBUG && WINDOWS
		builder.Services.AddSingleton<IBackendStartupService, BackendStartupService>();
#endif
		// Register view models
		builder.Services.AddSingleton<LoginViewModel>();
		builder.Services.AddSingleton<ChatViewModel>();

		// Register views
		builder.Services.AddSingleton<LoginPage>();
		builder.Services.AddSingleton<ChatPage>();

		return builder.Build();
	}
}
