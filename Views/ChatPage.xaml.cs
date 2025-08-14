using KnewFate.ViewModels;

namespace KnewFate.Views;

[QueryProperty(nameof(UserId), "userId")]
public partial class ChatPage : ContentPage
{
    public int UserId { get; set; }

    public ChatPage(ChatViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        if (BindingContext is ChatViewModel viewModel && UserId > 0)
        {
            await viewModel.LoadChatAsync(UserId);
        }
    }

    private void OnSendMessageClicked(object sender, EventArgs e)
    {
        if (BindingContext is ChatViewModel viewModel)
        {
            if (viewModel.SendMessageCommand.CanExecute(null))
            {
                viewModel.SendMessageCommand.Execute(null);
            }
        }
    }
}
