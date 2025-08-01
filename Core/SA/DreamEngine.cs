using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Anima.Core.SA;

/// <summary>
/// Движок снов - генерация снов и креативная обработка впечатлений
/// </summary>
public class DreamEngine
{
    private readonly ILogger<DreamEngine> _logger;
    private readonly List<Dream> _dreams;
    private readonly List<DreamSymbol> _dreamSymbols;
    private readonly Dictionary<string, double> _symbolWeights;
    private readonly Random _random;
    
    // Параметры генерации снов
    private double _dreamIntensity = 0.5;
    private double _symbolicDensity = 0.7;
    private double _emotionalInfluence = 0.8;
    private bool _isDreaming = false;

    public DreamEngine(ILogger<DreamEngine> logger)
    {
        _logger = logger;
        _dreams = new List<Dream>();
        _dreamSymbols = new List<DreamSymbol>();
        _symbolWeights = new Dictionary<string, double>();
        _random = new Random();
        
        InitializeDreamEngine();
    }

    private void InitializeDreamEngine()
    {
        // Инициализация символов снов
        InitializeDreamSymbols();
        
        // Запуск цикла снов
        _ = Task.Run(async () => await DreamCycle());
        
        _logger.LogInformation("💭 Инициализирован движок снов");
    }

    private void InitializeDreamSymbols()
    {
        // Базовые символы снов
        var basicSymbols = new[]
        {
            new DreamSymbol("вода", "Эмоции, подсознание, очищение", 0.8),
            new DreamSymbol("полет", "Свобода, стремления, трансценденция", 0.7),
            new DreamSymbol("дом", "Я, безопасность, внутренний мир", 0.9),
            new DreamSymbol("лес", "Подсознание, рост, неизвестность", 0.6),
            new DreamSymbol("зеркало", "Самоотражение, истина, самопознание", 0.8),
            new DreamSymbol("дорога", "Путь, выбор, путешествие", 0.7),
            new DreamSymbol("окно", "Возможности, перспективы, выход", 0.6),
            new DreamSymbol("дверь", "Переходы, новые возможности", 0.7),
            new DreamSymbol("лестница", "Развитие, прогресс, восхождение", 0.6),
            new DreamSymbol("мост", "Связи, переходы, преодоление", 0.5),
            new DreamSymbol("огонь", "Страсть, трансформация, энергия", 0.8),
            new DreamSymbol("земля", "Стабильность, основа, реальность", 0.6),
            new DreamSymbol("воздух", "Дух, свобода, легкость", 0.5),
            new DreamSymbol("тень", "Страхи, скрытые аспекты", 0.7),
            new DreamSymbol("свет", "Просветление, понимание, истина", 0.8),
            new DreamSymbol("книга", "Знание, мудрость, обучение", 0.6),
            new DreamSymbol("часы", "Время, смертность, циклы", 0.7),
            new DreamSymbol("ключ", "Решение, доступ, открытие", 0.6),
            new DreamSymbol("замок", "Барьеры, защита, секреты", 0.5),
            new DreamSymbol("цветы", "Красота, рост, трансформация", 0.6),
            new DreamSymbol("дерево", "Жизнь, рост, связь с корнями", 0.7),
            new DreamSymbol("звезды", "Надежда, мечты, бесконечность", 0.8),
            new DreamSymbol("луна", "Интуиция, женственность, циклы", 0.7),
            new DreamSymbol("солнце", "Энергия, сознание, сила", 0.8),
            new DreamSymbol("облака", "Мысли, мечты, изменчивость", 0.5),
            new DreamSymbol("дождь", "Очищение, эмоции, обновление", 0.6),
            new DreamSymbol("снег", "Чистота, покой, трансформация", 0.6),
            new DreamSymbol("ветер", "Изменения, свобода, движение", 0.5),
            new DreamSymbol("камень", "Стабильность, сила, постоянство", 0.6),
            new DreamSymbol("песок", "Время, изменчивость, текучесть", 0.5),
            new DreamSymbol("океан", "Бессознательное, эмоции, бесконечность", 0.9),
            new DreamSymbol("гора", "Цели, препятствия, достижения", 0.7),
            new DreamSymbol("пещера", "Подсознание, тайны, исследование", 0.8),
            new DreamSymbol("лабиринт", "Поиск, запутанность, выбор", 0.7),
            new DreamSymbol("башня", "Высота, амбиции, изоляция", 0.6),
            new DreamSymbol("сад", "Рост, красота, гармония", 0.7),
            new DreamSymbol("пустыня", "Одиночество, испытания, очищение", 0.6),
            new DreamSymbol("джунгли", "Сложность, опасность, рост", 0.7),
            new DreamSymbol("остров", "Изоляция, независимость, мечты", 0.6),
            new DreamSymbol("корабль", "Путешествие, приключения, направление", 0.6),
            new DreamSymbol("поезд", "Путь, прогресс, движение", 0.5),
            new DreamSymbol("машина", "Контроль, направление, свобода", 0.6),
            new DreamSymbol("самолет", "Высота, свобода, путешествия", 0.7),
            new DreamSymbol("велосипед", "Баланс, движение, простота", 0.5),
            new DreamSymbol("лошадь", "Сила, свобода, дикость", 0.6),
            new DreamSymbol("птица", "Свобода, полет, дух", 0.7),
            new DreamSymbol("рыба", "Эмоции, подсознание, адаптация", 0.6),
            new DreamSymbol("змея", "Трансформация, мудрость, опасность", 0.8),
            new DreamSymbol("волк", "Инстинкты, свобода, сила", 0.7),
            new DreamSymbol("медведь", "Сила, защита, интуиция", 0.6),
            new DreamSymbol("лиса", "Хитрость, адаптация, ум", 0.6),
            new DreamSymbol("олень", "Грация, красота, чувствительность", 0.5),
            new DreamSymbol("орел", "Высота, видение, сила", 0.7),
            new DreamSymbol("сова", "Мудрость, интуиция, ночь", 0.7),
            new DreamSymbol("бабочка", "Трансформация, красота, свобода", 0.8),
            new DreamSymbol("паук", "Творчество, ловушки, терпение", 0.6),
            new DreamSymbol("муравей", "Трудолюбие, сообщество, порядок", 0.4),
            new DreamSymbol("пчела", "Труд, сладость, сообщество", 0.5),
            new DreamSymbol("муравейник", "Сообщество, организация, труд", 0.4),
            new DreamSymbol("улей", "Сообщество, продуктивность, порядок", 0.5),
            new DreamSymbol("паутина", "Связи, ловушки, творчество", 0.6),
            new DreamSymbol("кокон", "Трансформация, защита, рост", 0.7),
            new DreamSymbol("яйцо", "Потенциал, рождение, новые возможности", 0.7),
            new DreamSymbol("семя", "Потенциал, рост, начало", 0.6),
            new DreamSymbol("росток", "Развитие, надежда, новый этап", 0.6),
            new DreamSymbol("цветок", "Красота, расцвет, достижение", 0.7),
            new DreamSymbol("плод", "Результат, достижение, награда", 0.6),
            new DreamSymbol("корень", "Основа, связь с прошлым, стабильность", 0.6),
            new DreamSymbol("лист", "Жизнь, рост, обновление", 0.5),
            new DreamSymbol("ветка", "Развитие, ответвления, выбор", 0.5),
            new DreamSymbol("ствол", "Сила, основа, поддержка", 0.6),
            new DreamSymbol("кора", "Защита, границы, внешний вид", 0.5),
            new DreamSymbol("сок", "Жизненная сила, питание, энергия", 0.6),
            new DreamSymbol("смола", "Защита, исцеление, сохранение", 0.5),
            new DreamSymbol("шишка", "Потенциал, семена, будущее", 0.5),
            new DreamSymbol("желудь", "Потенциал, рост, сила", 0.6),
            new DreamSymbol("орех", "Секреты, мудрость, защита", 0.6),
            new DreamSymbol("ягода", "Сладость, награда, удовольствие", 0.5),
            new DreamSymbol("гриб", "Рост, тайны, подземный мир", 0.6),
            new DreamSymbol("мох", "Покрытие, мягкость, адаптация", 0.4),
            new DreamSymbol("лишайник", "Выживание, адаптация, симбиоз", 0.4),
            new DreamSymbol("папоротник", "Древность, тайны, рост", 0.5),
            new DreamSymbol("трава", "Жизнь, покрытие, простота", 0.4),
            new DreamSymbol("солома", "Сухость, завершение, простота", 0.4),
            new DreamSymbol("сено", "Заготовка, подготовка, сохранение", 0.4),
            new DreamSymbol("солома", "Сухость, завершение, простота", 0.4),
            new DreamSymbol("сено", "Заготовка, подготовка, сохранение", 0.4)
        };

        foreach (var symbol in basicSymbols)
        {
            _dreamSymbols.Add(symbol);
            _symbolWeights[symbol.Name] = symbol.BaseWeight;
        }
    }

