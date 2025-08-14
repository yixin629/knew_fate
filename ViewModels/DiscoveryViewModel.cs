using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KnewFate.Models;
using KnewFate.Services;

namespace KnewFate.ViewModels;

public partial class DiscoveryViewModel : BaseViewModel
{
    private readonly ISocialService _socialService;
    private readonly IZodiacService _zodiacService;
    private readonly ILocalizationService _localizationService;

    [ObservableProperty]
    private ObservableCollection<UserProfile> users = new();

    [ObservableProperty]
    private string searchQuery = string.Empty;

    [ObservableProperty]
    private bool isCompatibilityMode = true;

    [ObservableProperty]
    private bool hasUsers = false;

    [ObservableProperty]
    private bool isEmpty = false;

    [ObservableProperty]
    private string matchModeText = "智能匹配";

    [ObservableProperty]
    private string pageTitle = "发现";

    [ObservableProperty]
    private string searchPlaceholder = "搜索用户...";

    [ObservableProperty]
    private string emptyStateText = "暂无推荐用户\n尝试调整筛选条件或稍后再试";

    [ObservableProperty]
    private string refreshButtonText = "刷新";

    // New filter summary (English comment required): human readable summary of current filters
    [ObservableProperty]
    private string filterSummary = "全部";

    public ICommand SearchCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand ToggleMatchModeCommand { get; }
    public ICommand ShowFiltersCommand { get; }
    public ICommand ViewUserProfileCommand { get; }
    public ICommand LikeUserCommand { get; }
    public ICommand SuperLikeUserCommand { get; }
    public ICommand ShowMyProfileCommand { get; }
    public ICommand PassUserCommand { get; } // Command for skipping a user card
    public ICommand ShowSettingsCommand { get; } // Navigate to settings page

    public DiscoveryViewModel(
        ISocialService socialService,
        IZodiacService zodiacService,
        ILocalizationService localizationService)
    {
        _socialService = socialService;
        _zodiacService = zodiacService;
        _localizationService = localizationService;

        SearchCommand = new AsyncRelayCommand(SearchUsersAsync);
        RefreshCommand = new AsyncRelayCommand(RefreshUsersAsync);
        ToggleMatchModeCommand = new AsyncRelayCommand(ToggleMatchModeAsync);
        ShowFiltersCommand = new AsyncRelayCommand(ShowFiltersAsync);
        ViewUserProfileCommand = new AsyncRelayCommand<int>(ViewUserProfileAsync);
        LikeUserCommand = new AsyncRelayCommand<int>(LikeUserAsync);
        SuperLikeUserCommand = new AsyncRelayCommand<int>(SuperLikeUserAsync);
        ShowMyProfileCommand = new AsyncRelayCommand(ShowMyProfileAsync);
    PassUserCommand = new AsyncRelayCommand<int>(PassUserAsync); // Initialize pass command
    ShowSettingsCommand = new AsyncRelayCommand(ShowSettingsAsync); // Initialize settings navigation

        LoadInitialData();
    }

    private async void LoadInitialData()
    {
        await UpdateLocalizationAsync();
        await LoadRecommendedUsersAsync();
    }

    private async Task UpdateLocalizationAsync()
    {
        PageTitle = await _localizationService.GetStringAsync("Discovery");
        SearchPlaceholder = await _localizationService.GetStringAsync("SearchUsers");
        EmptyStateText = await _localizationService.GetStringAsync("NoUsersFound");
        RefreshButtonText = await _localizationService.GetStringAsync("Refresh");
        UpdateMatchModeText();
    }

    private void UpdateMatchModeText()
    {
        MatchModeText = IsCompatibilityMode ? "智能匹配" : "附近的人";
    }

    private async Task LoadRecommendedUsersAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            Users.Clear();

            var currentUserId = await GetCurrentUserIdAsync();
            List<UserProfile> newUsers;

            if (IsCompatibilityMode)
            {
                newUsers = await _socialService.GetRecommendedUsersAsync(currentUserId, 0, 20);
                // Calculate compatibility scores
                foreach (var user in newUsers)
                {
                    var compatibility = await _socialService.CalculateCompatibilityAsync(currentUserId, user.UserId);
                    user.CompatibilityScore = compatibility.OverallScore;
                }
            }
            else
            {
                var currentUserProfile = await _socialService.GetUserProfileAsync(currentUserId);
                newUsers = await _socialService.GetNearbyUsersAsync(currentUserProfile.Country, currentUserProfile.City, 0, 20);
            }

            foreach (var user in newUsers)
            {
                // Load zodiac profile
                user.ZodiacProfile = await _zodiacService.GetZodiacProfileAsync(user.UserId);
                Users.Add(user);
            }

