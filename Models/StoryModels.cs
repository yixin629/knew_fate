using SQLite;

namespace KnewFate.Models;

/// <summary>
/// 故事元数据 - Story Metadata
/// </summary>
public class Story
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    
    /// <summary>
    /// 故事标题 - Story Title
    /// </summary>
    public string Title { get; set; } = "我在异界开茶馆，喝茶召唤神宠打怪升级";
    
    /// <summary>
    /// 故事大纲 - Story Outline
    /// </summary>
    public string Outline { get; set; } = "周茶生本想守着祖传茶馆安静度日，谁知一盏灵茶把他送进了异界。这里的茶能召唤茶宠、布结界、疗伤破幻。开茶馆接异客、带茶宠刷本打怪，周茶生在日常泡茶里不断悟道成长，把茶艺变成可封神的法门。温情与奇遇并存，泡一杯好茶，也能翻转天地。";
    
    /// <summary>
    /// 主角名字 - Protagonist Name
    /// </summary>
    public string ProtagonistName { get; set; } = "周茶生";
    
    /// <summary>
    /// 创建时间 - Created Date
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    /// <summary>
    /// 更新时间 - Updated Date
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
    
    /// <summary>
    /// 当前章节数 - Current Chapter Count
    /// </summary>
    public int CurrentChapterCount { get; set; } = 0;
    
    /// <summary>
    /// 故事状态 - Story Status (ongoing, paused, completed)
    /// </summary>
    public string Status { get; set; } = "ongoing";
}

/// <summary>
/// 章节内容 - Chapter Content
/// </summary>
public class Chapter
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    
    /// <summary>
    /// 所属故事ID - Story ID
    /// </summary>
    public int StoryId { get; set; }
    
    /// <summary>
    /// 章节编号 - Chapter Number
    /// </summary>
    public int ChapterNumber { get; set; }
    
    /// <summary>
    /// 章节标题 - Chapter Title
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// 章节内容 - Chapter Content
    /// </summary>
    public string Content { get; set; } = string.Empty;
    
    /// <summary>
    /// 字数 - Word Count
    /// </summary>
    public int WordCount { get; set; }
    
    /// <summary>
    /// 创建时间 - Created Date
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    /// <summary>
    /// 更新时间 - Updated Date
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}

/// <summary>
/// 故事生成请求 - Story Generation Request
/// </summary>
public class StoryGenerationRequest
{
    /// <summary>
    /// 故事ID - Story ID
    /// </summary>
    public int StoryId { get; set; }
    
    /// <summary>
    /// 起始章节编号 - Starting Chapter Number
    /// </summary>
    public int StartChapterNumber { get; set; }
    
    /// <summary>
    /// 生成章节数量 - Number of Chapters to Generate (default: 2)
    /// </summary>
    public int ChapterCount { get; set; } = 2;
    
    /// <summary>
    /// 上下文 - Previous chapters context for continuity
    /// </summary>
    public string Context { get; set; } = string.Empty;
}

/// <summary>
/// 故事生成响应 - Story Generation Response
/// </summary>
public class StoryGenerationResponse
{
    /// <summary>
    /// 是否成功 - Success Flag
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// 生成的章节 - Generated Chapters
    /// </summary>
    public List<Chapter> GeneratedChapters { get; set; } = new();
    
    /// <summary>
    /// 错误信息 - Error Message
    /// </summary>
    public string ErrorMessage { get; set; } = string.Empty;
}
