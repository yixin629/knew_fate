using KnewFate.ViewModels;

namespace KnewFate.Views;

public partial class DiscoveryPage : ContentPage
{
    public DiscoveryPage(DiscoveryViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private void OnUserTapped(object sender, TappedEventArgs e)
    {
        if (BindingContext is DiscoveryViewModel viewModel && sender is View view)
        {
            var userId = (int)view.BindingContext;
            if (viewModel.ViewUserProfileCommand.CanExecute(userId))
            {
                viewModel.ViewUserProfileCommand.Execute(userId);
            }
        }
    }

    private void OnLikeClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is int userId && 
            BindingContext is DiscoveryViewModel viewModel)
        {
            if (viewModel.LikeUserCommand.CanExecute(userId))
            {
                viewModel.LikeUserCommand.Execute(userId);
            }
        }
    }

    private void OnPassClicked(object sender, EventArgs e)
    {
        // Invoke pass (skip) user command when user presses the dismiss button
        if (sender is Button button && button.BindingContext is int userId &&
            BindingContext is DiscoveryViewModel viewModel)
        {
            if (viewModel.PassUserCommand.CanExecute(userId))
            {
                viewModel.PassUserCommand.Execute(userId);
            }
        }
    }

    private void OnSuperLikeClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is int userId && 
            BindingContext is DiscoveryViewModel viewModel)
        {
            if (viewModel.SuperLikeUserCommand.CanExecute(userId))
            {
                viewModel.SuperLikeUserCommand.Execute(userId);
            }
        }
    }
}
