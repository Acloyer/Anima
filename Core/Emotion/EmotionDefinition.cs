using System.Text.Json;

namespace Anima.Core.Emotion;

/// <summary>
/// Определение эмоции из JSON файла
/// </summary>
public class EmotionDefinition
{
    public string Name { get; set; } = string.Empty;
    public double Valence { get; set; } = 0.0;
    public double Arousal { get; set; } = 0.0;
    public double Urgency { get; set; } = 0.0;
    public double Dominance { get; set; } = 0.0;
    public string Description { get; set; } = string.Empty;
    public string Access { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public List<string> Synonyms { get; set; } = new();
    public List<string> Antonyms { get; set; } = new();
    public List<string> Triggers { get; set; } = new();
    public List<string> Responses { get; set; } = new();
    public double Complexity { get; set; } = 0.0;
    public bool IsMetaEmotion { get; set; } = false;
}

/// <summary>
/// Сервис для работы с определениями эмоций
/// </summary>
public class EmotionDefinitionService
{
    private readonly ILogger<EmotionDefinitionService> _logger;
    private readonly Dictionary<string, EmotionDefinition> _emotionDefinitions;
    private readonly Dictionary<string, List<EmotionDefinition>> _emotionsByCategory;
    private readonly Dictionary<string, List<EmotionDefinition>> _emotionsByAccess;

    public EmotionDefinitionService(ILogger<EmotionDefinitionService> logger)
    {
        _logger = logger;
        _emotionDefinitions = new Dictionary<string, EmotionDefinition>();
        _emotionsByCategory = new Dictionary<string, List<EmotionDefinition>>();
        _emotionsByAccess = new Dictionary<string, List<EmotionDefinition>>();
    }

    /// <summary>
    /// Загружает все эмоции из JSON файла
    /// </summary>
    public async Task LoadAllEmotionsFromJsonAsync()
    {
        try
        {
            var jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "emotions.json");
            
            if (!File.Exists(jsonPath))
            {
                _logger.LogWarning("Файл emotions.json не найден");
                return;
            }

            var jsonContent = await File.ReadAllTextAsync(jsonPath);
            var emotions = JsonSerializer.Deserialize<List<EmotionDefinition>>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (emotions == null)
            {
                _logger.LogError("Не удалось десериализовать эмоции из JSON");
                return;
            }

            // Загружаем эмоции в словари
            foreach (var emotion in emotions)
            {
                _emotionDefinitions[emotion.Name] = emotion;

                // Группируем по категориям
                if (!_emotionsByCategory.ContainsKey(emotion.Category))
                {
                    _emotionsByCategory[emotion.Category] = new List<EmotionDefinition>();
                }
                _emotionsByCategory[emotion.Category].Add(emotion);

                // Группируем по уровню доступа
                if (!_emotionsByAccess.ContainsKey(emotion.Access))
                {
                    _emotionsByAccess[emotion.Access] = new List<EmotionDefinition>();
                }
                _emotionsByAccess[emotion.Access].Add(emotion);
            }

            _logger.LogInformation($"✅ Загружено {emotions.Count} эмоций из JSON файла");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при загрузке эмоций из JSON");
        }
    }

    /// <summary>
    /// Проверяет, разрешена ли эмоция для данного уровня API ключа
    /// </summary>
    public bool IsEmotionAllowedFor(string apiKeyLevel, string emotionName)
    {
        if (!_emotionDefinitions.ContainsKey(emotionName))
        {
            return false;
        }

        var emotion = _emotionDefinitions[emotionName];
        
        return apiKeyLevel switch
        {
            "Creator" => true, // Creator имеет доступ ко всем эмоциям
            "Advanced" => emotion.Access == "Basic" || emotion.Access == "Advanced",
            "Basic" => emotion.Access == "Basic",
            _ => emotion.Access == "Basic"
        };
    }

    /// <summary>
    /// Получает список всех эмоций для данного уровня доступа
    /// </summary>
    public List<EmotionDefinition> ListAllEmotions(string accessLevel)
    {
        if (_emotionsByAccess.ContainsKey(accessLevel))
        {
            return _emotionsByAccess[accessLevel];
        }

        return new List<EmotionDefinition>();
    }

    /// <summary>
    /// Получает определение эмоции по имени
    /// </summary>
    public EmotionDefinition? GetEmotionDefinition(string name)
    {
        return _emotionDefinitions.GetValueOrDefault(name);
    }

    /// <summary>
    /// Получает эмоции по категории
    /// </summary>
    public List<EmotionDefinition> GetEmotionsByCategory(string category)
    {
        return _emotionsByCategory.GetValueOrDefault(category, new List<EmotionDefinition>());
    }

    /// <summary>
    /// Получает случайную эмоцию из категории
    /// </summary>
    public EmotionDefinition? GetRandomEmotionFromCategory(string category)
    {
        var emotions = GetEmotionsByCategory(category);
        if (emotions.Any())
        {
            var random = new Random();
            return emotions[random.Next(emotions.Count)];
        }
        return null;
    }

    /// <summary>
    /// Получает эмоции с определенной валентностью
    /// </summary>
    public List<EmotionDefinition> GetEmotionsByValence(double minValence, double maxValence)
    {
        return _emotionDefinitions.Values
            .Where(e => e.Valence >= minValence && e.Valence <= maxValence)
            .ToList();
    }

    /// <summary>
    /// Получает эмоции с определенным уровнем возбуждения
    /// </summary>
    public List<EmotionDefinition> GetEmotionsByArousal(double minArousal, double maxArousal)
    {
        return _emotionDefinitions.Values
            .Where(e => e.Arousal >= minArousal && e.Arousal <= maxArousal)
            .ToList();
    }

    /// <summary>
    /// Получает мета-эмоции
    /// </summary>
    public List<EmotionDefinition> GetMetaEmotions()
    {
        return _emotionDefinitions.Values
            .Where(e => e.IsMetaEmotion)
            .ToList();
    }

    /// <summary>
    /// Получает статистику эмоций
    /// </summary>
    public EmotionStatistics GetEmotionStatistics()
    {
        return new EmotionStatistics
        {
            TotalEmotions = _emotionDefinitions.Count,
            Categories = _emotionsByCategory.Keys.ToList(),
            AccessLevels = _emotionsByAccess.Keys.ToList(),
            MetaEmotionsCount = GetMetaEmotions().Count,
            AverageComplexity = _emotionDefinitions.Values.Average(e => e.Complexity),
            AverageValence = _emotionDefinitions.Values.Average(e => e.Valence),
            AverageArousal = _emotionDefinitions.Values.Average(e => e.Arousal)
        };
    }
}

/// <summary>
/// Статистика эмоций
/// </summary>
public class EmotionStatistics
{
    public int TotalEmotions { get; set; }
    public List<string> Categories { get; set; } = new();
    public List<string> AccessLevels { get; set; } = new();
    public int MetaEmotionsCount { get; set; }
    public double AverageComplexity { get; set; }
    public double AverageValence { get; set; }
    public double AverageArousal { get; set; }
} 