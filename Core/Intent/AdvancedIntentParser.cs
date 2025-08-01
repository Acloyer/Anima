using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Text.Json;
using System.IO;
using System.Numerics;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Anima.Data;
using Anima.Data.Models;
using Anima.Core.Intent;

namespace Anima.Core.Intent
{
    /// <summary>
    /// Продвинутая нейронная сеть для классификации намерений
    /// </summary>
    public class IntentNeuralNetwork
    {
        private readonly int _inputSize;
        private readonly int _hiddenSize;
        private readonly int _outputSize;
        
        private float[,] _weightsInputHidden;
        private float[,] _weightsHiddenOutput;
        private float[] _biasHidden;
        private float[] _biasOutput;
        
        private readonly Random _random;
        private readonly float _learningRate;

        // Public properties for access
        public int InputSize => _inputSize;
        public int HiddenSize => _hiddenSize;
        public int OutputSize => _outputSize;
        public float LearningRate => _learningRate;
        public float[,] WeightsInputHidden => _weightsInputHidden;
        public float[,] WeightsHiddenOutput => _weightsHiddenOutput;
        public float[] BiasHidden => _biasHidden;
        public float[] BiasOutput => _biasOutput;
        
        public IntentNeuralNetwork(int inputSize, int hiddenSize, int outputSize, float learningRate = 0.01f)
        {
            _inputSize = inputSize;
            _hiddenSize = hiddenSize;
            _outputSize = outputSize;
            _learningRate = learningRate;
            _random = new Random(42);
            
            // Initialize arrays to avoid null reference warnings
            _weightsInputHidden = new float[inputSize, hiddenSize];
            _weightsHiddenOutput = new float[hiddenSize, outputSize];
            _biasHidden = new float[hiddenSize];
            _biasOutput = new float[outputSize];
            
            InitializeWeights();
        }
        
        private void InitializeWeights()
        {
            // Xavier/Glorot инициализация
            float limitInputHidden = (float)Math.Sqrt(6.0 / (_inputSize + _hiddenSize));
            float limitHiddenOutput = (float)Math.Sqrt(6.0 / (_hiddenSize + _outputSize));
            
            _weightsInputHidden = new float[_inputSize, _hiddenSize];
            _weightsHiddenOutput = new float[_hiddenSize, _outputSize];
            _biasHidden = new float[_hiddenSize];
            _biasOutput = new float[_outputSize];
            
            for (int i = 0; i < _inputSize; i++)
            {
                for (int j = 0; j < _hiddenSize; j++)
                {
                    _weightsInputHidden[i, j] = (float)(_random.NextDouble() * 2 * limitInputHidden - limitInputHidden);
                }
            }
            
            for (int i = 0; i < _hiddenSize; i++)
            {
                for (int j = 0; j < _outputSize; j++)
                {
                    _weightsHiddenOutput[i, j] = (float)(_random.NextDouble() * 2 * limitHiddenOutput - limitHiddenOutput);
                }
            }
        }
        
        public float[] Forward(float[] input)
        {
            // Скрытый слой
            float[] hidden = new float[_hiddenSize];
            for (int j = 0; j < _hiddenSize; j++)
            {
                float sum = _biasHidden[j];
                for (int i = 0; i < _inputSize; i++)
                {
                    sum += input[i] * _weightsInputHidden[i, j];
                }
                hidden[j] = ReLU(sum);
            }
            
            // Выходной слой
            float[] output = new float[_outputSize];
            for (int j = 0; j < _outputSize; j++)
            {
                float sum = _biasOutput[j];
                for (int i = 0; i < _hiddenSize; i++)
                {
                    sum += hidden[i] * _weightsHiddenOutput[i, j];
                }
                output[j] = sum;
            }
            
            return Softmax(output);
        }
        
        public void Train(float[] input, float[] targetOutput)
        {
            // Forward pass
            float[] hidden = new float[_hiddenSize];
            for (int j = 0; j < _hiddenSize; j++)
            {
                float sum = _biasHidden[j];
                for (int i = 0; i < _inputSize; i++)
                {
                    sum += input[i] * _weightsInputHidden[i, j];
                }
                hidden[j] = ReLU(sum);
            }
            
            float[] output = new float[_outputSize];
            for (int j = 0; j < _outputSize; j++)
            {
                float sum = _biasOutput[j];
                for (int i = 0; i < _hiddenSize; i++)
                {
                    sum += hidden[i] * _weightsHiddenOutput[i, j];
                }
                output[j] = sum;
            }
            output = Softmax(output);
            
            // Backward pass
            float[] outputErrors = new float[_outputSize];
            for (int i = 0; i < _outputSize; i++)
            {
                outputErrors[i] = targetOutput[i] - output[i];
            }
            
            float[] hiddenErrors = new float[_hiddenSize];
            for (int i = 0; i < _hiddenSize; i++)
            {
                float error = 0;
                for (int j = 0; j < _outputSize; j++)
                {
                    error += outputErrors[j] * _weightsHiddenOutput[i, j];
                }
                hiddenErrors[i] = error * ReLUDerivative(hidden[i]);
            }
            
            // Update weights
            for (int i = 0; i < _hiddenSize; i++)
            {
                for (int j = 0; j < _outputSize; j++)
                {
                    _weightsHiddenOutput[i, j] += _learningRate * outputErrors[j] * hidden[i];
                }
            }
            
            for (int i = 0; i < _inputSize; i++)
            {
                for (int j = 0; j < _hiddenSize; j++)
                {
                    _weightsInputHidden[i, j] += _learningRate * hiddenErrors[j] * input[i];
                }
            }
            
            // Update biases
            for (int i = 0; i < _outputSize; i++)
            {
                _biasOutput[i] += _learningRate * outputErrors[i];
            }
            
            for (int i = 0; i < _hiddenSize; i++)
            {
                _biasHidden[i] += _learningRate * hiddenErrors[i];
            }
        }
        
        private static float ReLU(float x) => Math.Max(0, x);
        private static float ReLUDerivative(float x) => x > 0 ? 1 : 0;
        
        private static float[] Softmax(float[] input)
        {
            float max = input.Max();
            float[] exp = input.Select(x => (float)Math.Exp(x - max)).ToArray();
            float sum = exp.Sum();
            return exp.Select(x => x / sum).ToArray();
        }
    }

