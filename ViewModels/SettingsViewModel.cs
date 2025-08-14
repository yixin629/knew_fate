using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using KnewFate.Services;
using KnewFate.Models;

namespace KnewFate.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly ILocalizationService _localizationService;
    private readonly IUserPreferencesService _userPreferencesService;

    [ObservableProperty]
    private ObservableCollection<LanguageOption> availableLanguages = new();

    [ObservableProperty]
    private LanguageOption selectedLanguage;

    [ObservableProperty]
    private bool dailyGuidanceEnabled = true;

    [ObservableProperty]
    private bool timelineUpdatesEnabled = true;

    [ObservableProperty]
    private bool relationshipInsightsEnabled = true;

    [ObservableProperty]
    private bool shareAnonymousDataEnabled = false;

    [ObservableProperty]
    private bool personalizedRecommendationsEnabled = true;

    [ObservableProperty]
    private bool showPremiumSection = true;

    [ObservableProperty]
    private string subscriptionStatus = "Free Plan";

    [ObservableProperty]
    private string subscriptionButtonText = "Upgrade to Premium";

    [ObservableProperty]
    private string appVersion = string.Empty;

    public SettingsViewModel(ILocalizationService localizationService, IUserPreferencesService userPreferencesService)
    {
        _localizationService = localizationService;
        _userPreferencesService = userPreferencesService;
        
        InitializeLanguages();
        LoadSettings();
    }

    private void InitializeLanguages()
    {
        AvailableLanguages.Clear();
        
        var languages = new[]
        {
            new LanguageOption 
            { 
                Code = "en-US", 
                DisplayName = "English", 
                NativeName = "English", 
                FlagIcon = "flag_us.png",
                IsSelected = false
            },
            new LanguageOption 
            { 
                Code = "zh-CN", 
                DisplayName = "Chinese (Simplified)", 
                NativeName = "中文（简体）", 
                FlagIcon = "flag_cn.png",
                IsSelected = false
            },
            new LanguageOption 
            { 
                Code = "zh-TW", 
                DisplayName = "Chinese (Traditional)", 
                NativeName = "中文（繁體）", 
                FlagIcon = "flag_tw.png",
                IsSelected = false
            }
        };

        foreach (var language in languages)
        {
            AvailableLanguages.Add(language);
        }

        // Set current language as selected
        var currentLanguage = _localizationService.CurrentCulture.Name;
        var selectedLang = AvailableLanguages.FirstOrDefault(l => l.Code == currentLanguage);
        if (selectedLang != null)
        {
            selectedLang.IsSelected = true;
            SelectedLanguage = selectedLang;
        }
    }

    private async void LoadSettings()
    {
        // Load app version
        AppVersion = $"Version {AppInfo.VersionString} (Build {AppInfo.BuildString})";
        
        // Load notification preferences
        DailyGuidanceEnabled = await _userPreferencesService.GetBoolAsync("daily_guidance_enabled", true);
        TimelineUpdatesEnabled = await _userPreferencesService.GetBoolAsync("timeline_updates_enabled", true);
        RelationshipInsightsEnabled = await _userPreferencesService.GetBoolAsync("relationship_insights_enabled", true);
        
        // Load privacy preferences
        ShareAnonymousDataEnabled = await _userPreferencesService.GetBoolAsync("share_anonymous_data", false);
        PersonalizedRecommendationsEnabled = await _userPreferencesService.GetBoolAsync("personalized_recommendations", true);
        
        // Load subscription status
        var isPremium = await _userPreferencesService.GetBoolAsync("is_premium", false);
        if (isPremium)
        {
            SubscriptionStatus = "Premium Plan - Active";
            SubscriptionButtonText = "Manage Subscription";
        }
    }

    [RelayCommand]
    private async Task SelectLanguage(LanguageOption language)
    {
        if (language == null || language == SelectedLanguage) return;

        // Update UI selection
        if (SelectedLanguage != null)
            SelectedLanguage.IsSelected = false;
        
        language.IsSelected = true;
        SelectedLanguage = language;

        // Apply language change
        await _localizationService.SetLanguageAsync(language.Code);
        
        // Show confirmation
        await Application.Current.MainPage.DisplayAlert(
            "Language Changed", 
            "The app language has been updated.", 
            "OK");
    }

    [RelayCommand]
    private async Task SaveNotificationSettings()
    {
        await _userPreferencesService.SetBoolAsync("daily_guidance_enabled", DailyGuidanceEnabled);
        await _userPreferencesService.SetBoolAsync("timeline_updates_enabled", TimelineUpdatesEnabled);
        await _userPreferencesService.SetBoolAsync("relationship_insights_enabled", RelationshipInsightsEnabled);
    }

    [RelayCommand]
    private async Task SavePrivacySettings()
    {
        await _userPreferencesService.SetBoolAsync("share_anonymous_data", ShareAnonymousDataEnabled);
        await _userPreferencesService.SetBoolAsync("personalized_recommendations", PersonalizedRecommendationsEnabled);
    }

    [RelayCommand]
    private async Task ExportData()
    {
        var result = await Application.Current.MainPage.DisplayAlert(
            "Export Data", 
            "Your personal data will be exported and shared with you via email. Continue?", 
            "Yes", 
            "Cancel");

        if (result)
        {
            // TODO: Implement data export
            await Application.Current.MainPage.DisplayAlert(
                "Export Requested", 
                "Your data export request has been submitted. You will receive an email within 24 hours.", 
                "OK");
        }
    }

    [RelayCommand]
    private async Task DeleteAccount()
    {
        var confirm = await Application.Current.MainPage.DisplayAlert(
            "Delete Account", 
            "This action cannot be undone. All your data will be permanently deleted. Are you sure?", 
            "Delete", 
            "Cancel");

        if (confirm)
        {
            var doubleConfirm = await Application.Current.MainPage.DisplayAlert(
                "Final Confirmation", 
                "Last chance - are you absolutely sure you want to delete your account?", 
                "Yes, Delete Everything", 
                "Cancel");

            if (doubleConfirm)
            {
                // TODO: Implement account deletion
                await Application.Current.MainPage.DisplayAlert(
                    "Account Deleted", 
                    "Your account has been scheduled for deletion. Thank you for using KnewFate.", 
                    "OK");
            }
        }
    }

    [RelayCommand]
    private async Task ManageSubscription()
    {
        var isPremium = await _userPreferencesService.GetBoolAsync("is_premium", false);
        
        if (isPremium)
        {
            // Open subscription management
            await Application.Current.MainPage.DisplayAlert(
                "Manage Subscription", 
                "Redirecting to subscription management...", 
                "OK");
        }
        else
        {
            // Show upgrade options
            await ShowUpgradeOptions();
        }
    }

    private async Task ShowUpgradeOptions()
    {
        var action = await Application.Current.MainPage.DisplayActionSheet(
            "Choose Premium Plan",
            "Cancel",
            null,
            "Monthly - $9.99/month",
            "Yearly - $79.99/year (Save 33%)",
            "Lifetime - $199.99 (Best Value)");

        if (action != null && action != "Cancel")
        {
            await Application.Current.MainPage.DisplayAlert(
                "Premium Upgrade", 
                $"You selected: {action}\n\nRedirecting to payment...", 
                "OK");
        }
    }

    [RelayCommand]
    private async Task ShowPrivacyPolicy()
    {
        // TODO: Open privacy policy page or web view
        await Application.Current.MainPage.DisplayAlert(
            "Privacy Policy", 
            "Opening privacy policy...", 
            "OK");
    }

    [RelayCommand]
    private async Task ShowTermsOfService()
    {
        // TODO: Open terms of service page or web view
        await Application.Current.MainPage.DisplayAlert(
            "Terms of Service", 
            "Opening terms of service...", 
            "OK");
    }

    [RelayCommand]
    private async Task ContactSupport()
    {
        var action = await Application.Current.MainPage.DisplayActionSheet(
            "Contact Support",
            "Cancel",
            null,
            "Send Email",
            "Live Chat",
            "FAQ");

        switch (action)
        {
            case "Send Email":
                await Application.Current.MainPage.DisplayAlert("Email Support", "Opening email client...", "OK");
                break;
            case "Live Chat":
                await Application.Current.MainPage.DisplayAlert("Live Chat", "Connecting to support chat...", "OK");
                break;
            case "FAQ":
                await Application.Current.MainPage.DisplayAlert("FAQ", "Opening FAQ page...", "OK");
                break;
        }
    }

    // Property change handlers to save settings automatically
    partial void OnDailyGuidanceEnabledChanged(bool value)
    {
        Task.Run(async () => await _userPreferencesService.SetBoolAsync("daily_guidance_enabled", value));
    }

    partial void OnTimelineUpdatesEnabledChanged(bool value)
    {
        Task.Run(async () => await _userPreferencesService.SetBoolAsync("timeline_updates_enabled", value));
    }

    partial void OnRelationshipInsightsEnabledChanged(bool value)
    {
        Task.Run(async () => await _userPreferencesService.SetBoolAsync("relationship_insights_enabled", value));
    }

    partial void OnShareAnonymousDataEnabledChanged(bool value)
    {
        Task.Run(async () => await _userPreferencesService.SetBoolAsync("share_anonymous_data", value));
    }

    partial void OnPersonalizedRecommendationsEnabledChanged(bool value)
    {
        Task.Run(async () => await _userPreferencesService.SetBoolAsync("personalized_recommendations", value));
    }
}

public partial class LanguageOption : ObservableObject
{
    public string Code { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string NativeName { get; set; } = string.Empty;
    public string FlagIcon { get; set; } = string.Empty;
    
    [ObservableProperty]
    private bool isSelected;
}
