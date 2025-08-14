using KnewFate.ViewModels;

namespace KnewFate.Views;

public partial class TimelinePage : ContentPage
{
    public TimelinePage(TimelineViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
