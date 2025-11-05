# 小说创作助手 - Story Generation Feature

## 功能概述 / Overview

小说创作助手是一个基于AI的中文小说续写工具，专门为网络小说《我在异界开茶馆，喝茶召唤神宠打怪升级》设计。该功能可以根据故事大纲和已有章节，自动生成新的章节内容。

The Story Generation Assistant is an AI-powered Chinese novel writing tool specifically designed for the web novel "I Run a Teahouse in Another World: Drinking Tea to Summon Divine Pets and Level Up". It can automatically generate new chapter content based on the story outline and existing chapters.

## 故事信息 / Story Information

- **标题 / Title**: 我在异界开茶馆，喝茶召唤神宠打怪升级
- **主角 / Protagonist**: 周茶生 (Zhou Chasheng)
- **故事大纲 / Outline**: 
  > 周茶生本想守着祖传茶馆安静度日，谁知一盏灵茶把他送进了异界。这里的茶能召唤茶宠、布结界、疗伤破幻。开茶馆接异客、带茶宠刷本打怪，周茶生在日常泡茶里不断悟道成长，把茶艺变成可封神的法门。温情与奇遇并存，泡一杯好茶，也能翻转天地。

## 核心功能 / Core Features

### 1. 章节生成 / Chapter Generation
- **每次生成2章**: 默认每次运行生成2章新内容
- **AI驱动**: 使用多个免费AI API（Groq、Together AI、DeepSeek、智谱AI）
- **上下文连贯**: 基于前面章节内容保持故事连贯性
- **质量保证**: 每章2000-3000字，结构清晰，情节紧凑

### 2. 章节管理 / Chapter Management
- 查看所有已生成章节
- 展示章节标题、字数统计
- 点击章节可预览内容
- 按章节编号自动排序

### 3. 导出功能 / Export Function
- 导出所有章节为完整小说文本
- 包含故事标题、大纲和所有章节内容
- 适合后续编辑或发布

## 技术架构 / Technical Architecture

### 模型层 / Models (StoryModels.cs)

#### Story
故事元数据模型，包含：
- 故事标题和大纲
- 主角名字
- 章节计数
- 状态管理（进行中/暂停/完成）

#### Chapter
章节内容模型，包含：
- 章节编号和标题
- 章节正文内容
- 字数统计
- 创建和更新时间

#### StoryGenerationRequest
章节生成请求，包含：
- 故事ID
- 起始章节编号
- 生成章节数量
- 上下文信息

#### StoryGenerationResponse
章节生成响应，包含：
- 成功/失败标志
- 生成的章节列表
- 错误信息（如果有）

### 服务层 / Services (StoryGenerationService.cs)

#### IStoryGenerationService
提供以下接口：
- `GenerateChaptersAsync()`: 生成新章节
- `GetStoryAsync()`: 获取故事信息
- `GetChaptersAsync()`: 获取所有章节
- `SaveChapterAsync()`: 保存章节
- `InitializeDefaultStoryAsync()`: 初始化默认故事

#### StoryGenerationService
实现细节：
1. **多API支持**: 自动轮询多个免费AI服务
2. **智能上下文**: 提取最近章节作为续写参考
3. **结构化Prompt**: 详细的创作要求和格式规范
4. **错误处理**: API失败时自动切换备用服务
5. **内容解析**: 智能解析AI返回的标题和正文

### 视图模型层 / ViewModels (StoryWriterViewModel.cs)

#### StoryWriterViewModel
继承自BaseViewModel，提供：

**属性 / Properties:**
- `CurrentStory`: 当前故事信息
- `Chapters`: 章节集合
- `StatusMessage`: 状态消息
- `IsGenerating`: 是否正在生成
- `ChaptersToGenerate`: 每次生成章节数（可调整1-5章）

**命令 / Commands:**
- `InitializeCommand`: 初始化故事
- `LoadChaptersCommand`: 加载章节列表
- `GenerateChaptersCommand`: 生成新章节
- `ViewChapterCommand`: 查看章节内容
- `ExportChaptersCommand`: 导出所有章节
- `RefreshCommand`: 刷新数据

### 视图层 / Views (StoryWriterPage.xaml)

#### UI组件:
1. **头部区域**: 显示故事标题、大纲、状态和统计
2. **控制面板**: 
   - 章节数量选择器（Stepper）
   - 生成按钮
   - 进度指示器
3. **章节列表**: CollectionView显示所有章节
4. **统计面板**: 显示章节数、状态、更新时间

