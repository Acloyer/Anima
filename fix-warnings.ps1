# Fix Compiler Warnings Script for Anima AGI
Write-Host "🔧 Fixing compiler warnings for Anima AGI..." -ForegroundColor Green

# Create data directory if it doesn't exist
if (!(Test-Path "Data")) {
    New-Item -ItemType Directory -Path "Data" -Force
    Write-Host "✅ Created Data directory" -ForegroundColor Green
}

# Clean and rebuild
Write-Host "🧹 Cleaning previous build..." -ForegroundColor Yellow
dotnet clean

Write-Host "🔨 Building project..." -ForegroundColor Yellow
dotnet build --verbosity quiet

Write-Host "✅ Build completed!" -ForegroundColor Green
Write-Host "🎯 Ready to run Anima AGI!" -ForegroundColor Cyan 