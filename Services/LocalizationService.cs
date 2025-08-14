using System.Globalization;

namespace KnewFate.Services;

public interface ILocalizationService
{
    Task<string> GetSavedLanguageAsync();
    Task SetLanguageAsync(string languageCode);
    string GetLocalizedString(string key);
    CultureInfo CurrentCulture { get; }
    event EventHandler LanguageChanged;
    Task<string> GetCurrentLanguageAsync();
    Task<string> GetStringAsync(string key);
}

public class LocalizationService : ILocalizationService
{
    private const string LANGUAGE_KEY = "selected_language";
    private CultureInfo _currentCulture;

    public event EventHandler LanguageChanged;

    public CultureInfo CurrentCulture 
    { 
        get => _currentCulture;
        private set
        {
            _currentCulture = value;
            CultureInfo.CurrentCulture = value;
            CultureInfo.CurrentUICulture = value;
            Thread.CurrentThread.CurrentCulture = value;
            Thread.CurrentThread.CurrentUICulture = value;
        }
    }

    public LocalizationService()
    {
        _currentCulture = CultureInfo.CurrentCulture;
    }

    public async Task<string> GetSavedLanguageAsync()
    {
        return await SecureStorage.GetAsync(LANGUAGE_KEY) ?? "en-US";
    }

    public async Task SetLanguageAsync(string languageCode)
    {
        try
        {
            var culture = new CultureInfo(languageCode);
            CurrentCulture = culture;
            
            await SecureStorage.SetAsync(LANGUAGE_KEY, languageCode);
            
            LanguageChanged?.Invoke(this, EventArgs.Empty);
        }
        catch (CultureNotFoundException)
        {
            // Fall back to English if culture not found
            CurrentCulture = new CultureInfo("en-US");
            await SecureStorage.SetAsync(LANGUAGE_KEY, "en-US");
        }
    }

    public string GetLocalizedString(string key)
    {
        try
        {
            var resourceManager = KnewFate.Resources.Languages.AppResources.ResourceManager;
            return resourceManager.GetString(key, CurrentCulture) ?? key;
        }
        catch
        {
            return key;
        }
    }

    public async Task<string> GetCurrentLanguageAsync()
    {
        return await GetSavedLanguageAsync();
    }

    public async Task<string> GetStringAsync(string key)
    {
        return await Task.FromResult(GetLocalizedString(key));
    }
}
