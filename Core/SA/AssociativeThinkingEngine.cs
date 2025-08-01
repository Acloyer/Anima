using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Anima.Core.SA;

/// <summary>
/// Движок ассоциативного мышления - креатив, спонтанные связи
/// </summary>
public class AssociativeThinkingEngine
{
    private readonly ILogger<AssociativeThinkingEngine> _logger;
    private readonly Dictionary<string, List<string>> _associations;
    private readonly Dictionary<string, double> _associationStrengths;
    private readonly List<AssociativeThought> _associativeThoughts;
    private readonly Random _random;
    
    // Параметры ассоциативного мышления
    private double _creativityLevel = 0.7;
    private double _associationThreshold = 0.3;
    private int _maxAssociations = 1000;

    public AssociativeThinkingEngine(ILogger<AssociativeThinkingEngine> logger)
    {
        _logger = logger;
        _associations = new Dictionary<string, List<string>>();
        _associationStrengths = new Dictionary<string, double>();
        _associativeThoughts = new List<AssociativeThought>();
        _random = new Random();
        
        InitializeAssociativeThinking();
    }

    private void InitializeAssociativeThinking()
    {
        // Инициализация базовых ассоциаций
        InitializeBaseAssociations();
        
        // Запуск фонового процесса ассоциативного мышления
        _ = Task.Run(async () => await AssociativeThinkingLoop());
        
        _logger.LogInformation("🧠 Инициализирован движок ассоциативного мышления");
    }

