using System.ComponentModel;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;

namespace KnewFate.ViewModels;

public abstract class BaseViewModel : ObservableObject
{
    private bool isBusy;
    public bool IsBusy
    {
        get => isBusy;
        set => SetProperty(ref isBusy, value);
    }

    private bool isRefreshing;
    public bool IsRefreshing
    {
        get => isRefreshing;
        set => SetProperty(ref isRefreshing, value);
    }

    protected Task ShowErrorAsync(string title, string message)
    {
        return Application.Current?.MainPage?.DisplayAlert(title, message, "确定") ?? Task.CompletedTask;
    }
}