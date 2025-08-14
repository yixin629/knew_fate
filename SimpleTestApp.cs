using Microsoft.Maui.Controls;

namespace KnewFate;

public class SimpleTestApp : Application
{
    public SimpleTestApp()
    {
        // 创建一个极简的测试页面
        MainPage = new ContentPage
        {
            Title = "KnewFate - UI 测试",
            BackgroundColor = Colors.DarkBlue,
            Content = new StackLayout
            {
                Padding = 40,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center,
                Children =
                {
                    new Label
                    {
                        Text = "🌟 KnewFate 启动成功！🌟",
                        FontSize = 36,
                        TextColor = Colors.Gold,
                        HorizontalOptions = LayoutOptions.Center,
                        HorizontalTextAlignment = TextAlignment.Center,
                        Margin = new Thickness(0, 20)
                    },
                    new Label
                    {
                        Text = "多传统命理计算器",
                        FontSize = 24,
                        TextColor = Colors.White,
                        HorizontalOptions = LayoutOptions.Center,
                        HorizontalTextAlignment = TextAlignment.Center,
                        Margin = new Thickness(0, 10)
                    },
                    new Label
                    {
                        Text = "✨ 应用程序界面正常显示！ ✨",
                        FontSize = 20,
                        TextColor = Colors.LightGreen,
                        HorizontalOptions = LayoutOptions.Center,
                        HorizontalTextAlignment = TextAlignment.Center,
                        Margin = new Thickness(0, 20)
                    },
                    new Frame
                    {
                        BackgroundColor = Colors.Purple,
                        CornerRadius = 10,
                        Padding = 20,
                        Margin = new Thickness(0, 30),
                        Content = new Label
                        {
                            Text = "功能模块：\n🔮 占星算命\n⭐ 星座匹配\n🎯 八字命理\n🃏 塔罗牌占卜\n🤖 AI智能咨询",
                            FontSize = 16,
                            TextColor = Colors.White,
                            HorizontalOptions = LayoutOptions.Center,
                            HorizontalTextAlignment = TextAlignment.Center
                        }
                    },
                    new Button
                    {
                        Text = "🎉 测试按钮 - 点击我！ 🎉",
                        BackgroundColor = Colors.Orange,
                        TextColor = Colors.Black,
                        FontSize = 18,
                        CornerRadius = 25,
                        HeightRequest = 50,
                        Margin = new Thickness(0, 30),
                        Command = new Command(async () => 
                        {
                            await Application.Current?.MainPage?.DisplayAlert("🎊 测试成功！", "按钮点击成功！\n界面交互正常工作！\n\n🎯 Windows MAUI 应用程序运行良好！", "太棒了！");
                        })
                    }
                }
            }
        };
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = base.CreateWindow(activationState);
        
        window.Title = "KnewFate - 传统命理计算器测试版";
        
        // 设置窗口大小
        window.Width = 900;
        window.Height = 700;
        window.MinimumWidth = 600;
        window.MinimumHeight = 400;
        
        return window;
    }
}
