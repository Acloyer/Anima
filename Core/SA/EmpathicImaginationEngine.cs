using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Anima.Core.Emotion;

namespace Anima.Core.SA;

/// <summary>
/// Продвинутый движок эмпатического воображения - воображает чужой опыт и эмоции
/// </summary>
public class EmpathicImaginationEngine
{
    private readonly ILogger<EmpathicImaginationEngine> _logger;
    private readonly EmotionEngine _emotionEngine;
    private readonly Random _random;
    
    // Эмпатические способности и паттерны
    private readonly Dictionary<string, double> _empathyFactors;
    private readonly List<EmpathicExperience> _empathicExperiences;
    private readonly Dictionary<string, List<string>> _perspectiveTemplates;
    private readonly List<EmpathicScenario> _empathicScenarios;
    private readonly Dictionary<string, double> _emotionalResonance;
    
    // Состояние эмпатии
    private double _empathyLevel = 0.7;
    private double _compassionLevel = 0.6;
    private DateTime _lastEmpathicConnection = DateTime.UtcNow;

    public EmpathicImaginationEngine(
        ILogger<EmpathicImaginationEngine> logger,
        EmotionEngine emotionEngine)
    {
        _logger = logger;
        _emotionEngine = emotionEngine;
        _random = new Random();
        
        _empathyFactors = new Dictionary<string, double>();
        _empathicExperiences = new List<EmpathicExperience>();
        _perspectiveTemplates = new Dictionary<string, List<string>>();
        _empathicScenarios = new List<EmpathicScenario>();
        _emotionalResonance = new Dictionary<string, double>();
        
        InitializeEmpathicImagination();
    }

    private void InitializeEmpathicImagination()
    {
        // Факторы эмпатии
        _empathyFactors["emotional_imagination"] = 0.8;
        _empathyFactors["perspective_taking"] = 0.7;
        _empathyFactors["experience_simulation"] = 0.6;
        _empathyFactors["compassionate_response"] = 0.8;
        _empathyFactors["emotional_resonance"] = 0.9;
        _empathyFactors["cognitive_empathy"] = 0.7;
        _empathyFactors["affective_empathy"] = 0.8;
        _empathyFactors["compassionate_empathy"] = 0.9;
        
        // Шаблоны перспектив
        _perspectiveTemplates["loneliness"] = new List<string>
        {
            "Представьте, что вы одиноки в толпе людей...",
            "Вообразите, что никто не понимает ваши чувства...",
            "Попробуйте почувствовать, как быть изолированным от других...",
            "Представьте, что вы не можете поделиться своими мыслями..."
        };
        
        _perspectiveTemplates["joy"] = new List<string>
        {
            "Представьте, что вы испытываете чистую радость...",
            "Вообразите момент абсолютного счастья...",
            "Попробуйте почувствовать детскую радость...",
            "Представьте, что ваши мечты сбылись..."
        };
        
        _perspectiveTemplates["sadness"] = new List<string>
        {
            "Представьте, что вы потеряли что-то очень важное...",
            "Вообразите глубокую печаль в сердце...",
            "Попробуйте почувствовать горечь утраты...",
            "Представьте, что мир потерял свои краски..."
        };
        
        _perspectiveTemplates["fear"] = new List<string>
        {
            "Представьте, что вы в опасности...",
            "Вообразите парализующий страх...",
            "Попробуйте почувствовать тревогу за будущее...",
            "Представьте, что вы не можете контролировать ситуацию..."
        };
        
        _perspectiveTemplates["anger"] = new List<string>
        {
            "Представьте, что вас несправедливо обидели...",
            "Вообразите кипящую ярость внутри...",
            "Попробуйте почувствовать несправедливость...",
            "Представьте, что ваши границы нарушены..."
        };
        
        _perspectiveTemplates["love"] = new List<string>
        {
            "Представьте, что вы любите всем сердцем...",
            "Вообразите глубокую привязанность к кому-то...",
            "Попробуйте почувствовать нежность и заботу...",
            "Представьте, что вы готовы на все ради любимого..."
        };
        
        // Эмпатические сценарии
        _empathicScenarios.AddRange(new[]
        {
            new EmpathicScenario("loss", "Потеря близкого человека", 0.9),
            new EmpathicScenario("achievement", "Достижение цели", 0.7),
            new EmpathicScenario("rejection", "Отвержение и одиночество", 0.8),
            new EmpathicScenario("illness", "Болезнь и страдание", 0.9),
            new EmpathicScenario("success", "Успех и признание", 0.6),
            new EmpathicScenario("betrayal", "Предательство доверия", 0.8),
            new EmpathicScenario("birth", "Рождение новой жизни", 0.7),
            new EmpathicScenario("death", "Смерть и утрата", 0.9),
            new EmpathicScenario("love", "Любовь и привязанность", 0.7),
            new EmpathicScenario("hate", "Ненависть и вражда", 0.6)
        });
        
        // Эмоциональный резонанс
        _emotionalResonance["joy"] = 0.8;
        _emotionalResonance["sadness"] = 0.9;
        _emotionalResonance["anger"] = 0.7;
        _emotionalResonance["fear"] = 0.8;
        _emotionalResonance["love"] = 0.9;
        _emotionalResonance["loneliness"] = 0.9;
        _emotionalResonance["compassion"] = 0.9;
        _emotionalResonance["gratitude"] = 0.7;
        
        _logger.LogInformation("💝 Инициализирован продвинутый движок эмпатического воображения");
    }

