# Daily Discoveries Feature

## Overview

The Daily Discoveries feature provides users with a personalized daily list of interesting items to review, with the **top 10** items prominently highlighted. This feature helps users discover new connections, insights, and opportunities every day.

## Features

### 1. Daily Personalized Recommendations
- **Diverse Content**: Recommendations from multiple categories:
  - **Users**: High-compatibility matches with other users
  - **Readings**: Suggested tarot readings, astrology analyses, and other divination methods
  - **Insights**: Personalized daily insights based on zodiac and birth chart
  - **Events**: Time-sensitive activities and celestial events

### 2. Top 10 Highlighting
- The top 10 recommendations are displayed with **special visual styling**
- Each top 10 item shows:
  - **Rank badge** (#1-10) in a prominent corner
  - **Large card layout** with full details
  - **Relevance score** (0-100) with progress bar
  - **Category badge** for quick identification
  - **Detailed description** and action button

### 3. Smart Ranking
- Items are automatically ranked by relevance score
- Scoring considers:
  - Compatibility percentage (for user recommendations)
  - Personalization based on zodiac profile
  - Time relevance (weekends boost event recommendations)
  - Category importance

### 4. Interactive UI
- **Pull-to-refresh**: Get updated recommendations
- **Tap to view**: Navigate to detailed content
- **Category filters**: Visual badges for quick scanning
- **Empty states**: Friendly messages when no recommendations available
- **Last updated timestamp**: Know when recommendations were generated

## How to Use

### Accessing Daily Discoveries
1. Open the KnewFate app
2. Navigate to the **Daily Discoveries** page from the menu
3. View your personalized recommendations

### Viewing Details
- **Tap any recommendation** to view more details
- Top 10 items have a **"View Details"** button
- Other items can be tapped directly

### Refreshing Recommendations
- **Pull down** on the list to refresh
- Or tap the **refresh button** (🔄) in the header

## Technical Implementation

### New Components

#### Models
- `RecommendationItem`: Single recommendation with metadata
  ```csharp
  - Id, Title, Description
  - Category, Score, Rank
  - IsTopTen flag
  - ImageUrl, ActionUrl
  - Metadata dictionary
  ```

- `DailyRecommendationList`: Daily list container
  ```csharp
  - UserId, Date
  - List of RecommendationItems
  - GeneratedAt, ViewedAt timestamps
  ```

#### Service
- `DailyRecommendationService`: Core recommendation engine
  - Generates diverse recommendations
  - Calculates relevance scores
  - Ranks and marks top 10
  - Integrates with Social and Zodiac services

#### UI Components
- `DailyDiscoveryPage.xaml`: Main UI
- `DailyDiscoveryViewModel`: Business logic and data binding
- Custom converters for progress display

### Navigation
- Route: `dailydiscovery`
- Menu: "Daily Discoveries" with star icon (⭐)

### Dependencies
- `ISocialService`: For user recommendations and compatibility
- `IZodiacService`: For zodiac profile and personalization
- `ILocalizationService`: For multi-language support

## Customization

### Adding New Recommendation Types
To add new types of recommendations, edit `DailyRecommendationService.cs`:

1. Create a new generation method:
   ```csharp
   private async Task<List<RecommendationItem>> GenerateYourTypeAsync(int userId)
   ```

2. Add it to `GenerateRecommendationsAsync`:
   ```csharp
   items.AddRange(await GenerateYourTypeAsync(userId));
   ```

3. Update scoring logic if needed in `CalculateRelevanceScore`

### Adjusting Top N Count
To show more or fewer top items, modify the marking logic in `GetDailyRecommendationsAsync`:

```csharp
rankedItems[i].IsTopTen = i < 10; // Change 10 to your desired number
```

And update the UI header text accordingly.

### Styling
Modify `DailyDiscoveryPage.xaml` to change:
- Colors: Update `BackgroundColor` and `TextColor` properties
- Layout: Adjust `Margin`, `Padding`, and grid definitions
- Fonts: Change `FontSize` and `FontAttributes`

## Future Enhancements

Potential improvements for this feature:
- [ ] Filter recommendations by category
- [ ] Save favorite recommendations
- [ ] Share recommendations with friends
- [ ] Historical view of past recommendations
- [ ] Customizable notification for daily updates
- [ ] AI-powered recommendation refinement
- [ ] Integration with calendar for time-based suggestions
- [ ] Achievement system for acted-upon recommendations

## Troubleshooting

### No Recommendations Showing
- Ensure user profile is complete
- Check if services are properly registered in `MauiProgram.cs`
- Try refreshing the page

### Navigation Not Working
- Verify route registration in `AppShell.xaml.cs`
- Check ActionUrl format in recommendation items

### Scores Always 0
- Ensure compatibility calculation is working
- Check if ZodiacService is accessible
- Verify score calculation logic

## API Reference

### DailyRecommendationService

```csharp
Task<DailyRecommendationList> GetDailyRecommendationsAsync(int userId, DateTime date)
```
Gets complete daily recommendation list for a user.

```csharp
Task<List<RecommendationItem>> GetTopRecommendationsAsync(int userId, int count = 10)
```
Gets only the top N recommendations.

```csharp
Task MarkRecommendationViewedAsync(int userId, int recommendationId)
```
Records that a user viewed a recommendation.

## License

This feature is part of the KnewFate project and follows the same MIT license.
