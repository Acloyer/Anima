# Используем официальный .NET 8.0 SDK образ для сборки
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Устанавливаем рабочую директорию
WORKDIR /src

# Копируем файл проекта
COPY ["Anima.AGI.csproj", "."]

# Восстанавливаем зависимости
RUN dotnet restore "Anima.AGI.csproj"

# Копируем весь исходный код
COPY . .

# Устанавливаем рабочую директорию для сборки
WORKDIR "/src"

# Собираем проект в Release режиме
ARG BUILD_CONFIGURATION=Release
RUN dotnet build "Anima.AGI.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Этап публикации
FROM build AS publish
RUN dotnet publish "Anima.AGI.csproj" -c $BUILD_CONFIGURATION -o /app/publish --no-build

# Используем runtime образ для финального контейнера
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

# Устанавливаем необходимые пакеты
RUN apt-get update && apt-get install -y \
    curl \
    wget \
    sqlite3 \
    && rm -rf /var/lib/apt/lists/*

# Создаем пользователя без root прав
RUN adduser --disabled-password --gecos '' anima && \
    mkdir -p /app/logs /app/ssl /app/data && \
    chown -R anima:anima /app

# Устанавливаем рабочую директорию
WORKDIR /app

# Копируем опубликованные файлы
COPY --from=publish /app/publish .

# Создаем необходимые директории
RUN mkdir -p logs ssl data

# Устанавливаем права доступа
RUN chown -R anima:anima /app && \
    chmod +x /app/Anima.AGI

# Переключаемся на пользователя anima
USER anima

# Открываем порты
EXPOSE 8082
EXPOSE 8083

# Устанавливаем переменные окружения
ENV ASPNETCORE_URLS=http://+:8082
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV DOTNET_USE_POLLING_FILE_WATCHER=true

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD curl -f http://localhost:8082/health || exit 1

# Устанавливаем точку входа
ENTRYPOINT ["dotnet", "Anima.AGI.dll"]