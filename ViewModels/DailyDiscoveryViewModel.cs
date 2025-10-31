using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KnewFate.Models;
using KnewFate.Services;

namespace KnewFate.ViewModels;

public partial class DailyDiscoveryViewModel : BaseViewModel
{
    private readonly IDailyRecommendationService _recommendationService;
    private readonly ILocalizationService _localizationService;

    [ObservableProperty]
    private ObservableCollection<RecommendationItem> recommendations = new();

    [ObservableProperty]
    private ObservableCollection<RecommendationItem> topTenRecommendations = new();

    [ObservableProperty]
    private string pageTitle = "Daily Discoveries";

    [ObservableProperty]
    private string lastUpdated = string.Empty;

    [ObservableProperty]
    private bool hasRecommendations = false;

    [ObservableProperty]
    private bool isEmpty = false;

    [ObservableProperty]
    private int totalRecommendations = 0;

    [ObservableProperty]
    private DateTime currentDate = DateTime.Now;

    public ICommand RefreshCommand { get; }
    public ICommand ViewRecommendationCommand { get; }
    public ICommand ShowAllCommand { get; }

    public DailyDiscoveryViewModel(
        IDailyRecommendationService recommendationService,
        ILocalizationService localizationService)
    {
        _recommendationService = recommendationService;
        _localizationService = localizationService;

        RefreshCommand = new AsyncRelayCommand(RefreshRecommendationsAsync);
        ViewRecommendationCommand = new AsyncRelayCommand<RecommendationItem>(ViewRecommendationAsync);
        ShowAllCommand = new AsyncRelayCommand(ShowAllRecommendationsAsync);

        LoadInitialData();
    }

    private async void LoadInitialData()
    {
        await UpdateLocalizationAsync();
        await LoadRecommendationsAsync();
    }

    private async Task UpdateLocalizationAsync()
    {
        PageTitle = "Daily Discoveries";
        UpdateLastUpdatedText();
    }

    private void UpdateLastUpdatedText()
    {
        LastUpdated = $"Updated: {DateTime.Now:MMM dd, yyyy HH:mm}";
    }

    private async Task LoadRecommendationsAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            Recommendations.Clear();
            TopTenRecommendations.Clear();

            var currentUserId = await GetCurrentUserIdAsync();
            var dailyList = await _recommendationService.GetDailyRecommendationsAsync(currentUserId, CurrentDate);

            // Load all recommendations
            foreach (var item in dailyList.Items)
            {
                Recommendations.Add(item);
                
                // Also add to top 10 if applicable
                if (item.IsTopTen)
                {
                    TopTenRecommendations.Add(item);
                }
            }

            TotalRecommendations = Recommendations.Count;
            UpdateUIState();
            UpdateLastUpdatedText();
        }
        catch (Exception ex)
        {
            await ShowErrorAsync("Failed to load recommendations", ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task RefreshRecommendationsAsync()
    {
        IsRefreshing = true;
        CurrentDate = DateTime.Now; // Reset to today
        await LoadRecommendationsAsync();
        IsRefreshing = false;
    }

    private async Task ViewRecommendationAsync(RecommendationItem? item)
    {
        if (item == null) return;

        try
        {
            // Mark as viewed
            var currentUserId = await GetCurrentUserIdAsync();
            await _recommendationService.MarkRecommendationViewedAsync(currentUserId, item.Id);

            // Navigate based on action URL
            if (!string.IsNullOrEmpty(item.ActionUrl))
            {
                if (item.ActionUrl.Contains("?"))
                {
                    await Shell.Current.GoToAsync(item.ActionUrl);
                }
                else
                {
                    await Shell.Current.GoToAsync($"//{item.ActionUrl}");
                }
            }
        }
        catch (Exception ex)
        {
            await ShowErrorAsync("Navigation failed", ex.Message);
        }
    }

    private async Task ShowAllRecommendationsAsync()
    {
        // Toggle between showing all and showing top 10
        await Task.CompletedTask;
    }

    private void UpdateUIState()
    {
        HasRecommendations = Recommendations.Any();
        IsEmpty = !HasRecommendations && !IsBusy;
    }

    private async Task<int> GetCurrentUserIdAsync()
    {
        // In a real app, this would come from authentication service
        return 1;
    }

    public string GetCategoryIcon(string category)
    {
        return category switch
        {
            "User" => "👤",
            "Reading" => "🔮",
            "Insight" => "💡",
            "Event" => "📅",
            _ => "⭐"
        };
    }

    public Color GetCategoryColor(string category)
    {
        return category switch
        {
            "User" => Colors.Purple,
            "Reading" => Colors.Blue,
            "Insight" => Colors.Orange,
            "Event" => Colors.Green,
            _ => Colors.Gray
        };
    }

    public string GetRankBadge(int rank)
    {
        return rank switch
        {
            1 => "🥇",
            2 => "🥈",
            3 => "🥉",
            _ => $"#{rank}"
        };
    }
}
