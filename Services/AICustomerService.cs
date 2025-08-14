using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace KnewFate.Services;

public interface IAICustomerService
{
    Task<string> HandleCustomerInquiryAsync(string message, string userId);
    Task SendProactiveMessageAsync(string userId, string message);
    Task<CustomerServiceStats> GetServiceStatsAsync();
    void RegisterFloatingButton(string pageId, Controls.FloatingAIAssistantButton button);
    void UnregisterFloatingButton(string pageId);
}

public class AICustomerService : IAICustomerService
{
    private readonly AIService _aiService;
    private readonly ILogger<AICustomerService> _logger;
    private readonly ConcurrentDictionary<string, Controls.FloatingAIAssistantButton> _activeButtons;
    private readonly ConcurrentDictionary<string, List<CustomerMessage>> _conversations;

    public AICustomerService(AIService aiService, ILogger<AICustomerService> logger)
    {
        _aiService = aiService;
        _logger = logger;
        _activeButtons = new ConcurrentDictionary<string, Controls.FloatingAIAssistantButton>();
        _conversations = new ConcurrentDictionary<string, List<CustomerMessage>>();
    }

    public async Task<string> HandleCustomerInquiryAsync(string message, string userId)
    {
        try
        {
            // Record user message
            RecordMessage(userId, message, true);

            // Analyze message type and intent
            var intent = AnalyzeMessageIntent(message);
            
            // Create personalized prompt
            var prompt = CreateCustomerServicePrompt(message, userId, intent);
            
            // Call AI service
            var response = await _aiService.CallAIAsync(prompt);
            
            if (string.IsNullOrEmpty(response))
            {
                response = GetFallbackResponse(intent);
            }

            // Record AI response
            RecordMessage(userId, response, false);

            _logger.LogInformation($"AI customer service replied to user {userId}: {message.Substring(0, Math.Min(50, message.Length))}...");
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"AI customer service failed to handle user message: {userId}");
            return "Sorry, the system is temporarily unable to process your request. Our technical team is working on the issue, please try again later or contact human customer service.";
        }
    }

    public async Task SendProactiveMessageAsync(string userId, string message)
    {
        // Send proactive message to all active buttons
        foreach (var button in _activeButtons.Values)
        {
            await button.SendSystemMessage(message);
        }
    }

    public async Task<CustomerServiceStats> GetServiceStatsAsync()
    {
        return new CustomerServiceStats
        {
            ActiveConversations = _conversations.Count,
            TotalMessages = _conversations.Values.Sum(conv => conv.Count),
            AverageResponseTime = TimeSpan.FromSeconds(2.5), // Mock data
            SatisfactionRate = 0.92 // Mock data
        };
    }

    public void RegisterFloatingButton(string pageId, Controls.FloatingAIAssistantButton button)
    {
        _activeButtons.TryAdd(pageId, button);
        _logger.LogDebug($"Registered AI customer service button: {pageId}");
    }

    public void UnregisterFloatingButton(string pageId)
    {
        _activeButtons.TryRemove(pageId, out _);
        _logger.LogDebug($"Unregistered AI customer service button: {pageId}");
    }

    private void RecordMessage(string userId, string message, bool isUser)
    {
        var conversation = _conversations.GetOrAdd(userId, _ => new List<CustomerMessage>());
        conversation.Add(new CustomerMessage
        {
            Content = message,
            IsUser = isUser,
            Timestamp = DateTime.Now
        });

        // Keep only recent 50 messages
        if (conversation.Count > 50)
        {
            conversation.RemoveRange(0, conversation.Count - 50);
        }
    }

    private MessageIntent AnalyzeMessageIntent(string message)
    {
        var lowerMessage = message.ToLower();

        // Technical support category
        if (lowerMessage.Contains("login") || lowerMessage.Contains("register") || lowerMessage.Contains("password") ||
            lowerMessage.Contains("bug") || lowerMessage.Contains("error") || lowerMessage.Contains("problem"))
        {
            return MessageIntent.TechnicalSupport;
        }

        // Billing consultation category
        if (lowerMessage.Contains("payment") || lowerMessage.Contains("member") || lowerMessage.Contains("price") ||
            lowerMessage.Contains("fee") || lowerMessage.Contains("purchase"))
        {
            return MessageIntent.Billing;
        }

        // Feature inquiry category
        if (lowerMessage.Contains("how") || lowerMessage.Contains("feature") || 
            lowerMessage.Contains("use"))
        {
            return MessageIntent.FeatureInquiry;
        }

        // Fortune consultation category
        if (lowerMessage.Contains("fortune") || lowerMessage.Contains("zodiac") || lowerMessage.Contains("astrology") ||
            lowerMessage.Contains("tarot") || lowerMessage.Contains("divination"))
        {
            return MessageIntent.FortuneConsultation;
        }

        // Complaint/suggestion category
        if (lowerMessage.Contains("complain") || lowerMessage.Contains("suggest") || lowerMessage.Contains("feedback") ||
            lowerMessage.Contains("unsatisfied"))
        {
            return MessageIntent.Complaint;
        }

        return MessageIntent.General;
    }

    private string CreateCustomerServicePrompt(string message, string userId, MessageIntent intent)
    {
        var basePrompt = $@"You are a professional AI customer service representative for KnewFate app, providing assistance to users.

User ID: {userId}
User Message: {message}
Message Type: {intent}
Current Time: {DateTime.Now:yyyy-MM-dd HH:mm}

## Service Guidelines

### Identity
- You are a professional, warm, and efficient AI customer service representative
- Representing KnewFate brand image
- Have professional knowledge in fortune telling
- Familiar with all app features

### Response Principles
1. **Professional**: Provide accurate and useful information
2. **Warm**: Use friendly and caring tone
3. **Efficient**: Solve user problems quickly
4. **Personalized**: Customize answers based on user needs
5. **Concise**: Keep response length between 80-200 words

### Special Situation Handling";

        switch (intent)
        {
            case MessageIntent.TechnicalSupport:
                basePrompt += @"
- This is a technical support issue
- Provide clear operation steps
- If unsolvable, guide to contact technical team
- Express understanding for user's inconvenience";
                break;

            case MessageIntent.Billing:
                basePrompt += @"
- This is a payment-related inquiry
- Detail membership benefits
- Explain payment value
- Provide discount information (if applicable)";
                break;

            case MessageIntent.FortuneConsultation:
                basePrompt += @"
- This is a fortune consultation
- Can provide basic fortune guidance
- Recommend using professional features for detailed analysis
- Maintain mysterious yet professional atmosphere";
                break;

            case MessageIntent.Complaint:
                basePrompt += @"
- This is a complaint or suggestion
- First express apology and understanding
- Promise to forward to relevant departments
- Provide follow-up methods";
                break;
        }

        basePrompt += @"

### Common Features Introduction
- Chart Analysis: Professional birth chart interpretation
- Zodiac Fortune: Daily, weekly, monthly horoscopes
- Tarot Divination: Multiple spread options
- Social Matching: Intelligent pairing based on fortune telling
- AI Smart Assistant: 24/7 professional consultation

Please provide a professional and warm response based on the above guidelines:";

        return basePrompt;
    }

    private string GetFallbackResponse(MessageIntent intent)
    {
        return intent switch
        {
            MessageIntent.TechnicalSupport => "Thank you for your feedback! Our technical team will handle this issue as soon as possible. You can also try restarting the app or contact human customer service for help. 🛠️",
            MessageIntent.Billing => "Regarding payment and membership questions, we recommend checking the member center in settings page, or contact our dedicated customer service for detailed information. 💎",
            MessageIntent.FortuneConsultation => "Fortune consultation is our specialty! We recommend using the professional features in the app for more accurate analysis. Feel free to ask me if you have any questions! 🔮",
            MessageIntent.Complaint => "Thank you very much for your valuable feedback! We take every suggestion seriously and continuously improve our product experience. Your satisfaction is our goal! 😊",
            MessageIntent.FeatureInquiry => "KnewFate has many powerful features waiting for you to explore! We recommend starting from the homepage, and feel free to ask me about any usage questions. ✨",
            _ => "Thank you for contacting KnewFate customer service! I'll do my best to help you. For urgent issues, you can also contact our human customer service. 🌟"
        };
    }
}

// Support type definitions
public enum MessageIntent
{
    General,
    TechnicalSupport,
    Billing,
    FeatureInquiry,
    FortuneConsultation,
    Complaint
}

public class CustomerMessage
{
    public string Content { get; set; } = string.Empty;
    public bool IsUser { get; set; }
    public DateTime Timestamp { get; set; }
}

public class CustomerServiceStats
{
    public int ActiveConversations { get; set; }
    public int TotalMessages { get; set; }
    public TimeSpan AverageResponseTime { get; set; }
    public double SatisfactionRate { get; set; }
}
