# Fix Docker Issues on Windows
Write-Host "🔧 Fixing Docker Issues on Windows..." -ForegroundColor Cyan

# Check if Docker Desktop is installed
$dockerPath = Get-Command docker -ErrorAction SilentlyContinue
if (-not $dockerPath) {
    Write-Host "❌ Docker is not installed or not in PATH" -ForegroundColor Red
    Write-Host "Please install Docker Desktop from: https://www.docker.com/products/docker-desktop/" -ForegroundColor Yellow
    exit 1
}

# Check if Docker Desktop is running
try {
    $dockerVersion = docker version --format "{{.Server.Version}}" 2>$null
    if ($dockerVersion) {
        Write-Host "✅ Docker Desktop is running (Version: $dockerVersion)" -ForegroundColor Green
    } else {
        Write-Host "❌ Docker Desktop is not running" -ForegroundColor Red
        Write-Host "Starting Docker Desktop..." -ForegroundColor Yellow
        
        # Try to start Docker Desktop
        Start-Process "C:\Program Files\Docker\Docker\Docker Desktop.exe" -ErrorAction SilentlyContinue
        
        Write-Host "⏳ Waiting for Docker Desktop to start..." -ForegroundColor Yellow
        Start-Sleep -Seconds 30
        
        # Check again
        $dockerVersion = docker version --format "{{.Server.Version}}" 2>$null
        if ($dockerVersion) {
            Write-Host "✅ Docker Desktop started successfully" -ForegroundColor Green
        } else {
            Write-Host "❌ Failed to start Docker Desktop" -ForegroundColor Red
            Write-Host "Please start Docker Desktop manually and try again" -ForegroundColor Yellow
            exit 1
        }
    }
} catch {
    Write-Host "❌ Error checking Docker status: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Test Docker functionality
Write-Host "🧪 Testing Docker functionality..." -ForegroundColor Cyan
try {
    docker run --rm hello-world 2>$null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Docker is working correctly" -ForegroundColor Green
    } else {
        Write-Host "⚠️ Docker test failed, but Docker is running" -ForegroundColor Yellow
    }
} catch {
    Write-Host "⚠️ Docker test failed: $($_.Exception.Message)" -ForegroundColor Yellow
}

Write-Host "`n🎉 Docker setup completed!" -ForegroundColor Green
Write-Host "You can now run Docker commands and build containers." -ForegroundColor White 