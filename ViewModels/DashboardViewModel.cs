using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using KnewFate.Models;
using KnewFate.Services;

namespace KnewFate.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly ILocalizationService _localizationService;
    private readonly IFusionEngineService _fusionEngineService;

    [ObservableProperty]
    private string welcomeMessage = string.Empty;

    [ObservableProperty]
    private double loveEnergyProgress = 0.75;

    [ObservableProperty]
    private string loveEnergyText = "High";

    [ObservableProperty]
    private double careerEnergyProgress = 0.65;

    [ObservableProperty]
    private string careerEnergyText = "Good";

    [ObservableProperty]
    private double overallEnergyProgress = 0.70;

    [ObservableProperty]
    private string overallEnergyText = "Positive";

    [ObservableProperty]
    private string dailyGuidanceText = string.Empty;

    [ObservableProperty]
    private ObservableCollection<InsightItem> recentInsights = new();

    public DashboardViewModel(ILocalizationService localizationService, IFusionEngineService fusionEngineService)
    {
        _localizationService = localizationService;
        _fusionEngineService = fusionEngineService;
        
        InitializeData();
    }

    private async void InitializeData()
    {
        WelcomeMessage = await GetWelcomeMessage();
        DailyGuidanceText = await GetDailyGuidance();
        await LoadRecentInsights();
        await UpdateEnergyLevels();
    }

    private async Task<string> GetWelcomeMessage()
    {
        // In a real app, this would come from user data
        var hour = DateTime.Now.Hour;
        string greeting = hour < 12 ? "Good Morning" : hour < 18 ? "Good Afternoon" : "Good Evening";
        return $"{greeting}! Ready to explore your destiny today?";
    }

    private async Task<string> GetDailyGuidance()
    {
        // This would come from the fusion engine in a real app
        var guidances = new[]
        {
            "Today is a great day for new beginnings. The stars align favorably for taking decisive action.",
            "Focus on relationships today. Your emotional intelligence will be particularly strong.",
            "Creative energy flows strongly today. Consider artistic or innovative pursuits.",
            "A day for reflection and planning. Trust your intuition when making important decisions.",
            "Communication is highlighted today. Express yourself clearly and listen actively to others."
        };
        
        var random = new Random();
        return guidances[random.Next(guidances.Length)];
    }

    private async Task LoadRecentInsights()
    {
        // In a real app, this would come from database
        RecentInsights.Clear();
        
        var insights = new[]
        {
            new InsightItem 
            { 
                Title = "Career Breakthrough Period", 
                Description = "Your professional life enters a dynamic phase with Jupiter's influence on your 10th house.",
                Date = DateTime.Now.AddDays(-1)
            },
            new InsightItem 
            { 
                Title = "Relationship Harmony", 
                Description = "Venus aspects suggest improved communication with loved ones this week.",
                Date = DateTime.Now.AddDays(-2)
            },
            new InsightItem 
            { 
                Title = "Financial Stability", 
                Description = "Your BaZi chart indicates a stable financial period with steady growth potential.",
                Date = DateTime.Now.AddDays(-3)
            }
        };

        foreach (var insight in insights)
        {
            RecentInsights.Add(insight);
        }
    }

    private async Task UpdateEnergyLevels()
    {
        // In a real app, this would calculate from actual chart data
        var random = new Random();
        
        LoveEnergyProgress = random.NextDouble() * 0.4 + 0.5; // 50-90%
        CareerEnergyProgress = random.NextDouble() * 0.4 + 0.5; // 50-90%
        OverallEnergyProgress = (LoveEnergyProgress + CareerEnergyProgress) / 2;

        // Update text based on values
        LoveEnergyText = LoveEnergyProgress > 0.8 ? "Excellent" : 
                        LoveEnergyProgress > 0.6 ? "Good" : "Moderate";
        
        CareerEnergyText = CareerEnergyProgress > 0.8 ? "Excellent" : 
                          CareerEnergyProgress > 0.6 ? "Good" : "Moderate";
        
        OverallEnergyText = OverallEnergyProgress > 0.8 ? "Excellent" : 
                           OverallEnergyProgress > 0.6 ? "Positive" : "Stable";
    }

    [RelayCommand]
    private async Task NavigateToTarot()
    {
        await Shell.Current.GoToAsync("//tarot");
    }

    [RelayCommand]
    private async Task NavigateToAIAssistant()
    {
        await Shell.Current.GoToAsync("//ai_assistant");
    }

    [RelayCommand]
    private async Task NavigateToChartHub()
    {
        await Shell.Current.GoToAsync("//charthub");
    }

    [RelayCommand]
    private async Task NavigateToRelationship()
    {
        await Shell.Current.GoToAsync("//relationship");
    }

    [RelayCommand]
    private async Task NavigateToCareer()
    {
        await Shell.Current.GoToAsync("//career");
    }

    [RelayCommand]
    private async Task RefreshData()
    {
        await UpdateEnergyLevels();
        DailyGuidanceText = await GetDailyGuidance();
        await LoadRecentInsights();
    }
}
