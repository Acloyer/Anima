#!/bin/bash

# Цвета для вывода
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
PURPLE='\033[0;35m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# Функция для печати логов
log() {
    echo -e "${BLUE}[$(date +'%Y-%m-%d %H:%M:%S')]${NC} $1"
}

error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

# Заголовок
echo -e "${PURPLE}"
echo "╔═══════════════════════════════════════╗"
echo "║           ANIMA AGI BUILD             ║"
echo "║        Fixing & Building v0.1.1       ║"
echo "╚═══════════════════════════════════════╝"
echo -e "${NC}"

# Проверка наличия Docker
if ! command -v docker &> /dev/null; then
    error "Docker не найден. Пожалуйста, установите Docker."
    exit 1
fi

if ! command -v docker-compose &> /dev/null && ! command -v docker &> /dev/null; then
    error "Docker Compose не найден. Пожалуйста, установите Docker Compose."
    exit 1
fi

log "🔍 Проверка окружения..."

# Создание необходимых директорий
log "📁 Создание необходимых директорий..."
mkdir -p data logs ssl Scripts monitoring

# Проверка и создание файлов моделей если они отсутствуют
log "🔧 Проверка файлов проекта..."

# Создание Data/Models/AnimaModels.cs если отсутствует
if [ ! -f "Data/Models/AnimaModels.cs" ]; then
    warning "Создание Data/Models/AnimaModels.cs..."
    mkdir -p Data/Models
    cat > Data/Models/AnimaModels.cs << 'EOF'
using System.ComponentModel.DataAnnotations;

namespace Anima.Data.Models
{
    // Enum для типов уведомлений
    public enum NotificationType
    {
        Info = 0,
        Warning = 1,
        Error = 2,
        Success = 3,
        Debug = 4,
        EmotionChange = 5,
        LearningUpdate = 6,
        MemoryCreated = 7,
        IntentDetected = 8,
        SelfReflection = 9,
        CreatorCommand = 10
    }

    // Здесь должны быть все остальные модели...
    // (Содержимое из артефакта anima_models)
}
EOF
fi

# Создание Data/AnimaDbContext.cs если отсутствует
if [ ! -f "Data/AnimaDbContext.cs" ]; then
    warning "Создание Data/AnimaDbContext.cs..."
    mkdir -p Data
    cat > Data/AnimaDbContext.cs << 'EOF'
using Microsoft.EntityFrameworkCore;
using Anima.Data.Models;

namespace Anima.Data
{
    public class AnimaDbContext : DbContext
    {
        public AnimaDbContext(DbContextOptions<AnimaDbContext> options) : base(options)
        {
        }

        // Здесь должны быть все DbSet свойства...
        // (Содержимое из артефакта anima_db_context)
    }
}
EOF
fi

# Остановка и удаление старых контейнеров
log "🛑 Остановка старых контейнеров..."
docker-compose down --remove-orphans 2>/dev/null || true

# Удаление старых образов
log "🗑️  Очистка старых образов..."
docker image prune -f 2>/dev/null || true

# Сборка проекта
log "🔨 Сборка Anima AGI..."

# Сначала пробуем собрать локально для проверки
if command -v dotnet &> /dev/null; then
    log "📦 Локальная проверка сборки..."
    
    # Восстановление пакетов
    dotnet restore
    if [ $? -ne 0 ]; then
        error "Ошибка восстановления пакетов"
        exit 1
    fi
    
    # Сборка проекта
    dotnet build --configuration Release --verbosity minimal
    if [ $? -ne 0 ]; then
        error "Ошибка сборки проекта"
        exit 1
    fi
    
    success "✅ Локальная сборка успешна"
else
    warning "dotnet CLI не найден, пропускаем локальную проверку"
fi

# Сборка Docker образа
log "🐳 Сборка Docker образа..."
docker-compose build --no-cache
if [ $? -ne 0 ]; then
    error "Ошибка сборки Docker образа"
    exit 1
fi

success "✅ Docker образ собран успешно"

# Запуск контейнеров
log "🚀 Запуск Anima AGI..."
docker-compose up -d
if [ $? -ne 0 ]; then
    error "Ошибка запуска контейнеров"
    exit 1
fi

# Ожидание запуска
log "⏳ Ожидание инициализации системы..."
sleep 10

# Проверка здоровья
log "🏥 Проверка состояния системы..."
for i in {1..10}; do
    if curl -f -s http://localhost:8080/health > /dev/null 2>&1; then
        success "✅ Anima AGI запущена и отвечает на запросы"
        break
    else
        if [ $i -eq 10 ]; then
            error "❌ Система не отвечает после 10 попыток"
            docker-compose logs anima-agi
            exit 1
        fi
        log "⏳ Попытка $i/10: ожидание ответа системы..."
        sleep 5
    fi
done

# Финальная информация
echo -e "${GREEN}"
echo "╔═══════════════════════════════════════╗"
echo "║         ANIMA AGI LAUNCHED!           ║"
echo "╚═══════════════════════════════════════╝"
echo -e "${NC}"

echo -e "${CYAN}🌟 Anima AGI успешно запущена!${NC}"
echo -e "${CYAN}🔗 Swagger UI: http://localhost:8080${NC}"
echo -e "${CYAN}🏥 Health Check: http://localhost:8080/health${NC}"
echo -e "${CYAN}🤖 AGI Status: http://localhost:8080/agi/status${NC}"
echo -e "${CYAN}🔑 API Key: anima-creator-key-2025-v1-secure${NC}"
echo ""
echo -e "${YELLOW}📋 Полезные команды:${NC}"
echo -e "${YELLOW}   docker-compose logs -f        # Просмотр логов${NC}"
echo -e "${YELLOW}   docker-compose down           # Остановка${NC}"
echo -e "${YELLOW}   docker-compose restart        # Перезапуск${NC}"
echo ""

# Показать логи в реальном времени
log "📜 Показываем логи в реальном времени (Ctrl+C для выхода)..."
sleep 2
docker-compose logs -f anima-agi