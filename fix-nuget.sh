#!/bin/bash

# Цвета для вывода
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo -e "${BLUE}========================================"
echo "       ANIMA AGI - NUGET FIX"
echo -e "========================================${NC}"

echo -e "${BLUE}[INFO]${NC} Очистка NuGet кэша..."
dotnet nuget locals all --clear

echo -e "${BLUE}[INFO]${NC} Очистка bin и obj папок..."
rm -rf bin obj

echo -e "${BLUE}[INFO]${NC} Восстановление пакетов..."
dotnet restore --verbosity minimal

if [ $? -eq 0 ]; then
    echo -e "${GREEN}[SUCCESS]${NC} Пакеты успешно восстановлены!"
    echo -e "${BLUE}[INFO]${NC} Попытка сборки..."
    dotnet build --configuration Release --verbosity minimal
    
    if [ $? -eq 0 ]; then
        echo -e "${GREEN}[SUCCESS]${NC} Проект успешно собран!"
        echo -e "${BLUE}[INFO]${NC} Готов к запуску Docker сборки"
    else
        echo -e "${RED}[ERROR]${NC} Ошибка сборки проекта"
        exit 1
    fi
else
    echo -e "${RED}[ERROR]${NC} Ошибка восстановления пакетов"
    exit 1
fi