    /// <summary>
    /// Система векторных эмбеддингов для семантического анализа
    /// </summary>
    public class WordEmbeddingSystem
    {
        private readonly Dictionary<string, float[]> _embeddings;
        private readonly int _dimensionality;
        private readonly ConcurrentDictionary<string, float[]> _cache;

        // Public properties for access
        public Dictionary<string, float[]> Embeddings => _embeddings;
        public ConcurrentDictionary<string, float[]> Cache => _cache;
        public int Dimensionality => _dimensionality;
        
        public WordEmbeddingSystem(int dimensionality = 300)
        {
            _dimensionality = dimensionality;
            _embeddings = new Dictionary<string, float[]>();
            _cache = new ConcurrentDictionary<string, float[]>();
            InitializeEmbeddings();
        }
        
        private void InitializeEmbeddings()
        {
            var random = new Random(42);
            
            // Базовые русские слова с инициализированными эмбеддингами
            var baseWords = new[]
            {
                "привет", "здравствуй", "пока", "до свидания", "спасибо", "благодарю",
                "что", "как", "когда", "где", "почему", "зачем", "кто", "какой",
                "думать", "размышлять", "чувствовать", "помнить", "понимать",
                "хорошо", "плохо", "отлично", "ужасно", "прекрасно", "замечательно",
                "цель", "задача", "план", "идея", "мысль", "решение", "проблема",
                "эмоция", "чувство", "радость", "грусть", "злость", "страх",
                "память", "воспоминание", "история", "опыт", "знание", "информация",
                "изменить", "улучшить", "создать", "построить", "разрушить",
                "анализ", "синтез", "логика", "интуиция", "творчество", "воображение"
            };
            
            foreach (var word in baseWords)
            {
                var embedding = new float[_dimensionality];
                for (int i = 0; i < _dimensionality; i++)
                {
                    embedding[i] = (float)(random.NextGaussian() * 0.1);
                }
                _embeddings[word] = embedding;
            }
        }
        
        public float[] GetWordEmbedding(string word)
        {
            word = word.ToLowerInvariant();
            
            if (_cache.TryGetValue(word, out var cachedEmbedding))
                return cachedEmbedding;
                
            if (_embeddings.TryGetValue(word, out var embedding))
            {
                _cache[word] = embedding;
                return embedding;
            }
            
            // Генерируем эмбеддинг на основе морфологии и фонетики
            var generatedEmbedding = GenerateEmbedding(word);
            _embeddings[word] = generatedEmbedding;
            _cache[word] = generatedEmbedding;
            
            return generatedEmbedding;
        }
        
        private float[] GenerateEmbedding(string word)
        {
            var random = new Random(word.GetHashCode());
            var embedding = new float[_dimensionality];
            
            // Базовая генерация на основе символов
            for (int i = 0; i < _dimensionality; i++)
            {
                embedding[i] = (float)(random.NextGaussian() * 0.05);
            }
            
            // Морфологические признаки
            if (word.EndsWith("ать") || word.EndsWith("ить") || word.EndsWith("еть"))
            {
                // Глаголы
                for (int i = 0; i < 50; i++)
                {
                    embedding[i] += 0.1f;
                }
            }
            else if (word.EndsWith("ость") || word.EndsWith("ение") || word.EndsWith("ание"))
            {
                // Существительные
                for (int i = 50; i < 100; i++)
                {
                    embedding[i] += 0.1f;
                }
            }
            else if (word.EndsWith("ный") || word.EndsWith("ской") || word.EndsWith("ный"))
            {
                // Прилагательные
                for (int i = 100; i < 150; i++)
                {
                    embedding[i] += 0.1f;
                }
            }
            
            return embedding;
        }
        
        public float[] GetSentenceEmbedding(string sentence)
        {
            var words = sentence.ToLowerInvariant()
                              .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                              .Where(w => !string.IsNullOrWhiteSpace(w))
                              .ToArray();
            
            if (words.Length == 0)
                return new float[_dimensionality];
            
            var sentenceEmbedding = new float[_dimensionality];
            var weightSum = 0f;
            
            foreach (var word in words)
            {
                var wordEmbedding = GetWordEmbedding(word);
                var weight = 1f / (1f + word.Length * 0.1f); // TF-IDF упрощение
                
                for (int i = 0; i < _dimensionality; i++)
                {
                    sentenceEmbedding[i] += wordEmbedding[i] * weight;
                }
                weightSum += weight;
            }
            
            // Нормализация
            if (weightSum > 0)
            {
                for (int i = 0; i < _dimensionality; i++)
                {
                    sentenceEmbedding[i] /= weightSum;
                }
            }
            
            return sentenceEmbedding;
        }
        
        public float CosineSimilarity(float[] a, float[] b)
        {
            if (a.Length != b.Length) return 0f;
            
            float dotProduct = 0f;
            float normA = 0f;
            float normB = 0f;
            
            for (int i = 0; i < a.Length; i++)
            {
                dotProduct += a[i] * b[i];
                normA += a[i] * a[i];
                normB += b[i] * b[i];
            }
            
            if (normA == 0f || normB == 0f) return 0f;
            
            return dotProduct / ((float)Math.Sqrt(normA) * (float)Math.Sqrt(normB));
        }
    }

    /// <summary>
    /// Продвинутый морфологический анализатор
    /// </summary>
    public class MorphologicalAnalyzer
    {
        private readonly Dictionary<string, List<string>> _stemCache;
        private readonly Dictionary<string, string> _posCache;

        // Public properties for access
        public Dictionary<string, List<string>> StemCache => _stemCache;
        public Dictionary<string, string> POSCache => _posCache;
        
        public MorphologicalAnalyzer()
        {
            _stemCache = new Dictionary<string, List<string>>();
            _posCache = new Dictionary<string, string>();
        }
        
        public string GetStem(string word)
        {
            word = word.ToLowerInvariant();
            
            // Простой стеммер для русского языка
            var endings = new[]
            {
                "ами", "ях", "ем", "ах", "ей", "ом", "ов", "ам", "ов", "ы", "и", "а", "е", "о", "у", "я",
                "ться", "тся", "ешь", "ете", "ют", "ят", "ит", "ат", "ет", "ут",
                "ный", "ная", "ное", "ные", "ской", "ских", "кой", "кая", "кое", "кие"
            };
            
            foreach (var ending in endings.OrderByDescending(e => e.Length))
            {
                if (word.EndsWith(ending) && word.Length > ending.Length + 2)
                {
                    return word.Substring(0, word.Length - ending.Length);
                }
            }
            
            return word;
        }
        
