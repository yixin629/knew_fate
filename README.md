# KnewFate - 多传统命运测算 App

KnewFate is a comprehensive cross-platform fortune telling application built with .NET MAUI, combining traditional Chinese and Western divination systems with modern technology.

## Features / 功能特色

### Core Divination Systems / 核心占卜系统
- **八字命理 (BaZi)** - Four Pillars of Destiny calculation and analysis
- **紫微斗数 (Ziwei Doushu)** - Purple Star Astrology system
- **塔罗牌占卜 (Tarot)** - Western Tarot card readings with multiple spreads
- **西方占星术 (Western Astrology)** - Natal chart analysis and transit predictions
- **数字命理学 (Numerology)** - Life path and destiny number calculations
- **五行分析 (Five Elements)** - Traditional Chinese elemental balance analysis

### Advanced Features / 高级功能
- **融合分析 (Fusion Analysis)** - Combines multiple systems for comprehensive insights
- **关系匹配 (Relationship Compatibility)** - Multi-system compatibility analysis
- **职业指导 (Career Guidance)** - Professional path recommendations
- **时间线预测 (Timeline Predictions)** - Life phase analysis and important dates
- **每日运势 (Daily Fortune)** - Daily energy readings and guidance
- **小说创作助手 (Story Generation)** - AI-powered Chinese novel writing assistant

### Creative Writing / 创意写作
- **AI Story Generation** - Automated chapter generation for Chinese web novels
- **Story Management** - Track chapters, word counts, and progress
- **Export Function** - Export complete novels for publication
- **Contextual Writing** - Maintains story continuity across chapters
- For detailed documentation, see [STORY_GENERATION_GUIDE.md](STORY_GENERATION_GUIDE.md)

### Multi-Language Support / 多语言支持
- **中文 (简体)** - Simplified Chinese
- **English** - Full English localization
- **Easy Language Switching** - One-tap language change on any page
- **Extensible** - Ready for additional languages

## Technical Architecture / 技术架构

### Platform Support / 平台支持
- **Android** (API 21+)
- **iOS** (iOS 11.0+)
- **Windows** (Windows 10 version 17763+)
- **macOS** (macOS 10.15+)

### Technology Stack / 技术栈
- **.NET MAUI 8.0** - Cross-platform framework
- **SQLite** - Local data storage with Entity Framework
- **MVVM Pattern** - Clean architecture with data binding
- **CommunityToolkit.Maui** - Enhanced UI controls
- **Dependency Injection** - Service-oriented architecture

### Project Structure / 项目结构
```
KnewFate/
├── Models/           # Data models and entities
├── Services/         # Business logic and data services
├── ViewModels/       # MVVM ViewModels
├── Views/           # XAML pages and UI
├── Resources/       # Localization, styles, and assets
├── Converters/      # Value converters for data binding
└── Platforms/       # Platform-specific implementations
```

## Getting Started / 开始使用

### Prerequisites / 前置要求
- Visual Studio 2022 17.8+ or Visual Studio Code with C# Dev Kit
- .NET 8.0 SDK
- Platform-specific workloads:
  - Android SDK (for Android development)
  - Xcode (for iOS development on macOS)

### Installation / 安装
1. Clone the repository / 克隆仓库
```bash
git clone https://github.com/yourusername/knewfate.git
cd knewfate
```

2. Restore packages / 恢复包
```bash
dotnet restore
```

3. Run the application / 运行应用
```bash
dotnet build
dotnet run --framework net8.0-android    # For Android
dotnet run --framework net8.0-ios        # For iOS
dotnet run --framework net8.0-windows    # For Windows
```

## Usage / 使用方法

### First Launch / 首次启动
1. Complete the onboarding process / 完成引导流程
2. Enter your birth information / 输入出生信息
3. Choose your preferred language / 选择首选语言
4. Explore different divination systems / 探索不同的占卜系统

### Core Features / 核心功能

#### Dashboard / 仪表板
- View daily energy levels / 查看每日能量水平
- Quick access to all divination systems / 快速访问所有占卜系统
- Personalized insights and recommendations / 个性化见解和建议

#### Chart Hub / 命盘中心
- Generate comprehensive birth charts / 生成全面的出生命盘
- Compare different divination systems / 比较不同的占卜系统
- Export and share chart analyses / 导出和分享命盘分析

#### Tarot Readings / 塔罗牌占卜
- Multiple spread options / 多种牌阵选择
- Detailed card interpretations / 详细的牌意解释
- Reading history and journal / 占卜历史和日志

### Language Switching / 语言切换
- Access Settings from any page / 从任何页面访问设置
- Tap language preference / 点击语言偏好
- Changes apply immediately / 更改立即生效

## Development / 开发

### Adding New Languages / 添加新语言
1. Create new resource file: `Resources/Languages/AppResources.[culture].resx`
2. Add culture to `LocalizationService.GetAvailableLanguages()`
3. Update language picker in Settings

### Extending Divination Systems / 扩展占卜系统
1. Add new service in `Services/` folder
2. Create corresponding models in `Models/`
3. Register service in `MauiProgram.cs`
4. Create UI in `Views/` with ViewModel

## Contributing / 贡献

We welcome contributions to improve KnewFate! Please follow these guidelines:

1. Fork the repository / 分叉仓库
2. Create a feature branch / 创建功能分支
3. Make your changes / 进行更改
4. Add tests if applicable / 如适用，添加测试
5. Submit a pull request / 提交拉取请求

## License / 许可证

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments / 致谢

- Traditional Chinese divination masters for preserving ancient wisdom
- Open source community for .NET MAUI and related technologies
- Contributors who help improve the accuracy of divination algorithms

## Support / 支持

For questions, suggestions, or issues:
- Create an issue on GitHub
- Email: support@knewfate.app
- Join our community discussions

---

**KnewFate** - Where ancient wisdom meets modern technology / 古老智慧与现代科技的完美结合
