namespace Anima.AGI.Core.Intent
{
    /// <summary>
    /// Перечисление типов намерений для AGI системы Anima
    /// Используется для классификации пользовательского ввода и определения соответствующих действий
    /// </summary>
    public enum IntentType
    {
        /// <summary>
        /// Неопределенное намерение - когда парсер не смог классифицировать ввод
        /// </summary>
        Unknown,

        /// <summary>
        /// Приветствие - инициация общения
        /// </summary>
        Greet,

        /// <summary>
        /// Запрос информации или задание вопроса
        /// </summary>
        AskQuestion,

        /// <summary>
        /// Предоставление обратной связи о работе AGI
        /// </summary>
        GiveFeedback,

        /// <summary>
        /// Запрос на извлечение информации из памяти
        /// </summary>
        RequestMemory,

        /// <summary>
        /// Установка новой цели для AGI
        /// </summary>
        SetGoal,

        /// <summary>
        /// Действие, направленное на вызов эмоциональной реакции
        /// </summary>
        TriggerEmotion,

        /// <summary>
        /// Команда завершения работы системы
        /// </summary>
        Shutdown,

        /// <summary>
        /// Запрос на самоанализ текущего состояния
        /// </summary>
        Introspect,

        /// <summary>
        /// Запрос на рефлексию над прошлыми действиями или мыслями
        /// </summary>
        Reflect,

        /// <summary>
        /// Внедрение внешней мысли в систему размышлений
        /// </summary>
        InjectThought,

        /// <summary>
        /// Команда на модификацию собственного поведения или параметров
        /// </summary>
        ModifySelf,

        /// <summary>
        /// Запрос объяснения принятого решения
        /// </summary>
        ExplainDecision,

        /// <summary>
        /// Активация определенного сценария поведения
        /// </summary>
        ActivateScenario,

        /// <summary>
        /// Положительная обратная связь от пользователя
        /// </summary>
        UserFeedbackPositive,

        /// <summary>
        /// Негативная обратная связь от пользователя
        /// </summary>
        UserFeedbackNegative
    }
}