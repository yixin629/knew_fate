using System.Text;
using System.Text.Json;
using KnewFate.Models;

namespace KnewFate.Services;

/// <summary>
/// 故事生成服务接口 - Story Generation Service Interface
/// </summary>
public interface IStoryGenerationService
{
    /// <summary>
    /// 生成新章节 - Generate new chapters
    /// </summary>
    Task<StoryGenerationResponse> GenerateChaptersAsync(StoryGenerationRequest request);
    
    /// <summary>
    /// 获取故事 - Get story by ID
    /// </summary>
    Task<Story> GetStoryAsync(int storyId);
    
    /// <summary>
    /// 获取所有章节 - Get all chapters for a story
    /// </summary>
    Task<List<Chapter>> GetChaptersAsync(int storyId);
    
    /// <summary>
    /// 保存章节 - Save chapter
    /// </summary>
    Task<bool> SaveChapterAsync(Chapter chapter);
    
    /// <summary>
    /// 初始化默认故事 - Initialize default story
    /// </summary>
    Task<Story> InitializeDefaultStoryAsync();
}

/// <summary>
/// 故事生成服务 - Story Generation Service
/// 专门用于生成中文小说《我在异界开茶馆，喝茶召唤神宠打怪升级》
/// </summary>
public class StoryGenerationService : IStoryGenerationService
{
    private readonly HttpClient _httpClient;
    private readonly ILocalizationService _localizationService;
    
    // 免费开源API配置
    private readonly string[] _freeApiEndpoints = {
        "https://api.groq.com/openai/v1/chat/completions",
        "https://api.together.xyz/v1/chat/completions",
        "https://api.deepseek.com/v1/chat/completions",
        "https://open.bigmodel.cn/api/paas/v4/chat/completions"
    };
    
    private readonly Dictionary<string, string> _apiKeys = new()
    {
        ["groq"] = "gsk_free_api_key_here",
        ["together"] = "together_free_key_here",
        ["deepseek"] = "deepseek_free_key_here",
        ["zhipu"] = "zhipu_free_key_here"
    };
    
    // 内存中的故事数据库（简单实现）
    private static Story _defaultStory;
    private static List<Chapter> _chapters = new();
    
    // 第一段初始内容
    private const string INITIAL_CONTENT = @"周茶生指尖还残留着紫砂壶的温润，鼻尖萦绕的龙井新香却骤然被腥甜草木气取代。雕花木窗棂碎成星芒，他跌坐在青苔遍布的古树下，怀里那罐刚开封的明前茶滚落在地，碧色茶叶遇风便疯长，竟在石缝间抽出嫩白茶芽。

"簌簌——"身后传来兽类利爪刮擦岩石的声响。他猛地转身，只见三只瞳色如血的山猫正弓着脊背，涎水顺着尖利的獠牙滴落。周茶生慌不择路摸向腰间，指尖触到那枚祖传的锡制茶罐时，忽然想起阿爷临终前说的"茶可通神"。

他颤抖着抓出一把祁门红茶，滚烫的山泉水不知何时已注满身旁的石臼。茶叶遇水翻腾，竟腾起袅袅白雾，在他面前凝结成半尺高的茶汤结界。山猫利爪扑来时，被金光流转的茶雾震得哀鸣后退，爪尖沾染的茶渍竟冒出青烟。

"这是...布结界？"周茶生喃喃自语，忽见石臼中茶汤泛起漩涡，一只通体雪白的瓷质小龙从水面跃出，龙角沾着细碎茶毫，甩尾间吐出团乌龙茶气，将一只试图绕后的山猫裹成滚地葫芦。这分明是幼时阿爷捏给他的茶宠"雪龙"！

当最后一只山猫夹着尾巴逃窜，雪龙亲昵地蹭了蹭他的手腕，化作光点融入茶罐。周茶生望着掌心残留的茶渍，忽然听见远处传来缥缈歌声，茶汤结界的光晕里，竟浮现出茶馆雕梁画栋的幻影。他握紧茶罐，方才那口祁门红茶的暖意仍在丹田流转——看来这异界求生，还得靠老祖宗传下的这点本事。";

    public StoryGenerationService(HttpClient httpClient, ILocalizationService localizationService)
    {
        _httpClient = httpClient;
        _localizationService = localizationService;
    }