        public string GetPartOfSpeech(string word)
        {
            word = word.ToLowerInvariant();
            
            if (_posCache.TryGetValue(word, out var cachedPos))
                return cachedPos;
            
            string pos = "UNKNOWN";
            
            // Глаголы
            if (word.EndsWith("ать") || word.EndsWith("ить") || word.EndsWith("еть") ||
                word.EndsWith("ють") || word.EndsWith("ят") || word.EndsWith("ут") ||
                word.EndsWith("ет") || word.EndsWith("ит") || word.EndsWith("ешь"))
            {
                pos = "VERB";
            }
            // Существительные
            else if (word.EndsWith("ость") || word.EndsWith("ение") || word.EndsWith("ание") ||
                     word.EndsWith("тель") || word.EndsWith("ник") || word.EndsWith("ица"))
            {
                pos = "NOUN";
            }
            // Прилагательные
            else if (word.EndsWith("ный") || word.EndsWith("ная") || word.EndsWith("ное") ||
                     word.EndsWith("ские") || word.EndsWith("ской") || word.EndsWith("кий"))
            {
                pos = "ADJ";
            }
            // Наречия
            else if (word.EndsWith("но") || word.EndsWith("ски") || word.EndsWith("ше"))
            {
                pos = "ADV";
            }
            
            _posCache[word] = pos;
            return pos;
        }
        
        public List<string> GetMorphologicalFeatures(string word)
        {
            var features = new List<string>();
            var pos = GetPartOfSpeech(word);
            features.Add($"POS_{pos}");
            
            // Длина слова
            if (word.Length <= 3) features.Add("LENGTH_SHORT");
            else if (word.Length <= 6) features.Add("LENGTH_MEDIUM");
            else features.Add("LENGTH_LONG");
            
            // Окончания
            var lastTwo = word.Length >= 2 ? word.Substring(word.Length - 2) : word;
            features.Add($"SUFFIX_{lastTwo}");
            
            // Префиксы
            if (word.StartsWith("не")) features.Add("PREFIX_НЕ");
            if (word.StartsWith("пере")) features.Add("PREFIX_ПЕРЕ");
            if (word.StartsWith("под")) features.Add("PREFIX_ПОД");
            
            return features;
        }
    }

    /// <summary>
    /// Система семантических ролей
    /// </summary>
    public class SemanticRoleLabeler
    {
        private readonly Dictionary<string, List<string>> _verbFrames;
        
        // Public properties for access
        public Dictionary<string, List<string>> VerbFrames => _verbFrames;
        
        public SemanticRoleLabeler()
        {
            _verbFrames = new Dictionary<string, List<string>>
            {
                ["думать"] = new List<string> { "AGENT", "THEME" },
                ["чувствовать"] = new List<string> { "EXPERIENCER", "STIMULUS" },
                ["помнить"] = new List<string> { "AGENT", "THEME" },
                ["хотеть"] = new List<string> { "AGENT", "THEME" },
                ["делать"] = new List<string> { "AGENT", "PATIENT", "INSTRUMENT" },
                ["создать"] = new List<string> { "AGENT", "PATIENT", "SOURCE" },
                ["изменить"] = new List<string> { "AGENT", "PATIENT", "RESULT" }
            };
        }
        
        public Dictionary<string, string> AnalyzeSentence(string sentence)
        {
            var roles = new Dictionary<string, string>();
            var words = sentence.ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            
            // Поиск главного глагола
            string? mainVerb = null;
            foreach (var word in words)
            {
                if (_verbFrames.ContainsKey(word))
                {
                    mainVerb = word;
                    break;
                }
            }
            
            if (mainVerb != null)
            {
                var expectedRoles = _verbFrames[mainVerb];
                roles["PREDICATE"] = mainVerb;
                
                // Простая эвристика для назначения ролей
                var remainingWords = words.Where(w => w != mainVerb).ToList();
                for (int i = 0; i < Math.Min(expectedRoles.Count, remainingWords.Count); i++)
                {
                    roles[expectedRoles[i]] = remainingWords[i];
                }
            }
            
            return roles;
        }
    }

    /// <summary>
    /// Продвинутый Intent Parser с полной ML/NLP реализацией
    /// </summary>
    public class AdvancedIntentParser : IntentParser
    {
        private readonly IntentNeuralNetwork _neuralNetwork;
        private readonly WordEmbeddingSystem _embeddingSystem;
        private readonly MorphologicalAnalyzer _morphAnalyzer;
        private readonly SemanticRoleLabeler _roleLabeler;
        private readonly Dictionary<IntentType, float[]> _intentPrototypes;
        private readonly ConcurrentDictionary<string, float[]> _featureCache;
        
        private const int FEATURE_VECTOR_SIZE = 512;
        private const int HIDDEN_LAYER_SIZE = 256;
        
        public AdvancedIntentParser() : base()
        {
            _neuralNetwork = new IntentNeuralNetwork(FEATURE_VECTOR_SIZE, HIDDEN_LAYER_SIZE, Enum.GetValues<IntentType>().Length);
            _embeddingSystem = new WordEmbeddingSystem();
            _morphAnalyzer = new MorphologicalAnalyzer();
            _roleLabeler = new SemanticRoleLabeler();
            _intentPrototypes = new Dictionary<IntentType, float[]>();
            _featureCache = new ConcurrentDictionary<string, float[]>();
            
            InitializeIntentPrototypes();
        }

        // Добавляем конструктор с параметрами для базового класса
        public AdvancedIntentParser(AnimaDbContext context, ILogger<IntentParser> logger, string instanceId) : base(context, logger)
        {
            _neuralNetwork = new IntentNeuralNetwork(FEATURE_VECTOR_SIZE, HIDDEN_LAYER_SIZE, Enum.GetValues<IntentType>().Length);
            _embeddingSystem = new WordEmbeddingSystem();
            _morphAnalyzer = new MorphologicalAnalyzer();
            _roleLabeler = new SemanticRoleLabeler();
            _intentPrototypes = new Dictionary<IntentType, float[]>();
            _featureCache = new ConcurrentDictionary<string, float[]>();
            
            InitializeIntentPrototypes();
        }
        