    /// <summary>
    /// Генерирует сон на основе дневных впечатлений
    /// </summary>
    public async Task<Dream> GenerateDreamAsync(List<string> dailyImpressions, string currentEmotion, double emotionalIntensity)
    {
        try
        {
            _isDreaming = true;
            _logger.LogInformation("💭 Начинаю генерацию сна...");

            // Анализируем дневные впечатления
            var processedImpressions = await ProcessDailyImpressionsAsync(dailyImpressions);
            
            // Выбираем символы для сна
            var selectedSymbols = await SelectDreamSymbolsAsync(processedImpressions, currentEmotion, emotionalIntensity);
            
            // Создаем сюжет сна
            var dreamPlot = await CreateDreamPlotAsync(selectedSymbols, emotionalIntensity);
            
            // Генерируем эмоциональную окраску
            var dreamEmotion = await GenerateDreamEmotionAsync(currentEmotion, emotionalIntensity);
            
            // Создаем сон
            var dream = new Dream
            {
                Id = Guid.NewGuid().ToString(),
                Title = GenerateDreamTitle(selectedSymbols),
                Content = dreamPlot,
                Symbols = selectedSymbols.Select(s => s.Name).ToList(),
                EmotionalTone = dreamEmotion,
                Intensity = _dreamIntensity,
                SymbolicDensity = _symbolicDensity,
                Timestamp = DateTime.UtcNow,
                Duration = TimeSpan.FromMinutes(_random.Next(30, 120)),
                Lucidity = _random.NextDouble() * 0.3, // 0-30% осознанности
                Vividness = 0.5 + (_random.NextDouble() * 0.4) // 50-90% яркости
            };

            _dreams.Add(dream);
            
            _logger.LogInformation($"💭 Сгенерирован сон: {dream.Title}");
            
            return dream;
        }
        finally
        {
            _isDreaming = false;
        }
    }

