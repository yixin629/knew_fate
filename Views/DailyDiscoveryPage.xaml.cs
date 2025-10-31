using KnewFate.ViewModels;

namespace KnewFate.Views;

public partial class DailyDiscoveryPage : ContentPage
{
    public DailyDiscoveryPage(DailyDiscoveryViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
