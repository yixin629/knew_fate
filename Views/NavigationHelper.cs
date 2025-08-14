namespace KnewFate.Views;

public static class NavigationHelper
{
    public static async Task NavigateToLoginAsync()
    {
        await Shell.Current.GoToAsync("//login");
    }

    public static async Task NavigateToRegisterAsync()
    {
        await Shell.Current.GoToAsync("//register");
    }

    public static async Task NavigateToDashboardAsync()
    {
        await Shell.Current.GoToAsync("//dashboard");
    }

    public static async Task NavigateToForgotPasswordAsync()
    {
        await Shell.Current.GoToAsync("//forgotpassword");
    }

    public static async Task NavigateBackAsync()
    {
        await Shell.Current.GoToAsync("..");
    }

    public static async Task NavigateAndClearStackAsync(string route)
    {
        await Shell.Current.GoToAsync($"//{route}");
    }
}
