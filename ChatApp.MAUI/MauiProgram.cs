using Microsoft.Extensions.Logging;
using ChatApp.MAUI.Services;
using ChatApp.MAUI.ViewModels;
using ChatApp.MAUI.Views;
using ChatApp.Shared.Configuration;
using Microsoft.Extensions.Options;

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
		// Configure chat client options (DEV defaults) - can be overridden via preferences or environment
		builder.Services.AddOptions<ChatClientOptions>()
			.Configure(opt =>
			{
#if WINDOWS
				opt.BaseUrl = Environment.GetEnvironmentVariable("CHATAPP_BASEURL") ?? "https://localhost:7000";
#else
#if ANDROID
				opt.BaseUrl = Environment.GetEnvironmentVariable("CHATAPP_BASEURL") ?? "https://10.0.2.2:7000";
#else
				opt.BaseUrl = Environment.GetEnvironmentVariable("CHATAPP_BASEURL") ?? "https://localhost:7000";
#endif
#endif
			})
			.Services.AddSingleton<IValidateOptions<ChatClientOptions>, ChatClientOptionsValidator>();

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
