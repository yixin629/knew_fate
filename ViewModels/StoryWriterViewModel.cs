using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KnewFate.Models;
using KnewFate.Services;

namespace KnewFate.ViewModels;

/// <summary>
/// 故事写作器视图模型 - Story Writer ViewModel
/// </summary>
public partial class StoryWriterViewModel : BaseViewModel
{
    private readonly IStoryGenerationService _storyService;
    
    // 常量配置
    private const int ContentPreviewLength = 500; // 内容预览长度

    [ObservableProperty]
    private Story currentStory;

    [ObservableProperty]
    private ObservableCollection<Chapter> chapters;

    [ObservableProperty]
    private string statusMessage;

    [ObservableProperty]
    private bool isGenerating;

    [ObservableProperty]
    private int chaptersToGenerate = 2;

    [ObservableProperty]
    private Chapter selectedChapter;

    public StoryWriterViewModel(IStoryGenerationService storyService)
    {
        _storyService = storyService;
        Chapters = new ObservableCollection<Chapter>();
        StatusMessage = "准备就绪";
    }

    /// <summary>
    /// 初始化故事 - Initialize story
    /// </summary>
    [RelayCommand]
    public async Task InitializeAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            StatusMessage = "初始化故事...";

            // 初始化默认故事
            CurrentStory = await _storyService.InitializeDefaultStoryAsync();

            // 加载已有章节
            await LoadChaptersAsync();

            StatusMessage = $"故事《{CurrentStory.Title}》已加载，当前共{Chapters.Count}章";
        }
        catch (Exception ex)
        {
            StatusMessage = $"初始化失败：{ex.Message}";
            await ShowErrorAsync("错误", ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// 加载章节 - Load chapters
    /// </summary>
    [RelayCommand]
    public async Task LoadChaptersAsync()
    {
        if (CurrentStory == null) return;

        try
        {
            var chapterList = await _storyService.GetChaptersAsync(CurrentStory.Id);
            Chapters.Clear();
            foreach (var chapter in chapterList)
            {
                Chapters.Add(chapter);
            }
        }
        catch (Exception ex)
        {
            await ShowErrorAsync("加载章节失败", ex.Message);
        }
    }

    /// <summary>
    /// 生成新章节 - Generate new chapters
    /// </summary>
    [RelayCommand]
    public async Task GenerateChaptersAsync()
    {
        if (IsBusy || IsGenerating) return;
        if (CurrentStory == null)
        {
            await InitializeAsync();
            if (CurrentStory == null)
            {
                await ShowErrorAsync("错误", "故事初始化失败");
                return;
            }
        }

        try
        {
            IsGenerating = true;
            IsBusy = true;

            // 确定起始章节编号
            var startChapterNumber = Chapters.Any() 
                ? Chapters.Max(c => c.ChapterNumber) + 1 
                : 1;

            StatusMessage = $"正在生成第{startChapterNumber}章到第{startChapterNumber + ChaptersToGenerate - 1}章...";

            // 创建生成请求
            var request = new StoryGenerationRequest
            {
                StoryId = CurrentStory.Id,
                StartChapterNumber = startChapterNumber,
                ChapterCount = ChaptersToGenerate
            };

            // 生成章节
            var response = await _storyService.GenerateChaptersAsync(request);

            if (response.Success)
            {
                // 添加新章节到列表
                foreach (var chapter in response.GeneratedChapters)
                {
                    Chapters.Add(chapter);
                }

                StatusMessage = $"成功生成{response.GeneratedChapters.Count}章！当前共{Chapters.Count}章";
                
                // 显示成功消息
                await Application.Current.MainPage.DisplayAlert(
                    "生成成功", 
                    $"已成功生成{response.GeneratedChapters.Count}章新内容！", 
                    "确定");

                // 自动选择第一个新生成的章节
                if (response.GeneratedChapters.Any())
                {
                    SelectedChapter = response.GeneratedChapters.First();
                }
            }
            else
            {
                StatusMessage = $"生成失败：{response.ErrorMessage}";
                await ShowErrorAsync("生成失败", response.ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"生成出错：{ex.Message}";
            await ShowErrorAsync("生成章节时出错", ex.Message);
        }
        finally
        {
            IsGenerating = false;
            IsBusy = false;
        }
    }

    /// <summary>
    /// 查看章节 - View chapter
    /// </summary>
    [RelayCommand]
    public async Task ViewChapterAsync(Chapter chapter)
    {
        if (chapter == null) return;

        SelectedChapter = chapter;
        
        // 显示章节内容
        await Application.Current.MainPage.DisplayAlert(
            $"第{chapter.ChapterNumber}章：{chapter.Title}",
            chapter.Content.Length > ContentPreviewLength 
                ? chapter.Content.Substring(0, ContentPreviewLength) + "...\n\n（内容较长，请在详情页查看完整内容）"
                : chapter.Content,
            "关闭");
    }

    /// <summary>
    /// 导出所有章节 - Export all chapters
    /// </summary>
    [RelayCommand]
    public async Task ExportChaptersAsync()
    {
        if (CurrentStory == null || !Chapters.Any())
        {
            await ShowErrorAsync("错误", "没有可导出的内容");
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = "正在导出...";

            var content = new System.Text.StringBuilder();
            content.AppendLine($"《{CurrentStory.Title}》");
            content.AppendLine();
            content.AppendLine($"故事大纲：{CurrentStory.Outline}");
            content.AppendLine();
            content.AppendLine("=" + new string('=', 50));
            content.AppendLine();

            foreach (var chapter in Chapters.OrderBy(c => c.ChapterNumber))
            {
                content.AppendLine();
                content.AppendLine($"第{chapter.ChapterNumber}章：{chapter.Title}");
                content.AppendLine();
                content.AppendLine(chapter.Content);
                content.AppendLine();
                content.AppendLine("-" + new string('-', 50));
            }

            // TODO: 实现实际的文件保存功能
            // 目前仅在对话框中显示导出成功消息
            // 未来可以使用 FileSaver 或 MAUI 文件选择器保存到本地
            await Application.Current.MainPage.DisplayAlert(
                "导出成功",
                $"已导出{Chapters.Count}章，共{content.Length}字\n\n注意：实际文件保存功能待实现",
                "确定");

            StatusMessage = $"导出成功：{Chapters.Count}章";
        }
        catch (Exception ex)
        {
            StatusMessage = "导出失败";
            await ShowErrorAsync("导出失败", ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// 刷新故事信息 - Refresh story info
    /// </summary>
    [RelayCommand]
    public async Task RefreshAsync()
    {
        if (IsRefreshing) return;

        try
        {
            IsRefreshing = true;
            await LoadChaptersAsync();
            StatusMessage = "刷新完成";
        }
        catch (Exception ex)
        {
            await ShowErrorAsync("刷新失败", ex.Message);
        }
        finally
        {
            IsRefreshing = false;
        }
    }
}
