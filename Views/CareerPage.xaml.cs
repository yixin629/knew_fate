using KnewFate.ViewModels;

namespace KnewFate.Views;

public partial class CareerPage : ContentPage
{
    public CareerPage(CareerViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
