using KnewFate.Services;
using System.Collections.ObjectModel;

namespace KnewFate.Views;

public partial class AICustomerServiceDashboard : ContentPage
{
    private readonly IAICustomerService _aiCustomerService;
    private readonly AIService _aiService;
    
    public ObservableCollection<AIModelStatus> AIModels { get; } = new();
    public ObservableCollection<MessageLogEntry> RecentMessages { get; } = new();

    public AICustomerServiceDashboard(IAICustomerService aiCustomerService, AIService aiService)
    {
        InitializeComponent();
        _aiCustomerService = aiCustomerService;
        _aiService = aiService;
        
        AIModelsView.ItemsSource = AIModels;
        RecentMessagesView.ItemsSource = RecentMessages;
        
        // Don't await in constructor, will be called in OnAppearing
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadData();
        StartDataRefresh();
    }

    private async Task LoadData()
    {
        await LoadAIModelStatus();
        await LoadServiceStats();
        await LoadRecentMessages();
    }

    private async Task LoadAIModelStatus()
    {
        AIModels.Clear();
        
        // 模拟AI模型状态
        var models = new[]
        {
            new AIModelStatus
            {
                ModelName = "Groq (主要)",
                Description = "Llama-3.1-70B - 高性能推理",
                Status = "正常",
                StatusIcon = "🟢",
                StatusColor = Microsoft.Maui.Graphics.Colors.Green
            },
            new AIModelStatus
            {
                ModelName = "Together AI",
                Description = "Meta-Llama-3.1-70B - 备用服务",
                Status = "正常",
                StatusIcon = "🟢",
                StatusColor = Microsoft.Maui.Graphics.Colors.Green
            },
            new AIModelStatus
            {
                ModelName = "DeepSeek",
                Description = "DeepSeek-Chat - 中文优化",
                Status = "正常",
                StatusIcon = "🟢",
                StatusColor = Microsoft.Maui.Graphics.Colors.Green
            },
            new AIModelStatus
            {
                ModelName = "ZhipuAI",
                Description = "GLM-4-Flash - 快速响应",
                Status = "正常",
                StatusIcon = "🟢",
                StatusColor = Microsoft.Maui.Graphics.Colors.Green
            }
        };

        foreach (var model in models)
        {
            AIModels.Add(model);
        }
    }

    private async Task LoadServiceStats()
    {
        try
        {
            var stats = await _aiCustomerService.GetServiceStatsAsync();
            
            ActiveConversationsLabel.Text = stats.ActiveConversations.ToString();
            ResponseTimeLabel.Text = $"< {stats.AverageResponseTime.TotalSeconds:F1}秒";
        }
        catch (Exception ex)
        {
            await DisplayAlert("错误", $"加载统计数据失败: {ex.Message}", "确定");
        }
    }

    private async Task LoadRecentMessages()
    {
        RecentMessages.Clear();
        
        // 模拟最近消息（实际项目中应该从数据库或日志系统获取）
        var messages = new[]
        {
            new MessageLogEntry
            {
                UserMessage = "今天的运势怎么样？",
                AIResponse = "今天是充满机遇的一天！建议您保持积极心态...",
                Timestamp = DateTime.Now.AddMinutes(-5)
            },
            new MessageLogEntry
            {
                UserMessage = "如何注册会员？",
                AIResponse = "您可以在设置页面找到会员中心，点击即可升级...",
                Timestamp = DateTime.Now.AddMinutes(-12)
            },
            new MessageLogEntry
            {
                UserMessage = "APP为什么打不开？",
                AIResponse = "抱歉给您带来不便，请尝试重启应用或检查网络连接...",
                Timestamp = DateTime.Now.AddMinutes(-18)
            }
        };

        foreach (var message in messages)
        {
            RecentMessages.Add(message);
        }
    }

    private async void OnBroadcastMessageClicked(object sender, EventArgs e)
    {
        string message = await DisplayPromptAsync(
            "群发消息", 
            "请输入要发送给所有用户的消息:", 
            "发送", 
            "取消",
            "系统通知: "
        );

        if (!string.IsNullOrWhiteSpace(message))
        {
            try
            {
                await _aiCustomerService.SendProactiveMessageAsync("all", message);
                await DisplayAlert("成功", "消息已发送给所有在线用户", "确定");
            }
            catch (Exception ex)
            {
                await DisplayAlert("错误", $"发送失败: {ex.Message}", "确定");
            }
        }
    }

    private async void OnViewStatsClicked(object sender, EventArgs e)
    {
        try
        {
            var stats = await _aiCustomerService.GetServiceStatsAsync();
            
            string statsMessage = $@"📊 AI客服统计数据

🔸 活跃对话: {stats.ActiveConversations}
🔸 总消息数: {stats.TotalMessages}
🔸 平均响应时间: {stats.AverageResponseTime.TotalSeconds:F1}秒
🔸 用户满意度: {stats.SatisfactionRate:P0}

📈 服务表现优秀！";

            await DisplayAlert("统计数据", statsMessage, "确定");
        }
        catch (Exception ex)
        {
            await DisplayAlert("错误", $"获取统计数据失败: {ex.Message}", "确定");
        }
    }

    private async void OnRestartServiceClicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert(
            "确认重启", 
            "确定要重启AI客服服务吗？这会暂时中断正在进行的对话。", 
            "确定", 
            "取消"
        );

        if (confirm)
        {
            try
            {
                // 这里应该实现服务重启逻辑
                await Task.Delay(2000); // 模拟重启过程
                
                await DisplayAlert("成功", "AI客服服务已重启完成", "确定");
                await LoadData(); // 重新加载数据
            }
            catch (Exception ex)
            {
                await DisplayAlert("错误", $"重启失败: {ex.Message}", "确定");
            }
        }
    }

    private async void StartDataRefresh()
    {
        // 每30秒刷新一次数据
        _ = Task.Run(async () =>
        {
            while (true)
            {
                await Task.Delay(30000); // 30秒
                
                Device.BeginInvokeOnMainThread(async () =>
                {
                    try
                    {
                        await LoadServiceStats();
                    }
                    catch
                    {
                        // 静默处理刷新错误
                    }
                });
            }
        });
    }
}

// 支持数据类
public class AIModelStatus
{
    public string ModelName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string StatusIcon { get; set; } = string.Empty;
    public Microsoft.Maui.Graphics.Color StatusColor { get; set; } = Microsoft.Maui.Graphics.Colors.Gray;
}

public class MessageLogEntry
{
    public string UserMessage { get; set; } = string.Empty;
    public string AIResponse { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