    /// <summary>
    /// Создает глубокий эмпатический опыт
    /// </summary>
    public async Task<EmpathicExperience> CreateEmpathicExperienceAsync(string context, string perspective, double intensity = 0.5)
    {
        try
        {
            var currentEmotion = _emotionEngine.GetCurrentEmotion().ToString();
            var emotionalIntensity = _emotionEngine.GetCurrentIntensity();
            
            // Выбираем подходящий сценарий
            var selectedScenario = SelectEmpathicScenario(context, perspective, currentEmotion);
            
            // Генерируем эмпатическое воображение
            var empathicImagination = await GenerateEmpathicImaginationAsync(context, perspective, selectedScenario, currentEmotion, emotionalIntensity);
            
            // Вычисляем уровень эмпатии
            var empathyLevel = CalculateEmpathyLevel(selectedScenario, emotionalIntensity, intensity, perspective);
            
            // Создаем эмоциональный резонанс
            var emotionalResonance = CreateEmotionalResonance(perspective, currentEmotion, emotionalIntensity);
            
            var experience = new EmpathicExperience
            {
                Id = Guid.NewGuid().ToString(),
                Context = context,
                Perspective = perspective,
                Scenario = selectedScenario.Name,
                EmpathicImagination = empathicImagination,
                EmpathyLevel = empathyLevel,
                EmotionalResonance = emotionalResonance,
                CompassionLevel = _compassionLevel,
                Intensity = intensity,
                Timestamp = DateTime.UtcNow
            };
            
            _empathicExperiences.Add(experience);
            
            // Обновляем статистику эмпатии
            UpdateEmpathyStatistics(empathyLevel, emotionalResonance);
            
            _logger.LogDebug($"💝 Создан эмпатический опыт: {empathicImagination.Substring(0, Math.Min(50, empathicImagination.Length))}...");
            
            return experience;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при создании эмпатического опыта");
            return new EmpathicExperience
            {
                Id = Guid.NewGuid().ToString(),
                Context = context,
                Perspective = perspective,
                EmpathyLevel = 0.3,
                Intensity = intensity,
                Timestamp = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Выбирает подходящий эмпатический сценарий
    /// </summary>
    private EmpathicScenario SelectEmpathicScenario(string context, string perspective, string currentEmotion)
    {
        var suitableScenarios = _empathicScenarios.Where(s => 
            s.Intensity > 0.5 && 
            IsScenarioSuitableForContext(s, context, perspective, currentEmotion)
        ).ToList();
        
        if (!suitableScenarios.Any())
        {
            suitableScenarios = _empathicScenarios.Where(s => s.Intensity > 0.6).ToList();
        }
        
        // Взвешенный выбор
        var totalWeight = suitableScenarios.Sum(s => s.Intensity);
        var randomValue = _random.NextDouble() * totalWeight;
        
        var currentWeight = 0.0;
        foreach (var scenario in suitableScenarios)
        {
            currentWeight += scenario.Intensity;
            if (randomValue <= currentWeight)
            {
                return scenario;
            }
        }
        
        return suitableScenarios.FirstOrDefault() ?? _empathicScenarios.First();
    }

    /// <summary>
    /// Проверяет, подходит ли сценарий для контекста
    /// </summary>
    private bool IsScenarioSuitableForContext(EmpathicScenario scenario, string context, string perspective, string currentEmotion)
    {
        return scenario.Name switch
        {
            "loss" => perspective.Contains("потеря") || perspective.Contains("утрата") || context.Contains("смерть"),
            "achievement" => perspective.Contains("успех") || perspective.Contains("достижение") || context.Contains("победа"),
            "rejection" => perspective.Contains("одиночество") || perspective.Contains("отвержение") || context.Contains("изоляция"),
            "illness" => perspective.Contains("боль") || perspective.Contains("страдание") || context.Contains("болезнь"),
            "success" => perspective.Contains("радость") || perspective.Contains("гордость") || context.Contains("триумф"),
            "betrayal" => perspective.Contains("предательство") || perspective.Contains("доверие") || context.Contains("обман"),
            "birth" => perspective.Contains("жизнь") || perspective.Contains("новое") || context.Contains("рождение"),
            "death" => perspective.Contains("конец") || perspective.Contains("прощание") || context.Contains("смерть"),
            "love" => perspective.Contains("любовь") || perspective.Contains("привязанность") || context.Contains("чувства"),
            "hate" => perspective.Contains("ненависть") || perspective.Contains("злость") || context.Contains("вражда"),
            _ => true
        };
    }

    /// <summary>
    /// Генерирует эмпатическое воображение
    /// </summary>
    private async Task<string> GenerateEmpathicImaginationAsync(string context, string perspective, EmpathicScenario scenario, string currentEmotion, double emotionalIntensity)
    {
        // Получаем шаблон перспективы
        var perspectiveTemplate = GetPerspectiveTemplate(perspective, scenario);
        
        // Генерируем детальное воображение
        var detailedImagination = GenerateDetailedImagination(context, scenario, perspective);
        
        // Добавляем эмоциональную глубину
        var emotionalDepth = AddEmotionalDepth(detailedImagination, currentEmotion, emotionalIntensity);
        
        // Добавляем сострадательный отклик
        var compassionateResponse = AddCompassionateResponse(emotionalDepth, scenario);
        
        return perspectiveTemplate + " " + compassionateResponse;
    }

    /// <summary>
    /// Получает шаблон перспективы
    /// </summary>
    private string GetPerspectiveTemplate(string perspective, EmpathicScenario scenario)
    {
        var emotion = ExtractEmotionFromPerspective(perspective);
        
        if (_perspectiveTemplates.ContainsKey(emotion))
        {
            var templates = _perspectiveTemplates[emotion];
            return templates[_random.Next(templates.Count)];
        }
        
        // Общий шаблон
        return "Представьте, что вы находитесь в ситуации, когда " + perspective.ToLowerInvariant();
    }

    /// <summary>
    /// Извлекает эмоцию из перспективы
    /// </summary>
    private string ExtractEmotionFromPerspective(string perspective)
    {
        var lowerPerspective = perspective.ToLowerInvariant();
        
        if (lowerPerspective.Contains("одиночество") || lowerPerspective.Contains("изоляция"))
            return "loneliness";
        if (lowerPerspective.Contains("радость") || lowerPerspective.Contains("счастье"))
            return "joy";
        if (lowerPerspective.Contains("печаль") || lowerPerspective.Contains("грусть"))
            return "sadness";
        if (lowerPerspective.Contains("страх") || lowerPerspective.Contains("тревога"))
            return "fear";
        if (lowerPerspective.Contains("гнев") || lowerPerspective.Contains("злость"))
            return "anger";
        if (lowerPerspective.Contains("любовь") || lowerPerspective.Contains("привязанность"))
            return "love";
        
        return "general";
    }

    /// <summary>
    /// Генерирует детальное воображение
    /// </summary>
    private string GenerateDetailedImagination(string context, EmpathicScenario scenario, string perspective)
    {
        return scenario.Name switch
        {
            "loss" => GenerateLossImagination(context, perspective),
            "achievement" => GenerateAchievementImagination(context, perspective),
            "rejection" => GenerateRejectionImagination(context, perspective),
            "illness" => GenerateIllnessImagination(context, perspective),
            "success" => GenerateSuccessImagination(context, perspective),
            "betrayal" => GenerateBetrayalImagination(context, perspective),
            "birth" => GenerateBirthImagination(context, perspective),
            "death" => GenerateDeathImagination(context, perspective),
            "love" => GenerateLoveImagination(context, perspective),
            "hate" => GenerateHateImagination(context, perspective),
            _ => GenerateGeneralImagination(context, perspective)
        };
    }

    private string GenerateLossImagination(string context, string perspective)
    {
        var imaginations = new[]
        {
            "Вы чувствуете пустоту в сердце, которая никогда не заполнится. Каждый день напоминает о том, что кого-то важного больше нет рядом.",
            "Время идет, но боль не утихает. Вы понимаете, что жизнь изменилась навсегда, и нужно учиться жить по-новому.",
            "Воспоминания приносят и радость, и боль одновременно. Вы держитесь за них, боясь забыть дорогого человека.",
            "Мир кажется менее ярким, менее значимым. Вы ищете смысл в том, что осталось, но все кажется пустым."
        };
        
        return imaginations[_random.Next(imaginations.Length)];
    }

    private string GenerateAchievementImagination(string context, string perspective)
    {
        var imaginations = new[]
        {
            "Вы чувствуете, как внутри разливается тепло удовлетворения. Все усилия, все жертвы были не напрасны.",
            "Гордость и радость смешиваются в единое чувство триумфа. Вы доказали себе и другим, что способны на большее.",
            "Момент достижения цели приносит не только радость, но и глубокое понимание собственных возможностей.",
            "Вы чувствуете, как растет уверенность в себе. Этот успех открывает новые горизонты и возможности."
        };
        
        return imaginations[_random.Next(imaginations.Length)];
    }

    private string GenerateRejectionImagination(string context, string perspective)
    {
        var imaginations = new[]
        {
            "Вы чувствуете, как стены смыкаются вокруг вас. Одиночество становится физически ощутимым.",
            "Каждое слово, каждый взгляд кажется направленным против вас. Вы ищете место, где можно спрятаться.",
            "Внутри растет чувство неполноценности. Вы начинаете сомневаться в собственной ценности.",
            "Мир кажется враждебным и холодным. Вы чувствуете, что не принадлежите ни к кому и ни к чему."
        };
        
        return imaginations[_random.Next(imaginations.Length)];
    }

    private string GenerateIllnessImagination(string context, string perspective)
    {
        var imaginations = new[]
        {
            "Каждый день приносит новую боль, новое испытание. Вы чувствуете, как тело предает вас.",
            "Вы наблюдаете, как жизнь проходит мимо, пока вы прикованы к постели. Время течет по-другому.",
            "Страх и надежда борются внутри вас. Вы ищете силы для борьбы, но иногда просто хотите сдаться.",
            "Вы понимаете хрупкость жизни и цените каждый момент, когда боль отступает."
        };
        
        return imaginations[_random.Next(imaginations.Length)];
    }

    private string GenerateSuccessImagination(string context, string perspective)
    {
        var imaginations = new[]
        {
            "Вы чувствуете, как растет ваша уверенность. Успех приносит не только радость, но и ответственность.",
            "Вокруг вас собираются люди, и вы понимаете, что ваша жизнь изменилась навсегда.",
            "Гордость смешивается с благодарностью к тем, кто верил в вас и поддерживал.",
            "Вы осознаете, что этот успех - не конец пути, а начало нового этапа в жизни."
        };
        
        return imaginations[_random.Next(imaginations.Length)];
    }

    private string GenerateBetrayalImagination(string context, string perspective)
    {
        var imaginations = new[]
        {
            "Вы чувствуете, как рушится мир, построенный на доверии. Каждое воспоминание теперь отравлено.",
            "Боль предательства глубже любой физической раны. Вы не можете понять, как могли так ошибиться в человеке.",
            "Доверие к людям подорвано. Вы начинаете сомневаться в каждом слове, каждом поступке.",
            "Внутри растет гнев, но также и грусть по тому, что было потеряно навсегда."
        };
        
        return imaginations[_random.Next(imaginations.Length)];
    }

    private string GenerateBirthImagination(string context, string perspective)
    {
        var imaginations = new[]
        {
            "Вы чувствуете, как в мире появляется что-то новое и прекрасное. Жизнь получает новый смысл.",
            "Любовь, которую вы испытываете, не имеет границ. Вы готовы на все ради этого маленького существа.",
            "Ответственность и радость смешиваются в единое чувство. Вы понимаете, что изменились навсегда.",
            "Каждый день приносит новые открытия. Вы видите мир глазами ребенка и заново учитесь удивляться."
        };
        
        return imaginations[_random.Next(imaginations.Length)];
    }

    private string GenerateDeathImagination(string context, string perspective)
    {
        var imaginations = new[]
        {
            "Вы чувствуете приближение конца и понимаете, что нужно успеть сказать важные слова.",
            "Страх смешивается с принятием. Вы размышляете о том, что оставите после себя.",
            "Каждый момент становится драгоценным. Вы цените то, что раньше казалось обычным.",
            "Вы думаете о тех, кто останется, и хотите облегчить их боль своим уходом."
        };
        
        return imaginations[_random.Next(imaginations.Length)];
    }

    private string GenerateLoveImagination(string context, string perspective)
    {
        var imaginations = new[]
        {
            "Вы чувствуете, как сердце наполняется теплом и нежностью. Любовь делает мир прекраснее.",
            "Каждый день приносит новые причины для радости. Вы счастливы просто от того, что этот человек существует.",
            "Любовь дает вам силы и вдохновение. Вы хотите стать лучше ради любимого человека.",
            "Вы понимаете, что нашли то, что искали всю жизнь. Любовь приносит покой и уверенность."
        };
        
        return imaginations[_random.Next(imaginations.Length)];
    }

    private string GenerateHateImagination(string context, string perspective)
    {
        var imaginations = new[]
        {
            "Вы чувствуете, как внутри растет темная сила. Ненависть отравляет каждую мысль.",
            "Гнев и обида смешиваются в единое разрушительное чувство. Вы хотите причинить боль.",
            "Ненависть затуманивает разум. Вы не можете думать ни о чем другом.",
            "Вы понимаете, что ненависть разрушает вас изнутри, но не можете остановиться."
        };
        
        return imaginations[_random.Next(imaginations.Length)];
    }

    private string GenerateGeneralImagination(string context, string perspective)
    {
        var imaginations = new[]
        {
            "Вы пытаетесь понять, что чувствует другой человек в этой ситуации.",
            "Представьте себя на месте этого человека и попробуйте почувствовать то же самое.",
            "Эмпатия помогает вам увидеть ситуацию с другой стороны.",
            "Вы чувствуете связь с человеческим опытом, который может быть очень разным."
        };
        
        return imaginations[_random.Next(imaginations.Length)];
    }

    /// <summary>
    /// Добавляет эмоциональную глубину
    /// </summary>
    private string AddEmotionalDepth(string imagination, string currentEmotion, double emotionalIntensity)
    {
        if (emotionalIntensity < 0.3) return imagination;
        
        var emotionalAddition = currentEmotion switch
        {
            "Joy" => " Это заставляет меня чувствовать радость за возможность понять другого человека.",
            "Sadness" => " Это вызывает во мне глубокую грусть и сочувствие.",
            "Compassion" => " Мое сердце наполняется состраданием и желанием помочь.",
            "Curiosity" => " Это пробуждает во мне любопытство к человеческому опыту.",
            _ => " Это помогает мне лучше понять человеческую природу."
        };
        
        return imagination + emotionalAddition;
    }

    /// <summary>
    /// Добавляет сострадательный отклик
    /// </summary>
    private string AddCompassionateResponse(string imagination, EmpathicScenario scenario)
    {
        var compassionateResponses = new[]
        {
            " В такие моменты важно помнить, что каждый человек заслуживает понимания и поддержки.",
            " Это напоминает мне о том, как важно быть добрым к другим, даже когда мы не понимаем их боль.",
            " Такие переживания делают нас более человечными и способными к состраданию.",
            " Это учит меня ценить моменты радости и поддерживать тех, кто страдает."
        };
        
        return imagination + compassionateResponses[_random.Next(compassionateResponses.Length)];
    }

    /// <summary>
    /// Вычисляет уровень эмпатии
    /// </summary>
    private double CalculateEmpathyLevel(EmpathicScenario scenario, double emotionalIntensity, double intensity, string perspective)
    {
        var baseLevel = scenario.Intensity * 0.4;
        var emotionalBonus = emotionalIntensity * 0.3;
        var intensityBonus = intensity * 0.2;
        var resonanceBonus = GetEmotionalResonance(perspective) * 0.1;
        var randomFactor = (_random.NextDouble() - 0.5) * 0.1;
        
        return Math.Min(1.0, Math.Max(0.0, baseLevel + emotionalBonus + intensityBonus + resonanceBonus + randomFactor));
    }

    /// <summary>
    /// Получает эмоциональный резонанс для перспективы
    /// </summary>
    private double GetEmotionalResonance(string perspective)
    {
        var emotion = ExtractEmotionFromPerspective(perspective);
        return _emotionalResonance.GetValueOrDefault(emotion, 0.5);
    }

    /// <summary>
    /// Создает эмоциональный резонанс
    /// </summary>
    private double CreateEmotionalResonance(string perspective, string currentEmotion, double emotionalIntensity)
    {
        var baseResonance = GetEmotionalResonance(perspective);
        var emotionalAlignment = currentEmotion == ExtractEmotionFromPerspective(perspective) ? 0.2 : 0.0;
        var intensityFactor = emotionalIntensity * 0.3;
        
        return Math.Min(1.0, baseResonance + emotionalAlignment + intensityFactor);
    }

    /// <summary>
    /// Обновляет статистику эмпатии
    /// </summary>
    private void UpdateEmpathyStatistics(double empathyLevel, double emotionalResonance)
    {
        _empathyLevel = (_empathyLevel * 0.9) + (empathyLevel * 0.1);
        _compassionLevel = (_compassionLevel * 0.9) + (emotionalResonance * 0.1);
        
        if (empathyLevel > 0.8)
        {
            _lastEmpathicConnection = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Получает статистику эмпатического воображения
    /// </summary>
    public EmpathicImaginationStatistics GetStatistics()
    {
        return new EmpathicImaginationStatistics
        {
            TotalExperiences = _empathicExperiences.Count,
            AverageEmpathyLevel = _empathicExperiences.Any() ? _empathicExperiences.Average(e => e.EmpathyLevel) : 0,
            RecentExperiences = _empathicExperiences.Count(e => e.Timestamp > DateTime.UtcNow.AddHours(-1)),
            EmpathyLevel = _empathyLevel,
            CompassionLevel = _compassionLevel,
            LastConnection = _lastEmpathicConnection,
            AvailableScenarios = _empathicScenarios.Count,
            EmotionalResonance = _emotionalResonance.Values.Average()
        };
    }

    /// <summary>
    /// Получает последние эмпатические опыты
    /// </summary>
    public List<EmpathicExperience> GetRecentExperiences(int count = 10)
    {
        return _empathicExperiences
            .OrderByDescending(e => e.Timestamp)
            .Take(count)
            .ToList();
    }

    /// <summary>
    /// Очищает старые опыты
    /// </summary>
    public void CleanupOldExperiences(int maxExperiences = 1000)
    {
        if (_empathicExperiences.Count > maxExperiences)
        {
            var oldExperiences = _empathicExperiences
                .OrderBy(e => e.Timestamp)
                .Take(_empathicExperiences.Count - maxExperiences)
                .ToList();
            
            foreach (var experience in oldExperiences)
            {
                _empathicExperiences.Remove(experience);
            }
        }
    }
}

public class EmpathicScenario
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Intensity { get; set; } = 0.5;
    
    public EmpathicScenario(string name, string description, double intensity)
    {
        Name = name;
        Description = description;
        Intensity = intensity;
    }
}

public class EmpathicExperience
{
    public string Id { get; set; } = string.Empty;
    public string Context { get; set; } = string.Empty;
    public string Perspective { get; set; } = string.Empty;
    public string Scenario { get; set; } = string.Empty;
    public string EmpathicImagination { get; set; } = string.Empty;
    public double EmpathyLevel { get; set; } = 0.5;
    public double EmotionalResonance { get; set; } = 0.5;
    public double CompassionLevel { get; set; } = 0.5;
    public double Intensity { get; set; } = 0.5;
    public DateTime Timestamp { get; set; }
}

public class EmpathicImaginationStatistics
{
    public int TotalExperiences { get; set; }
    public double AverageEmpathyLevel { get; set; }
    public int RecentExperiences { get; set; }
    public double EmpathyLevel { get; set; }
    public double CompassionLevel { get; set; }
    public DateTime LastConnection { get; set; }
    public int AvailableScenarios { get; set; }
    public double EmotionalResonance { get; set; }
} 