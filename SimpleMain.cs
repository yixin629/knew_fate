using Microsoft.Maui.Controls;

namespace KnewFate;

public class App : Application
{
    public App()
    {
        MainPage = new MainPage();
    }
}

public class MainPage : ContentPage
{
    public MainPage()
    {
        Title = "KnewFate 测试应用";
        BackgroundColor = Color.FromArgb("#1a1a1a");

        var layout = new StackLayout
        {
            Spacing = 30,
            Padding = 40,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Center
        };

        var welcomeLabel = new Label
        {
            Text = "🎯 欢迎使用 KnewFate！",
            FontSize = 32,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#FFD700"),
            HorizontalOptions = LayoutOptions.Center
        };

        var statusLabel = new Label
        {
            Text = "✅ 应用界面已成功显示",
            FontSize = 20,
            TextColor = Color.FromArgb("#00FF00"),
            HorizontalOptions = LayoutOptions.Center
        };

        var frame = new Frame
        {
            BackgroundColor = Color.FromArgb("#2d2d2d"),
            BorderColor = Color.FromArgb("#FFD700"),
            CornerRadius = 15,
            Padding = 20,
            Content = new Label
            {
                Text = "🎉 恭喜！你现在可以看到 Windows 界面了！\n" +
                       "这表明 .NET MAUI 框架工作正常。\n" +
                       "点击下面的按钮测试交互功能。",
                FontSize = 16,
                TextColor = Colors.White,
                HorizontalTextAlignment = TextAlignment.Center
            }
        };

        var testButton = new Button
        {
            Text = "🚀 测试按钮交互",
            FontSize = 18,
            BackgroundColor = Color.FromArgb("#4CAF50"),
            TextColor = Colors.White,
            CornerRadius = 10,
            Padding = new Thickness(30, 15),
            HorizontalOptions = LayoutOptions.Center
        };

        testButton.Clicked += async (sender, e) =>
        {
            await DisplayAlert("成功！", "🎉 按钮点击功能正常工作！\n现在你可以继续开发你的应用了。", "太好了！");
        };

        layout.Children.Add(welcomeLabel);
        layout.Children.Add(statusLabel);
        layout.Children.Add(frame);
        layout.Children.Add(testButton);

        Content = layout;
    }
}

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        return builder.Build();
    }
}
