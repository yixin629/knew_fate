# Daily Discoveries Feature - Architecture Overview

## Component Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                         User Interface                           │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │            DailyDiscoveryPage.xaml                        │  │
│  │  ┌────────────────────────────────────────────────────┐  │  │
│  │  │  Header (Title, Last Updated, Refresh)             │  │  │
│  │  ├────────────────────────────────────────────────────┤  │  │
│  │  │  Top 10 Section                                     │  │  │
│  │  │  ┌──────────────────────────────────────────────┐  │  │  │
│  │  │  │  #1  ⭐ Recommendation Title                 │  │  │  │
│  │  │  │       Description                             │  │  │  │
│  │  │  │       [Progress: 95%] [Category Badge]       │  │  │  │
│  │  │  │       [View Details Button]                  │  │  │  │
│  │  │  ├──────────────────────────────────────────────┤  │  │  │
│  │  │  │  #2  ⭐ Recommendation Title                 │  │  │  │
│  │  │  │  ...                                          │  │  │  │
│  │  │  ├──────────────────────────────────────────────┤  │  │  │
│  │  │  │  #10 ⭐ Recommendation Title                 │  │  │  │
│  │  │  └──────────────────────────────────────────────┘  │  │  │
│  │  ├────────────────────────────────────────────────────┤  │  │
│  │  │  Other Recommendations Section                     │  │  │
│  │  │  ┌──────────────────────────────────────────────┐  │  │  │
│  │  │  │  #11  Title | Category                      │  │  │  │
│  │  │  │  #12  Title | Category                      │  │  │  │
│  │  │  │  ...                                          │  │  │  │
│  │  │  └──────────────────────────────────────────────┘  │  │  │
│  │  └────────────────────────────────────────────────────┘  │  │
│  └──────────────────────────────────────────────────────────┘  │
│                           │                                     │
│                           ▼                                     │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │         DailyDiscoveryViewModel                          │  │
│  │                                                           │  │
│  │  Properties:                                             │  │
│  │  - Recommendations (ObservableCollection)                │  │
│  │  - TopTenRecommendations (ObservableCollection)          │  │
│  │  - IsBusy, IsEmpty, LastUpdated                          │  │
│  │                                                           │  │
│  │  Commands:                                               │  │
│  │  - RefreshCommand                                        │  │
│  │  - ViewRecommendationCommand                             │  │
│  │                                                           │  │
│  └──────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                      Business Logic Layer                        │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │      DailyRecommendationService                          │  │
│  │                                                           │  │
│  │  Methods:                                                │  │
│  │  ┌─────────────────────────────────────────────────┐    │  │
│  │  │ GetDailyRecommendationsAsync(userId, date)      │    │  │
│  │  │   ├─ GenerateRecommendationsAsync()             │    │  │
│  │  │   │   ├─ GenerateUserRecommendations()          │────┼──┼──┐
│  │  │   │   ├─ GenerateReadingRecommendations()       │    │  │  │
│  │  │   │   ├─ GenerateInsightRecommendations() ◄─────┼────┼──┼──┤
│  │  │   │   └─ GenerateEventRecommendations()         │    │  │  │
│  │  │   ├─ CalculateRelevanceScore()                  │    │  │  │
│  │  │   └─ RankAndMarkTopTen()                        │    │  │  │
│  │  │         (Sort by score, set IsTopTen flag)      │    │  │  │
│  │  └─────────────────────────────────────────────────┘    │  │  │
│  └──────────────────────────────────────────────────────────┘  │  │
│                                                                  │  │
└──────────────────────────────────────────────────────────────────┘  │
                              │                                       │
                              ▼                                       │
┌─────────────────────────────────────────────────────────────────┐  │
│                       Data Services Layer                        │  │
├─────────────────────────────────────────────────────────────────┤  │
│                                                                  │  │
│  ┌─────────────────┐  ┌─────────────────┐  ┌────────────────┐  │  │
│  │  SocialService  │  │  ZodiacService  │  │ Localization   │  │  │
│  │                 │  │                 │  │    Service     │  │  │
│  │ • GetRecommended│  │ • GetZodiac     │  │ • GetString    │  │  │
│  │   Users()  ◄────┼──┤   Profile()  ◄──┼──┤   Async()      │  │  │
│  │ • Calculate     │  │ • GetDaily      │  │                │  │  │
│  │   Compatibility │  │   Horoscope()   │  │                │  │  │
│  │ • GetNearby     │  │                 │  │                │  │  │
│  │   Users()       │  │                 │  │                │  │  │
│  └─────────────────┘  └─────────────────┘  └────────────────┘  │  │
│           │                     │                                │  │
└───────────┼─────────────────────┼────────────────────────────────┘  │
            │                     │                                    │
            ▼                     ▼                                    │
