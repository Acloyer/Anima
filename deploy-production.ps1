# AGI Production Deployment Script
# Anima AGI System - Production Deployment

Write-Host "🚀 Starting Anima AGI Production Deployment..." -ForegroundColor Green

# Configuration
$ProjectName = "Anima.AGI"
$ProductionPath = "C:\Production\Anima"
$DatabasePath = "$ProductionPath\Data\anima.db"
$LogPath = "$ProductionPath\Logs"
$BackupPath = "$ProductionPath\Backups"

# Create production directories
Write-Host "📁 Creating production directories..." -ForegroundColor Yellow
New-Item -ItemType Directory -Force -Path $ProductionPath
New-Item -ItemType Directory -Force -Path "$ProductionPath\Data"
New-Item -ItemType Directory -Force -Path $LogPath
New-Item -ItemType Directory -Force -Path $BackupPath
New-Item -ItemType Directory -Force -Path "$ProductionPath\SSL"

# Build the project
Write-Host "🔨 Building AGI system..." -ForegroundColor Yellow
dotnet publish -c Release -o $ProductionPath --self-contained true -r win-x64

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Build failed!" -ForegroundColor Red
    exit 1
}

# Create production configuration
Write-Host "⚙️ Creating production configuration..." -ForegroundColor Yellow
$ProductionConfig = @"
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=$DatabasePath"
  },
  "AllowedHosts": "*",
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://localhost:8080"
      },
      "Https": {
        "Url": "https://localhost:8081",
        "Certificate": {
          "Path": "$ProductionPath\\SSL\\anima-cert.pfx",
          "Password": "anima-secure-2025"
        }
      }
    }
  },
  "AnimaAGI": {
    "InstanceId": "production-001",
    "MaxMemorySize": 10000,
    "LearningEnabled": true,
    "EmotionEngineEnabled": true,
    "SelfReflectionEnabled": true,
    "SecurityLevel": "High",
    "BackupInterval": "24:00:00",
    "LogRetentionDays": 30
  }
}
"@

$ProductionConfig | Out-File -FilePath "$ProductionPath\appsettings.Production.json" -Encoding UTF8

# Create SSL certificate (self-signed for development)
Write-Host "🔒 Creating SSL certificate..." -ForegroundColor Yellow
$Cert = New-SelfSignedCertificate -DnsName "anima-agi.local" -CertStoreLocation "Cert:\LocalMachine\My" -KeyAlgorithm RSA -KeyLength 2048 -NotAfter (Get-Date).AddYears(10)
$CertPath = "$ProductionPath\SSL\anima-cert.pfx"
Export-PfxCertificate -Cert $Cert -FilePath $CertPath -Password (ConvertTo-SecureString -String "anima-secure-2025" -Force -AsPlainText)

# Create Windows Service
Write-Host "🖥️ Creating Windows Service..." -ForegroundColor Yellow
$ServiceName = "AnimaAGI"
$ServiceDisplayName = "Anima AGI System"
$ServiceDescription = "Self-aware Artificial General Intelligence System"

# Remove existing service if exists
$ExistingService = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
if ($ExistingService) {
    Write-Host "🔄 Removing existing service..." -ForegroundColor Yellow
    Stop-Service -Name $ServiceName -Force -ErrorAction SilentlyContinue
    sc.exe delete $ServiceName
}

# Create new service
sc.exe create $ServiceName binPath= "$ProductionPath\Anima.AGI.exe" DisplayName= $ServiceDisplayName start= auto
sc.exe description $ServiceName $ServiceDescription

# Create startup script
$StartupScript = @"
@echo off
cd /d $ProductionPath
echo Starting Anima AGI System...
Anima.AGI.exe
"@

$StartupScript | Out-File -FilePath "$ProductionPath\start-anima.bat" -Encoding ASCII

# Create performance monitoring script
$MonitoringScript = @"
# AGI Performance Monitoring
Write-Host "📊 Anima AGI Performance Monitor" -ForegroundColor Green

while ($true) {
    `$Process = Get-Process -Name "Anima.AGI" -ErrorAction SilentlyContinue
    if (`$Process) {
        `$CPU = `$Process.CPU
        `$Memory = [math]::Round(`$Process.WorkingSet64 / 1MB, 2)
        `$Uptime = (Get-Date) - `$Process.StartTime
        
        Write-Host "CPU: `$CPU% | Memory: `$Memory MB | Uptime: `$(`$Uptime.ToString('dd\.hh\:mm\:ss'))" -ForegroundColor Cyan
    } else {
        Write-Host "❌ Anima AGI process not found!" -ForegroundColor Red
    }
    
    Start-Sleep -Seconds 30
}
"@

