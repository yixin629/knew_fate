using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using KnewFate.Models;
using KnewFate.Services;

namespace KnewFate.ViewModels;

public partial class TarotViewModel : ObservableObject
{
    private readonly ITarotService _tarotService;
    private readonly IDatabaseService _databaseService;
    private readonly IUserPreferencesService _userPreferencesService;

    [ObservableProperty]
    private string question = string.Empty;

    [ObservableProperty]
    private bool showQuestionInput = false;

    [ObservableProperty]
    private TarotSpread? currentReading;

    [ObservableProperty]
    private bool hasCurrentReading = false;

    [ObservableProperty]
    private bool showInterpretation = false;

    [ObservableProperty]
    private bool isLoading = false;

    [ObservableProperty]
    private int cardsPerRow = 3;

    [ObservableProperty]
    private ObservableCollection<TarotSpread> readingHistory = new();

    [ObservableProperty]
    private TarotCard? selectedCard;

    public TarotViewModel(ITarotService tarotService, IDatabaseService databaseService, IUserPreferencesService userPreferencesService)
    {
        _tarotService = tarotService;
        _databaseService = databaseService;
        _userPreferencesService = userPreferencesService;
        
        LoadReadingHistory();
    }

    [RelayCommand]
    private async Task DrawSingleCard()
    {
        IsLoading = true;
        try
        {
            var userId = await GetCurrentUserIdAsync();
            var spread = await _tarotService.DrawCardsAsync("SingleCard", Question, userId);
            
            CurrentReading = spread;
            HasCurrentReading = true;
            ShowInterpretation = true;
            CardsPerRow = 1;
            
            // Clear question after use
            Question = string.Empty;
            ShowQuestionInput = false;
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"Failed to draw card: {ex.Message}", "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task DrawThreeCard()
    {
        if (string.IsNullOrWhiteSpace(Question))
        {
            ShowQuestionInput = true;
            await Application.Current.MainPage.DisplayAlert("Question Required", "Please enter a question for your three-card reading.", "OK");
            return;
        }

        IsLoading = true;
        try
        {
            var userId = await GetCurrentUserIdAsync();
            var spread = await _tarotService.DrawCardsAsync("ThreeCard", Question, userId);
            
            CurrentReading = spread;
            HasCurrentReading = true;
            ShowInterpretation = true;
            CardsPerRow = 3;
            
            Question = string.Empty;
            ShowQuestionInput = false;
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"Failed to draw cards: {ex.Message}", "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task DrawCelticCross()
    {
        var result = await Application.Current.MainPage.DisplayAlert(
            "Celtic Cross Reading", 
            "This comprehensive 10-card reading provides deep insights into your situation. Continue?", 
            "Yes", 
            "Cancel");

        if (!result) return;

        if (string.IsNullOrWhiteSpace(Question))
        {
            ShowQuestionInput = true;
            await Application.Current.MainPage.DisplayAlert("Question Required", "Please enter a question for your Celtic Cross reading.", "OK");
            return;
        }

        IsLoading = true;
        try
        {
            var userId = await GetCurrentUserIdAsync();
            var spread = await _tarotService.DrawCardsAsync("CelticCross", Question, userId);
            
            CurrentReading = spread;
            HasCurrentReading = true;
            ShowInterpretation = true;
            CardsPerRow = 2; // Better layout for 10 cards
            
            Question = string.Empty;
            ShowQuestionInput = false;
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"Failed to draw Celtic Cross: {ex.Message}", "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ShowCardDetails(TarotCard card)
    {
        if (card == null) return;

        SelectedCard = card;
        
        var reversal = card.IsReversed ? " (Reversed)" : "";
        var details = $"{card.Name}{reversal}\n\n" +
                     $"Suit: {card.Suit}\n" +
                     $"Position: {card.Position}\n\n" +
                     $"Meaning: {card.Meaning}";

        if (card.IsReversed)
        {
            details += "\n\nReversed meanings often indicate internal reflection, blocked energy, or the need to look within for answers.";
        }

        await Application.Current.MainPage.DisplayAlert(card.Name, details, "OK");
    }

    [RelayCommand]
    private async Task SaveReading()
    {
        if (CurrentReading == null) return;

        try
        {
            await _databaseService.SaveAsync(CurrentReading);
            
            // Add to history if not already there
            if (!ReadingHistory.Any(r => r.Id == CurrentReading.Id))
            {
                ReadingHistory.Insert(0, CurrentReading);
                
                // Keep only last 10 readings in memory
                while (ReadingHistory.Count > 10)
                {
                    ReadingHistory.RemoveAt(ReadingHistory.Count - 1);
                }
            }
            
            await Application.Current.MainPage.DisplayAlert("Success", "Reading saved successfully!", "OK");
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"Failed to save reading: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private async Task LoadReading(TarotSpread spread)
    {
        if (spread == null) return;

        CurrentReading = spread;
        HasCurrentReading = true;
        ShowInterpretation = true;
        
        // Set appropriate layout based on spread type
        CardsPerRow = spread.SpreadType switch
        {
            "SingleCard" => 1,
            "ThreeCard" => 3,
            "CelticCross" => 2,
            _ => 3
        };

        await Application.Current.MainPage.DisplayAlert("Reading Loaded", $"Loaded {spread.SpreadType} reading from {spread.CreatedAt:MMM dd, yyyy}", "OK");
    }

    [RelayCommand]
    private async Task ClearReading()
    {
        CurrentReading = null;
        HasCurrentReading = false;
        ShowInterpretation = false;
        Question = string.Empty;
        ShowQuestionInput = false;
        SelectedCard = null;
    }

    private async Task LoadReadingHistory()
    {
        try
        {
            var userId = await GetCurrentUserIdAsync();
            var readings = await _databaseService.QueryAsync<TarotSpread>(
                "SELECT * FROM TarotSpread WHERE UserId = ? ORDER BY CreatedAt DESC LIMIT 10", 
                userId);

            ReadingHistory.Clear();
            foreach (var reading in readings)
            {
                ReadingHistory.Add(reading);
            }
        }
        catch (Exception ex)
        {
            // Log error but don't show to user unless critical
            System.Diagnostics.Debug.WriteLine($"Failed to load reading history: {ex.Message}");
        }
    }

    private async Task<int> GetCurrentUserIdAsync()
    {
        // In a real app, this would get the current logged-in user ID
        var userIdString = await _userPreferencesService.GetStringAsync("current_user_id", "1");
        return int.TryParse(userIdString, out var userId) ? userId : 1;
    }

    // Property change handlers
    partial void OnQuestionChanged(string value)
    {
        ShowQuestionInput = !string.IsNullOrWhiteSpace(value);
    }
}
