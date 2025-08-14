using KnewFate.ViewModels;

namespace KnewFate.Views;

public partial class TarotPage : ContentPage
{
    public TarotPage(TarotViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
