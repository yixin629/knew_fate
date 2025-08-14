using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace KnewFate.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private string title = "KnewFate";

    [ObservableProperty]
    private bool isLoading = false;

    public MainViewModel()
    {
        
    }

    [RelayCommand]
    private async Task Initialize()
    {
        IsLoading = true;
        try
        {
            // Initialize app
            await Task.Delay(1000); // Simulate initialization
        }
        finally
        {
            IsLoading = false;
        }
    }
}

public partial class ChartHubViewModel : ObservableObject
{
    [ObservableProperty]
    private string title = "Chart Hub";

    public ChartHubViewModel()
    {
        
    }
}

public partial class RelationshipViewModel : ObservableObject
{
    [ObservableProperty]
    private string title = "Relationship";

    public RelationshipViewModel()
    {
        
    }
}

public partial class CareerViewModel : ObservableObject
{
    [ObservableProperty]
    private string title = "Career";

    public CareerViewModel()
    {
        
    }
}

public partial class TimelineViewModel : ObservableObject
{
    [ObservableProperty]
    private string title = "Timeline";

    public TimelineViewModel()
    {
        
    }
}

public partial class OnboardingViewModel : ObservableObject
{
    [ObservableProperty]
    private string title = "Welcome";

    public OnboardingViewModel()
    {
        
    }
}
