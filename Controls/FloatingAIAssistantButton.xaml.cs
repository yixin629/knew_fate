using KnewFate.Services;
using KnewFate.Views;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;

namespace KnewFate.Controls;

public partial class FloatingAIAssistantButton : ContentView
{
    private readonly AIService _aiService;
    private readonly ILocalizationService _localizationService;
    private bool _isChatOpen = false;
    private bool _isTyping = false;
    private readonly ObservableCollection<string> _quickQuestions;

    public FloatingAIAssistantButton()
    {
        InitializeComponent();
        
        // Get services
        _aiService = Handler?.MauiContext?.Services?.GetService<AIService>() 
                    ?? throw new InvalidOperationException("AIService not found");
        _localizationService = Handler?.MauiContext?.Services?.GetService<ILocalizationService>() 
                             ?? throw new InvalidOperationException("LocalizationService not found");
        
        // Initialize quick questions with localized strings
        _quickQuestions = new ObservableCollection<string>();
        LoadLocalizedQuickQuestions();
        
        QuickQuestionsView.ItemsSource = _quickQuestions;
        
        // Subscribe to language changes
        _localizationService.LanguageChanged += OnLanguageChanged;
        
        // Start welcome animation
        StartWelcomeAnimation();
    }

    private void LoadLocalizedQuickQuestions()
    {
        _quickQuestions.Clear();
        _quickQuestions.Add(_localizationService.GetLocalizedString("HowIsMyFortuneToday"));
        _quickQuestions.Add(_localizationService.GetLocalizedString("AnalyzeMyPersonality"));
        _quickQuestions.Add(_localizationService.GetLocalizedString("LoveFortuneGuidance"));
        _quickQuestions.Add(_localizationService.GetLocalizedString("CareerDevelopmentAdvice"));
        _quickQuestions.Add(_localizationService.GetLocalizedString("TarotCardReading"));
        _quickQuestions.Add(_localizationService.GetLocalizedString("WealthEnhancementTips"));
    }

    private void OnLanguageChanged(object sender, EventArgs e)
    {
        LoadLocalizedQuickQuestions();
    }

    private async void OnFloatingButtonTapped(object sender, EventArgs e)
    {
        if (!_isChatOpen)
        {
            await OpenChatWindow();
        }
        else
        {
            await CloseChatWindow();
        }
    }

    private async void OnCloseChatClicked(object sender, EventArgs e)
    {
        await CloseChatWindow();
    }

    private async Task OpenChatWindow()
    {
        _isChatOpen = true;
        
        // Show background overlay
        BackgroundOverlay.IsVisible = true;
        BackgroundOverlay.InputTransparent = false;
        
        // Show chat window
        ChatContainer.IsVisible = true;
        
        // Animation effects
        await Task.WhenAll(
            BackgroundOverlay.FadeTo(0.3, 250, Easing.CubicOut),
            ChatContainer.TranslateTo(0, 0, 300, Easing.CubicOut),
            ChatContainer.FadeTo(1, 300, Easing.CubicOut)
        );
        
        // Hide unread badge
        UnreadBadge.IsVisible = false;
        
        // Focus input field
        MessageEntry.Focus();
    }

    private async Task CloseChatWindow()
    {
        _isChatOpen = false;
        
        // Animation effects
        await Task.WhenAll(
            BackgroundOverlay.FadeTo(0, 250, Easing.CubicIn),
            ChatContainer.TranslateTo(0, 50, 300, Easing.CubicIn),
            ChatContainer.FadeTo(0, 300, Easing.CubicIn)
        );
        
        // Hide elements
        ChatContainer.IsVisible = false;
        BackgroundOverlay.IsVisible = false;
        BackgroundOverlay.InputTransparent = true;
    }

    private async void OnQuickQuestionTapped(object sender, EventArgs e)
    {
        if (sender is Border border && border.BindingContext is string question)
        {
            await SendMessage(question, isUserMessage: true);
        }
    }

    private async void OnSendMessageClicked(object sender, EventArgs e)
    {
        var message = MessageEntry.Text?.Trim();
        if (!string.IsNullOrEmpty(message))
        {
            MessageEntry.Text = string.Empty;
            await SendMessage(message, isUserMessage: true);
        }
    }

    private async Task SendMessage(string message, bool isUserMessage)
    {
        if (isUserMessage)
        {
            // Add user message
            AddUserMessage(message);
            
            // Show typing indicator
            ShowTypingIndicator();
            
            // Get AI response
            try
            {
                var prompt = CreateAIPrompt(message);
                var aiResponse = await _aiService.CallAIAsync(prompt);
                
                HideTypingIndicator();
                
                if (!string.IsNullOrEmpty(aiResponse))
                {
                    AddAIMessage(aiResponse);
                }
                else
                {
                    AddAIMessage(_localizationService.GetLocalizedString("AITemporarilyUnavailable"));
                }
            }
            catch (Exception ex)
            {
                HideTypingIndicator();
                AddAIMessage(_localizationService.GetLocalizedString("NetworkConnectionError"));
            }
        }
    }

