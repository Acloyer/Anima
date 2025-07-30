@echo off
echo ========================================
echo     ANIMA AGI - QUICK FIX
echo ========================================

echo [INFO] Очистка сборки...
if exist bin rmdir /s /q bin
if exist obj rmdir /s /q obj

echo [INFO] Восстановление пакетов...
dotnet restore --verbosity quiet

echo [INFO] Сборка проекта...
dotnet build --configuration Release --verbosity minimal

if %errorlevel% equ 0 (
    echo [SUCCESS] ✅ Проект успешно собран!
    echo [INFO] Запуск Docker сборки...
    docker-compose build --no-cache
    
    if %errorlevel% equ 0 (
        echo [SUCCESS] ✅ Docker образ создан!
        echo [INFO] Запуск контейнеров...
        docker-compose up -d
        
        echo [SUCCESS] 🚀 Anima AGI запускается...
        echo [INFO] Ожидание инициализации (10 сек)...
        timeout /t 10 /nobreak >nul
        
        echo [INFO] Проверка состояния...
        curl -f -s http://localhost:8080/health >nul 2>&1
        if %errorlevel% equ 0 (
            echo [SUCCESS] 🌟 Anima AGI успешно запущена!
            echo [INFO] 🔗 Swagger UI: http://localhost:8080
            echo [INFO] 🏥 Health Check: http://localhost:8080/health
            echo [INFO] 🔑 API Key: anima-creator-key-2025-v1-secure
        ) else (
            echo [WARNING] ⚠️  Система может еще загружаться...
            echo [INFO] Проверьте логи: docker-compose logs -f
        )
    ) else (
        echo [ERROR] ❌ Ошибка создания Docker образа
    )
) else (
    echo [ERROR] ❌ Ошибка сборки проекта
    exit /b 1
)

pause