#### 特色设计:
- Material Design风格卡片布局
- 流畅的交互动画
- 清晰的视觉层次
- 响应式加载状态

## 使用方法 / Usage

### 1. 启动应用
打开KnewFate应用，从侧边栏菜单选择"小说创作助手"

### 2. 查看初始内容
应用会自动加载序章"茶通异界"，显示故事的开端

### 3. 生成新章节
1. 使用Stepper调整每次生成章节数（默认2章）
2. 点击"✨ 生成新章节"按钮
3. 等待AI生成（通常需要10-30秒）
4. 生成完成后，新章节自动添加到列表

### 4. 查看章节
点击任意章节卡片，弹出对话框显示章节内容（长章节会显示预览）

### 5. 导出作品
点击右上角"导出"按钮，导出完整小说文本

## AI配置 / AI Configuration

### 支持的AI服务:
1. **Groq** (llama3-70b-8192)
2. **Together AI** (Llama-2-70b-chat)
3. **DeepSeek** (deepseek-chat)
4. **智谱AI** (glm-4-flash)
5. **OpenAI** (gpt-3.5-turbo) - 需要环境变量

### 配置API Key:
在 `StoryGenerationService.cs` 中修改：
```csharp
private readonly Dictionary<string, string> _apiKeys = new()
{
    ["groq"] = "your_groq_api_key",
    ["together"] = "your_together_api_key",
    ["deepseek"] = "your_deepseek_api_key",
    ["zhipu"] = "your_zhipu_api_key"
};
```

或设置环境变量：
```bash
export OPENAI_API_KEY=your_openai_key
```

## 写作规则 / Writing Rules

AI生成章节时遵循以下规则：
1. **字数要求**: 每章2000-3000字
2. **结构要求**: 清晰的情节发展，有起承转合
3. **内容要求**:
   - 紧扣茶文化主题
   - 展现茶宠系统
   - 推进主线剧情
   - 体现人物成长
4. **风格要求**:
   - 细腻的感官描写
   - 自然的对话
   - 生动的环境渲染
   - 适当的悬念设置

## 示例输出 / Sample Output

### 序章：茶通异界
> 周茶生指尖还残留着紫砂壶的温润，鼻尖萦绕的龙井新香却骤然被腥甜草木气取代...
> 
> （初始内容已预设在系统中）

### 第1章：[AI生成标题]
> [AI生成的2000-3000字章节内容]

### 第2章：[AI生成标题]
> [AI生成的2000-3000字章节内容]

## 扩展功能 / Future Enhancements

### 计划中的功能:
- [ ] 本地SQLite数据库持久化
- [ ] 更多自定义写作参数（风格、节奏等）
- [ ] 章节编辑功能
- [ ] 多故事管理
- [ ] 云端同步
- [ ] 社区分享功能
- [ ] 写作建议和反馈系统

## 故障排查 / Troubleshooting

### 问题：生成失败
**原因**: AI API不可用或API Key无效
**解决**: 
1. 检查API Key配置
2. 确认网络连接
3. 尝试其他AI服务

### 问题：生成内容质量低
**原因**: 使用的AI模型能力有限
**解决**:
1. 配置更强大的AI服务（如GPT-4）
2. 调整Prompt提示词
3. 增加上下文章节数量

### 问题：生成速度慢
**原因**: AI服务响应时间较长
**解决**:
1. 使用响应更快的API（如Groq）
2. 减少每次生成的章节数
3. 优化Prompt长度

## 开发者信息 / Developer Info

### 文件结构:
```
KnewFate/
├── Models/
│   └── StoryModels.cs          # 故事和章节数据模型
├── Services/
│   └── StoryGenerationService.cs  # 故事生成服务
├── ViewModels/
│   └── StoryWriterViewModel.cs    # 视图模型
├── Views/
│   ├── StoryWriterPage.xaml       # UI界面
│   └── StoryWriterPage.xaml.cs    # 代码后台
└── Resources/
    └── Styles/
        └── Styles.xaml            # 样式定义
```

### 依赖项:
- .NET MAUI 8.0
- CommunityToolkit.Mvvm
- SQLite-net-pcl (用于未来持久化)
- System.Text.Json (JSON处理)

## 贡献指南 / Contributing

欢迎提交改进建议和代码：
1. Fork本仓库
2. 创建功能分支
3. 实现新功能或修复
4. 提交Pull Request

## 许可证 / License

本功能遵循KnewFate项目的MIT许可证。

---

**祝您创作愉快！Happy Writing!** ✨📚🍵
