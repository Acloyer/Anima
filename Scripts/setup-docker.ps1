# Anima AGI Docker Setup Script for Windows
Write-Host "🔧 Настройка Docker окружения для Anima AGI..." -ForegroundColor Green

# Создаем необходимые директории
Write-Host "📁 Создание директорий..." -ForegroundColor Yellow
New-Item -ItemType Directory -Force -Path "data"
New-Item -ItemType Directory -Force -Path "logs"
New-Item -ItemType Directory -Force -Path "ssl"
New-Item -ItemType Directory -Force -Path "monitoring"

# Создаем пустые файлы если они не существуют
Write-Host "📄 Создание файлов конфигурации..." -ForegroundColor Yellow
New-Item -ItemType File -Force -Path "data\.gitkeep" | Out-Null
New-Item -ItemType File -Force -Path "logs\.gitkeep" | Out-Null
New-Item -ItemType File -Force -Path "ssl\.gitkeep" | Out-Null

# Создаем базовую конфигурацию Prometheus если её нет
if (-not (Test-Path "monitoring\prometheus.yml")) {
    Write-Host "📊 Создание конфигурации Prometheus..." -ForegroundColor Yellow
    @"
global:
  scrape_interval: 15s

scrape_configs:
  - job_name: 'anima-agi'
    static_configs:
      - targets: ['anima-agi-container:8082']
    metrics_path: '/metrics'
"@ | Out-File -FilePath "monitoring\prometheus.yml" -Encoding UTF8
}

Write-Host "✅ Настройка завершена!" -ForegroundColor Green
Write-Host "🚀 Запустите приложение командой: docker-compose up -d" -ForegroundColor Cyan 