            UpdateUIState();
        }
        catch (Exception ex)
        {
            await ShowErrorAsync("加载用户失败", ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task SearchUsersAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchQuery))
        {
            await LoadRecommendedUsersAsync();
            return;
        }

        if (IsBusy) return;

        try
        {
            IsBusy = true;
            Users.Clear();

            var searchResults = await _socialService.SearchUsersAsync(SearchQuery, 0, 20);
            
            foreach (var user in searchResults)
            {
                user.ZodiacProfile = await _zodiacService.GetZodiacProfileAsync(user.UserId);
                Users.Add(user);
            }

            UpdateUIState();
        }
        catch (Exception ex)
        {
            await ShowErrorAsync("搜索失败", ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task RefreshUsersAsync()
    {
        IsRefreshing = true;
        await LoadRecommendedUsersAsync();
        IsRefreshing = false;
    }

    private async Task ToggleMatchModeAsync()
    {
        IsCompatibilityMode = !IsCompatibilityMode;
        UpdateMatchModeText();
        await LoadRecommendedUsersAsync();
    }

    private async Task ShowFiltersAsync()
    {
        // Navigate to filters page
        await Shell.Current.DisplayAlert("筛选", "筛选功能即将推出", "确定");
    }

    private async Task ViewUserProfileAsync(int userId)
    {
        try
        {
            // Record profile view
            var currentUserId = await GetCurrentUserIdAsync();
            await _socialService.ViewProfileAsync(currentUserId, userId);

            // Navigate to user profile page
            await Shell.Current.GoToAsync($"userprofile?userId={userId}");
        }
        catch (Exception ex)
        {
            await ShowErrorAsync("查看用户失败", ex.Message);
        }
    }

    private async Task LikeUserAsync(int userId)
    {
        try
        {
            var currentUserId = await GetCurrentUserIdAsync();
            var success = await _socialService.LikeUserAsync(currentUserId, userId);

            if (success)
            {
                // Check if it's a mutual match
                var isMutual = await _socialService.IsMutualMatchAsync(currentUserId, userId);
                
                if (isMutual)
                {
                    await Shell.Current.DisplayAlert("匹配成功！", "你们互相喜欢，现在可以开始聊天了！", "太棒了");
                    await Shell.Current.GoToAsync($"chat?userId={userId}");
                }
                else
                {
                    await Shell.Current.DisplayAlert("点赞成功", "已向对方发送喜欢信号", "确定");
                }

                // Remove user from list or update UI
                var user = Users.FirstOrDefault(u => u.UserId == userId);
                if (user != null)
                {
                    Users.Remove(user);
                    UpdateUIState();
                }
            }
        }
        catch (Exception ex)
        {
            await ShowErrorAsync("点赞失败", ex.Message);
        }
    }

    private async Task SuperLikeUserAsync(int userId)
    {
        try
        {
            var currentUserId = await GetCurrentUserIdAsync();
            
            // Check if user can super like
            var canSuperLike = await _socialService.CanAccessFeatureAsync(currentUserId, "SuperLike");
            
            if (!canSuperLike)
            {
                var result = await Shell.Current.DisplayAlert(
                    "超级喜欢", 
                    "您需要VIP会员或更多积分才能使用超级喜欢功能。是否查看会员套餐？", 
                    "查看", "取消");
                
                if (result)
                {
                    await Shell.Current.GoToAsync("premium");
                }
                return;
            }

            var success = await _socialService.SuperLikeUserAsync(currentUserId, userId);

            if (success)
            {
                await Shell.Current.DisplayAlert("超级喜欢发送成功！", "对方会优先看到您的超级喜欢", "确定");
                
                // Remove user from list
                var user = Users.FirstOrDefault(u => u.UserId == userId);
                if (user != null)
                {
                    Users.Remove(user);
                    UpdateUIState();
                }
            }
        }
        catch (Exception ex)
        {
            await ShowErrorAsync("超级喜欢失败", ex.Message);
        }
    }

    private async Task PassUserAsync(int userId)
    {
        // Skip current user card: remove from list and (optionally) record interaction
        try
        {
            var user = Users.FirstOrDefault(u => u.UserId == userId);
            if (user != null)
            {
                Users.Remove(user);
                UpdateUIState();
            }

            // Placeholder: record a "pass" interaction if needed in future
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            await ShowErrorAsync("跳过失败", ex.Message);
        }
    }

    private async Task ShowSettingsAsync()
    {
        // Navigate to a settings shell route (adjust if route differs)
        await Shell.Current.GoToAsync("settings");
    }

    private async Task ShowMyProfileAsync()
    {
        await Shell.Current.GoToAsync("myprofile");
    }

    private void UpdateUIState()
    {
        HasUsers = Users.Any();
        IsEmpty = !HasUsers && !IsBusy;
    }

    private async Task<int> GetCurrentUserIdAsync()
    {
        // This would typically come from a user service or authentication
        // For now, return a placeholder
        return 1;
    }
}
