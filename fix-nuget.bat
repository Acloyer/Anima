@echo off
echo ========================================
echo        ANIMA AGI - NUGET FIX
echo ========================================

echo [INFO] Очистка NuGet кэша...
dotnet nuget locals all --clear

echo [INFO] Очистка bin и obj папок...
if exist bin rmdir /s /q bin
if exist obj rmdir /s /q obj

echo [INFO] Восстановление пакетов...
dotnet restore --verbosity minimal

if %errorlevel% equ 0 (
    echo [SUCCESS] Пакеты успешно восстановлены!
    echo [INFO] Попытка сборки...
    dotnet build --configuration Release --verbosity minimal
    
    if %errorlevel% equ 0 (
        echo [SUCCESS] Проект успешно собран!
        echo [INFO] Готов к запуску Docker сборки
    ) else (
        echo [ERROR] Ошибка сборки проекта
        exit /b 1
    )
) else (
    echo [ERROR] Ошибка восстановления пакетов
    exit /b 1
)

pause