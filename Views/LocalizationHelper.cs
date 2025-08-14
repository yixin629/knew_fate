using System.ComponentModel;
using System.Runtime.CompilerServices;
using KnewFate.Services;

namespace KnewFate.Views;

public class LocalizationHelper : INotifyPropertyChanged
{
    // Lazy singleton instance (thread-safe for MAUI single UI thread scenario)
    private static LocalizationHelper? _instance;
    private readonly ILocalizationService? _localizationService;

    public static LocalizationHelper Instance => _instance ??= new LocalizationHelper();

    public event PropertyChangedEventHandler? PropertyChanged;

    private LocalizationHelper()
    {
        _localizationService = ServiceHelper.GetService<ILocalizationService>();
        if (_localizationService != null)
        {
            _localizationService.LanguageChanged += OnLanguageChanged;
        }
    }

    public string this[string key] => _localizationService?.GetLocalizedString(key) ?? key;

    private void OnLanguageChanged(object? sender, EventArgs e)
    {
        // Notify indexer bindings to refresh localized strings
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item"));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        if (propertyName != null)
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public static class ServiceHelper
{
    // Return service or default if container not ready to avoid null warnings
    public static T? GetService<T>() where T : class => Current?.GetService<T>();

    public static IServiceProvider? Current =>
#if WINDOWS10_0_17763_0_OR_GREATER
        MauiWinUIApplication.Current.Services;
#elif ANDROID
        MauiApplication.Current.Services;
#elif IOS || MACCATALYST
        MauiUIApplicationDelegate.Current.Services; // TODO: migrate to IPlatformApplication.Current.Services when updating target
#else
        null;
#endif
}