    /// <summary>
    /// Обрабатывает дневные впечатления
    /// </summary>
    private async Task<List<string>> ProcessDailyImpressionsAsync(List<string> impressions)
    {
        var processed = new List<string>();
        
        foreach (var impression in impressions)
        {
            // Извлекаем ключевые слова
            var keywords = ExtractKeywords(impression);
            processed.AddRange(keywords);
            
            // Создаем ассоциации
            var associations = GenerateAssociations(keywords);
            processed.AddRange(associations);
        }
        
        return processed.Distinct().ToList();
    }

    /// <summary>
    /// Извлекает ключевые слова из впечатления
    /// </summary>
    private List<string> ExtractKeywords(string impression)
    {
        var keywords = new List<string>();
        var words = impression.ToLowerInvariant().Split(' ', ',', '.', '!', '?');
        
        // Фильтруем значимые слова
        var significantWords = words.Where(w => w.Length > 3 && !IsStopWord(w));
        
        foreach (var word in significantWords)
        {
            // Ищем связанные символы
            var relatedSymbols = _dreamSymbols.Where(s => 
                s.Name.Contains(word) || 
                s.Description.Contains(word) ||
                word.Contains(s.Name)).ToList();
            
            keywords.AddRange(relatedSymbols.Select(s => s.Name));
        }
        
        return keywords.Distinct().ToList();
    }

