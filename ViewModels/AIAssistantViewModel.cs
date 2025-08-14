using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using KnewFate.Models;
using KnewFate.Services;

namespace KnewFate.ViewModels;

public class AIAssistantViewModel : INotifyPropertyChanged
{
    private readonly AIService _aiService;
    private readonly ZodiacService _zodiacService;
    
    private string _inputText = string.Empty;
    private bool _isLoading = false;
    private bool _isFirstTime = true;
    private bool _showQuickQuestions = true;

    public ObservableCollection<ChatMessage> Messages { get; } = new();
    
    public List<string> QuickQuestions { get; } = new()
    {
        "我想了解自己的性格特点",
        "最近感情运势如何？",
        "如何提升财运？",
        "职业发展方向建议",
        "今日运势解读",
        "塔罗牌抽卡占卜"
    };

    public string InputText
    {
        get => _inputText;
        set
        {
            _inputText = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanSendMessage));
        }
    }

    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            _isLoading = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanSendMessage));
        }
    }

    public bool IsFirstTime
    {
        get => _isFirstTime;
        set
        {
            _isFirstTime = value;
            OnPropertyChanged();
        }
    }

    public bool ShowQuickQuestions
    {
        get => _showQuickQuestions;
        set
        {
            _showQuickQuestions = value;
            OnPropertyChanged();
        }
    }

    public bool CanSendMessage => !string.IsNullOrWhiteSpace(InputText) && !IsLoading;

    public ICommand SendMessageCommand { get; }
    public ICommand SendQuickQuestionCommand { get; }
    public ICommand ShowQuickQuestionsCommand { get; }

    public AIAssistantViewModel(AIService aiService, ZodiacService zodiacService)
    {
        _aiService = aiService;
        _zodiacService = zodiacService;
        
        SendMessageCommand = new Command(async () => await SendMessageAsync());
        SendQuickQuestionCommand = new Command<string>(async (question) => await SendQuickQuestionAsync(question));
        ShowQuickQuestionsCommand = new Command(() => ShowQuickQuestions = !ShowQuickQuestions);
    }

    private async Task SendMessageAsync()
    {
        if (string.IsNullOrWhiteSpace(InputText) || IsLoading) return;

        var userMessage = InputText.Trim();
        InputText = string.Empty;
        
        // Hide first time message and quick questions after first interaction
        IsFirstTime = false;
        ShowQuickQuestions = false;

        // Add user message
        Messages.Add(new ChatMessage
        {
            Content = userMessage,
            IsUser = true,
            Timestamp = DateTime.Now
        });

        await ProcessMessageAsync(userMessage);
    }

    private async Task SendQuickQuestionAsync(string question)
    {
        if (string.IsNullOrWhiteSpace(question) || IsLoading) return;

        // Hide first time message and quick questions
        IsFirstTime = false;
        ShowQuickQuestions = false;

        // Add user message
        Messages.Add(new ChatMessage
        {
            Content = question,
            IsUser = true,
            Timestamp = DateTime.Now
        });

        await ProcessMessageAsync(question);
    }

    private async Task ProcessMessageAsync(string userMessage)
    {
        IsLoading = true;

        try
        {
            // Determine the type of consultation and create appropriate prompt
            var prompt = await CreateEnhancedPromptAsync(userMessage);
            
            // Get AI response
            var aiResponse = await _aiService.CallAIAsync(prompt);
            
            if (!string.IsNullOrWhiteSpace(aiResponse))
            {
                // Add AI response
                Messages.Add(new ChatMessage
                {
                    Content = aiResponse,
                    IsUser = false,
                    Timestamp = DateTime.Now
                });
            }
            else
            {
                // Fallback response
                Messages.Add(new ChatMessage
                {
                    Content = GetFallbackResponse(userMessage),
                    IsUser = false,
                    Timestamp = DateTime.Now
                });
            }
        }
        catch (Exception ex)
        {
            Messages.Add(new ChatMessage
            {
                Content = "抱歉，智能助手暂时无法回应。请稍后重试，或者您可以尝试换个方式提问。💫",
                IsUser = false,
                Timestamp = DateTime.Now
            });
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task<string> CreateEnhancedPromptAsync(string userMessage)
    {
        var currentDate = DateTime.Now;
        var basePrompt = $@"你是KnewFate的专业命理顾问，结合了现代心理学和传统命理智慧。今天是{currentDate:yyyy年M月d日}。

用户咨询：{userMessage}

请根据以下指导原则回答：

1. 语言风格：温暖、专业、充满智慧，使用中文回答
2. 内容要求：
   - 结合占星学、塔罗牌、中国传统命理等知识
   - 提供具体可行的建议
   - 保持积极正面的态度
   - 适当使用象征符号增加神秘感（如✨🔮💫⭐️🌙）

3. 回答结构：
   - 开场：简短的共鸣或理解
   - 分析：基于命理知识的深度解读
   - 建议：具体可行的指导方案
   - 结尾：鼓励和祝福

4. 特殊情况处理：
   - 如果涉及星座，考虑当前时间的星座运势
   - 如果涉及塔罗，可以虚拟抽取相关牌面
   - 如果涉及情感，提供理性而温暖的建议
   - 如果涉及财运，结合实际行动建议

请提供一个专业、温暖且富有洞察力的回答（150-300字）：";

        // Add zodiac context if relevant
        if (ContainsZodiacKeywords(userMessage))
        {
            try
            {
                var zodiacContext = await GetZodiacContextAsync(currentDate);
                basePrompt += $"\n\n当前星座运势参考：{zodiacContext}";
            }
            catch
            {
                // Continue without zodiac context if error occurs
            }
        }

        return basePrompt;
    }

    private bool ContainsZodiacKeywords(string message)
    {
        var zodiacKeywords = new[] 
        { 
            "星座", "运势", "白羊", "金牛", "双子", "巨蟹", "狮子", "处女", 
            "天秤", "天蝎", "射手", "摩羯", "水瓶", "双鱼", "占星", "星象" 
        };
        
        return zodiacKeywords.Any(keyword => message.Contains(keyword));
    }

    private async Task<string> GetZodiacContextAsync(DateTime date)
    {
        try
        {
            // Get current zodiac sign
            var currentSign = await _zodiacService.GetZodiacSignFromDateAsync(date);
            var horoscope = await _zodiacService.GetDailyHoroscopeAsync(currentSign, "zh");
            
            return $"当前{currentSign}座运势：{horoscope}";
        }
        catch
        {
            return "星座运势：今日是充满可能性的一天，适合保持开放的心态。";
        }
    }

    private string GetFallbackResponse(string userMessage)
    {
        var fallbackResponses = new[]
        {
            "✨ 感谢您的咨询。根据您的问题，我建议您保持内心的平静，相信自己的直觉。每个人都有自己独特的人生轨迹，关键是要学会倾听内心的声音。",
            
            "🔮 您提出了一个很有深度的问题。在命理学中，我们相信一切都有其因果联系。建议您多关注当下的感受，同时保持对未来的希望和信心。",
            
            "💫 从您的问题中，我感受到了您对生活的思考。建议您可以通过冥想或静心来寻找答案，有时候最好的指导就来自我们内心深处的智慧。",
            
            "⭐️ 这是一个很好的咨询方向。我建议您保持积极的心态，相信生活中的每一个挑战都是成长的机会。同时，多与身边的人交流，往往能获得意想不到的启发。"
        };

        var random = new Random();
        return fallbackResponses[random.Next(fallbackResponses.Length)];
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