    private void AddUserMessage(string message)
    {
        var messageContainer = new Border
        {
            BackgroundColor = Microsoft.Maui.Graphics.Colors.Blue,
            Padding = new Thickness(12, 8),
            Margin = new Thickness(50, 0, 0, 0),
            HorizontalOptions = LayoutOptions.End
        };
        messageContainer.StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(15, 15, 5, 15) };

        var messageLabel = new Label
        {
            Text = message,
            TextColor = Microsoft.Maui.Graphics.Colors.White,
            FontSize = 14,
            LineBreakMode = LineBreakMode.WordWrap
        };

        var timeLabel = new Label
        {
            Text = DateTime.Now.ToString("HH:mm"),
            TextColor = Microsoft.Maui.Graphics.Colors.White,
            FontSize = 11,
            Opacity = 0.8,
            HorizontalOptions = LayoutOptions.End,
            Margin = new Thickness(0, 3, 0, 0)
        };

        var stackLayout = new StackLayout();
        stackLayout.Children.Add(messageLabel);
        stackLayout.Children.Add(timeLabel);
        messageContainer.Content = stackLayout;

        DynamicMessagesContainer.Children.Add(messageContainer);
        ScrollToBottom();
    }

    private void AddAIMessage(string message)
    {
        var messageContainer = new Border
        {
            BackgroundColor = Microsoft.Maui.Graphics.Color.FromArgb("#F0F0F0"),
            Padding = new Thickness(12, 8),
            Margin = new Thickness(0, 0, 50, 0),
            HorizontalOptions = LayoutOptions.Start
        };
        messageContainer.StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(15, 15, 15, 5) };

        var headerGrid = new Grid
        {
            ColumnDefinitions = 
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Star }
            }
        };

        var avatarLabel = new Label
        {
            Text = "🔮",
            FontSize = 16,
            VerticalOptions = LayoutOptions.Start,
            Margin = new Thickness(0, 0, 8, 0)
        };

        var messageLabel = new Label
        {
            Text = message,
            TextColor = Microsoft.Maui.Graphics.Color.FromArgb("#333333"),
            FontSize = 14,
            LineBreakMode = LineBreakMode.WordWrap
        };

        var timeLabel = new Label
        {
            Text = DateTime.Now.ToString("HH:mm"),
            TextColor = Microsoft.Maui.Graphics.Color.FromArgb("#999999"),
            FontSize = 11,
            HorizontalOptions = LayoutOptions.Start,
            Margin = new Thickness(24, 3, 0, 0)
        };

        headerGrid.Children.Add(avatarLabel);
        Grid.SetColumn(avatarLabel, 0);
        
        headerGrid.Children.Add(messageLabel);
        Grid.SetColumn(messageLabel, 1);

        var stackLayout = new StackLayout();
        stackLayout.Children.Add(headerGrid);
        stackLayout.Children.Add(timeLabel);
        messageContainer.Content = stackLayout;

        DynamicMessagesContainer.Children.Add(messageContainer);
        ScrollToBottom();
    }

    private void ShowTypingIndicator()
    {
        _isTyping = true;
        TypingIndicator.IsVisible = true;
        ScrollToBottom();
    }

    private void HideTypingIndicator()
    {
        _isTyping = false;
        TypingIndicator.IsVisible = false;
    }

    private async void ScrollToBottom()
    {
        await Task.Delay(100); // Wait for UI update
        await ChatScrollView.ScrollToAsync(0, double.MaxValue, false);
    }

    private string CreateAIPrompt(string userMessage)
    {
        return $@"You are a professional AI customer service representative for KnewFate app, providing assistance to users.

User Question: {userMessage}

Response Requirements:
1. Use English in your response
2. Maintain a friendly and professional tone
3. If it involves fortune telling, provide useful advice
4. If it's a technical issue, provide clear solutions
5. Use appropriate emojis to add warmth
6. Keep response length between 50-150 words

Please provide a concise and helpful response:";
    }

    private async void StartWelcomeAnimation()
    {
        await Task.Delay(2000); // Show welcome prompt after 2 seconds
        
        if (!_isChatOpen)
        {
            // Show unread badge
            UnreadBadge.IsVisible = true;
            
            // Pulse animation
            _ = Task.Run(async () =>
            {
                for (int i = 0; i < 3; i++)
                {
                    Device.BeginInvokeOnMainThread(() => PulseRing.IsVisible = true);
                    await PulseRing.ScaleTo(1.3, 1000, Easing.CubicOut);
                    await PulseRing.FadeTo(0, 500, Easing.CubicIn);
                    await PulseRing.ScaleTo(1, 0);
                    await PulseRing.FadeTo(0.6, 0);
                    Device.BeginInvokeOnMainThread(() => PulseRing.IsVisible = false);
                    await Task.Delay(1000);
                }
            });
        }
    }

    // Public method: Show unread message
    public void ShowUnreadMessage(int count = 1)
    {
        UnreadBadge.IsVisible = true;
        UnreadCount.Text = count.ToString();
    }

    // Public method: Send system message
    public async Task SendSystemMessage(string message)
    {
        if (_isChatOpen)
        {
            AddAIMessage($"📢 System Notification: {message}");
        }
        else
        {
            ShowUnreadMessage();
        }
    }
}