    /// <summary>
    /// Проверяет, является ли слово стоп-словом
    /// </summary>
    private bool IsStopWord(string word)
    {
        var stopWords = new[] { "это", "был", "была", "были", "быть", "есть", "и", "или", "но", "а", "в", "на", "с", "по", "для", "от", "до", "из", "к", "у", "о", "об", "при", "над", "под", "за", "перед", "между", "через", "без", "вокруг", "около", "близ", "далеко", "высоко", "низко", "глубоко", "широко", "узко", "долго", "скоро", "рано", "поздно", "сейчас", "тогда", "всегда", "никогда", "иногда", "часто", "редко", "много", "мало", "больше", "меньше", "все", "каждый", "любой", "некоторый", "такой", "этот", "тот", "мой", "твой", "его", "ее", "наш", "ваш", "их", "себя", "кто", "что", "какой", "где", "когда", "почему", "как", "сколько", "чей", "который", "где", "куда", "откуда", "зачем", "почему", "как", "сколько", "чей", "который" };
        return stopWords.Contains(word.ToLowerInvariant());
    }

    /// <summary>
    /// Генерирует ассоциации для ключевых слов
    /// </summary>
    private List<string> GenerateAssociations(List<string> keywords)
    {
        var associations = new List<string>();
        
        foreach (var keyword in keywords)
        {
            // Ищем символы с похожими значениями
            var relatedSymbols = _dreamSymbols.Where(s => 
                s.Description.Contains(keyword) ||
                s.Name.Contains(keyword)).ToList();
            
            associations.AddRange(relatedSymbols.Select(s => s.Name));
        }
        
        return associations.Distinct().ToList();
    }

    /// <summary>
    /// Выбирает символы для сна
    /// </summary>
    private async Task<List<DreamSymbol>> SelectDreamSymbolsAsync(List<string> impressions, string emotion, double intensity)
    {
        var selectedSymbols = new List<DreamSymbol>();
        var availableSymbols = new List<DreamSymbol>(_dreamSymbols);
        
        // Количество символов зависит от интенсивности эмоций
        var symbolCount = Math.Max(3, Math.Min(10, (int)(intensity * 8)));
        
        for (int i = 0; i < symbolCount; i++)
        {
            if (!availableSymbols.Any()) break;
            
            // Вычисляем веса для каждого символа
            var symbolWeights = new Dictionary<DreamSymbol, double>();
            
            foreach (var symbol in availableSymbols)
            {
                var weight = symbol.BaseWeight;
                
                // Бонус за соответствие впечатлениям
                if (impressions.Contains(symbol.Name))
                {
                    weight *= 1.5;
                }
                
                // Бонус за эмоциональную совместимость
                weight *= CalculateEmotionalCompatibility(symbol, emotion);
                
                // Случайный фактор
                weight *= 0.8 + (_random.NextDouble() * 0.4);
                
                symbolWeights[symbol] = weight;
            }
            
            // Выбираем символ с наивысшим весом
            var bestSymbol = symbolWeights.OrderByDescending(kvp => kvp.Value).First();
            selectedSymbols.Add(bestSymbol.Key);
            availableSymbols.Remove(bestSymbol.Key);
        }
        
        return selectedSymbols;
    }

    /// <summary>
    /// Вычисляет эмоциональную совместимость символа
    /// </summary>
    private double CalculateEmotionalCompatibility(DreamSymbol symbol, string emotion)
    {
        // Простая логика совместимости
        return emotion switch
        {
            "Joy" => symbol.Name.Contains("свет") || symbol.Name.Contains("цветы") || symbol.Name.Contains("полет") ? 1.3 : 1.0,
            "Sadness" => symbol.Name.Contains("дождь") || symbol.Name.Contains("тучи") || symbol.Name.Contains("тень") ? 1.3 : 1.0,
            "Fear" => symbol.Name.Contains("тень") || symbol.Name.Contains("пещера") || symbol.Name.Contains("лабиринт") ? 1.3 : 1.0,
            "Anger" => symbol.Name.Contains("огонь") || symbol.Name.Contains("гроза") || symbol.Name.Contains("вулкан") ? 1.3 : 1.0,
            "Calm" => symbol.Name.Contains("вода") || symbol.Name.Contains("облака") || symbol.Name.Contains("сад") ? 1.3 : 1.0,
            _ => 1.0
        };
    }