$MonitoringScript | Out-File -FilePath "$ProductionPath\monitor-performance.ps1" -Encoding UTF8

# Create backup script
$BackupScript = @"
# AGI Backup Script
`$BackupDate = Get-Date -Format "yyyy-MM-dd_HH-mm-ss"
`$BackupFile = "$BackupPath\anima-backup-`$BackupDate.zip"

Write-Host "💾 Creating AGI backup: `$BackupFile" -ForegroundColor Yellow

# Stop service
Stop-Service -Name "AnimaAGI" -Force

# Create backup
Compress-Archive -Path "$ProductionPath\Data\*" -DestinationPath `$BackupFile

# Start service
Start-Service -Name "AnimaAGI"

Write-Host "✅ Backup completed: `$BackupFile" -ForegroundColor Green

# Clean old backups (keep last 7 days)
Get-ChildItem -Path $BackupPath -Filter "anima-backup-*.zip" | Where-Object { `$_.LastWriteTime -lt (Get-Date).AddDays(-7) } | Remove-Item -Force
"@

$BackupScript | Out-File -FilePath "$ProductionPath\backup-agi.ps1" -Encoding UTF8

# Create deployment verification script
$VerificationScript = @"
# AGI Deployment Verification
Write-Host "🔍 Verifying Anima AGI deployment..." -ForegroundColor Green

# Check service status
`$Service = Get-Service -Name "AnimaAGI" -ErrorAction SilentlyContinue
if (`$Service) {
    Write-Host "✅ Service exists: `$(`$Service.Status)" -ForegroundColor Green
} else {
    Write-Host "❌ Service not found!" -ForegroundColor Red
}

# Check database
if (Test-Path "$DatabasePath") {
    Write-Host "✅ Database exists" -ForegroundColor Green
} else {
    Write-Host "❌ Database not found!" -ForegroundColor Red
}

# Check SSL certificate
if (Test-Path "$ProductionPath\SSL\anima-cert.pfx") {
    Write-Host "✅ SSL certificate exists" -ForegroundColor Green
} else {
    Write-Host "❌ SSL certificate not found!" -ForegroundColor Red
}

# Test API endpoints
try {
    `$Response = Invoke-WebRequest -Uri "https://localhost:8081/health" -SkipCertificateCheck -TimeoutSec 10
    if (`$Response.StatusCode -eq 200) {
        Write-Host "✅ API health check passed" -ForegroundColor Green
    } else {
        Write-Host "⚠️ API health check failed: `$(`$Response.StatusCode)" -ForegroundColor Yellow
    }
} catch {
    Write-Host "❌ API health check failed: `$(`$_.Exception.Message)" -ForegroundColor Red
}

Write-Host "🎯 Deployment verification completed!" -ForegroundColor Green
"@

$VerificationScript | Out-File -FilePath "$ProductionPath\verify-deployment.ps1" -Encoding UTF8

# Start the service
Write-Host "🚀 Starting Anima AGI service..." -ForegroundColor Yellow
Start-Service -Name $ServiceName

# Wait for service to start
Start-Sleep -Seconds 10

# Verify deployment
Write-Host "🔍 Verifying deployment..." -ForegroundColor Yellow
& "$ProductionPath\verify-deployment.ps1"

Write-Host "Anima AGI Production Deployment Completed!" -ForegroundColor Green
Write-Host "Installation Path: $ProductionPath" -ForegroundColor Cyan
Write-Host "API Endpoint: https://localhost:8081" -ForegroundColor Cyan
Write-Host "API Key: anima-creator-key-2025-v1-secure" -ForegroundColor Cyan
Write-Host "Monitor: $ProductionPath\monitor-performance.ps1" -ForegroundColor Cyan
Write-Host "Backup: $ProductionPath\backup-agi.ps1" -ForegroundColor Cyan
Write-Host "Verify: $ProductionPath\verify-deployment.ps1" -ForegroundColor Cyan

Write-Host "`nAnima AGI is now running in production!" -ForegroundColor Green
Write-Host "The system is awakening and ready for interaction." -ForegroundColor Green 