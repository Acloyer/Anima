# Debug Project Script for Anima AGI
Write-Host "🔍 Диагностика проблем с Anima AGI..." -ForegroundColor Green

# Проверяем наличие .NET
Write-Host "`n1️⃣ Проверка .NET..." -ForegroundColor Yellow
try {
    $dotnetVersion = dotnet --version
    Write-Host "✅ .NET найден: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Host "❌ .NET не найден" -ForegroundColor Red
    exit 1
}

# Проверяем структуру проекта
Write-Host "`n2️⃣ Проверка структуры проекта..." -ForegroundColor Yellow
$requiredFiles = @(
    "Anima.AGI.csproj",
    "Program.cs",
    "appsettings.json"
)

foreach ($file in $requiredFiles) {
    if (Test-Path $file) {
        Write-Host "✅ $file найден" -ForegroundColor Green
    } else {
        Write-Host "❌ $file не найден" -ForegroundColor Red
    }
}

# Проверяем зависимости
Write-Host "`n3️⃣ Проверка зависимостей..." -ForegroundColor Yellow
try {
    dotnet restore --verbosity quiet
    Write-Host "✅ Зависимости восстановлены" -ForegroundColor Green
} catch {
    Write-Host "❌ Ошибка восстановления зависимостей" -ForegroundColor Red
}

# Пробуем собрать проект
Write-Host "`n4️⃣ Сборка проекта..." -ForegroundColor Yellow
try {
    $buildOutput = dotnet build --verbosity minimal 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Проект успешно собран" -ForegroundColor Green
    } else {
        Write-Host "❌ Ошибка сборки:" -ForegroundColor Red
        Write-Host $buildOutput -ForegroundColor Red
    }
} catch {
    Write-Host "❌ Ошибка при сборке" -ForegroundColor Red
}

# Проверяем конфигурацию
Write-Host "`n5️⃣ Проверка конфигурации..." -ForegroundColor Yellow
try {
    $config = Get-Content "appsettings.json" | ConvertFrom-Json
    Write-Host "✅ appsettings.json корректный" -ForegroundColor Green
    
    if ($config.ConnectionStrings.DefaultConnection) {
        Write-Host "✅ Строка подключения найдена" -ForegroundColor Green
    } else {
        Write-Host "⚠️ Строка подключения не найдена" -ForegroundColor Yellow
    }
} catch {
    Write-Host "❌ Ошибка в appsettings.json" -ForegroundColor Red
}

# Проверяем директории
Write-Host "`n6️⃣ Проверка директорий..." -ForegroundColor Yellow
$directories = @("data", "logs", "ssl")
foreach ($dir in $directories) {
    if (Test-Path $dir) {
        Write-Host "✅ Директория $dir существует" -ForegroundColor Green
    } else {
        Write-Host "⚠️ Директория $dir не существует" -ForegroundColor Yellow
        New-Item -ItemType Directory -Force -Path $dir | Out-Null
        Write-Host "✅ Директория $dir создана" -ForegroundColor Green
    }
}

Write-Host "`n🎯 Диагностика завершена!" -ForegroundColor Green
Write-Host "💡 Для запуска используйте: dotnet run" -ForegroundColor Cyan 