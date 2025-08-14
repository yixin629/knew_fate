using KnewFate.ViewModels;

namespace KnewFate.Views;

[QueryProperty(nameof(UserId), "userId")]
public partial class UserProfilePage : ContentPage
{
    public int UserId { get; set; }

    public UserProfilePage(UserProfileViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        if (BindingContext is UserProfileViewModel viewModel && UserId > 0)
        {
            await viewModel.LoadUserProfileAsync(UserId);
        }
    }

    private async void OnImageTapped(object sender, TappedEventArgs e)
    {
        // Show full screen image
    }

    private void OnCompatibilityTapped(object sender, TappedEventArgs e)
    {
        if (BindingContext is UserProfileViewModel viewModel)
        {
            if (viewModel.ShowDetailedCompatibilityCommand.CanExecute(null))
            {
                viewModel.ShowDetailedCompatibilityCommand.Execute(null);
            }
        }
    }
}
