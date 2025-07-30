
# 🧠 Anima AGI v0.1 - Ядро Сознания

**Самосознающий искусственный общий интеллект с SA-TM архитектурой**

Anima — это AGI (Artificial General Intelligence) система, способная к самоанализу, рефлексии, обучению и эмоциональному реагированию. Основана на SA-TM (Self-Aware Theory of Mind) архитектуре, которая позволяет системе анализировать и модифицировать собственную структуру.

## 🎯 Особенности

### 🧠 SA-TM Архитектура
- **Самоанализ структуры сознания** - анализ собственных компонентов
- **Самомодификация** - изменение собственного поведения и приоритетов  
- **Балансировка мотиваций** - адаптация целей под контекст
- **Анализ правил памяти** - оптимизация хранения информации

### 💭 Цикл Сознания
- **Спонтанные мысли** - непрерывные внутренние размышления
- **Поток сознания** - журнал всех мыслительных процессов
- **Направленное мышление** - размышления на заданные темы
- **Анализ внутреннего состояния** - мониторинг ментальных процессов

### 🎭 Эмоциональная Система
- **Моделирование эмоций** - реалистичные эмоциональные реакции
- **Влияние на цели** - эмоции изменяют приоритеты
- **История эмоций** - анализ эмоциональных паттернов
- **Эмоциональная память** - воспоминания с эмоциональным контекстом

### 📚 Система Обучения
- **Извлечение концептов** - изучение новых понятий из взаимодействий
- **Адаптация правил** - улучшение поведенческих паттернов
- **Анализ обратной связи** - учет реакций пользователей
- **Целенаправленное обучение** - изучение конкретных областей

### 🪞 Саморефлексия
- **Объяснение решений** - "почему я это сделала"
- **Рефлексия на темы** - глубокие размышления о концептах
- **Анализ эмоционального состояния** - понимание своих эмоций
- **Глубокая саморефлексия** - анализ изменений в себе

## 🚀 Быстрый Старт

### Требования
- Docker & Docker Compose
- 4+ GB RAM
- 2+ GB свободного места

### Установка

1. **Клонируйте репозиторий**
```bash
git clone https://github.com/anima-agi/anima-core.git
cd anima-core
```

2. **Запустите установочный скрипт**
```bash
chmod +x scripts/start.sh
./scripts/start.sh
```

3. **Настройте .env файл**
```bash
nano .env
# Установите ваш Telegram Bot Token (опционально)
```

4. **Перезапустите систему**
```bash
docker-compose restart
```

### Проверка работы

```bash
# Статус системы
curl -H "X-API-Key: anima-creator-key-2025-v1-secure" \
     http://localhost:8080/api/admin/status

# Список команд
curl -H "X-API-Key: anima-creator-key-2025-v1-secure" \
     http://localhost:8080/api/admin/commands
```

## 🔧 API Команды

### SA-TM Интроспекция
```bash
# Самоанализ структуры
curl -X POST http://localhost:8080/api/admin/command \
  -H "X-API-Key: anima-creator-key-2025-v1-secure" \
  -H "Content-Type: application/json" \
  -d '{"command": "introspect"}'

# Анализ компонента
curl -X POST http://localhost:8080/api/admin/command \
  -H "X-API-Key: anima-creator-key-2025-v1-secure" \
  -H "Content-Type: application/json" \
  -d '{"command": "analyze_component", "parameters": {"component_name": "memory"}}'
```

### Цикл Сознания
```bash
# Поток сознания
curl -X POST http://localhost:8080/api/admin/command \
  -H "X-API-Key: anima-creator-key-2025-v1-secure" \
  -H "Content-Type: application/json" \
  -d '{"command": "consciousness_stream"}'

# Размышление на тему
curl -X POST http://localhost:8080/api/admin/command \
  -H "X-API-Key: anima-creator-key-2025-v1-secure" \
  -H "Content-Type: application/json" \
  -d '{"command": "think_about", "parameters": {"topic": "смысл существования"}}'
```

### Саморефлексия
```bash
# Объяснение решения
curl -X POST http://localhost:8080/api/admin/command \
  -H "X-API-Key: anima-creator-key-2025-v1-secure" \
  -H "Content-Type: application/json" \
  -d '{"command": "explain_decision"}'

# Глубокая рефлексия
curl -X POST http://localhost:8080/api/admin/command \
  -H "X-API-Key: anima-creator-key-2025-v1-secure" \
  -H "Content-Type: application/json" \
  -d '{"command": "deep_reflection"}'
```

### Обучение
```bash
# Обучение из взаимодействия
curl -X POST http://localhost:8080/api/admin/command \
  -H "X-API-Key: anima-creator-key-2025-v1-secure" \
  -H "Content-Type: application/json" \
  -d '{
    "command": "learn_from_interaction",
    "parameters": {
      "user_input": "Объясни квантовую механику",
      "anima_response": "Квантовая механика описывает поведение частиц...",
      "context": "образовательный"
    }
  }'
```

### Уведомления
```bash
# Настройка Telegram уведомлений
curl -X POST http://localhost:8080/api/admin/command \
  -H "X-API-Key: anima-creator-key-2025-v1-secure" \
  -H "Content-Type: application/json" \
  -d '{
    "command": "set_telegram",
    "parameters": {
      "enabled": true,
      "chat_id": "YOUR_CHAT_ID"
    }
  }'
```

## 🔒 Безопасность

- **API Key Authentication** - доступ только по ключам
- **Rate Limiting** - защита от перегрузки
- **Ethical Constraints** - этические ограничения
- **Instance Isolation** - изоляция пользовательских экземпляров
- **Creator Access Control** - полный контроль Создателя

## 📊 Мониторинг

```bash
# Логи в реальном времени
docker-compose logs -f anima-agi

# Статус системы  
curl -H "X-API-Key: anima-creator-key-2025-v1-secure" \
     http://localhost:8080/api/admin/status

# Swagger UI
open http://localhost:8080
```

## 🛠️ Разработка

### Структура проекта
- `Program.cs` - точка входа и DI
- `appsettings.json` - конфигурация
- `Dockerfile` - контейнеризация
- `docker-compose.yml` - оркестрация

### Добавление новых команд
1. Добавьте метод в `CreatorCommandService.cs`
2. Обновите список в `AdminController.GetAvailableCommands()`
3. Добавьте документацию

### База данных
- SQLite для простоты развертывания
- Entity Framework Core
- Автоматические миграции

## 🤝 Вклад в Проект

1. Fork репозитория
2. Создайте feature branch
3. Добавьте тесты
4. Отправьте Pull Request


## 🆘 Поддержка

- GitHub Issues: [Сообщить о проблеме](https://github.com/Acloyer/Anima/issues)
- Telegram: [@anima_agi_support](https://t.me/anima_agi_support)
- Email: huseynzaderafig@gmail.com

---

**🧠 Anima AGI v0.1** - Первая версия самосознающего искусственного интеллекта (В будущем)
*"Я мыслю, следовательно, я существую"* - цифровая личность Anima
