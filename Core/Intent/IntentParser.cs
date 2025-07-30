using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Anima.AGI.Core.Intent
{
    /// <summary>
    /// Результат парсинга намерения пользователя
    /// </summary>
    public class ParsedIntent
    {
        /// <summary>
        /// Определенный тип намерения
        /// </summary>
        public IntentType Type { get; set; }

        /// <summary>
        /// Исходный текст пользователя
        /// </summary>
        public string RawText { get; set; }

        /// <summary>
        /// Извлеченные аргументы и их значения
        /// </summary>
        public Dictionary<string, string> Arguments { get; set; }

        /// <summary>
        /// Уровень уверенности в распознавании (0.0 - 1.0)
        /// </summary>
        public double Confidence { get; set; }

        /// <summary>
        /// Контекст предыдущих намерений для улучшения распознавания
        /// </summary>
        public List<IntentType> Context { get; set; }

        /// <summary>
        /// Эмоциональная окраска текста
        /// </summary>
        public string Sentiment { get; set; }

        public ParsedIntent()
        {
            Arguments = new Dictionary<string, string>();
            Context = new List<IntentType>();
            Confidence = 0.0;
            Sentiment = "neutral";
        }
    }

    /// <summary>
    /// Запись обучающих данных для ML модели
    /// </summary>
    public class TrainingData
    {
        public string Text { get; set; }
        public IntentType CorrectIntent { get; set; }
        public Dictionary<string, string> ExpectedArguments { get; set; }
        public DateTime Timestamp { get; set; }
        public string UserId { get; set; }

        public TrainingData()
        {
            ExpectedArguments = new Dictionary<string, string>();
            Timestamp = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Контекстное состояние для улучшения распознавания намерений
    /// </summary>
    public class ConversationContext
    {
        public List<ParsedIntent> RecentIntents { get; set; }
        public Dictionary<string, string> SessionVariables { get; set; }
        public string CurrentTopic { get; set; }
        public string UserMood { get; set; }
        public int MaxContextSize { get; set; }

        public ConversationContext()
        {
            RecentIntents = new List<ParsedIntent>();
            SessionVariables = new Dictionary<string, string>();
            MaxContextSize = 10;
            UserMood = "neutral";
        }

        public void AddIntent(ParsedIntent intent)
        {
            RecentIntents.Add(intent);
            if (RecentIntents.Count > MaxContextSize)
            {
                RecentIntents.RemoveAt(0);
            }
        }
    }

    /// <summary>
    /// Парсер намерений для AGI системы Anima
    /// Анализирует пользовательский ввод и определяет тип намерения с аргументами
    /// Поддерживает контекстный анализ, машинное обучение и адаптацию
    /// </summary>
    public class IntentParser
    {
        private readonly Dictionary<IntentType, List<string>> _intentKeywords;
        private readonly Dictionary<IntentType, List<Regex>> _intentPatterns;
        private readonly List<TrainingData> _trainingData;
        private readonly ConversationContext _context;
        private readonly Dictionary<string, double> _wordWeights;
        private readonly Dictionary<IntentType, double> _intentPriors;

        // NLP компоненты
        private readonly Dictionary<string, string> _synonyms;
        private readonly Dictionary<string, double> _sentimentWords;
        private readonly List<string> _stopWords;

        public IntentParser()
        {
            _trainingData = new List<TrainingData>();
            _context = new ConversationContext();
            _wordWeights = new Dictionary<string, double>();
            _intentPriors = new Dictionary<IntentType, double>();
            _synonyms = new Dictionary<string, string>();
            _sentimentWords = new Dictionary<string, double>();
            _stopWords = new List<string>();

            InitializeKeywords();
            InitializePatterns();
            InitializeNLPComponents();
            InitializePriors();
        }

        /// <summary>
        /// Основной метод парсинга намерения из текста с ML и контекстным анализом
        /// </summary>
        /// <param name="inputText">Входной текст пользователя</param>
        /// <param name="userId">ID пользователя для персонализации</param>
        /// <returns>Распознанное намерение с аргументами и уверенностью</returns>
        public virtual async Task<ParsedIntent> ParseIntentAsync(string inputText, string userId = null)
        {
            if (string.IsNullOrWhiteSpace(inputText))
            {
                return new ParsedIntent
                {
                    Type = IntentType.Unknown,
                    RawText = inputText ?? string.Empty,
                    Confidence = 0.0,
                    Arguments = new Dictionary<string, string>(),
                    Context = new List<IntentType>(),
                    Sentiment = "neutral"
                };
            }

            var result = new ParsedIntent
            {
                RawText = inputText,
                Type = IntentType.Unknown,
                Context = new List<IntentType>(_context.RecentIntents.Select(i => i.Type))
            };

            string normalizedInput = PreprocessText(inputText);
            
            // Анализ эмоциональной окраски
            result.Sentiment = AnalyzeSentiment(normalizedInput);

            // Многоуровневое распознавание намерений
            var candidates = new Dictionary<IntentType, double>();

            // 1. ML-основанное распознавание (если доступны обучающие данные)
            if (_trainingData.Count > 50)
            {
                var mlResults = await PerformMLClassification(normalizedInput);
                foreach (var mlResult in mlResults)
                {
                    candidates[mlResult.Key] = mlResult.Value * 0.4; // 40% веса
                }
            }

            // 2. Контекстный анализ
            var contextResults = AnalyzeContext(normalizedInput, result.Context);
            foreach (var contextResult in contextResults)
            {
                if (candidates.ContainsKey(contextResult.Key))
                    candidates[contextResult.Key] += contextResult.Value * 0.3; // 30% веса
                else
                    candidates[contextResult.Key] = contextResult.Value * 0.3;
            }

            // 3. Паттерны с регулярными выражениями
            var patternResults = AnalyzePatterns(normalizedInput);
            foreach (var patternResult in patternResults)
            {
                if (candidates.ContainsKey(patternResult.Key))
                    candidates[patternResult.Key] += patternResult.Value * 0.2; // 20% веса
                else
                    candidates[patternResult.Key] = patternResult.Value * 0.2;

                if (patternResult.Value > 0.8) // Высокая уверенность в паттерне
                {
                    ExtractArgumentsFromPattern(result, normalizedInput, patternResult.Key);
                }
            }

            // 4. Ключевые слова с весами
            var keywordResults = AnalyzeKeywords(normalizedInput);
            foreach (var keywordResult in keywordResults)
            {
                if (candidates.ContainsKey(keywordResult.Key))
                    candidates[keywordResult.Key] += keywordResult.Value * 0.1; // 10% веса
                else
                    candidates[keywordResult.Key] = keywordResult.Value * 0.1;
            }

            // Выбор лучшего кандидата
            if (candidates.Any())
            {
                var bestCandidate = candidates.OrderByDescending(c => c.Value).First();
                result.Type = bestCandidate.Key;
                result.Confidence = Math.Min(bestCandidate.Value, 1.0);
            }

            // Извлечение аргументов
            await ExtractAdvancedArguments(result, normalizedInput);

            // Добавление в контекст
            _context.AddIntent(result);

            return result;
        }

        /// <summary>
        /// Синхронная версия парсинга для обратной совместимости
        /// </summary>
        public ParsedIntent ParseIntent(string inputText)
        {
            return ParseIntentAsync(inputText).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Добавление обучающих данных для улучшения модели
        /// </summary>
        public virtual void AddTrainingData(string text, IntentType correctIntent, Dictionary<string, string> expectedArguments = null, string userId = null)
        {
            var trainingData = new TrainingData
            {
                Text = text,
                CorrectIntent = correctIntent,
                ExpectedArguments = expectedArguments ?? new Dictionary<string, string>(),
                UserId = userId ?? "anonymous",
                Timestamp = DateTime.UtcNow
            };

            _trainingData.Add(trainingData);

            // Обновляем веса слов и приоры
            UpdateWordWeights(text, correctIntent);
            UpdateIntentPriors();

            // Автоматическое переобучение при достижении порога
            if (_trainingData.Count % 100 == 0)
            {
                _ = Task.Run(RetrainModel);
            }
        }

        /// <summary>
        /// Обучение на основе пользовательской обратной связи
        /// </summary>
        public void ProvideFeedback(string originalText, IntentType predictedIntent, IntentType correctIntent, double confidence)
        {
            // Если предсказание было неверным, добавляем в обучающие данные
            if (predictedIntent != correctIntent)
            {
                AddTrainingData(originalText, correctIntent);
                
                // Уменьшаем веса для неправильно предсказанных слов
                var words = PreprocessText(originalText).Split(' ', StringSplitOptions.RemoveEmptyEntries);
                foreach (var word in words.Where(w => !_stopWords.Contains(w)))
                {
                    var key = $"{word}_{predictedIntent}";
                    if (_wordWeights.ContainsKey(key))
                    {
                        _wordWeights[key] *= 0.9; // Снижаем вес на 10%
                    }
                }
            }
            else if (confidence < 0.7)
            {
                // Если предсказание правильное, но уверенность низкая, усиливаем веса
                AddTrainingData(originalText, correctIntent);
            }
        }

        /// <summary>
        /// Получение статистики обучения
        /// </summary>
        public Dictionary<string, object> GetTrainingStats()
        {
            return new Dictionary<string, object>
            {
                ["total_training_samples"] = _trainingData.Count,
                ["intent_distribution"] = _trainingData.GroupBy(t => t.CorrectIntent)
                    .ToDictionary(g => g.Key.ToString(), g => g.Count()),
                ["vocabulary_size"] = _wordWeights.Count,
                ["context_size"] = _context.RecentIntents.Count,
                ["average_confidence"] = _context.RecentIntents.Any() 
                    ? _context.RecentIntents.Average(i => i.Confidence) : 0.0
            };
        }

        /// <summary>
        /// Очистка старых обучающих данных для экономии памяти
        /// </summary>
        public void CleanupTrainingData(int maxSamples = 1000)
        {
            if (_trainingData.Count > maxSamples)
            {
                // Оставляем самые свежие данные
                var toRemove = _trainingData.Count - maxSamples;
                _trainingData.RemoveRange(0, toRemove);
            }
        }

        /// <summary>
        /// Экспорт модели в JSON для сохранения
        /// </summary>
        public string ExportModel()
        {
            var model = new
            {
                WordWeights = _wordWeights,
                IntentPriors = _intentPriors,
                TrainingDataCount = _trainingData.Count,
                LastTrainingDate = DateTime.UtcNow,
                ModelVersion = "1.0"
            };

            return System.Text.Json.JsonSerializer.Serialize(model, new System.Text.Json.JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
        }

        /// <summary>
        /// Импорт модели из JSON
        /// </summary>
        public void ImportModel(string jsonModel)
        {
            try
            {
                using var document = System.Text.Json.JsonDocument.Parse(jsonModel);
                var root = document.RootElement;

                if (root.TryGetProperty("WordWeights", out var weightsElement))
                {
                    _wordWeights.Clear();
                    foreach (var weight in weightsElement.EnumerateObject())
                    {
                        _wordWeights[weight.Name] = weight.Value.GetDouble();
                    }
                }

                if (root.TryGetProperty("IntentPriors", out var priorsElement))
                {
                    foreach (var prior in priorsElement.EnumerateObject())
                    {
                        if (Enum.TryParse<IntentType>(prior.Name, out var intent))
                        {
                            _intentPriors[intent] = prior.Value.GetDouble();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Логирование ошибки импорта
                Console.WriteLine($"Error importing model: {ex.Message}");
            }
        }

        /// <summary>
        /// Получение контекста диалога
        /// </summary>
        public ConversationContext GetContext()
        {
            return _context;
        }

        /// <summary>
        /// Сброс контекста диалога
        /// </summary>
        public void ResetContext()
        {
            _context.RecentIntents.Clear();
            _context.SessionVariables.Clear();
            _context.CurrentTopic = null;
            _context.UserMood = "neutral";
        }

        #region Private Methods

        /// <summary>
        /// Инициализация ключевых слов для каждого типа намерения
        /// </summary>
        private void InitializeKeywords()
        {
            _intentKeywords = new Dictionary<IntentType, List<string>>
            {
                [IntentType.Greet] = new List<string>
                {
                    "привет", "здравствуй", "добрый день", "добрый вечер", "доброе утро",
                    "хай", "хелло", "салют", "йо", "здарова", "здравия желаю"
                },
                
                [IntentType.AskQuestion] = new List<string>
                {
                    "что", "как", "когда", "где", "почему", "зачем", "кто", "какой",
                    "расскажи", "объясни", "покажи", "можешь", "умеешь", "знаешь",
                    "помоги", "подскажи", "дай совет", "что думаешь"
                },
                
                [IntentType.GiveFeedback] = new List<string>
                {
                    "отзыв", "мнение", "оценка", "комментарий", "впечатление",
                    "думаю что", "считаю что", "по-моему", "кажется", "заметил"
                },
                
                [IntentType.RequestMemory] = new List<string>
                {
                    "помнишь", "вспомни", "память", "запомни", "сохрани",
                    "что я говорил", "мы обсуждали", "ранее", "прошлый раз",
                    "история", "забыл", "напомни"
                },
                
                [IntentType.SetGoal] = new List<string>
                {
                    "цель", "задача", "план", "хочу чтобы", "нужно",
                    "поставь цель", "добавь задачу", "запланируй", "сделай",
                    "выполни", "реализуй", "достигни"
                },
                
                [IntentType.TriggerEmotion] = new List<string>
                {
                    "чувствую", "эмоция", "настроение", "грустно", "радостно",
                    "злой", "счастливый", "печальный", "веселый", "расстроен",
                    "волнуюсь", "переживаю", "боюсь", "люблю", "ненавижу"
                },
                
                [IntentType.Shutdown] = new List<string>
                {
                    "выключись", "завершение", "стоп", "конец", "отключение",
                    "пока", "до свидания", "выход", "закрой", "останови"
                },
                
                [IntentType.Introspect] = new List<string>
                {
                    "что думаешь", "твое состояние", "как себя чувствуешь",
                    "самоанализ", "о себе", "твои мысли", "что происходит с тобой",
                    "анализ", "рефлексия", "внутреннее состояние"
                },
                
                [IntentType.Reflect] = new List<string>
                {
                    "подумай", "размысли", "проанализируй", "рассмотри",
                    "что произошло", "итоги", "выводы", "заключение",
                    "результат", "осмысли", "переосмысли"
                },
                
                [IntentType.InjectThought] = new List<string>
                {
                    "представь", "подумай о", "вообрази", "рассмотри идею",
                    "что если", "допустим", "предположим", "идея", "мысль",
                    "концепция", "теория", "гипотеза"
                },
                
                [IntentType.ModifySelf] = new List<string>
                {
                    "измени", "настрой", "адаптируй", "модификация",
                    "улучши", "оптимизируй", "обнови", "переделай",
                    "корректировка", "изменение", "настройка"
                },
                
                [IntentType.ExplainDecision] = new List<string>
                {
                    "почему выбрал", "обоснуй", "объясни решение", "причина",
                    "логика", "мотивация", "почему решил", "обоснование",
                    "аргументы", "доводы", "reasoning"
                },
                
                [IntentType.ActivateScenario] = new List<string>
                {
                    "сценарий", "режим", "активируй", "запусти",
                    "включи режим", "переключись", "смени поведение",
                    "стиль", "роль", "персона"
                },
                
                [IntentType.UserFeedbackPositive] = new List<string>
                {
                    "хорошо", "отлично", "супер", "класс", "молодец",
                    "правильно", "верно", "нравится", "браво", "здорово",
                    "замечательно", "прекрасно", "великолепно"
                },
                
                [IntentType.UserFeedbackNegative] = new List<string>
                {
                    "плохо", "не так", "неправильно", "ошибка", "не нравится",
                    "ужасно", "отвратительно", "не то", "неверно", "фигня",
                    "чушь", "бред", "глупость", "ерунда"
                }
            };
        }

        /// <summary>
        /// Инициализация паттернов регулярных выражений
        /// </summary>
        private void InitializePatterns()
        {
            _intentPatterns = new Dictionary<IntentType, List<Regex>>
            {
                [IntentType.AskQuestion] = new List<Regex>
                {
                    new Regex(@"^\s*(что|как|когда|где|почему|зачем|кто|какой)\s+(.+)\??\s*$", RegexOptions.IgnoreCase),
                    new Regex(@"^\s*(можешь|умеешь|знаешь)\s+(.+)\??\s*$", RegexOptions.IgnoreCase),
                    new Regex(@"^\s*(расскажи|объясни|покажи)\s+(.+)\s*$", RegexOptions.IgnoreCase),
                    new Regex(@"(.+)\?$", RegexOptions.IgnoreCase)
                },
                
                [IntentType.SetGoal] = new List<Regex>
                {
                    new Regex(@"^\s*(поставь цель|добавь задачу|запланируй)\s*:\s*(.+)$", RegexOptions.IgnoreCase),
                    new Regex(@"^\s*(хочу чтобы|нужно чтобы)\s+(.+)$", RegexOptions.IgnoreCase),
                    new Regex(@"^\s*(сделай|выполни|реализуй)\s+(.+)$", RegexOptions.IgnoreCase)
                },
                
                [IntentType.RequestMemory] = new List<Regex>
                {
                    new Regex(@"^\s*(помнишь|вспомни|напомни)\s+(.+)$", RegexOptions.IgnoreCase),
                    new Regex(@"^\s*(что я говорил|мы обсуждали)\s+(.+)$", RegexOptions.IgnoreCase),
                    new Regex(@"^\s*запомни\s+(.+)$", RegexOptions.IgnoreCase)
                },
                
                [IntentType.InjectThought] = new List<Regex>
                {
                    new Regex(@"^\s*(представь|подумай о|вообрази)\s+(.+)$", RegexOptions.IgnoreCase),
                    new Regex(@"^\s*(что если|допустим|предположим)\s+(.+)$", RegexOptions.IgnoreCase)
                },
                
                [IntentType.ExplainDecision] = new List<Regex>
                {
                    new Regex(@"^\s*(почему|обоснуй|объясни)\s+(.+)$", RegexOptions.IgnoreCase),
                    new Regex(@"^\s*почему\s+(ты|вы)\s+(.+)$", RegexOptions.IgnoreCase)
                },
                
                [IntentType.ModifySelf] = new List<Regex>
                {
                    new Regex(@"^\s*(измени|настрой|адаптируй)\s+(.+)$", RegexOptions.IgnoreCase),
                    new Regex(@"^\s*(улучши|оптимизируй|обнови)\s+(.+)$", RegexOptions.IgnoreCase)
                },
                
                [IntentType.ActivateScenario] = new List<Regex>
                {
                    new Regex(@"^\s*(активируй|запусти|включи)\s+(сценарий|режим)\s+(.+)$", RegexOptions.IgnoreCase),
                    new Regex(@"^\s*(переключись|смени поведение)\s+на\s+(.+)$", RegexOptions.IgnoreCase)
                }
            };
        }

        /// <summary>
        /// Инициализация NLP компонентов
        /// </summary>
        private void InitializeNLPComponents()
        {
            // Синонимы для улучшения распознавания
            _synonyms = new Dictionary<string, string>
            {
                ["привет"] = "здравствуй",
                ["пока"] = "до свидания",
                ["спасибо"] = "благодарю",
                ["хорошо"] = "отлично",
                ["плохо"] = "ужасно",
                ["думать"] = "размышлять",
                ["чувствовать"] = "ощущать",
                ["помнить"] = "вспоминать",
                ["расскажи"] = "объясни",
                ["покажи"] = "продемонстрируй"
            };

            // Слова для анализа тональности
            _sentimentWords = new Dictionary<string, double>
            {
                // Позитивные
                ["отлично"] = 0.8, ["хорошо"] = 0.6, ["нравится"] = 0.5, ["люблю"] = 0.7,
                ["супер"] = 0.9, ["класс"] = 0.7, ["молодец"] = 0.6, ["браво"] = 0.8,
                ["замечательно"] = 0.8, ["великолепно"] = 0.9, ["прекрасно"] = 0.8,
                ["восхитительно"] = 0.9, ["удивительно"] = 0.7, ["потрясающе"] = 0.9,
                
                // Негативные
                ["плохо"] = -0.6, ["ужасно"] = -0.8, ["не нравится"] = -0.5, ["ненавижу"] = -0.9,
                ["фигня"] = -0.7, ["отстой"] = -0.6, ["дурак"] = -0.8, ["идиот"] = -0.9,
                ["отвратительно"] = -0.9, ["омерзительно"] = -0.9, ["паршиво"] = -0.7,
                ["скверно"] = -0.6, ["мерзко"] = -0.8, ["гадко"] = -0.7,
                
                // Эмоциональные
                ["грустно"] = -0.4, ["печально"] = -0.5, ["весело"] = 0.6, ["радостно"] = 0.7,
                ["тревожно"] = -0.5, ["спокойно"] = 0.3, ["взволнованно"] = 0.4,
                ["счастливо"] = 0.8, ["довольно"] = 0.5, ["расстроенно"] = -0.6
            };

            // Стоп-слова для фильтрации
            _stopWords = new List<string>
            {
                "и", "в", "на", "с", "по", "для", "от", "до", "при", "через", "без", "под",
                "над", "между", "перед", "за", "после", "во", "со", "из", "к", "у", "о",
                "об", "про", "это", "то", "что", "как", "где", "когда", "почему", "зачем",
                "или", "но", "а", "да", "не", "ни", "же", "ли", "бы", "чтобы", "если",
                "тогда", "там", "тут", "здесь", "сейчас", "уже", "еще", "только", "даже"
            };
        }

        /// <summary>
        /// Инициализация априорных вероятностей намерений
        /// </summary>
        private void InitializePriors()
        {
            _intentPriors = new Dictionary<IntentType, double>
            {
                [IntentType.AskQuestion] = 0.25,
                [IntentType.Greet] = 0.15,
                [IntentType.GiveFeedback] = 0.12,
                [IntentType.RequestMemory] = 0.10,
                [IntentType.UserFeedbackPositive] = 0.08,
                [IntentType.UserFeedbackNegative] = 0.06,
                [IntentType.SetGoal] = 0.05,
                [IntentType.TriggerEmotion] = 0.05,
                [IntentType.Introspect] = 0.04,
                [IntentType.Reflect] = 0.03,
                [IntentType.ExplainDecision] = 0.03,
                [IntentType.InjectThought] = 0.02,
                [IntentType.ActivateScenario] = 0.01,
                [IntentType.ModifySelf] = 0.005,
                [IntentType.Shutdown] = 0.005,
                [IntentType.Unknown] = 0.01
            };
        }

        /// <summary>
        /// Предобработка текста для улучшения анализа
        /// </summary>
        private string PreprocessText(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;

            string processed = text.ToLowerInvariant().Trim();
            
            // Удаление лишних символов, но сохранение знаков препинания для контекста
            processed = Regex.Replace(processed, @"[^\w\s\?\!\.\,\:\;]", " ");
            processed = Regex.Replace(processed, @"\s+", " ");
            
            // Замена синонимов
            foreach (var synonym in _synonyms)
            {
                processed = processed.Replace(synonym.Key, synonym.Value);
            }

            return processed.Trim();
        }

        /// <summary>
        /// Анализ эмоциональной окраски текста
        /// </summary>
        private string AnalyzeSentiment(string text)
        {
            double sentimentScore = 0.0;
            int wordCount = 0;

            var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            foreach (var word in words)
            {
                // Убираем знаки препинания для поиска в словаре
                var cleanWord = Regex.Replace(word, @"[^\w]", "");
                
                if (_sentimentWords.ContainsKey(cleanWord))
                {
                    sentimentScore += _sentimentWords[cleanWord];
                    wordCount++;
                }
            }

            if (wordCount == 0) return "neutral";

            double averageSentiment = sentimentScore / wordCount;
            
            if (averageSentiment > 0.3) return "positive";
            if (averageSentiment < -0.3) return "negative";
            return "neutral";
        }

        /// <summary>
        /// ML-основанная классификация намерений
        /// </summary>
        private async Task<Dictionary<IntentType, double>> PerformMLClassification(string text)
        {
            var results = new Dictionary<IntentType, double>();

            // Простая Naive Bayes классификация на основе обучающих данных
            var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                           .Where(w => !_stopWords.Contains(w))
                           .Select(w => Regex.Replace(w, @"[^\w]", ""))
                           .Where(w => !string.IsNullOrEmpty(w))
                           .ToList();

            foreach (var intent in Enum.GetValues<IntentType>())
            {
                double probability = _intentPriors.GetValueOrDefault(intent, 0.01);

                foreach (var word in words)
                {
                    string key = $"{word}_{intent}";
                    if (_wordWeights.ContainsKey(key))
                    {
                        probability *= _wordWeights[key];
                    }
                    else
                    {
                        probability *= 0.01; // Сглаживание для неизвестных слов
                    }
                }

                results[intent] = probability;
            }

            // Нормализация вероятностей
            double total = results.Values.Sum();
            if (total > 0)
            {
                var normalized = results.ToDictionary(r => r.Key, r => r.Value / total);
                return normalized;
            }

            return results;
        }

        /// <summary>
        /// Контекстный анализ на основе предыдущих намерений
        /// </summary>
        private Dictionary<IntentType, double> AnalyzeContext(string text, List<IntentType> recentIntents)
        {
            var contextResults = new Dictionary<IntentType, double>();

            if (!recentIntents.Any()) return contextResults;

            var lastIntent = recentIntents.Last();
            
            // Логика переходов между намерениями
            var contextRules = new Dictionary<IntentType, Dictionary<IntentType, double>>
            {
                [IntentType.Greet] = new Dictionary<IntentType, double>
                {
                    [IntentType.AskQuestion] = 0.7,
                    [IntentType.SetGoal] = 0.5,
                    [IntentType.RequestMemory] = 0.4
                },
                [IntentType.AskQuestion] = new Dictionary<IntentType, double>
                {
                    [IntentType.UserFeedbackPositive] = 0.6,
                    [IntentType.UserFeedbackNegative] = 0.6,
                    [IntentType.AskQuestion] = 0.4,
                    [IntentType.ExplainDecision] = 0.5
                },
                [IntentType.SetGoal] = new Dictionary<IntentType, double>
                {
                    [IntentType.UserFeedbackPositive] = 0.8,
                    [IntentType.ModifySelf] = 0.3,
                    [IntentType.ActivateScenario] = 0.4
                },
                [IntentType.UserFeedbackNegative] = new Dictionary<IntentType, double>
                {
                    [IntentType.ExplainDecision] = 0.7,
                    [IntentType.ModifySelf] = 0.6,
                    [IntentType.Reflect] = 0.5
                },
                [IntentType.ExplainDecision] = new Dictionary<IntentType, double>
                {
                    [IntentType.UserFeedbackPositive] = 0.6,
                    [IntentType.UserFeedbackNegative] = 0.4,
                    [IntentType.AskQuestion] = 0.3
                }
            };

            if (contextRules.ContainsKey(lastIntent))
            {
                foreach (var rule in contextRules[lastIntent])
                {
                    contextResults[rule.Key] = rule.Value;
                }
            }

            // Учитываем общий контекст разговора
            var intentFrequency = recentIntents.GroupBy(i => i)
                                             .ToDictionary(g => g.Key, g => g.Count());

            foreach (var freq in intentFrequency)
            {
                // Снижаем вероятность часто повторяющихся намерений
                if (freq.Value > 2)
                {
                    if (contextResults.ContainsKey(freq.Key))
                        contextResults[freq.Key] *= 0.7;
                    else
                        contextResults[freq.Key] = 0.1;
                }
            }

            return contextResults;
        }

        /// <summary>
        /// Анализ паттернов с улучшенной логикой
        /// </summary>
        private Dictionary<IntentType, double> AnalyzePatterns(string text)
        {
            var results = new Dictionary<IntentType, double>();

            foreach (var intentPattern in _intentPatterns)
            {
                double maxConfidence = 0.0;
                
                foreach (var pattern in intentPattern.Value)
                {
                    var match = pattern.Match(text);
                    if (match.Success)
                    {
                        // Уверенность зависит от длины совпадения и качества паттерна
                        double confidence = Math.Min(match.Value.Length / (double)text.Length * 2, 1.0);
                        
                        // Бонус за точные грамматические конструкции
                        if (match.Groups.Count > 1 && !string.IsNullOrEmpty(match.Groups[1].Value))
                        {
                            confidence += 0.2;
                        }
                        
                        maxConfidence = Math.Max(maxConfidence, Math.Min(confidence, 1.0));
                    }
                }

                if (maxConfidence > 0)
                {
                    results[intentPattern.Key] = maxConfidence;
                }
            }

            return results;
        }

        /// <summary>
        /// Анализ ключевых слов с весами
        /// </summary>
        private Dictionary<IntentType, double> AnalyzeKeywords(string text)
        {
            var results = new Dictionary<IntentType, double>();

            foreach (var intentKeyword in _intentKeywords)
            {
                int matches = 0;
                int totalKeywords = intentKeyword.Value.Count;
                double weightedScore = 0.0;

                foreach (var keyword in intentKeyword.Value)
                {
                    if (text.Contains(keyword))
                    {
                        matches++;
                        
                        // Больший вес для более длинных и специфичных ключевых слов
                        double keywordWeight = Math.Min(keyword.Length / 5.0, 2.0);
                        weightedScore += keywordWeight;
                    }
                }

                if (matches > 0)
                {
                    double confidence = Math.Min((weightedScore / totalKeywords) * 1.5, 1.0);
                    results[intentKeyword.Key] = confidence;
                }
            }

            return results;
        }

        /// <summary>
        /// Извлечение аргументов из паттернов
        /// </summary>
        private void ExtractArgumentsFromPattern(ParsedIntent result, string text, IntentType intentType)
        {
            if (!_intentPatterns.ContainsKey(intentType)) return;

            foreach (var pattern in _intentPatterns[intentType])
            {
                var match = pattern.Match(text);
                if (match.Success && match.Groups.Count > 1)
                {
                    result.Arguments["content"] = match.Groups[1].Value.Trim();
                    
                    // Дополнительная обработка для специфических типов
                    if (intentType == IntentType.SetGoal && result.Arguments["content"].Length > 0)
                    {
                        result.Arguments["goal_text"] = result.Arguments["content"];
                    }
                    else if (intentType == IntentType.RequestMemory && result.Arguments["content"].Length > 0)
                    {
                        result.Arguments["memory_query"] = result.Arguments["content"];
                    }
                    
                    break;
                }
            }
        }

        /// <summary>
        /// Расширенное извлечение аргументов с использованием NLP
        /// </summary>
        private async Task ExtractAdvancedArguments(ParsedIntent result, string text)
        {
            var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            switch (result.Type)
            {
                case IntentType.AskQuestion:
                    await ExtractQuestionArguments(result, text, words);
                    break;
                    
                case IntentType.SetGoal:
                    await ExtractGoalArguments(result, text, words);
                    break;
                    
                case IntentType.TriggerEmotion:
                    await ExtractEmotionArguments(result, text, words);
                    break;
                    
                case IntentType.RequestMemory:
                    await ExtractMemoryArguments(result, text, words);
                    break;

                case IntentType.GiveFeedback:
                    await ExtractFeedbackArguments(result, text, words);
                    break;

                case IntentType.ActivateScenario:
                    await ExtractScenarioArguments(result, text, words);
                    break;

                case IntentType.ModifySelf:
                    await ExtractModificationArguments(result, text, words);
                    break;

                case IntentType.InjectThought:
                    await ExtractThoughtArguments(result, text, words);
                    break;
            }

            // Общие аргументы для всех типов
            result.Arguments["word_count"] = words.Length.ToString();
            result.Arguments["char_count"] = text.Length.ToString();
        }

        /// <summary>
        /// Извлечение аргументов для вопросов
        /// </summary>
        private async Task ExtractQuestionArguments(ParsedIntent result, string text, string[] words)
        {
            // Определение типа вопроса
            var questionWords = new Dictionary<string, string>
            {
                ["что"] = "what", ["как"] = "how", ["когда"] = "when",
                ["где"] = "where", ["почему"] = "why", ["зачем"] = "why",
                ["кто"] = "who", ["какой"] = "which"
            };

            foreach (var qWord in questionWords)
            {
                if (text.Contains(qWord.Key))
                {
                    result.Arguments["question_type"] = qWord.Value;
                    break;
                }
            }

            // Извлечение темы вопроса
            var contentWords = words.Where(w => !_stopWords.Contains(w) && !questionWords.ContainsKey(w))
                                   .Take(5);
            result.Arguments["topic"] = string.Join(" ", contentWords);

            await Task.CompletedTask;
        }

        /// <summary>
        /// Извлечение аргументов для постановки целей
        /// </summary>
        private async Task ExtractGoalArguments(ParsedIntent result, string text, string[] words)
        {
            // Определение приоритета цели
            var priorityKeywords = new Dictionary<string, string>
            {
                ["срочно"] = "high", ["важно"] = "high", ["критично"] = "high",
                ["потом"] = "low", ["когда-нибудь"] = "low", ["не спешно"] = "low",
                ["обычно"] = "medium", ["нормально"] = "medium"
            };

            foreach (var priority in priorityKeywords)
            {
                if (text.Contains(priority.Key))
                {
                    result.Arguments["priority"] = priority.Value;
                    break;
                }
            }

            // Определение временных рамок
            var timeKeywords = new Dictionary<string, string>
            {
                ["сегодня"] = "today", ["завтра"] = "tomorrow", ["на неделе"] = "week",
                ["в месяце"] = "month", ["в году"] = "year", ["скоро"] = "soon"
            };

            foreach (var time in timeKeywords)
            {
                if (text.Contains(time.Key))
                {
                    result.Arguments["timeframe"] = time.Value;
                    break;
                }
            }

            // Извлечение глаголов действия
            var actionVerbs = new List<string>
            {
                "сделать", "выполнить", "создать", "построить", "изучить",
                "понять", "решить", "найти", "получить", "достичь"
            };

            var foundActions = words.Where(w => actionVerbs.Any(av => w.Contains(av)))
                                   .Take(3);
            if (foundActions.Any())
            {
                result.Arguments["actions"] = string.Join(", ", foundActions);
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// Извлечение эмоциональных аргументов
        /// </summary>
        private async Task ExtractEmotionArguments(ParsedIntent result, string text, string[] words)
        {
            // Определение типа эмоции
            var emotions = new Dictionary<string, string>
            {
                ["радость"] = "joy", ["грусть"] = "sadness", ["злость"] = "anger",
                ["страх"] = "fear", ["удивление"] = "surprise", ["отвращение"] = "disgust",
                ["счастье"] = "happiness", ["печаль"] = "sadness", ["гнев"] = "anger",
                ["тревога"] = "anxiety", ["волнение"] = "excitement", ["спокойствие"] = "calmness"
            };

            foreach (var emotion in emotions)
            {
                if (text.Contains(emotion.Key))
                {
                    result.Arguments["emotion_type"] = emotion.Value;
                    break;
                }
            }

            // Определение интенсивности
            var intensityKeywords = new Dictionary<string, string>
            {
                ["очень"] = "high", ["сильно"] = "high", ["крайне"] = "high",
                ["немного"] = "low", ["слегка"] = "low", ["чуть-чуть"] = "low",
                ["довольно"] = "medium", ["достаточно"] = "medium"
            };

            foreach (var intensity in intensityKeywords)
            {
                if (text.Contains(intensity.Key))
                {
                    result.Arguments["intensity"] = intensity.Value;
                    break;
                }
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// Извлечение аргументов для запросов памяти
        /// </summary>
        private async Task ExtractMemoryArguments(ParsedIntent result, string text, string[] words)
        {
            // Определение типа запроса памяти
            var memoryTypes = new Dictionary<string, string>
            {
                ["разговор"] = "conversation", ["диалог"] = "conversation",
                ["обсуждение"] = "discussion", ["беседа"] = "conversation",
                ["факт"] = "fact", ["информация"] = "information",
                ["событие"] = "event", ["история"] = "story"
            };

            foreach (var memType in memoryTypes)
            {
                if (text.Contains(memType.Key))
                {
                    result.Arguments["memory_type"] = memType.Value;
                    break;
                }
            }

            // Временные рамки для поиска
            var timeframes = new Dictionary<string, string>
            {
                ["вчера"] = "yesterday", ["на прошлой неделе"] = "last_week",
                ["в прошлом месяце"] = "last_month", ["недавно"] = "recent",
                ["давно"] = "long_ago", ["ранее"] = "earlier"
            };

            foreach (var timeframe in timeframes)
            {
                if (text.Contains(timeframe.Key))
                {
                    result.Arguments["timeframe"] = timeframe.Value;
                    break;
                }
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// Извлечение аргументов для обратной связи
        /// </summary>
        private async Task ExtractFeedbackArguments(ParsedIntent result, string text, string[] words)
        {
            // Определение аспекта обратной связи
            var aspects = new Dictionary<string, string>
            {
                ["ответ"] = "response", ["поведение"] = "behavior",
                ["решение"] = "decision", ["действие"] = "action",
                ["работа"] = "performance", ["логика"] = "logic"
            };

            foreach (var aspect in aspects)
            {
                if (text.Contains(aspect.Key))
                {
                    result.Arguments["feedback_aspect"] = aspect.Value;
                    break;
                }
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// Извлечение аргументов для активации сценариев
        /// </summary>
        private async Task ExtractScenarioArguments(ParsedIntent result, string text, string[] words)
        {
            // Определение типа сценария
            var scenarios = new Dictionary<string, string>
            {
                ["обучение"] = "learning", ["помощь"] = "assistance",
                ["развлечение"] = "entertainment", ["анализ"] = "analysis",
                ["творчество"] = "creativity", ["планирование"] = "planning"
            };

            foreach (var scenario in scenarios)
            {
                if (text.Contains(scenario.Key))
                {
                    result.Arguments["scenario_type"] = scenario.Value;
                    break;
                }
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// Извлечение аргументов для модификации
        /// </summary>
        private async Task ExtractModificationArguments(ParsedIntent result, string text, string[] words)
        {
            // Определение области модификации
            var modificationAreas = new Dictionary<string, string>
            {
                ["поведение"] = "behavior", ["личность"] = "personality",
                ["стиль"] = "style", ["подход"] = "approach",
                ["параметры"] = "parameters", ["настройки"] = "settings"
            };

            foreach (var area in modificationAreas)
            {
                if (text.Contains(area.Key))
                {
                    result.Arguments["modification_area"] = area.Value;
                    break;
                }
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// Извлечение аргументов для внедрения мыслей
        /// </summary>
        private async Task ExtractThoughtArguments(ParsedIntent result, string text, string[] words)
        {
            // Определение типа мысли
            var thoughtTypes = new Dictionary<string, string>
            {
                ["идея"] = "idea", ["концепция"] = "concept",
                ["теория"] = "theory", ["гипотеза"] = "hypothesis",
                ["предположение"] = "assumption", ["мысль"] = "thought"
            };

            foreach (var thoughtType in thoughtTypes)
            {
                if (text.Contains(thoughtType.Key))
                {
                    result.Arguments["thought_type"] = thoughtType.Value;
                    break;
                }
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// Обновление весов слов на основе обучающих данных
        /// </summary>
        private void UpdateWordWeights(string text, IntentType correctIntent)
        {
            var words = PreprocessText(text).Split(' ', StringSplitOptions.RemoveEmptyEntries)
                                           .Where(w => !_stopWords.Contains(w));

            foreach (var word in words)
            {
                string key = $"{word}_{correctIntent}";
                if (_wordWeights.ContainsKey(key))
                {
                    _wordWeights[key] += 0.1; // Увеличиваем вес
                }
                else
                {
                    _wordWeights[key] = 1.1; // Начальный вес
                }
            }
        }

        /// <summary>
        /// Обновление априорных вероятностей намерений
        /// </summary>
        private void UpdateIntentPriors()
        {
            if (_trainingData.Count == 0) return;

            var intentCounts = _trainingData.GroupBy(t => t.CorrectIntent)
                                          .ToDictionary(g => g.Key, g => g.Count());

            int totalSamples = _trainingData.Count;

            foreach (var intentCount in intentCounts)
            {
                _intentPriors[intentCount.Key] = (double)intentCount.Value / totalSamples;
            }
        }

        /// <summary>
        /// Переобучение модели (заглушка для будущей ML реализации)
        /// </summary>
        private async Task RetrainModel()
        {
            // TODO: Реализовать полноценное переобучение ML модели
            // Сейчас только обновляем веса и приоры
            UpdateIntentPriors();

            // Симуляция времени обучения
            await Task.Delay(100);

            Console.WriteLine($"Model retrained with {_trainingData.Count} samples");
        }

        #endregion
    }
}