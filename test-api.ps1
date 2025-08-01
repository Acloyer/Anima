# Test API Script for Anima AGI
Write-Host "🧪 Тестирование API Anima AGI..." -ForegroundColor Green

$baseUrl = "http://localhost:8082"
$apiKey = "anima-creator-key-2025-v1-secure"

$headers = @{
    "X-API-Key" = $apiKey
    "Content-Type" = "application/json"
}

Write-Host "`n1️⃣ Проверка статуса Anima..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "$baseUrl/api/anima/status" -Headers $headers -Method GET
    Write-Host "✅ Статус: $($response.StatusCode)" -ForegroundColor Green
    Write-Host "📄 Ответ: $($response.Content)" -ForegroundColor Cyan
} catch {
    Write-Host "❌ Ошибка: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n2️⃣ Проверка тестового эндпоинта..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "$baseUrl/api/test/health" -Headers $headers -Method GET
    Write-Host "✅ Статус: $($response.StatusCode)" -ForegroundColor Green
    Write-Host "📄 Ответ: $($response.Content)" -ForegroundColor Cyan
} catch {
    Write-Host "❌ Ошибка: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n3️⃣ Проверка Swagger..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "$baseUrl/swagger/v1/swagger.json" -Method GET
    Write-Host "✅ Swagger доступен: $($response.StatusCode)" -ForegroundColor Green
} catch {
    Write-Host "❌ Swagger недоступен: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n4️⃣ Проверка корневого эндпоинта..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "$baseUrl/" -Method GET
    Write-Host "✅ Корневой эндпоинт: $($response.StatusCode)" -ForegroundColor Green
} catch {
    Write-Host "❌ Ошибка: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n✅ Тестирование завершено!" -ForegroundColor Green 