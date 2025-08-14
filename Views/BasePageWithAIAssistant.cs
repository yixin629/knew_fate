using KnewFate.Controls;
using KnewFate.Services;

namespace KnewFate.Views;

/// <summary>
/// Base page class with AI customer service functionality
/// All pages that need AI customer service should inherit from this class
/// </summary>
public class BasePageWithAIAssistant : ContentPage
{
    protected FloatingAIAssistantButton? AIAssistantButton { get; private set; }
    protected IAICustomerService? AICustomerService { get; private set; }
    
    private Grid? _rootContainer;
    private View? _originalContent;

    protected override void OnAppearing()
    {
        base.OnAppearing();
        
        // Initialize AI customer service only on first display
        if (AIAssistantButton == null)
        {
            InitializeAIAssistant();
        }
        
        RegisterAIAssistant();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        UnregisterAIAssistant();
    }

    private void InitializeAIAssistant()
    {
        try
        {
            // Get AI customer service
            AICustomerService = Handler?.MauiContext?.Services?.GetService<IAICustomerService>();
            
            if (AICustomerService == null)
            {
                // If AI customer service is not registered, don't show floating button
                return;
            }

            // Save original content
            _originalContent = Content;
            
            // Create root container
            _rootContainer = new Grid();
            
            // Add original content to root container
            if (_originalContent != null)
            {
                _rootContainer.Children.Add(_originalContent);
            }
            
            // Create AI assistant floating button
            AIAssistantButton = new FloatingAIAssistantButton();
            
            // Add floating button to root container (top layer)
            _rootContainer.Children.Add(AIAssistantButton);
            
            // Set new root content
            Content = _rootContainer;
        }
        catch (Exception ex)
        {
            // If initialization fails, log error but don't affect normal page display
            System.Diagnostics.Debug.WriteLine($"AI customer service initialization failed: {ex.Message}");
        }
    }

    private void RegisterAIAssistant()
    {
        if (AICustomerService != null && AIAssistantButton != null)
        {
            var pageId = GetType().Name + "_" + GetHashCode();
            AICustomerService.RegisterFloatingButton(pageId, AIAssistantButton);
        }
    }

    private void UnregisterAIAssistant()
    {
        if (AICustomerService != null)
        {
            var pageId = GetType().Name + "_" + GetHashCode();
            AICustomerService.UnregisterFloatingButton(pageId);
        }
    }

    /// <summary>
    /// Send system message to user
    /// Can be used for important notifications, reminders, etc.
    /// </summary>
    /// <param name="message">Message to send</param>
    protected async Task SendSystemMessageToUser(string message)
    {
        if (AIAssistantButton != null)
        {
            await AIAssistantButton.SendSystemMessage(message);
        }
    }

    /// <summary>
    /// Show unread message badge
    /// Used to attract user attention
    /// </summary>
    /// <param name="count">Number of unread messages</param>
    protected void ShowUnreadMessage(int count = 1)
    {
        AIAssistantButton?.ShowUnreadMessage(count);
    }

    /// <summary>
    /// Disable AI customer service functionality
    /// Some special pages may not need customer service
    /// </summary>
    protected virtual bool ShouldShowAIAssistant => true;
}