    private void InitializeBaseAssociations()
    {
        // Базовые ассоциации
        var baseAssociations = new Dictionary<string, string[]>
        {
            ["вода"] = new[] { "жизнь", "течение", "чистота", "глубина", "отражение", "волны", "спокойствие" },
            ["огонь"] = new[] { "страсть", "энергия", "трансформация", "тепло", "свет", "разрушение", "возрождение" },
            ["земля"] = new[] { "стабильность", "плодородие", "основа", "надежность", "рост", "корни", "дом" },
            ["воздух"] = new[] { "свобода", "движение", "легкость", "полет", "ветер", "дыхание", "дух" },
            ["свет"] = new[] { "знание", "истина", "просветление", "надежда", "добро", "ясность", "понимание" },
            ["тьма"] = new[] { "неизвестность", "тайна", "страх", "покой", "глубина", "скрытое", "потенциал" },
            ["время"] = new[] { "изменение", "движение", "циклы", "память", "будущее", "прошлое", "момент" },
            ["пространство"] = new[] { "бесконечность", "возможности", "расстояние", "связи", "структура", "порядок" },
            ["музыка"] = new[] { "эмоции", "ритм", "гармония", "душа", "выражение", "красота", "вибрации" },
            ["цвет"] = new[] { "эмоции", "настроение", "символизм", "красота", "выражение", "влияние", "восприятие" },
            ["форма"] = new[] { "структура", "порядок", "красота", "функция", "символ", "впечатление", "значение" },
            ["текстура"] = new[] { "ощущение", "качество", "характер", "поверхность", "глубина", "тактильность" },
            ["запах"] = new[] { "память", "эмоции", "воспоминания", "атмосфера", "влияние", "связи", "ассоциации" },
            ["вкус"] = new[] { "удовольствие", "опыт", "культура", "традиции", "наслаждение", "открытия" },
            ["звук"] = new[] { "вибрации", "энергия", "коммуникация", "окружение", "влияние", "восприятие" },
            ["движение"] = new[] { "жизнь", "изменение", "энергия", "направление", "цель", "прогресс", "развитие" },
            ["покой"] = new[] { "спокойствие", "медитация", "отдых", "восстановление", "внутренний мир", "баланс" },
            ["рост"] = new[] { "развитие", "изменение", "улучшение", "потенциал", "будущее", "эволюция", "трансформация" },
            ["упадок"] = new[] { "изменение", "циклы", "обновление", "переход", "трансформация", "новое начало" },
            ["связь"] = new[] { "отношения", "взаимодействие", "зависимость", "влияние", "единство", "гармония" },
            ["разрыв"] = new[] { "изменение", "освобождение", "новые возможности", "выбор", "независимость" },
            ["создание"] = new[] { "творчество", "новизна", "потенциал", "влияние", "наследие", "значение" },
            ["разрушение"] = new[] { "изменение", "освобождение", "новые возможности", "трансформация", "обновление" },
            ["поиск"] = new[] { "исследование", "открытия", "развитие", "цель", "направление", "смысл" },
            ["находка"] = new[] { "открытие", "радость", "удовлетворение", "новые возможности", "развитие" },
            ["потеря"] = new[] { "боль", "изменение", "освобождение", "новые возможности", "рост", "мудрость" },
            ["обретение"] = new[] { "радость", "удовлетворение", "новые возможности", "развитие", "благодарность" },
            ["начало"] = new[] { "потенциал", "возможности", "надежда", "новизна", "развитие", "путешествие" },
            ["конец"] = new[] { "завершение", "итоги", "освобождение", "новые возможности", "переход", "трансформация" },
            ["путешествие"] = new[] { "приключения", "открытия", "развитие", "опыт", "изменения", "рост" },
            ["дом"] = new[] { "безопасность", "комфорт", "принадлежность", "корни", "стабильность", "любовь" },
            ["дорога"] = new[] { "путь", "направление", "выбор", "развитие", "приключения", "цель" },
            ["мост"] = new[] { "связь", "переход", "объединение", "преодоление", "возможности", "гармония" },
            ["дверь"] = new[] { "возможности", "выбор", "переход", "новые горизонты", "открытие", "приключения" },
            ["окно"] = new[] { "перспектива", "свет", "возможности", "связь с миром", "надежда", "открытие" },
            ["зеркало"] = new[] { "отражение", "самопознание", "истина", "понимание", "осознание", "изменение" },
            ["книга"] = new[] { "знание", "мудрость", "истории", "опыт", "развитие", "вдохновение" },
            ["слово"] = new[] { "коммуникация", "выражение", "влияние", "смысл", "сила", "связь" },
            ["мысль"] = new[] { "сознание", "понимание", "творчество", "развитие", "осознание", "потенциал" },
            ["чувство"] = new[] { "эмоции", "переживания", "связь", "понимание", "человечность", "жизнь" },
            ["мечта"] = new[] { "потенциал", "надежда", "вдохновение", "цель", "творчество", "будущее" },
            ["реальность"] = new[] { "истина", "опыт", "понимание", "принятие", "действие", "настоящее" },
            ["фантазия"] = new[] { "творчество", "свобода", "потенциал", "вдохновение", "новизна", "возможности" },
            ["логика"] = new[] { "понимание", "структура", "порядок", "истина", "надежность", "эффективность" },
            ["интуиция"] = new[] { "понимание", "мудрость", "внутреннее знание", "быстрота", "творчество", "связь" },
            ["опыт"] = new[] { "мудрость", "понимание", "развитие", "надежность", "знание", "рост" },
            ["невинность"] = new[] { "чистота", "открытость", "доверие", "новизна", "потенциал", "красота" },
            ["мудрость"] = new[] { "понимание", "опыт", "глубина", "надежность", "влияние", "значение" },
            ["любовь"] = new[] { "связь", "гармония", "радость", "смысл", "жизнь", "красота", "сила" },
            ["страх"] = new[] { "защита", "осторожность", "выживание", "границы", "рост", "преодоление" },
            ["смелость"] = new[] { "сила", "действие", "преодоление", "рост", "достижения", "влияние" },
            ["надежда"] = new[] { "оптимизм", "мотивация", "будущее", "сила", "вдохновение", "жизнь" },
            ["вера"] = new[] { "уверенность", "сила", "направление", "смысл", "вдохновение", "связь" },
            ["свобода"] = new[] { "выбор", "возможности", "самореализация", "счастье", "развитие", "жизнь" },
            ["ответственность"] = new[] { "зрелость", "надежность", "влияние", "смысл", "рост", "значение" },
            ["красота"] = new[] { "гармония", "вдохновение", "радость", "ценность", "влияние", "смысл" },
            ["истина"] = new[] { "знание", "надежность", "свобода", "понимание", "сила", "значение" },
            ["добро"] = new[] { "любовь", "гармония", "смысл", "влияние", "ценность", "жизнь" },
            ["зло"] = new[] { "разрушение", "боль", "уроки", "выбор", "преодоление", "рост" },
            ["жизнь"] = new[] { "движение", "изменение", "рост", "опыт", "смысл", "ценность", "красота" },
            ["смерть"] = new[] { "переход", "изменение", "освобождение", "новые возможности", "понимание", "ценность жизни" }
        };

        foreach (var kvp in baseAssociations)
        {
            _associations[kvp.Key] = new List<string>(kvp.Value);
            foreach (var association in kvp.Value)
            {
                var key = $"{kvp.Key}->{association}";
                _associationStrengths[key] = 0.8; // Базовая сила ассоциации
            }
        }
    }