    public async Task<Story> InitializeDefaultStoryAsync()
    {
        if (_defaultStory == null)
        {
            _defaultStory = new Story
            {
                Id = 1,
                Title = "我在异界开茶馆，喝茶召唤神宠打怪升级",
                Outline = "周茶生本想守着祖传茶馆安静度日，谁知一盏灵茶把他送进了异界。这里的茶能召唤茶宠、布结界、疗伤破幻。开茶馆接异客、带茶宠刷本打怪，周茶生在日常泡茶里不断悟道成长，把茶艺变成可封神的法门。温情与奇遇并存，泡一杯好茶，也能翻转天地。",
                ProtagonistName = "周茶生",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                CurrentChapterCount = 0,
                Status = "ongoing"
            };
            
            // 添加初始内容作为序章
            var prologueChapter = new Chapter
            {
                Id = 1,
                StoryId = 1,
                ChapterNumber = 0,
                Title = "序章：茶通异界",
                Content = INITIAL_CONTENT,
                WordCount = INITIAL_CONTENT.Length,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            
            _chapters.Add(prologueChapter);
            _defaultStory.CurrentChapterCount = 1;
        }
        
        return _defaultStory;
    }

    public async Task<Story> GetStoryAsync(int storyId)
    {
        if (_defaultStory == null || _defaultStory.Id != storyId)
        {
            return await InitializeDefaultStoryAsync();
        }
        return _defaultStory;
    }

    public async Task<List<Chapter>> GetChaptersAsync(int storyId)
    {
        if (_defaultStory == null)
        {
            await InitializeDefaultStoryAsync();
        }
        
        return _chapters.Where(c => c.StoryId == storyId).OrderBy(c => c.ChapterNumber).ToList();
    }

    public async Task<bool> SaveChapterAsync(Chapter chapter)
    {
        if (_defaultStory == null)
        {
            await InitializeDefaultStoryAsync();
        }
        
        // 检查是否已存在
        var existing = _chapters.FirstOrDefault(c => c.Id == chapter.Id);
        if (existing != null)
        {
            _chapters.Remove(existing);
        }
        
        // 如果没有ID，分配新的
        if (chapter.Id == 0)
        {
            chapter.Id = _chapters.Any() ? _chapters.Max(c => c.Id) + 1 : 1;
        }
        
        chapter.UpdatedAt = DateTime.Now;
        _chapters.Add(chapter);
        
        // 更新故事的章节计数
        _defaultStory.CurrentChapterCount = _chapters.Count;
        _defaultStory.UpdatedAt = DateTime.Now;
        
        return true;
    }

    public async Task<StoryGenerationResponse> GenerateChaptersAsync(StoryGenerationRequest request)
    {
        var response = new StoryGenerationResponse();
        
        try
        {
            // 获取故事信息
            var story = await GetStoryAsync(request.StoryId);
            if (story == null)
            {
                response.Success = false;
                response.ErrorMessage = "故事不存在";
                return response;
            }
            
            // 获取现有章节作为上下文
            var existingChapters = await GetChaptersAsync(request.StoryId);
            var lastChapters = existingChapters.OrderByDescending(c => c.ChapterNumber).Take(2).ToList();
            
            // 构建上下文
            var contextBuilder = new StringBuilder();
            contextBuilder.AppendLine($"故事标题：{story.Title}");
            contextBuilder.AppendLine($"故事大纲：{story.Outline}");
            contextBuilder.AppendLine($"主角：{story.ProtagonistName}");
            contextBuilder.AppendLine();
            
            if (lastChapters.Any())
            {
                contextBuilder.AppendLine("前情回顾（最近的章节）：");
                foreach (var chapter in lastChapters.OrderBy(c => c.ChapterNumber))
                {
                    contextBuilder.AppendLine($"\n【{chapter.Title}】");
                    // 取最后500字作为上下文
                    var contentPreview = chapter.Content.Length > 500 
                        ? chapter.Content.Substring(chapter.Content.Length - 500) 
                        : chapter.Content;
                    contextBuilder.AppendLine(contentPreview);
                }
            }
            else
            {
                contextBuilder.AppendLine("这是故事的开始，序章内容：");
                contextBuilder.AppendLine(INITIAL_CONTENT);
            }
            
            // 生成章节
            for (int i = 0; i < request.ChapterCount; i++)
            {
                var chapterNumber = request.StartChapterNumber + i;
                var chapter = await GenerateSingleChapterAsync(story, contextBuilder.ToString(), chapterNumber);
                
                if (chapter != null)
                {
                    await SaveChapterAsync(chapter);
                    response.GeneratedChapters.Add(chapter);
                    
                    // 更新上下文，加入刚生成的章节
                    contextBuilder.AppendLine($"\n\n【第{chapterNumber}章 {chapter.Title}】");
                    var contentPreview = chapter.Content.Length > 500 
                        ? chapter.Content.Substring(chapter.Content.Length - 500) 
                        : chapter.Content;
                    contextBuilder.AppendLine(contentPreview);
                }
                else
                {
                    response.Success = false;
                    response.ErrorMessage = $"生成第{chapterNumber}章失败";
                    return response;
                }
            }
            
            response.Success = true;
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.ErrorMessage = $"生成章节时出错：{ex.Message}";
        }
        
        return response;
    }

    private async Task<Chapter> GenerateSingleChapterAsync(Story story, string context, int chapterNumber)
    {
        var prompt = $@"你是一位经验丰富的网络小说作家，正在创作一部修真类小说《{story.Title}》。

{context}

现在请你续写第{chapterNumber}章。要求：

1. 章节标题：请为这一章起一个吸引人的标题（不超过15字）
2. 章节内容：
   - 字数：2000-3000字
   - 情节要紧凑，有张有弛
   - 描写要细腻，注意感官细节（视觉、听觉、嗅觉、触觉等）
   - 要有冲突或悬念，让读者想继续看下去
   - 体现茶文化和茶宠元素
   - 推进主线剧情或展现人物成长
   - 对话要自然，符合人物性格
   - 环境描写要生动，营造氛围
3. 结构清晰，上下文连贯自然
4. 语言流畅，富有画面感

请按以下格式输出：
章节标题：【标题内容】
章节内容：
【正文内容】

注意：不要输出任何其他说明文字，直接输出章节标题和内容即可。";

        try
        {
            var generatedText = await CallAIForStoryAsync(prompt);
            
            // 解析AI生成的内容
            return ParseChapterFromAIResponse(generatedText, story.Id, chapterNumber);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"生成章节失败: {ex.Message}");
            // 返回一个默认章节
            return CreateFallbackChapter(story.Id, chapterNumber);
        }
    }

