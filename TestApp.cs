using Microsoft.Maui.Controls;

namespace KnewFate;

public class TestApp : Application
{
    public TestApp()
    {
        MainPage = new TestPage();
    }
}

public class TestPage : ContentPage
{
    public TestPage()
    {
        Title = "测试界面";
        BackgroundColor = Color.FromArgb("#1a1a1a");

        var layout = new StackLayout
        {
            Spacing = 20,
            Padding = 30,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Center
        };

        var titleLabel = new Label
        {
            Text = "🎯 KnewFate 测试应用",
            FontSize = 28,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#FFD700"),
            HorizontalOptions = LayoutOptions.Center
        };

        var statusLabel = new Label
        {
            Text = "✅ 应用程序启动成功！",
            FontSize = 18,
            TextColor = Color.FromArgb("#00FF00"),
            HorizontalOptions = LayoutOptions.Center
        };

        var infoLabel = new Label
        {
            Text = "这是一个简化的测试界面，用于验证 MAUI 框架是否正常工作。",
            FontSize = 14,
            TextColor = Colors.White,
            HorizontalOptions = LayoutOptions.Center,
            HorizontalTextAlignment = TextAlignment.Center
        };

        var testButton = new Button
        {
            Text = "🚀 点击测试",
            FontSize = 16,
            BackgroundColor = Color.FromArgb("#4CAF50"),
            TextColor = Colors.White,
            CornerRadius = 10,
            Padding = new Thickness(20, 10),
            HorizontalOptions = LayoutOptions.Center
        };

        testButton.Clicked += async (sender, e) =>
        {
            await DisplayAlert("测试成功", "恭喜！界面交互正常工作！🎉", "确定");
        };

        layout.Children.Add(titleLabel);
        layout.Children.Add(statusLabel);
        layout.Children.Add(infoLabel);
        layout.Children.Add(testButton);

        Content = layout;
    }
}
