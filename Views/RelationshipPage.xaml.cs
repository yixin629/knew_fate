using KnewFate.ViewModels;

namespace KnewFate.Views;

public partial class RelationshipPage : ContentPage
{
    public RelationshipPage(RelationshipViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