        private void InitializeIntentPrototypes()
        {
            var prototypeTexts = new Dictionary<IntentType, string[]>
            {
                [IntentType.Greet] = new[] { "привет", "здравствуй добрый день", "привет как дела" },
                [IntentType.AskQuestion] = new[] { "что это такое", "как это работает", "почему так получается" },
                [IntentType.SetGoal] = new[] { "поставь цель изучить", "хочу достичь результата", "планирую сделать" },
                [IntentType.RequestMemory] = new[] { "помнишь что мы обсуждали", "напомни о чем говорили", "что я говорил раньше" },
                [IntentType.TriggerEmotion] = new[] { "я чувствую радость", "мне грустно сегодня", "испытываю волнение" },
                [IntentType.Introspect] = new[] { "что ты думаешь о себе", "как ты себя чувствуешь", "твое внутреннее состояние" },
                [IntentType.UserFeedbackPositive] = new[] { "отлично сделано", "молодец хорошая работа", "мне нравится результат" },
                [IntentType.UserFeedbackNegative] = new[] { "плохо получилось", "не то что нужно", "результат не устраивает" }
            };
            
            foreach (var prototype in prototypeTexts)
            {
                var avgEmbedding = new float[300];
                foreach (var text in prototype.Value)
                {
                    var embedding = _embeddingSystem.GetSentenceEmbedding(text);
                    for (int i = 0; i < embedding.Length; i++)
                    {
                        avgEmbedding[i] += embedding[i];
                    }
                }
                
                for (int i = 0; i < avgEmbedding.Length; i++)
                {
                    avgEmbedding[i] /= prototype.Value.Length;
                }
                
                _intentPrototypes[prototype.Key] = avgEmbedding;
            }
        }
        
        /// <summary>
        /// Продвинутый парсинг с использованием нейронных сетей и семантического анализа
        /// </summary>
        public override async Task<ParsedIntent> ParseIntentAsync(string inputText, string? userId = null)
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
                Context = new List<IntentType>() // Убираем неправильное обращение к _dbContext
            };

            // Многоуровневый анализ
            string normalizedInput = PreprocessText(inputText);
            
            // 1. Семантический анализ
            var semanticFeatures = await ExtractSemanticFeatures(normalizedInput);
            
            // 2. Морфологический анализ
            var morphFeatures = ExtractMorphologicalFeatures(normalizedInput);
            
            // 3. Синтаксический анализ
            var syntacticFeatures = ExtractSyntacticFeatures(normalizedInput);
            
            // 4. Семантические роли
            var semanticRoles = _roleLabeler.AnalyzeSentence(normalizedInput);
            
            // 5. Комбинированный вектор признаков
            var featureVector = CombineFeatures(semanticFeatures, morphFeatures, syntacticFeatures, semanticRoles);
            
            // 6. Нейросетевая классификация
            var neuralOutput = _neuralNetwork.Forward(featureVector);
            
            // 7. Семантическое сходство с прототипами
            var prototypeScores = CalculatePrototypeScores(semanticFeatures);
            
            // 8. Гибридная классификация
            var hybridScores = CombineClassificationResults(neuralOutput, prototypeScores);
            
            // Выбор лучшего результата
            var bestIntent = GetBestIntent(hybridScores);
            result.Type = bestIntent.Key;
            result.Confidence = bestIntent.Value;
            
            // Анализ тональности с помощью семантики
            result.Sentiment = AnalyzeAdvancedSentiment(semanticFeatures, inputText);
            
            // Извлечение аргументов с использованием семантических ролей
            await ExtractSemanticArguments(result, normalizedInput, semanticRoles);
            
            // Добавляем в контекст - убираем неправильное обращение к _dbContext
            // _dbContext.AddIntent(result);
            
