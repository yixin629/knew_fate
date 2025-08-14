using KnewFate.Services;
using KnewFate.Views;
using System.Globalization;

namespace KnewFate;

public partial class App : Application
{
    private static readonly string StartupLogPath = System.IO.Path.Combine(FileSystem.AppDataDirectory, "startup.log");

    static App()
    {
        try
        {
            System.IO.File.AppendAllText(StartupLogPath, DateTime.Now.ToString("O") + " [Static Constructor] App class loaded" + Environment.NewLine);
        }
        catch
        {
            // Ignore logging errors
        }
    }

    public App()
    {
        try
        {
            SafeLog("=== App constructor start ===");

            // Global exception hooks (to capture silent crashes)
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                SafeLog($"[UnhandledException] {e.ExceptionObject}");
            };
            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                SafeLog($"[UnobservedTaskException] {e.Exception}" );
                e.SetObserved();
            };

            try
            {
                InitializeComponent();
                SafeLog("InitializeComponent OK");
            }
            catch (Exception initEx)
            {
                SafeLog("InitializeComponent FAILED: " + initEx);
            }

            // Prefer full application Shell
            try
            {
                MainPage = new AppShell();
                SafeLog("AppShell created and set as MainPage");
            }
            catch (Exception ex)
            {
                SafeLog("AppShell creation FAILED: " + ex);
                // Fallback minimal diagnostic page if Shell fails to construct
                MainPage = new ContentPage
                {
                    Title = "KnewFate Diagnostics",
                    BackgroundColor = Colors.DarkBlue,
                    Content = new ScrollView
                    {
                        Content = new VerticalStackLayout
                        {
                            Padding = 30,
                            Spacing = 14,
                            Children =
                            {
                                new Label{ Text = "Shell failed to load.", FontSize=24, TextColor=Colors.Orange, HorizontalTextAlignment= TextAlignment.Center },
                                new Label{ Text = ex.Message, FontSize=14, TextColor=Colors.LightPink },
                                new Label{ Text = ex.StackTrace ?? "(no stack trace)", FontSize=12, TextColor=Colors.LightGray },
                                new Button
                                {
                                    Text = "Retry Load Shell",
                                    Command = new Command(() =>
                                    {
                                        try { MainPage = new AppShell(); SafeLog("Retry succeeded"); } catch (Exception retryEx) { SafeLog("Retry failed: " + retryEx); }
                                    })
                                }
                            }
                        }
                    }
                };
            }
            SafeLog("=== App constructor end ===");
        }
        catch (Exception ctorEx)
        {
            SafeLog("App constructor FAILED: " + ctorEx);
        }
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = base.CreateWindow(activationState);
        
        window.Title = "KnewFate - Multi-Traditional Destiny Calculator";
        
        // 设置合适的窗口大小
        window.Width = 800;
        window.Height = 600;
        window.MinimumWidth = 400;
        window.MinimumHeight = 300;
        
        return window;
    }

    private static void SafeLog(string line)
    {
        try
        {
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(StartupLogPath)!);
            System.IO.File.AppendAllText(StartupLogPath, DateTime.Now.ToString("O") + " " + line + Environment.NewLine);
        }
        catch
        {
            // swallow any logging error
        }
    }
}
