using KnewFate.ViewModels;

namespace KnewFate.Views;

public partial class StoryWriterPage : ContentPage
{
    public StoryWriterPage(StoryWriterViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        
        // 自动初始化故事
        Loaded += async (s, e) =>
        {
            await viewModel.InitializeCommand.ExecuteAsync(null);
        };
    }
}
