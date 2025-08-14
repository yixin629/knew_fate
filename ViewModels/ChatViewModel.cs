using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KnewFate.Models;
using KnewFate.Services;

namespace KnewFate.ViewModels;

public partial class ChatViewModel : BaseViewModel
{
    private readonly ISocialService _socialService;

    [ObservableProperty]
    private int chatUserId;

    [ObservableProperty]
    private string chatTitle = "聊天";

    [ObservableProperty]
    private string messageText = string.Empty;

    [ObservableProperty]
    private ObservableCollection<ChatMessageViewModel> messages = new();

    public ICommand SendMessageCommand { get; }

    public ChatViewModel(ISocialService socialService)
    {
        _socialService = socialService;
        SendMessageCommand = new AsyncRelayCommand(SendMessageAsync);
    }

    public async Task LoadChatAsync(int userId)
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            ChatUserId = userId;

            // Load user profile to get name for title
            var userProfile = await _socialService.GetUserProfileAsync(userId);
            ChatTitle = userProfile.DisplayName;

            // Load chat history
            var currentUserId = await GetCurrentUserIdAsync();
            var chatHistory = await _socialService.GetChatHistoryAsync(currentUserId, userId, 0, 50);

            Messages.Clear();
            foreach (var message in chatHistory.OrderBy(m => m.SentAt))
            {
                Messages.Add(new ChatMessageViewModel
                {
                    Content = message.Content,
                    SentAt = message.SentAt,
                    IsSent = message.SenderId == currentUserId,
                    IsReceived = message.SenderId != currentUserId
                });
            }
        }
        catch (Exception ex)
        {
            await ShowErrorAsync("加载聊天记录失败", ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task SendMessageAsync()
    {
        if (string.IsNullOrWhiteSpace(MessageText) || IsBusy)
            return;

        try
        {
            var currentUserId = await GetCurrentUserIdAsync();
            var message = await _socialService.SendMessageAsync(currentUserId, ChatUserId, MessageText);

            Messages.Add(new ChatMessageViewModel
            {
                Content = message.Content,
                SentAt = message.SentAt,
                IsSent = true,
                IsReceived = false
            });

            MessageText = string.Empty;
        }
        catch (Exception ex)
        {
            await ShowErrorAsync("发送消息失败", ex.Message);
        }
    }

    private async Task<int> GetCurrentUserIdAsync()
    {
        // This would typically come from a user service or authentication
        return 1; // Placeholder
    }
}

public class ChatMessageViewModel
{
    public string Content { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
    public bool IsSent { get; set; }
    public bool IsReceived { get; set; }
}
