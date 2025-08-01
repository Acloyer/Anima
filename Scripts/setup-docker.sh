#!/bin/bash

# Anima AGI Docker Setup Script
echo "🔧 Настройка Docker окружения для Anima AGI..."

# Создаем необходимые директории
echo "📁 Создание директорий..."
mkdir -p data logs ssl monitoring

# Устанавливаем правильные права доступа
echo "🔐 Установка прав доступа..."
chmod 755 data logs ssl monitoring

# Создаем пустые файлы если они не существуют
echo "📄 Создание файлов конфигурации..."
touch data/.gitkeep
touch logs/.gitkeep
touch ssl/.gitkeep

# Создаем базовую конфигурацию Prometheus если её нет
if [ ! -f monitoring/prometheus.yml ]; then
    cat > monitoring/prometheus.yml << EOF
global:
  scrape_interval: 15s

scrape_configs:
  - job_name: 'anima-agi'
    static_configs:
      - targets: ['anima-agi-container:8082']
    metrics_path: '/metrics'
EOF
fi

echo "✅ Настройка завершена!"
echo "🚀 Запустите приложение командой: docker-compose up -d" 