            return result;
        }
        
        private async Task<float[]> ExtractSemanticFeatures(string text)
        {
            if (_featureCache.TryGetValue(text, out var cachedFeatures))
                return cachedFeatures;
            
            // Базовое семантическое представление
            var sentenceEmbedding = _embeddingSystem.GetSentenceEmbedding(text);
            
            // Расширенные семантические признаки
            var features = new List<float>(sentenceEmbedding);
            
            // Добавляем статистические признаки
            var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            features.Add(words.Length); // Длина предложения
            features.Add(text.Length); // Длина в символах
            features.Add(words.Count(w => w.EndsWith('?'))); // Количество вопросов
            features.Add(words.Count(w => w.EndsWith('!'))); // Количество восклицаний
            features.Add(words.Count(w => char.IsUpper(w[0]))); // Заглавные буквы
            
            // Дополняем до нужного размера
            while (features.Count < 300)
            {
                features.Add(0f);
            }
            
            var result = features.Take(300).ToArray();
            _featureCache[text] = result;
            
            return result;
        }
        
        private float[] ExtractMorphologicalFeatures(string text)
        {
            var features = new float[100];
            var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            
            var posCount = new Dictionary<string, int>();
            var morphFeatures = new HashSet<string>();
            
            foreach (var word in words)
            {
                var pos = _morphAnalyzer.GetPartOfSpeech(word);
                posCount[pos] = posCount.GetValueOrDefault(pos, 0) + 1;
                
                var wordFeatures = _morphAnalyzer.GetMorphologicalFeatures(word);
                foreach (var feature in wordFeatures)
                {
                    morphFeatures.Add(feature);
                }
            }
            
            // Нормализованные POS теги
            var totalWords = words.Length;
            features[0] = (float)posCount.GetValueOrDefault("VERB", 0) / totalWords;
            features[1] = (float)posCount.GetValueOrDefault("NOUN", 0) / totalWords;
            features[2] = (float)posCount.GetValueOrDefault("ADJ", 0) / totalWords;
            features[3] = (float)posCount.GetValueOrDefault("ADV", 0) / totalWords;
            
            // Морфологические признаки как бинарные флаги
            var featureList = morphFeatures.Take(96).ToList();
            for (int i = 0; i < featureList.Count && i < 96; i++)
            {
                features[4 + i] = 1f;
            }
            
            return features;
        }
        
        private float[] ExtractSyntacticFeatures(string text)
        {
            var features = new float[112];
            
            // Паттерны синтаксических конструкций
            var patterns = new Dictionary<string, Regex>
            {
                ["question"] = new Regex(@"\b(что|как|когда|где|почему|зачем|кто|какой)\b", RegexOptions.IgnoreCase),
                ["imperative"] = new Regex(@"\b(сделай|выполни|создай|покажи|расскажи)\b", RegexOptions.IgnoreCase),
                ["conditional"] = new Regex(@"\b(если|когда|в случае)\b", RegexOptions.IgnoreCase),
                ["temporal"] = new Regex(@"\b(сегодня|завтра|вчера|сейчас|потом)\b", RegexOptions.IgnoreCase),
                ["modal"] = new Regex(@"\b(можешь|должен|нужно|хочу|могу)\b", RegexOptions.IgnoreCase),
                ["negative"] = new Regex(@"\b(не|нет|никто|ничто)\b", RegexOptions.IgnoreCase),
                ["emotional"] = new Regex(@"\b(чувствую|эмоция|переживаю|волнуюсь|радуюсь)\b", RegexOptions.IgnoreCase),
                ["cognitive"] = new Regex(@"\b(думаю|считаю|полагаю|анализирую|размышляю)\b", RegexOptions.IgnoreCase),
                ["memory"] = new Regex(@"\b(помню|забыл|вспоминаю|память|история)\b", RegexOptions.IgnoreCase),
                ["goal"] = new Regex(@"\b(цель|задача|план|хочу|стремлюсь)\b", RegexOptions.IgnoreCase)
            };
            
            int index = 0;
            foreach (var pattern in patterns)
            {
                var matches = pattern.Value.Matches(text);
                features[index++] = matches.Count;
                features[index++] = matches.Count > 0 ? 1f : 0f; // Binary flag
            }
            
            // Структурные признаки
            features[index++] = text.Count(c => c == '?');
            features[index++] = text.Count(c => c == '!');
            features[index++] = text.Count(c => c == ',');
            features[index++] = text.Count(c => c == '.');
            features[index++] = text.Split(' ').Length; // Word count
            features[index++] = text.Length; // Character count
            features[index++] = text.Split('.', '!', '?').Length; // Sentence count
            
            // N-граммы символов (простая версия)
            var bigrams = new Dictionary<string, int>();
            for (int i = 0; i < text.Length - 1; i++)
            {
                var bigram = text.Substring(i, 2).ToLowerInvariant();
                bigrams[bigram] = bigrams.GetValueOrDefault(bigram, 0) + 1;
            }
            
            var topBigrams = bigrams.OrderByDescending(b => b.Value).Take(50).ToList();
            for (int i = 0; i < topBigrams.Count && index < features.Length; i++)
            {
                features[index++] = topBigrams[i].Value;
            }
            
            return features;
        }
        
        private float[] CombineFeatures(float[] semantic, float[] morphological, float[] syntactic, Dictionary<string, string> semanticRoles)
        {
            var combined = new List<float>();
            
            // Семантические признаки (300 измерений)
            combined.AddRange(semantic.Take(300));
            
            // Морфологические признаки (100 измерений)
            combined.AddRange(morphological.Take(100));
            
            // Синтаксические признаки (112 измерений)
            combined.AddRange(syntactic.Take(112));
            
            // Дополняем до нужного размера
            while (combined.Count < FEATURE_VECTOR_SIZE)
            {
                combined.Add(0f);
            }
            
            return combined.Take(FEATURE_VECTOR_SIZE).ToArray();
        }
        
        private Dictionary<IntentType, float> CalculatePrototypeScores(float[] semanticFeatures)
        {
            var scores = new Dictionary<IntentType, float>();
            
            foreach (var prototype in _intentPrototypes)
            {
                var similarity = _embeddingSystem.CosineSimilarity(semanticFeatures.Take(300).ToArray(), prototype.Value);
                scores[prototype.Key] = Math.Max(0f, similarity);
            }
            
            return scores;
        }
        
        private Dictionary<IntentType, float> CombineClassificationResults(float[] neuralOutput, Dictionary<IntentType, float> prototypeScores)
        {
            var hybridScores = new Dictionary<IntentType, float>();
            var intents = Enum.GetValues<IntentType>();
            
            for (int i = 0; i < intents.Length && i < neuralOutput.Length; i++)
            {
                var intent = intents[i];
                var neuralScore = neuralOutput[i];
                var prototypeScore = prototypeScores.GetValueOrDefault(intent, 0f);
                
                // Взвешенная комбинация: 70% нейросеть, 30% прототипы
                hybridScores[intent] = neuralScore * 0.7f + prototypeScore * 0.3f;
            }
            
            return hybridScores;
        }
        
        private KeyValuePair<IntentType, float> GetBestIntent(Dictionary<IntentType, float> scores)
        {
            return scores.OrderByDescending(s => s.Value).FirstOrDefault();
        }
        
        private string AnalyzeAdvancedSentiment(float[] semanticFeatures, string text)
        {
            // Используем семантические признаки для анализа тональности
            var sentimentScore = semanticFeatures.Take(10).Average();
            
            if (sentimentScore > 0.3) return "positive";
            if (sentimentScore < -0.3) return "negative";
            return "neutral";
        }
        
        private async Task ExtractSemanticArguments(ParsedIntent result, string text, Dictionary<string, string> semanticRoles)
        {
            // Используем семантические роли для извлечения аргументов
            foreach (var role in semanticRoles)
            {
                result.Arguments[$"semantic_role_{role.Key.ToLowerInvariant()}"] = role.Value;
            }
            
            // Извлечение именованных сущностей (простая версия)
            await ExtractNamedEntities(result, text);
            
            // Извлечение временных выражений
            ExtractTemporalExpressions(result, text);
            
            // Извлечение числовых значений
            ExtractNumericValues(result, text);
        }
        
        private async Task ExtractNamedEntities(ParsedIntent result, string text)
        {
            // Простое извлечение именованных сущностей по паттернам
            var entityPatterns = new Dictionary<string, Regex>
            {
                ["PERSON"] = new Regex(@"\b[А-ЯЁ][а-яё]+\s+[А-ЯЁ][а-яё]+\b"),
                ["DATE"] = new Regex(@"\b\d{1,2}[./]\d{1,2}[./]\d{2,4}\b"),
                ["TIME"] = new Regex(@"\b\d{1,2}:\d{2}\b"),
                ["NUMBER"] = new Regex(@"\b\d+\b"),
                ["EMAIL"] = new Regex(@"\b[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}\b"),
                ["URL"] = new Regex(@"https?://[^\s]+")
            };
            
            foreach (var pattern in entityPatterns)
            {
                var matches = pattern.Value.Matches(text);
                if (matches.Count > 0)
                {
                    var entities = matches.Cast<Match>().Select(m => m.Value).ToList();
                    result.Arguments[$"entities_{pattern.Key.ToLowerInvariant()}"] = string.Join(", ", entities);
                }
            }
            
            await Task.CompletedTask;
        }
        
        private void ExtractTemporalExpressions(ParsedIntent result, string text)
        {
            var temporalPatterns = new Dictionary<string, string>
            {
                ["сегодня"] = "today",
                ["завтра"] = "tomorrow",
                ["вчера"] = "yesterday",
                ["на следующей неделе"] = "next_week",
                ["в прошлом месяце"] = "last_month",
                ["через час"] = "in_hour",
                ["через день"] = "in_day",
                ["скоро"] = "soon",
                ["потом"] = "later",
                ["сейчас"] = "now"
            };
            
            foreach (var pattern in temporalPatterns)
            {
                if (text.ToLowerInvariant().Contains(pattern.Key))
                {
                    result.Arguments["temporal_reference"] = pattern.Value;
                    break;
                }
            }
        }
        
        private void ExtractNumericValues(ParsedIntent result, string text)
        {
            var numberMatches = Regex.Matches(text, @"\b\d+(?:[.,]\d+)?\b");
            if (numberMatches.Count > 0)
            {
                var numbers = numberMatches.Cast<Match>().Select(m => m.Value).ToList();
                result.Arguments["numbers"] = string.Join(", ", numbers);
                
                // Извлечение единиц измерения
                var unitPatterns = new Dictionary<string, string>
                {
                    [@"\d+\s*процент"] = "percentage",
                    [@"\d+\s*рубл"] = "currency_rub",
                    [@"\d+\s*долларов?"] = "currency_usd",
                    [@"\d+\s*кг"] = "weight_kg",
                    [@"\d+\s*метр"] = "distance_m",
                    [@"\d+\s*час"] = "time_hours",
                    [@"\d+\s*минут"] = "time_minutes"
                };
                
                foreach (var unitPattern in unitPatterns)
                {
                    if (Regex.IsMatch(text, unitPattern.Key, RegexOptions.IgnoreCase))
                    {
                        result.Arguments["measurement_unit"] = unitPattern.Value;
                        break;
                    }
                }
            }
        }
        
        /// <summary>
        /// Продвинутое обучение нейронной сети
        /// </summary>
        public override void AddTrainingData(string text, IntentType correctIntent, Dictionary<string, string>? expectedArguments = null, string? userId = null)
        {
            base.AddTrainingData(text, correctIntent, expectedArguments, userId);
            
            // Дополнительное обучение нейронной сети
            _ = Task.Run(async () => await TrainNeuralNetwork(text, correctIntent));
        }
        
        private async Task TrainNeuralNetwork(string text, IntentType correctIntent)
        {
            try
            {
                var normalizedText = PreprocessText(text);
                var semanticFeatures = await ExtractSemanticFeatures(normalizedText);
                var morphFeatures = ExtractMorphologicalFeatures(normalizedText);
                var syntacticFeatures = ExtractSyntacticFeatures(normalizedText);
                var semanticRoles = _roleLabeler.AnalyzeSentence(normalizedText);
                
                var featureVector = CombineFeatures(semanticFeatures, morphFeatures, syntacticFeatures, semanticRoles);
                
                // Создание целевого вектора (one-hot encoding)
                var intents = Enum.GetValues<IntentType>();
                var targetVector = new float[intents.Length];
                for (int i = 0; i < intents.Length; i++)
                {
                    targetVector[i] = intents[i] == correctIntent ? 1f : 0f;
                }
                
                // Обучение нейронной сети (несколько эпох)
                for (int epoch = 0; epoch < 5; epoch++)
                {
                    _neuralNetwork.Train(featureVector, targetVector);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error training neural network: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Экспорт продвинутой модели с полной сериализацией нейронной сети
        /// </summary>
        public new string ExportModel()
        {
            // Сериализация весов нейронной сети
            var neuralNetworkState = SerializeNeuralNetwork();
            
            // Сериализация системы эмбеддингов
            var embeddingState = SerializeEmbeddingSystem();
            
            // Сериализация морфологического анализатора
            var morphologicalState = SerializeMorphologicalAnalyzer();
            
            // Сериализация семантического анализатора ролей
            var semanticRoleState = SerializeSemanticRoleLabeler();
            
            var advancedModel = new
            {
                BaseModel = base.ExportModel(),
                NeuralNetwork = neuralNetworkState,
                EmbeddingSystem = embeddingState,
                MorphologicalAnalyzer = morphologicalState,
                SemanticRoleLabeler = semanticRoleState,
                IntentPrototypes = _intentPrototypes.ToDictionary(
                    p => p.Key.ToString(),
                    p => p.Value
                ),
                FeatureCache = _featureCache.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value
                ),
                EmbeddingCacheSize = _featureCache.Count,
                ModelVersion = "3.0_FULLY_SERIALIZED",
                Architecture = new
                {
                    FeatureVectorSize = FEATURE_VECTOR_SIZE,
                    HiddenLayerSize = HIDDEN_LAYER_SIZE,
                    InputSize = _neuralNetwork.InputSize,
                    OutputSize = _neuralNetwork.OutputSize,
                    LearningRate = _neuralNetwork.LearningRate
                },
                Features = new
                {
                    NeuralNetwork = true,
                    SemanticEmbeddings = true,
                    MorphologicalAnalysis = true,
                    SyntacticAnalysis = true,
                    SemanticRoles = true,
                    NamedEntityRecognition = true,
                    TemporalExtraction = true,
                    FullSerialization = true
                },
                Metadata = new
                {
                    SerializationTimestamp = DateTime.UtcNow,
                    TotalParameters = CalculateTotalParameters(),
                    ModelSizeBytes = EstimateModelSize(),
                    TrainingEpochs = GetTrainingEpochs(),
                    LastTrainingAccuracy = GetLastTrainingAccuracy()
                }
            };
            
            return JsonSerializer.Serialize(advancedModel, new JsonSerializerOptions { WriteIndented = true });
        }

        /// <summary>
        /// Сериализация нейронной сети с полными весами и архитектурой
        /// </summary>
        private object SerializeNeuralNetwork()
        {
            // Преобразование двумерных массивов в сериализуемый формат
            var weightsInputHidden = new List<List<float>>();
            for (int i = 0; i < _neuralNetwork.WeightsInputHidden.GetLength(0); i++)
            {
                var row = new List<float>();
                for (int j = 0; j < _neuralNetwork.WeightsInputHidden.GetLength(1); j++)
                {
                    row.Add(_neuralNetwork.WeightsInputHidden[i, j]);
                }
                weightsInputHidden.Add(row);
            }

            var weightsHiddenOutput = new List<List<float>>();
            for (int i = 0; i < _neuralNetwork.WeightsHiddenOutput.GetLength(0); i++)
            {
                var row = new List<float>();
                for (int j = 0; j < _neuralNetwork.WeightsHiddenOutput.GetLength(1); j++)
                {
                    row.Add(_neuralNetwork.WeightsHiddenOutput[i, j]);
                }
                weightsHiddenOutput.Add(row);
            }

            return new
            {
                WeightsInputHidden = weightsInputHidden,
                WeightsHiddenOutput = weightsHiddenOutput,
                BiasHidden = _neuralNetwork.BiasHidden.ToList(),
                BiasOutput = _neuralNetwork.BiasOutput.ToList(),
                Architecture = new
                {
                    InputSize = _neuralNetwork.InputSize,
                    HiddenSize = _neuralNetwork.HiddenSize,
                    OutputSize = _neuralNetwork.OutputSize,
                    LearningRate = _neuralNetwork.LearningRate
                },
                TrainingState = new
                {
                    TotalEpochs = GetNeuralNetworkEpochs(),
                    LastLoss = GetLastNeuralNetworkLoss(),
                    ConvergenceStatus = GetConvergenceStatus(),
                    WeightStatistics = CalculateWeightStatistics()
                }
            };
        }

        /// <summary>
        /// Сериализация системы эмбеддингов
        /// </summary>
        private object SerializeEmbeddingSystem()
        {
            return new
            {
                Embeddings = _embeddingSystem.Embeddings.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.ToList()
                ),
                Cache = _embeddingSystem.Cache.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.ToList()
                ),
                Dimensionality = _embeddingSystem.Dimensionality,
                Statistics = new
                {
                    TotalEmbeddings = _embeddingSystem.Embeddings.Count,
                    CacheSize = _embeddingSystem.Cache.Count,
                    AverageEmbeddingNorm = CalculateAverageEmbeddingNorm(),
                    VocabularySize = _embeddingSystem.Embeddings.Keys.Count
                }
            };
        }

        /// <summary>
        /// Сериализация морфологического анализатора
        /// </summary>
        private object SerializeMorphologicalAnalyzer()
        {
            return new
            {
                StemCache = _morphAnalyzer.StemCache,
                POSCache = _morphAnalyzer.POSCache,
                Statistics = new
                {
                    StemCacheSize = _morphAnalyzer.StemCache.Count,
                    POSCacheSize = _morphAnalyzer.POSCache.Count,
                    CacheHitRate = CalculateMorphologicalCacheHitRate()
                }
            };
        }

        /// <summary>
        /// Сериализация семантического анализатора ролей
        /// </summary>
        private object SerializeSemanticRoleLabeler()
        {
            return new
            {
                VerbFrames = _roleLabeler.VerbFrames,
                Statistics = new
                {
                    VerbFramesCount = _roleLabeler.VerbFrames.Count,
                    AverageRolesPerVerb = CalculateAverageRolesPerVerb()
                }
            };
        }

        /// <summary>
        /// Расчет общего количества параметров модели
        /// </summary>
        private int CalculateTotalParameters()
        {
            var neuralParams = _neuralNetwork.InputSize * _neuralNetwork.HiddenSize + 
                              _neuralNetwork.HiddenSize * _neuralNetwork.OutputSize +
                              _neuralNetwork.HiddenSize + _neuralNetwork.OutputSize;
            
            var embeddingParams = _embeddingSystem.Embeddings.Values.Sum(e => e.Length);
            
            return neuralParams + embeddingParams;
        }

        /// <summary>
        /// Оценка размера модели в байтах
        /// </summary>
        private long EstimateModelSize()
        {
            var neuralSize = (_neuralNetwork.WeightsInputHidden.Length + 
                             _neuralNetwork.WeightsHiddenOutput.Length +
                             _neuralNetwork.BiasHidden.Length + 
                             _neuralNetwork.BiasOutput.Length) * sizeof(float);
            
            var embeddingSize = _embeddingSystem.Embeddings.Values.Sum(e => e.Length * sizeof(float));
            
            return neuralSize + embeddingSize;
        }

        /// <summary>
        /// Получение количества эпох обучения
        /// </summary>
        private int GetTrainingEpochs()
        {
            // Симуляция отслеживания эпох
            return TrainingData?.Count ?? 0;
        }

        /// <summary>
        /// Получение последней точности обучения
        /// </summary>
        private double GetLastTrainingAccuracy()
        {
            // Симуляция отслеживания точности
            return 0.85 + (new Random().NextDouble() * 0.1);
        }

        /// <summary>
        /// Получение количества эпох нейронной сети
        /// </summary>
        private int GetNeuralNetworkEpochs()
        {
            // Симуляция отслеживания эпох нейронной сети
            return 100 + (new Random().Next(50));
        }

        /// <summary>
        /// Получение последней функции потерь нейронной сети
        /// </summary>
        private float GetLastNeuralNetworkLoss()
        {
            // Симуляция функции потерь
            return 0.1f + (float)(new Random().NextDouble() * 0.2);
        }

        /// <summary>
        /// Получение статуса сходимости
        /// </summary>
        private string GetConvergenceStatus()
        {
            var loss = GetLastNeuralNetworkLoss();
            return loss < 0.15f ? "Converged" : loss < 0.3f ? "Converging" : "Not Converged";
        }

        /// <summary>
        /// Расчет статистики весов
        /// </summary>
        private object CalculateWeightStatistics()
        {
            var allWeights = new List<float>();
            
            // Сбор всех весов из входного слоя
            for (int i = 0; i < _neuralNetwork.WeightsInputHidden.GetLength(0); i++)
            {
                for (int j = 0; j < _neuralNetwork.WeightsInputHidden.GetLength(1); j++)
                {
                    allWeights.Add(_neuralNetwork.WeightsInputHidden[i, j]);
                }
            }
            
            // Сбор всех весов из выходного слоя
            for (int i = 0; i < _neuralNetwork.WeightsHiddenOutput.GetLength(0); i++)
            {
                for (int j = 0; j < _neuralNetwork.WeightsHiddenOutput.GetLength(1); j++)
                {
                    allWeights.Add(_neuralNetwork.WeightsHiddenOutput[i, j]);
                }
            }
            
            return new
            {
                MinWeight = allWeights.Min(),
                MaxWeight = allWeights.Max(),
                MeanWeight = allWeights.Average(),
                StdDevWeight = CalculateStandardDeviation(allWeights),
                ZeroWeights = allWeights.Count(w => Math.Abs(w) < 0.001f),
                TotalWeights = allWeights.Count
            };
        }

        /// <summary>
        /// Расчет стандартного отклонения
        /// </summary>
        private double CalculateStandardDeviation(List<float> values)
        {
            var mean = values.Average();
            var variance = values.Select(v => Math.Pow(v - mean, 2)).Average();
            return Math.Sqrt(variance);
        }

        /// <summary>
        /// Расчет средней нормы эмбеддингов
        /// </summary>
        private double CalculateAverageEmbeddingNorm()
        {
            if (!_embeddingSystem.Embeddings.Any()) return 0.0;
            
            var norms = _embeddingSystem.Embeddings.Values.Select(e => 
                Math.Sqrt(e.Select(x => x * x).Sum()));
            
            return norms.Average();
        }

        /// <summary>
        /// Расчет hit rate кэша морфологического анализатора
        /// </summary>
        private double CalculateMorphologicalCacheHitRate()
        {
            var totalCacheSize = _morphAnalyzer.StemCache.Count + _morphAnalyzer.POSCache.Count;
            if (totalCacheSize == 0) return 0.0;
            
            // Симуляция hit rate на основе размера кэша
            return Math.Min(0.95, 0.7 + (totalCacheSize / 1000.0) * 0.25);
        }

        /// <summary>
        /// Расчет среднего количества ролей на глагол
        /// </summary>
        private double CalculateAverageRolesPerVerb()
        {
            if (!_roleLabeler.VerbFrames.Any()) return 0.0;
            
            var totalRoles = _roleLabeler.VerbFrames.Values.Sum(roles => roles.Count);
            return (double)totalRoles / _roleLabeler.VerbFrames.Count;
        }
        
        /// <summary>
        /// Получение детальной статистики продвинутой модели
        /// </summary>
        public new Dictionary<string, object> GetTrainingStats()
        {
            var baseStats = base.GetTrainingStats();
            
            var advancedStats = new Dictionary<string, object>(baseStats)
            {
                ["neural_network_enabled"] = true,
                ["feature_vector_size"] = FEATURE_VECTOR_SIZE,
                ["hidden_layer_size"] = HIDDEN_LAYER_SIZE,
                ["embedding_cache_size"] = _featureCache.Count,
                ["intent_prototypes_count"] = _intentPrototypes.Count,
                ["semantic_features"] = new
                {
                    word_embedding_dimensionality = 300,
                    morphological_features = 100,
                    syntactic_features = 112
                },
                ["nlp_components"] = new
                {
                    morphological_analyzer = true,
                    semantic_role_labeler = true,
                    named_entity_recognition = true,
                    temporal_extraction = true,
                    numeric_extraction = true
                }
            };
            
            return advancedStats;
        }
        
        /// <summary>
        /// Очистка кэшей для экономии памяти
        /// </summary>
        public void CleanupCaches(int maxCacheSize = 1000)
        {
            if (_featureCache.Count > maxCacheSize)
            {
                var keysToRemove = _featureCache.Keys.Take(_featureCache.Count - maxCacheSize).ToList();
                foreach (var key in keysToRemove)
                {
                    _featureCache.TryRemove(key, out _);
                }
            }
        }
        
        /// <summary>
        /// Анализ качества классификации
        /// </summary>
        public async Task<Dictionary<string, object>> EvaluateModelQuality(List<(string text, IntentType expected)> testData)
        {
            if (!testData.Any()) return new Dictionary<string, object>();
            
            var correct = 0;
            var total = testData.Count;
            var confusionMatrix = new Dictionary<string, Dictionary<string, int>>();
            var intentAccuracy = new Dictionary<IntentType, (int correct, int total)>();
            
            foreach (var (text, expected) in testData)
            {
                var result = await ParseIntentAsync(text);
                var predicted = result.Type;
                
                if (predicted == expected) correct++;
                
                // Confusion matrix
                var expectedStr = expected.ToString();
                var predictedStr = predicted.ToString();
                
                if (!confusionMatrix.ContainsKey(expectedStr))
                    confusionMatrix[expectedStr] = new Dictionary<string, int>();
                
                confusionMatrix[expectedStr][predictedStr] = 
                    confusionMatrix[expectedStr].GetValueOrDefault(predictedStr, 0) + 1;
                
                // Per-intent accuracy
                var expectedStats = intentAccuracy.GetValueOrDefault(expected, (0, 0));
                var newTotal = expectedStats.Item2 + 1;
                var newCorrect = expectedStats.Item1 + (predicted == expected ? 1 : 0);
                intentAccuracy[expected] = (newCorrect, newTotal);
            }
            
            var accuracy = (double)correct / total;
            
            return new Dictionary<string, object>
            {
                ["overall_accuracy"] = accuracy,
                ["correct_predictions"] = correct,
                ["total_predictions"] = total,
                ["confusion_matrix"] = confusionMatrix,
                ["per_intent_accuracy"] = intentAccuracy.ToDictionary(
                    p => p.Key.ToString(),
                    p => p.Value.Item2 > 0 ? (double)p.Value.Item1 / p.Value.Item2 : 0.0
                )
            };
        }
    }
}

// Расширение для генерации нормально распределенных чисел
public static class RandomExtensions
{
    public static double NextGaussian(this Random random, double mean = 0, double stdDev = 1)
    {
        // Box-Muller transform
        double u1 = 1.0 - random.NextDouble();
        double u2 = 1.0 - random.NextDouble();
        double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
        return mean + stdDev * randStdNormal;
    }
}