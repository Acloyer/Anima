# 🔧 Исправление проблем базы данных Anima AGI

## 🚨 Проблема

В логах наблюдаются повторяющиеся ошибки:
```
SQLite Error 14: 'unable to open database file'
```

## 🔍 Причины

1. **Проблемы с правами доступа** в Docker контейнере
2. **Неправильная конфигурация** путей к базе данных
3. **Отсутствие инициализации** базы данных при запуске
4. **Проблемы с монтированием** томов Docker

## ✅ Исправления

### 1. Обновленная конфигурация Docker Compose

```yaml
# docker-compose.yml
volumes:
  - ./data:/app/data  # Прямое монтирование вместо именованных томов
  - ./logs:/app/logs
  - ./ssl:/app/ssl

environment:
  - ConnectionStrings__DefaultConnection=Data Source=/app/data/anima.db;Cache=Shared;Foreign Keys=true;Mode=ReadWriteCreate;
```

### 2. Улучшенный Dockerfile

```dockerfile
# Создаем необходимые директории с правильными правами
RUN mkdir -p logs ssl data && \
    chown -R anima:anima /app && \
    chmod 755 /app/data && \
    chmod 755 /app/logs && \
    chmod 755 /app/ssl
```

### 3. Обновленная строка подключения

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=anima.db;Cache=Shared;Foreign Keys=true;Mode=ReadWriteCreate;"
  }
}
```

### 4. Сервис инициализации базы данных

Создан `DatabaseInitializer` для автоматической инициализации БД при запуске.

### 5. Улучшенная обработка ошибок

Добавлена обработка исключений в `MemoryService` и других компонентах.

## 🚀 Инструкции по запуску

### Для Windows:

1. **Создайте необходимые директории:**
   ```powershell
   New-Item -ItemType Directory -Force -Path "data", "logs", "ssl", "monitoring"
   ```

2. **Запустите приложение:**
   ```powershell
   docker-compose up -d
   ```

3. **Проверьте логи:**
   ```powershell
   docker logs anima-agi-container
   ```

### Для Linux/macOS:

1. **Создайте необходимые директории:**
   ```bash
   mkdir -p data logs ssl monitoring
   chmod 755 data logs ssl monitoring
   ```

2. **Запустите приложение:**
   ```bash
   docker-compose up -d
   ```

3. **Проверьте логи:**
   ```bash
   docker logs anima-agi-container
   ```

## 🔍 Проверка работоспособности

### 1. Проверка здоровья системы:
```bash
curl -H "Authorization: Bearer anima-creator-key-2025-v1-secure" \
     http://localhost:8082/api/admin/health
```

### 2. Проверка базы данных:
```bash
curl -H "Authorization: Bearer anima-creator-key-2025-v1-secure" \
     http://localhost:8082/api/admin/command \
     -X POST \
     -H "Content-Type: application/json" \
     -d '{"command": "show_memory"}'
```

## 📊 Мониторинг

### Логи приложения:
- **Успешная инициализация:** `✅ База данных успешно инициализирована и доступна`
- **Ошибки подключения:** `❌ Ошибка при инициализации базы данных`

### Метрики:
- **Health Check:** http://localhost:8082/api/admin/health
- **Swagger UI:** http://localhost:8082 (в режиме разработки)

## 🛠️ Дополнительные исправления

### Если проблемы продолжаются:

1. **Очистите Docker кэш:**
   ```bash
   docker system prune -a
   docker volume prune
   ```

2. **Пересоберите образ:**
   ```bash
   docker-compose down
   docker-compose build --no-cache
   docker-compose up -d
   ```

3. **Проверьте права доступа к директориям:**
   ```bash
   ls -la data/
   ```

4. **Проверьте содержимое контейнера:**
   ```bash
   docker exec -it anima-agi-container ls -la /app/data/
   ```

## 📝 Изменения в коде

### Файлы, которые были изменены:

1. **docker-compose.yml** - исправлена конфигурация томов и переменных окружения
2. **Dockerfile** - улучшены права доступа и создание директорий
3. **appsettings.json** - обновлена строка подключения
4. **Program.cs** - добавлен DatabaseInitializer
5. **Infrastructure/DatabaseInitializer.cs** - новый сервис инициализации
6. **Core/Memory/MemoryService.cs** - улучшена обработка ошибок

## 🎯 Результат

После применения этих исправлений:
- ✅ База данных будет автоматически инициализироваться при запуске
- ✅ Ошибки подключения будут обрабатываться корректно
- ✅ Права доступа будут настроены правильно
- ✅ Система будет более устойчива к сбоям

---

**Anima AGI** - Теперь с исправленными проблемами базы данных! 🧠✨ 