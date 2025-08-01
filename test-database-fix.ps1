# Test Database Fix Script for Anima AGI
Write-Host "🧪 Тестирование исправлений базы данных Anima AGI..." -ForegroundColor Green

# Проверяем наличие Docker
try {
    docker --version | Out-Null
    Write-Host "✅ Docker найден" -ForegroundColor Green
} catch {
    Write-Host "❌ Docker не найден. Установите Docker Desktop" -ForegroundColor Red
    exit 1
}

# Проверяем наличие docker-compose
try {
    docker-compose --version | Out-Null
    Write-Host "✅ Docker Compose найден" -ForegroundColor Green
} catch {
    Write-Host "❌ Docker Compose не найден" -ForegroundColor Red
    exit 1
}

# Останавливаем существующие контейнеры
Write-Host "🛑 Остановка существующих контейнеров..." -ForegroundColor Yellow
docker-compose down 2>$null

# Очищаем старые образы
Write-Host "🧹 Очистка старых образов..." -ForegroundColor Yellow
docker system prune -f 2>$null

# Создаем необходимые директории
Write-Host "📁 Создание директорий..." -ForegroundColor Yellow
New-Item -ItemType Directory -Force -Path "data", "logs", "ssl", "monitoring" | Out-Null

# Собираем и запускаем контейнер
Write-Host "🔨 Сборка и запуск контейнера..." -ForegroundColor Yellow
docker-compose up -d --build

# Ждем запуска
Write-Host "⏳ Ожидание запуска приложения..." -ForegroundColor Yellow
Start-Sleep -Seconds 30

# Проверяем статус контейнера
Write-Host "🔍 Проверка статуса контейнера..." -ForegroundColor Yellow
$containerStatus = docker ps --filter "name=anima-agi-container" --format "table {{.Status}}"
Write-Host $containerStatus

# Проверяем логи
Write-Host "📋 Последние логи приложения:" -ForegroundColor Yellow
docker logs --tail 20 anima-agi-container

# Тестируем API
Write-Host "🌐 Тестирование API..." -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "http://localhost:8082/api/admin/health" -Headers @{
        "Authorization" = "Bearer anima-creator-key-2025-v1-secure"
    } -Method GET -TimeoutSec 10
    
    Write-Host "✅ API отвечает корректно" -ForegroundColor Green
    Write-Host "📊 Статус: $($response.status)" -ForegroundColor Cyan
} catch {
    Write-Host "❌ API не отвечает: $($_.Exception.Message)" -ForegroundColor Red
}

# Проверяем базу данных
Write-Host "🗄️ Тестирование базы данных..." -ForegroundColor Yellow
try {
    $dbResponse = Invoke-RestMethod -Uri "http://localhost:8082/api/admin/command" -Headers @{
        "Authorization" = "Bearer anima-creator-key-2025-v1-secure"
        "Content-Type" = "application/json"
    } -Method POST -Body '{"command": "show_memory"}' -TimeoutSec 10
    
    Write-Host "✅ База данных работает корректно" -ForegroundColor Green
    Write-Host "📊 Результат: $($dbResponse.result)" -ForegroundColor Cyan
} catch {
    Write-Host "❌ Проблема с базой данных: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n🎯 Тестирование завершено!" -ForegroundColor Green
Write-Host "📖 Для просмотра логов используйте: docker logs -f anima-agi-container" -ForegroundColor Cyan
Write-Host "🌐 API доступен по адресу: http://localhost:8082" -ForegroundColor Cyan 