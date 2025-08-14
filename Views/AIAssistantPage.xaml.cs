using KnewFate.ViewModels;

namespace KnewFate.Views;

public partial class AIAssistantPage : ContentPage
{
    private readonly AIAssistantViewModel _viewModel;

    public AIAssistantPage(AIAssistantViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        
        // Scroll to bottom when messages are added
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
    }

    private async void OnViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(AIAssistantViewModel.Messages))
        {
            // Scroll to bottom when new messages are added
            await Task.Delay(100); // Small delay to ensure UI is updated
            await ChatScrollView.ScrollToAsync(0, double.MaxValue, false);
        }
    }
}
