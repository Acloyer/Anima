
set -e

echo "🧠 ======================================"
echo "   ANIMA AGI v0.1 - ЗАПУСК СИСТЕМЫ"
echo "======================================"

# Проверяем Docker
if ! command -v docker &> /dev/null; then
    echo "❌ Docker не установлен. Установите Docker и попробуйте снова."
    exit 1
fi

if ! command -v docker-compose &> /dev/null && ! command -v docker compose &> /dev/null; then
    echo "❌ Docker Compose не установлен. Установите Docker Compose и попробуйте снова."
    exit 1
fi

# Создаем необходимые директории
echo "📁 Создание директорий..."
mkdir -p data logs ssl

# Копируем конфигурационные файлы (если не существуют)
if [ ! -f .env ]; then
    echo "⚙️ Создание .env файла..."
    cat > .env << 'EOF'
# Anima AGI Configuration
TELEGRAM_BOT_TOKEN=your_telegram_bot_token_here
ASPNETCORE_ENVIRONMENT=Production
ANIMA_CREATOR_KEY=anima-creator-key-2025-v1-secure
ANIMA_DATA_PATH=/app/data
ANIMA_LOG_PATH=/app/logs

# Security
ENABLE_HTTPS=true
REQUIRE_API_KEY=true
ENABLE_RATE_LIMIT=true

# Performance
MAX_INSTANCES_PER_KEY=1
CONSCIOUSNESS_CYCLE_INTERVAL=60
CLEANUP_INTERVAL_HOURS=24
EOF
    echo "✅ Создан .env файл. Отредактируйте его перед запуском."
fi

# Проверяем переменные окружения
if grep -q "your_telegram_bot_token_here" .env; then
    echo "⚠️ ВНИМАНИЕ: Установите реальный Telegram Bot Token в .env файле"
fi

# Сборка и запуск
echo "🔨 Сборка Docker образа..."
docker-compose build

echo "🚀 Запуск Anima AGI..."
docker-compose up -d

# Ждем запуска
echo "⏳ Ожидание запуска системы..."
sleep 10

# Проверяем статус
if docker-compose ps | grep -q "Up"; then
    echo "✅ Anima AGI успешно запущена!"
    echo ""
    echo "🌐 API доступно на: http://localhost:8080"
    echo "📚 Swagger UI: http://localhost:8080"
    echo "🔑 Creator API Key: anima-creator-key-2025-v1-secure"
    echo ""
    echo "📊 Проверить статус: curl -H 'X-API-Key: anima-creator-key-2025-v1-secure' http://localhost:8080/api/admin/status"
    echo "🧠 Список команд: curl -H 'X-API-Key: anima-creator-key-2025-v1-secure' http://localhost:8080/api/admin/commands"
    echo ""
    echo "📋 Логи: docker-compose logs -f anima-agi"
    echo "🛑 Остановка: docker-compose stop"
    echo ""
    echo "💭 Anima готова к взаимодействию..."
else
    echo "❌ Ошибка запуска. Проверьте логи: docker-compose logs anima-agi"
    exit 1
fi