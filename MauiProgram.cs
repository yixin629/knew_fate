using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
using KnewFate.Services;
using KnewFate.ViewModels;
using KnewFate.Views;
using Microsoft.Extensions.Localization;
using System.Globalization;
using System.Reflection;
using System.Diagnostics;
using System.IO;

namespace KnewFate;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        // Early diagnostic logging before anything else (helps capture crashes before App() constructor)
        var earlyLogPath = Path.Combine(AppContext.BaseDirectory, "early_startup.log");
        void EarlyLog(string msg)
        {
            try { File.AppendAllText(earlyLogPath, DateTime.Now.ToString("O") + " [MauiProgram] " + msg + Environment.NewLine); } catch { /* ignore */ }
        }
        EarlyLog("=== CreateMauiApp entered ===");
        EarlyLog($"Process: {Process.GetCurrentProcess().Id} PID, FrameworkDescription={System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}");

        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        EarlyLog("Builder created and fonts configured");

#if DEBUG
        builder.Services.AddLogging(logging => logging.AddDebug());
#endif

        EarlyLog("Logging service added (DEBUG)");

        // Register Services
        builder.Services.AddSingleton<IDatabaseService, DatabaseService>();
        builder.Services.AddSingleton<ILocalizationService, LocalizationService>();
        builder.Services.AddSingleton<IChartCalculationService, ChartCalculationService>();
        builder.Services.AddSingleton<IFusionEngineService, FusionEngineService>();
        builder.Services.AddSingleton<ITarotService, TarotService>();
        builder.Services.AddSingleton<IAstrologyService, AstrologyService>();
        builder.Services.AddSingleton<IBaziService, BaziService>();
        builder.Services.AddSingleton<IRelationshipService, RelationshipService>();
        builder.Services.AddSingleton<IUserPreferencesService, UserPreferencesService>();
        builder.Services.AddSingleton<IAuthenticationService, AuthenticationService>();
        
        // AI Services
        builder.Services.AddHttpClient<IAIService, AIService>();
        builder.Services.AddSingleton<IAICustomerService, AICustomerService>();
        builder.Services.AddHttpClient<IStoryGenerationService, StoryGenerationService>();
        // 暂时注释掉占星服务直到定义了相关类型
        // builder.Services.AddSingleton<ZodiacService>(); // Base zodiac service
        // builder.Services.AddSingleton<IZodiacService, AIEnhancedZodiacService>(); // AI-enhanced wrapper
        
        // Social Services
        builder.Services.AddSingleton<ISocialService, SocialService>();
        builder.Services.AddSingleton<IZiweiService, ZiweiService>();
        builder.Services.AddSingleton<INumerologyService, NumerologyService>();
        builder.Services.AddSingleton<IFiveElementsService, FiveElementsService>();
        builder.Services.AddSingleton<ICareerGuidanceService, CareerGuidanceService>();
        builder.Services.AddSingleton<ITimelinePredictionService, TimelinePredictionService>();
        builder.Services.AddSingleton<IDailyFortuneService, DailyFortuneService>();

        // Register ViewModels
        builder.Services.AddTransient<MainViewModel>();
        builder.Services.AddTransient<DashboardViewModel>();
        builder.Services.AddTransient<ChartHubViewModel>();
        builder.Services.AddTransient<RelationshipViewModel>();
        builder.Services.AddTransient<CareerViewModel>();
        builder.Services.AddTransient<TimelineViewModel>();
        builder.Services.AddTransient<TarotViewModel>();
        builder.Services.AddTransient<SettingsViewModel>();
        builder.Services.AddTransient<OnboardingViewModel>();
        builder.Services.AddTransient<DiscoveryViewModel>();
        builder.Services.AddTransient<UserProfileViewModel>();
        builder.Services.AddTransient<ChatViewModel>();
        builder.Services.AddTransient<AIAssistantViewModel>();
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<RegisterViewModel>();
        builder.Services.AddTransient<ForgotPasswordViewModel>();
        builder.Services.AddTransient<StoryWriterViewModel>();

        // Register Views
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<DashboardPage>();
        builder.Services.AddTransient<ChartHubPage>();
        builder.Services.AddTransient<RelationshipPage>();
        builder.Services.AddTransient<CareerPage>();
        builder.Services.AddTransient<TimelinePage>();
        builder.Services.AddTransient<TarotPage>();
        builder.Services.AddTransient<SettingsPage>();
        builder.Services.AddTransient<OnboardingPage>();
        builder.Services.AddTransient<DiscoveryPage>();
        builder.Services.AddTransient<UserProfilePage>();
        builder.Services.AddTransient<ChatPage>();
        builder.Services.AddTransient<AIAssistantPage>();
        builder.Services.AddTransient<AICustomerServiceDashboard>();
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<RegisterPage>();
        builder.Services.AddTransient<ForgotPasswordPage>();
        builder.Services.AddTransient<StoryWriterPage>();

        // Configure Localization
        builder.Services.AddLocalization();

        try
        {
            var app = builder.Build();
            EarlyLog("MauiApp built successfully");
            return app;
        }
        catch (Exception buildEx)
        {
            EarlyLog("MauiApp build FAILED: " + buildEx);
            // Re-throw so the process still fails, but we keep the log.
            throw;
        }
    }
}