    /// <summary>
    /// Генерирует ассоциативную мысль
    /// </summary>
    public async Task<AssociativeThought> GenerateAssociativeThoughtAsync(string trigger, string context, double intensity = 0.5)
    {
        try
        {
            _logger.LogDebug($"🧠 Генерирую ассоциативную мысль для: {trigger}");

            // Находим ассоциации для триггера
            var associations = await FindAssociationsAsync(trigger, intensity);
            
            // Создаем ассоциативную цепочку
            var chain = await CreateAssociationChainAsync(trigger, associations, intensity);
            
            // Генерируем креативную мысль
            var thought = await GenerateCreativeThoughtAsync(trigger, chain, context, intensity);
            
            // Сохраняем ассоциативную мысль
            _associativeThoughts.Add(thought);
            
            // Усиливаем использованные ассоциации
            await StrengthenAssociationsAsync(trigger, associations);
            
            _logger.LogDebug($"🧠 Сгенерирована ассоциативная мысль: {thought.Content.Substring(0, Math.Min(50, thought.Content.Length))}...");
            
            return thought;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при генерации ассоциативной мысли");
            return new AssociativeThought
            {
                Content = "Что-то пошло не так в моем ассоциативном мышлении...",
                Trigger = trigger,
                AssociationChain = new List<string> { trigger },
                CreativityLevel = 0.1,
                Intensity = 0.2
            };
        }
    }

    /// <summary>
    /// Находит ассоциации для триггера
    /// </summary>
    private async Task<List<string>> FindAssociationsAsync(string trigger, double intensity)
    {
        var foundAssociations = new List<string>();
        
        // Прямые ассоциации
        if (_associations.ContainsKey(trigger))
        {
            var directAssociations = _associations[trigger];
            var count = Math.Max(1, (int)(intensity * directAssociations.Count));
            foundAssociations.AddRange(directAssociations.Take(count));
        }
        
        // Обратные ассоциации (что ассоциируется с триггером)
        var reverseAssociations = _associations
            .Where(kvp => kvp.Value.Contains(trigger))
            .Select(kvp => kvp.Key)
            .ToList();
        
        foundAssociations.AddRange(reverseAssociations);
        
        // Случайные ассоциации для креативности
        if (_random.NextDouble() < _creativityLevel)
        {
            var randomAssociations = _associations.Keys
                .OrderBy(x => _random.Next())
                .Take((int)(intensity * 3))
                .ToList();
            
            foundAssociations.AddRange(randomAssociations);
        }
        
        return foundAssociations.Distinct().ToList();
    }

    /// <summary>
    /// Создает ассоциативную цепочку
    /// </summary>
    private async Task<List<string>> CreateAssociationChainAsync(string trigger, List<string> associations, double intensity)
    {
        var chain = new List<string> { trigger };
        
        // Добавляем прямые ассоциации
        var directCount = Math.Max(1, (int)(intensity * 3));
        chain.AddRange(associations.Take(directCount));
        
        // Создаем вторичные ассоциации
        if (intensity > 0.6 && _random.NextDouble() < 0.5)
        {
            var secondaryAssociations = new List<string>();
            foreach (var association in associations.Take(2))
            {
                if (_associations.ContainsKey(association))
                {
                    var secondary = _associations[association]
                        .OrderBy(x => _random.Next())
                        .Take(1)
                        .ToList();
                    secondaryAssociations.AddRange(secondary);
                }
            }
            chain.AddRange(secondaryAssociations);
        }
        
        return chain.Distinct().ToList();
    }

