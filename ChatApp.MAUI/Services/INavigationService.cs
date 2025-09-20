namespace ChatApp.MAUI.Services;

public interface INavigationService
{
    Task GoToAsync(string route, IDictionary<string, object>? parameters = null);
    Task GoBackAsync();
}

public class NavigationService : INavigationService
{
    public Task GoToAsync(string route, IDictionary<string, object>? parameters = null)
        => Shell.Current.GoToAsync(route, parameters);

    public Task GoBackAsync()
    {
        if (Shell.Current is null)
            return Task.CompletedTask;
        return Shell.Current.GoToAsync("..");
    }
}
