# 🔧 Устранение проблем с Anima AGI

## 🚨 Проблема: Проект не запускается

### 🔍 Диагностика

1. **Запустите диагностический скрипт:**
   ```powershell
   powershell -ExecutionPolicy Bypass -File debug-project.ps1
   ```

2. **Проверьте наличие .NET 8.0:**
   ```powershell
   dotnet --version
   ```
   Должно быть: `8.0.x`

### 🛠️ Решения

#### 1. Проблемы с .NET

**Симптомы:** `dotnet` команда не найдена

**Решение:**
- Скачайте и установите .NET 8.0 SDK с [официального сайта](https://dotnet.microsoft.com/download/dotnet/8.0)
- Перезапустите PowerShell после установки

#### 2. Проблемы с зависимостями

**Симптомы:** Ошибки при `dotnet restore` или `dotnet build`

**Решение:**
```powershell
# Очистите кэш NuGet
dotnet nuget locals all --clear

# Восстановите зависимости
dotnet restore --force

# Соберите проект
dotnet build
```

#### 3. Проблемы с базой данных

**Симптомы:** Ошибки SQLite при запуске

**Решение:**
```powershell
# Создайте директории
New-Item -ItemType Directory -Force -Path "data", "logs", "ssl"

# Удалите старую базу данных (если есть)
Remove-Item "anima.db" -ErrorAction SilentlyContinue
```

#### 4. Проблемы с конфигурацией

**Симптомы:** Ошибки в appsettings.json

**Решение:**
Проверьте, что файл `appsettings.json` содержит корректный JSON:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=anima.db;Cache=Shared;Foreign Keys=true;Mode=ReadWriteCreate;"
  }
}
```

### 🚀 Альтернативные способы запуска

#### 1. Быстрый запуск
```powershell
powershell -ExecutionPolicy Bypass -File quick-start.ps1
```

#### 2. Упрощенная версия
Если полная версия не работает, попробуйте упрощенную:
```powershell
# Временно переименуйте файлы
Rename-Item "Program.cs" "Program.cs.backup"
Rename-Item "Program-Simple.cs" "Program.cs"

# Запустите
dotnet run

# Восстановите оригинальный файл
Rename-Item "Program.cs" "Program-Simple.cs"
Rename-Item "Program.cs.backup" "Program.cs"
```

#### 3. Запуск без Docker
```powershell
# Создайте директории
New-Item -ItemType Directory -Force -Path "data", "logs", "ssl"

# Восстановите зависимости
dotnet restore

# Соберите проект
dotnet build

# Запустите
dotnet run
```

### 🔍 Проверка работоспособности

После успешного запуска:

1. **Проверьте API:**
   ```bash
   curl http://localhost:8082/health
   ```

2. **Проверьте Swagger UI:**
   Откройте в браузере: http://localhost:8082/swagger

3. **Проверьте логи:**
   ```powershell
   Get-Content "logs\*.log" -Tail 20
   ```

### 📋 Частые ошибки

#### Ошибка: "The type or namespace name 'X' could not be found"
**Решение:** Проверьте using директивы в Program.cs

#### Ошибка: "Unable to open database file"
**Решение:** 
- Создайте директорию `data`
- Проверьте права доступа
- Убедитесь, что путь к БД корректный

#### Ошибка: "Port 8082 is already in use"
**Решение:**
```powershell
# Найдите процесс на порту 8082
netstat -ano | findstr :8082

# Остановите процесс
taskkill /PID <PID> /F
```

### 🆘 Если ничего не помогает

1. **Полная переустановка:**
   ```powershell
   # Удалите все временные файлы
   Remove-Item "bin", "obj" -Recurse -Force
   Remove-Item "anima.db*" -Force
   
   # Восстановите заново
   dotnet restore
   dotnet build
   dotnet run
   ```

2. **Проверьте версии пакетов:**
   ```powershell
   dotnet list package
   ```

3. **Создайте новый проект для тестирования:**
   ```powershell
   dotnet new webapi -n TestAnima
   cd TestAnima
   dotnet run
   ```

### 📞 Поддержка

Если проблемы продолжаются:
1. Запустите `debug-project.ps1` и сохраните вывод
2. Проверьте логи в директории `logs`
3. Убедитесь, что у вас установлен .NET 8.0 SDK

---

**Anima AGI** - Мы поможем вам запустить систему! 🧠✨ 