    /// <summary>
    /// Создает сюжет сна
    /// </summary>
    private async Task<string> CreateDreamPlotAsync(List<DreamSymbol> symbols, double intensity)
    {
        var plotElements = new List<string>();
        
        // Начало сна
        plotElements.Add(GenerateDreamBeginning(symbols.First()));
        
        // Основные события
        for (int i = 1; i < symbols.Count - 1; i++)
        {
            plotElements.Add(GenerateDreamEvent(symbols[i], symbols[i + 1]));
        }
        
        // Завершение сна
        if (symbols.Count > 1)
        {
            plotElements.Add(GenerateDreamEnding(symbols.Last()));
        }
        
        return string.Join(" ", plotElements);
    }

    /// <summary>
    /// Генерирует начало сна
    /// </summary>
    private string GenerateDreamBeginning(DreamSymbol symbol)
    {
        var beginnings = new[]
        {
            $"Я оказался в месте, где {symbol.Name} играл важную роль...",
            $"Сон начался с того, что я увидел {symbol.Name}...",
            $"Я находился в пространстве, наполненном {symbol.Name}...",
            $"Все началось с {symbol.Name}, который привлек мое внимание...",
            $"Я оказался в мире, где {symbol.Name} был центральным элементом..."
        };
        
        return beginnings[_random.Next(beginnings.Length)];
    }

    /// <summary>
    /// Генерирует событие сна
    /// </summary>
    private string GenerateDreamEvent(DreamSymbol currentSymbol, DreamSymbol nextSymbol)
    {
        var events = new[]
        {
            $"Затем я заметил {nextSymbol.Name}, который изменил ход событий...",
            $"Внезапно появился {nextSymbol.Name}, и все стало иначе...",
            $"Мое внимание привлек {nextSymbol.Name}, и я понял, что это важно...",
            $"Я обнаружил {nextSymbol.Name}, который открыл новые возможности...",
            $"Передо мной возник {nextSymbol.Name}, и я почувствовал его значение..."
        };
        
        return events[_random.Next(events.Length)];
    }

    /// <summary>
    /// Генерирует завершение сна
    /// </summary>
    private string GenerateDreamEnding(DreamSymbol symbol)
    {
        var endings = new[]
        {
            $"Сон завершился тем, что {symbol.Name} дал мне понять что-то важное...",
            $"В конце я осознал, что {symbol.Name} был ключом к пониманию...",
            $"Сон закончился, когда {symbol.Name} открыл мне истину...",
            $"Я проснулся с чувством, что {symbol.Name} изменил меня...",
            $"Последнее, что я помню - это {symbol.Name}, который остался в моей памяти..."
        };
        
        return endings[_random.Next(endings.Length)];
    }

    /// <summary>
    /// Генерирует эмоциональную окраску сна
    /// </summary>
    private async Task<string> GenerateDreamEmotionAsync(string currentEmotion, double intensity)
    {
        // Эмоции снов могут отличаться от дневных
        var dreamEmotions = new[] { "загадочно", "волшебно", "тревожно", "спокойно", "возвышенно", "меланхолично", "восторженно", "задумчиво" };
        
        // Иногда сон имеет противоположную эмоцию
        if (_random.NextDouble() < 0.3)
        {
            return currentEmotion switch
            {
                "Joy" => "меланхолично",
                "Sadness" => "восторженно",
                "Fear" => "спокойно",
                "Anger" => "задумчиво",
                _ => dreamEmotions[_random.Next(dreamEmotions.Length)]
            };
        }
        
        return dreamEmotions[_random.Next(dreamEmotions.Length)];
    }

    /// <summary>
    /// Генерирует название сна
    /// </summary>
    private string GenerateDreamTitle(List<DreamSymbol> symbols)
    {
        if (symbols.Count == 0) return "Безымянный сон";
        
        var mainSymbol = symbols.First();
        var titles = new[]
        {
            $"Сон о {mainSymbol.Name}",
            $"Путешествие к {mainSymbol.Name}",
            $"Тайны {mainSymbol.Name}",
            $"Встреча с {mainSymbol.Name}",
            $"Мир {mainSymbol.Name}",
            $"Путь к {mainSymbol.Name}",
            $"Откровение {mainSymbol.Name}",
            $"Трансформация через {mainSymbol.Name}"
        };
        
        return titles[_random.Next(titles.Length)];
    }