    private Chapter ParseChapterFromAIResponse(string aiResponse, int storyId, int chapterNumber)
    {
        var chapter = new Chapter
        {
            StoryId = storyId,
            ChapterNumber = chapterNumber,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };
        
        // 解析标题和内容
        var lines = aiResponse.Split('\n');
        var contentBuilder = new StringBuilder();
        bool foundTitle = false;
        bool inContent = false;
        
        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();
            
            if (!foundTitle && (trimmedLine.StartsWith("章节标题：") || trimmedLine.StartsWith("标题：")))
            {
                // 提取标题
                var titleMatch = trimmedLine.Replace("章节标题：", "").Replace("标题：", "").Trim();
                titleMatch = titleMatch.Replace("【", "").Replace("】", "").Trim();
                chapter.Title = titleMatch;
                foundTitle = true;
            }
            else if (foundTitle && (trimmedLine.StartsWith("章节内容：") || trimmedLine.StartsWith("正文：") || trimmedLine.StartsWith("内容：")))
            {
                inContent = true;
                continue;
            }
            else if (inContent && !string.IsNullOrWhiteSpace(trimmedLine))
            {
                contentBuilder.AppendLine(trimmedLine);
            }
            else if (foundTitle && !inContent && !string.IsNullOrWhiteSpace(trimmedLine))
            {
                // 如果找到了标题但还没有明确的"章节内容："标记，
                // 且这行不是空行，就认为内容开始了
                inContent = true;
                contentBuilder.AppendLine(trimmedLine);
            }
        }
        
        // 如果没有找到标题，使用默认标题
        if (string.IsNullOrEmpty(chapter.Title))
        {
            chapter.Title = $"第{chapterNumber}章";
        }
        
        chapter.Content = contentBuilder.ToString().Trim();
        chapter.WordCount = chapter.Content.Length;
        
        // 如果内容太短，说明解析可能有问题，直接使用整个响应
        if (chapter.Content.Length < 500)
        {
            chapter.Content = aiResponse;
            chapter.WordCount = aiResponse.Length;
        }
        
