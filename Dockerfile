FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Anima.AGI.csproj", "."]
RUN dotnet restore "Anima.AGI.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "Anima.AGI.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Anima.AGI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Создаем директории для данных
RUN mkdir -p /app/data
RUN chmod 755 /app/data

# Создаем пользователя для безопасности
RUN addgroup --gid 1001 --system anima
RUN adduser --uid 1001 --system --gid 1001 anima
RUN chown -R anima:anima /app/data

USER anima

ENTRYPOINT ["dotnet", "Anima.AGI.dll"]