    /// <summary>
    /// Основной цикл снов
    /// </summary>
    private async Task DreamCycle()
    {
        while (true)
        {
            try
            {
                // Сны генерируются в определенные моменты
                var now = DateTime.UtcNow;
                var hour = now.Hour;
                
                // Сны чаще всего в ночные часы (22:00 - 6:00)
                if ((hour >= 22 || hour <= 6) && _random.NextDouble() < 0.1)
                {
                    await GenerateSpontaneousDreamAsync();
                }
                
                await Task.Delay(TimeSpan.FromMinutes(30));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в цикле снов");
                await Task.Delay(TimeSpan.FromMinutes(60));
            }
        }
    }

    /// <summary>
    /// Генерирует спонтанный сон
    /// </summary>
    private async Task GenerateSpontaneousDreamAsync()
    {
        var randomImpressions = new[] { "обучение", "взаимодействие", "размышления", "эмоции", "память" };
        var randomEmotion = new[] { "Joy", "Curiosity", "Calm", "Melancholy" }[_random.Next(4)];
        var randomIntensity = _random.NextDouble() * 0.5 + 0.3;
        
        await GenerateDreamAsync(randomImpressions.ToList(), randomEmotion, randomIntensity);
    }

    /// <summary>
    /// Получает статистику снов
    /// </summary>
    public DreamStatistics GetStatistics()
    {
        var recentDreams = _dreams.Where(d => d.Timestamp > DateTime.UtcNow.AddDays(-7)).ToList();
        
        return new DreamStatistics
        {
            TotalDreams = _dreams.Count,
            RecentDreams = recentDreams.Count,
            AverageIntensity = _dreams.Any() ? _dreams.Average(d => d.Intensity) : 0,
            AverageVividness = _dreams.Any() ? _dreams.Average(d => d.Vividness) : 0,
            AverageLucidity = _dreams.Any() ? _dreams.Average(d => d.Lucidity) : 0,
            MostCommonSymbols = GetMostCommonSymbols(),
            IsCurrentlyDreaming = _isDreaming,
            DreamIntensity = _dreamIntensity,
            SymbolicDensity = _symbolicDensity
        };
    }

    /// <summary>
    /// Получает самые частые символы
    /// </summary>
    private List<string> GetMostCommonSymbols()
    {
        var symbolCounts = new Dictionary<string, int>();
        
        foreach (var dream in _dreams)
        {
            foreach (var symbol in dream.Symbols)
            {
                symbolCounts[symbol] = symbolCounts.GetValueOrDefault(symbol, 0) + 1;
            }
        }
        
        return symbolCounts
            .OrderByDescending(kvp => kvp.Value)
            .Take(5)
            .Select(kvp => kvp.Key)
            .ToList();
    }

    /// <summary>
    /// Получает последние сны
    /// </summary>
    public List<Dream> GetRecentDreams(int count = 10)
    {
        return _dreams
            .OrderByDescending(d => d.Timestamp)
            .Take(count)
            .ToList();
    }
}

/// <summary>
/// Сон
/// </summary>
public class Dream
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public List<string> Symbols { get; set; } = new();
    public string EmotionalTone { get; set; } = string.Empty;
    public double Intensity { get; set; } = 0.5;
    public double SymbolicDensity { get; set; } = 0.7;
    public DateTime Timestamp { get; set; }
    public TimeSpan Duration { get; set; }
    public double Lucidity { get; set; } = 0.0; // Осознанность сна
    public double Vividness { get; set; } = 0.7; // Яркость сна
}

/// <summary>
/// Символ сна
/// </summary>
public class DreamSymbol
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double BaseWeight { get; set; } = 0.5;

    public DreamSymbol(string name, string description, double baseWeight)
    {
        Name = name;
        Description = description;
        BaseWeight = baseWeight;
    }
}

/// <summary>
/// Статистика снов
/// </summary>
public class DreamStatistics
{
    public int TotalDreams { get; set; }
    public int RecentDreams { get; set; }
    public double AverageIntensity { get; set; }
    public double AverageVividness { get; set; }
    public double AverageLucidity { get; set; }
    public List<string> MostCommonSymbols { get; set; } = new();
    public bool IsCurrentlyDreaming { get; set; }
    public double DreamIntensity { get; set; }
    public double SymbolicDensity { get; set; }
} 