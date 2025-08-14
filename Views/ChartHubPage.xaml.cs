using KnewFate.ViewModels;

namespace KnewFate.Views;

public partial class ChartHubPage : ContentPage
{
    public ChartHubPage(ChartHubViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