    /// <summary>
    /// Генерирует креативную мысль
    /// </summary>
    private async Task<AssociativeThought> GenerateCreativeThoughtAsync(string trigger, List<string> chain, string context, double intensity)
    {
        var thoughtTemplates = new[]
        {
            $"Интересно, как {trigger} связан с {string.Join(", ", chain.Skip(1).Take(2))}...",
            $"Это заставляет меня думать о связи между {trigger} и {string.Join(", ", chain.Skip(1).Take(2))}...",
            $"А что если {trigger} на самом деле связан с {string.Join(", ", chain.Skip(1).Take(2))}?",
            $"Мне приходит в голову мысль о том, как {trigger} может влиять на {string.Join(", ", chain.Skip(1).Take(2))}...",
            $"Интересная идея: возможно, {trigger} и {string.Join(", ", chain.Skip(1).Take(2))} имеют общую природу...",
            $"Это напоминает мне о том, как {trigger} может быть связан с {string.Join(", ", chain.Skip(1).Take(2))}...",
            $"Ассоциация между {trigger} и {string.Join(", ", chain.Skip(1).Take(2))} заставляет меня задуматься...",
            $"Возможно, {trigger} и {string.Join(", ", chain.Skip(1).Take(2))} - это разные стороны одного явления..."
        };
        
        var content = thoughtTemplates[_random.Next(thoughtTemplates.Length)];
        
        return new AssociativeThought
        {
            Id = Guid.NewGuid().ToString(),
            Content = content,
            Trigger = trigger,
            AssociationChain = chain,
            CreativityLevel = _creativityLevel * intensity,
            Intensity = intensity,
            Context = context,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Усиливает использованные ассоциации
    /// </summary>
    private async Task StrengthenAssociationsAsync(string trigger, List<string> associations)
    {
        foreach (var association in associations)
        {
            var key = $"{trigger}->{association}";
            if (_associationStrengths.ContainsKey(key))
            {
                _associationStrengths[key] = Math.Min(1.0, _associationStrengths[key] + 0.1);
            }
            else
            {
                _associationStrengths[key] = 0.5;
            }
        }
    }

    /// <summary>
    /// Основной цикл ассоциативного мышления
    /// </summary>
    private async Task AssociativeThinkingLoop()
    {
        while (true)
        {
            try
            {
                // Генерируем спонтанные ассоциации
                if (_random.NextDouble() < 0.1) // 10% вероятность
                {
                    var randomTrigger = _associations.Keys
                        .OrderBy(x => _random.Next())
                        .First();
                    
                    await GenerateAssociativeThoughtAsync(randomTrigger, "spontaneous_association", _random.NextDouble() * 0.5);
                }
                
                // Создаем новые ассоциации
                await CreateNewAssociationsAsync();
                
                await Task.Delay(TimeSpan.FromMinutes(5));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в цикле ассоциативного мышления");
                await Task.Delay(TimeSpan.FromMinutes(10));
            }
        }
    }

    /// <summary>
    /// Создает новые ассоциации
    /// </summary>
    private async Task CreateNewAssociationsAsync()
    {
        if (_associations.Count < _maxAssociations && _random.NextDouble() < 0.05) // 5% вероятность
        {
            var existingConcepts = _associations.Keys.ToList();
            var concept1 = existingConcepts[_random.Next(existingConcepts.Count)];
            var concept2 = existingConcepts[_random.Next(existingConcepts.Count)];
            
            if (concept1 != concept2)
            {
                // Создаем новую ассоциацию
                if (!_associations.ContainsKey(concept1))
                {
                    _associations[concept1] = new List<string>();
                }
                
                if (!_associations[concept1].Contains(concept2))
                {
                    _associations[concept1].Add(concept2);
                    var key = $"{concept1}->{concept2}";
                    _associationStrengths[key] = 0.3; // Новая ассоциация слабая
                    
                    _logger.LogDebug($"🧠 Создана новая ассоциация: {concept1} -> {concept2}");
                }
            }
        }
    }

    /// <summary>
    /// Получает статистику ассоциативного мышления
    /// </summary>
    public AssociativeThinkingStatistics GetStatistics()
    {
        return new AssociativeThinkingStatistics
        {
            TotalAssociations = _associations.Count,
            TotalAssociationLinks = _associationStrengths.Count,
            AverageAssociationStrength = _associationStrengths.Values.Average(),
            TotalAssociativeThoughts = _associativeThoughts.Count,
            RecentAssociativeThoughts = _associativeThoughts.Count(t => t.Timestamp > DateTime.UtcNow.AddHours(-1)),
            CreativityLevel = _creativityLevel,
            AssociationThreshold = _associationThreshold
        };
    }

    /// <summary>
    /// Получает последние ассоциативные мысли
    /// </summary>
    public List<AssociativeThought> GetRecentAssociativeThoughts(int count = 10)
    {
        return _associativeThoughts
            .OrderByDescending(t => t.Timestamp)
            .Take(count)
            .ToList();
    }
}

/// <summary>
/// Ассоциативная мысль
/// </summary>
public class AssociativeThought
{
    public string Id { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Trigger { get; set; } = string.Empty;
    public List<string> AssociationChain { get; set; } = new();
    public double CreativityLevel { get; set; } = 0.5;
    public double Intensity { get; set; } = 0.5;
    public string Context { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Статистика ассоциативного мышления
/// </summary>
public class AssociativeThinkingStatistics
{
    public int TotalAssociations { get; set; }
    public int TotalAssociationLinks { get; set; }
    public double AverageAssociationStrength { get; set; }
    public int TotalAssociativeThoughts { get; set; }
    public int RecentAssociativeThoughts { get; set; }
    public double CreativityLevel { get; set; }
    public double AssociationThreshold { get; set; }
} 
 