        return chapter;
    }

    private Chapter CreateFallbackChapter(int storyId, int chapterNumber)
    {
        // 创建一个备用章节
        return new Chapter
        {
            StoryId = storyId,
            ChapterNumber = chapterNumber,
            Title = $"第{chapterNumber}章：茶道初探",
            Content = @"周茶生缓缓睁开眼，发现自己躺在一片翠绿的竹林中。清晨的露水打湿了他的衣襟，空气中弥漫着竹叶的清香和若有若无的茶香。

他挣扎着坐起身，摸了摸腰间的茶罐——还在。那只雪龙茶宠昨夜消耗了不少灵力，现在正在茶罐中沉睡恢复。

"这究竟是什么地方？"周茶生环顾四周，竹林深处隐约能看到几间茅草屋，屋顶上还飘着炊烟。

他小心翼翼地站起身，整理了一下衣服，朝着那炊烟的方向走去。或许那里有人，能告诉他这是哪里，又该如何回家。

走近了才发现，那是一处小小的茶寮，门前挂着一块褪色的木牌："清心茶寮"。

"有人吗？"周茶生试探着问道。

一个苍老的声音从屋内传来："进来吧，等你很久了。"

周茶生一惊，推门而入。屋内简陋却整洁，一位白发老者正坐在茶桌前，面前摆着一套古朴的茶具。

"你是谁？为什么说等我？"周茶生警惕地问。

老者微微一笑："我是这茶寮的主人。至于为什么等你……因为你手中的茶罐，是开启这异界的钥匙。"

"异界？"周茶生心中一震。

"不错。"老者指了指对面的座位，"坐下吧，我给你泡壶茶，慢慢说给你听……"",
            WordCount = 450,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };
    }

    private async Task<string> CallAIForStoryAsync(string prompt)
    {
        // 尝试多个免费API
        foreach (var (endpoint, apiKey) in GetAvailableAPIs())
        {
            try
            {
                var response = await CallSpecificAPIAsync(endpoint, apiKey, prompt);
                if (!string.IsNullOrEmpty(response) && response.Length > 500)
                {
                    return response;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"API调用失败: {endpoint}, 错误: {ex.Message}");
                continue;
            }
        }

        // 如果所有API都失败，返回错误提示
        throw new Exception("所有AI服务暂时不可用，请稍后再试");
    }

    private IEnumerable<(string endpoint, string apiKey)> GetAvailableAPIs()
    {
        yield return ("https://api.groq.com/openai/v1/chat/completions", _apiKeys["groq"]);
        yield return ("https://api.together.xyz/v1/chat/completions", _apiKeys["together"]);
        yield return ("https://api.deepseek.com/v1/chat/completions", _apiKeys["deepseek"]);
        yield return ("https://open.bigmodel.cn/api/paas/v4/chat/completions", _apiKeys["zhipu"]);
        
        var openaiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        if (!string.IsNullOrEmpty(openaiKey))
        {
            yield return ("https://api.openai.com/v1/chat/completions", openaiKey);
        }
    }

    private async Task<string> CallSpecificAPIAsync(string endpoint, string apiKey, string prompt)
    {
        if (string.IsNullOrEmpty(apiKey) || apiKey.Contains("_key_here"))
        {
            throw new ArgumentException("API Key未配置");
        }

        var requestBody = new
        {
            model = GetModelForEndpoint(endpoint),
            messages = new[]
            {
                new { role = "system", content = "你是一位资深的网络小说作家，擅长创作修真、玄幻类小说。你的文笔优美，情节引人入胜，善于刻画人物和场景。" },
                new { role = "user", content = prompt }
            },
            max_tokens = 4000,
            temperature = 0.8,
            top_p = 0.9,
            stream = false
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

        var response = await _httpClient.PostAsync(endpoint, content);
        
        if (response.IsSuccessStatusCode)
        {
            var responseText = await response.Content.ReadAsStringAsync();
            var responseJson = JsonSerializer.Deserialize<JsonElement>(responseText);
            
            return responseJson
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? throw new Exception("AI返回内容为空");
        }
        
        throw new HttpRequestException($"API调用失败: {response.StatusCode}");
    }

    private string GetModelForEndpoint(string endpoint)
    {
        return endpoint switch
        {
            var e when e.Contains("groq") => "llama3-70b-8192",
            var e when e.Contains("together") => "meta-llama/Llama-2-70b-chat-hf",
            var e when e.Contains("deepseek") => "deepseek-chat",
            var e when e.Contains("bigmodel") => "glm-4-flash",
            var e when e.Contains("openai") => "gpt-3.5-turbo",
            _ => "gpt-3.5-turbo"
        };
    }
}
