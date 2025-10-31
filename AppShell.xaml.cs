namespace KnewFate;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        
        // Register routes for navigation
        Routing.RegisterRoute("onboarding", typeof(Views.OnboardingPage));
        Routing.RegisterRoute("dashboard", typeof(Views.DashboardPage));
        Routing.RegisterRoute("discovery", typeof(Views.DiscoveryPage));
        Routing.RegisterRoute("dailydiscovery", typeof(Views.DailyDiscoveryPage));
        Routing.RegisterRoute("charthub", typeof(Views.ChartHubPage));
        Routing.RegisterRoute("relationship", typeof(Views.RelationshipPage));
        Routing.RegisterRoute("career", typeof(Views.CareerPage));
        Routing.RegisterRoute("timeline", typeof(Views.TimelinePage));
        Routing.RegisterRoute("tarot", typeof(Views.TarotPage));
        Routing.RegisterRoute("ai_assistant", typeof(Views.AIAssistantPage));
        Routing.RegisterRoute("ai_dashboard", typeof(Views.AICustomerServiceDashboard));
        Routing.RegisterRoute("settings", typeof(Views.SettingsPage));
        
        // Register modal/detail pages
        Routing.RegisterRoute("userprofile", typeof(Views.UserProfilePage));
        Routing.RegisterRoute("compatibility", typeof(Views.CompatibilityPage));
        Routing.RegisterRoute("chat", typeof(Views.ChatPage));
        Routing.RegisterRoute("premium", typeof(Views.PremiumPage));
        Routing.RegisterRoute("credits", typeof(Views.CreditsPage));
        Routing.RegisterRoute("myprofile", typeof(Views.MyProfilePage));
        
        // Register authentication pages
        Routing.RegisterRoute("login", typeof(Views.LoginPage));
        Routing.RegisterRoute("register", typeof(Views.RegisterPage));
        Routing.RegisterRoute("forgotpassword", typeof(Views.ForgotPasswordPage));
    }
}
