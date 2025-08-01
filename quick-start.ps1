# Quick Start Script for Anima AGI
Write-Host "🚀 Быстрый запуск Anima AGI..." -ForegroundColor Green

# Проверяем наличие .NET
Write-Host "🔍 Проверка .NET..." -ForegroundColor Yellow
try {
    $dotnetVersion = dotnet --version
    Write-Host "✅ .NET найден: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Host "❌ .NET не найден. Установите .NET 8.0 SDK" -ForegroundColor Red
    exit 1
}

# Очищаем предыдущую сборку
Write-Host "🧹 Очистка предыдущей сборки..." -ForegroundColor Yellow
dotnet clean 2>$null
dotnet restore 2>$null

# Собираем проект
Write-Host "🔨 Сборка проекта..." -ForegroundColor Yellow
$buildResult = dotnet build --no-restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Ошибка сборки проекта" -ForegroundColor Red
    Write-Host $buildResult
    exit 1
}
Write-Host "✅ Проект успешно собран" -ForegroundColor Green

# Создаем необходимые директории
Write-Host "📁 Создание директорий..." -ForegroundColor Yellow
New-Item -ItemType Directory -Force -Path "data", "logs", "ssl" | Out-Null

# Запускаем проект
Write-Host "🚀 Запуск Anima AGI..." -ForegroundColor Green
Write-Host "🌐 Приложение будет доступно по адресу: http://localhost:8082" -ForegroundColor Cyan
Write-Host "📖 API Key: anima-creator-key-2025-v1-secure" -ForegroundColor Cyan
Write-Host "🛑 Для остановки нажмите Ctrl+C" -ForegroundColor Yellow

dotnet run 