┌─────────────────────────────────────────────────────────────────┐  │
│                        Data Models                               │  │
├─────────────────────────────────────────────────────────────────┤  │
│                                                                  │  │
│  ┌──────────────────────────────────────────────────────────┐  │  │
│  │  RecommendationItem                                       │  │  │
│  │  ────────────────────────────────────────────            │  │  │
│  │  + Id: int                                                │  │  │
│  │  + Title: string                                          │  │  │
│  │  + Description: string                                    │  │  │
│  │  + Category: string  (User/Reading/Insight/Event)        │  │  │
│  │  + Score: double  (0-100)                                 │  │  │
│  │  + IsTopTen: bool  ◄─────────────────────────────────────┼──┘
│  │  + Rank: int  (1-based)                                   │
│  │  + ImageUrl: string                                       │
│  │  + ActionUrl: string  (navigation target)                │
│  │  + Metadata: Dictionary<string, string>                  │
│  │  + CreatedAt: DateTime                                    │
│  └──────────────────────────────────────────────────────────┘
│                                                                  
│  ┌──────────────────────────────────────────────────────────┐
│  │  DailyRecommendationList                                  │
│  │  ────────────────────────────────────────────            │
│  │  + Id: int                                                │
│  │  + UserId: int                                            │
│  │  + Date: DateTime                                         │
│  │  + Items: List<RecommendationItem>                        │
│  │  + GeneratedAt: DateTime                                  │
│  │  + ViewedAt: DateTime?                                    │
│  └──────────────────────────────────────────────────────────┘
│
└─────────────────────────────────────────────────────────────────┘

## Data Flow

1. **User Opens Page**
   - DailyDiscoveryPage loads
   - DailyDiscoveryViewModel initialized
   - Calls LoadRecommendationsAsync()

2. **Service Generates Recommendations**
   - DailyRecommendationService.GetDailyRecommendationsAsync()
   - Calls multiple generation methods:
     * GenerateUserRecommendationsAsync() → queries SocialService
     * GenerateReadingRecommendationsAsync() → suggests divination types
     * GenerateInsightRecommendationsAsync() → uses ZodiacService
     * GenerateEventRecommendationsAsync() → time-based suggestions

3. **Scoring and Ranking**
   - Each item gets a relevance score (0-100)
   - Items sorted by score descending
   - Top 10 items flagged with IsTopTen = true
   - Rank assigned (1, 2, 3, ..., N)

4. **UI Update**
   - ViewModel receives ranked list
   - Separates into TopTenRecommendations and all Recommendations
   - UI displays two sections with different styling

5. **User Interaction**
   - User taps recommendation
   - ViewRecommendationCommand executes
   - Navigation based on ActionUrl
   - MarkRecommendationViewedAsync() records the view

## Key Features

### Smart Scoring Algorithm
```
Base Score (from category):
├─ User: compatibility score × 100 + 5 (category boost)
├─ Reading: random 60-95
├─ Insight: personalized 70-88
└─ Event: time-based 72-90

Adjustments:
├─ Weekend + Event: +3
├─ Randomization: ±2
└─ Clamped to [0, 100]
```

### Top 10 Highlighting
- Visual distinction: Large cards vs compact list items
- Rank badges: #1, #2, #3, ..., #10
- Full details displayed (description, score bar, action button)
- Prominent positioning at top of list

### Categories
1. **User** 👤: Compatible profiles to connect with
2. **Reading** 🔮: Suggested divination activities
3. **Insight** 💡: Personalized guidance and tips
4. **Event** 📅: Time-sensitive opportunities and activities

## Navigation Integration

```
AppShell.xaml
├─ FlyoutItem: "Daily Discoveries"
│   └─ ShellContent: Route="dailydiscovery"
│       └─ ContentTemplate: DailyDiscoveryPage
│
AppShell.xaml.cs
└─ Routing.RegisterRoute("dailydiscovery", typeof(DailyDiscoveryPage))

MauiProgram.cs
├─ Services:
│   ├─ AddSingleton<IDailyRecommendationService, DailyRecommendationService>()
│   ├─ AddSingleton<ISocialService, SocialService>()  (dependency)
│   └─ AddSingleton<IZodiacService, ZodiacService>()  (dependency)
└─ Views/ViewModels:
    ├─ AddTransient<DailyDiscoveryPage>()
    └─ AddTransient<DailyDiscoveryViewModel>()
```

## Future Extensibility Points

1. **Add new recommendation categories**
   - Create new `Generate{Type}RecommendationsAsync()` method
   - Add to `GenerateRecommendationsAsync()` pipeline

2. **Customize scoring**
   - Modify `CalculateRelevanceScore()` logic
   - Add user preference weighting

3. **Persistent storage**
   - Implement caching in database
   - Store viewed recommendations
   - Track user engagement metrics

4. **AI enhancement**
   - Integrate ML-based scoring
   - Personalized recommendation models
   - Learning from user interactions

5. **Filtering and preferences**
   - Category filters in UI
   - User preference settings
   - Time-of-day customization
