# AGI Performance Testing Script
# Anima AGI System - Performance Benchmark

Write-Host "🚀 Starting Anima AGI Performance Testing..." -ForegroundColor Green

# Configuration
$BaseUrl = "https://localhost:8081"
$ApiKey = "anima-creator-key-2025-v1-secure"
$TestDuration = 300  # 5 minutes
$ConcurrentUsers = 10
$RequestsPerSecond = 5

# Headers
$Headers = @{
    "X-API-Key" = $ApiKey
    "Content-Type" = "application/json"
}

# Test scenarios
$TestScenarios = @(
    @{
        Name = "Health Check"
        Endpoint = "/health"
        Method = "GET"
        ExpectedResponseTime = 100
    },
    @{
        Name = "AGI Status"
        Endpoint = "/agi/status"
        Method = "GET"
        ExpectedResponseTime = 200
    },
    @{
        Name = "Memory Query"
        Endpoint = "/api/admin/memory"
        Method = "GET"
        ExpectedResponseTime = 500
    },
    @{
        Name = "Emotion Analysis"
        Endpoint = "/api/admin/emotions"
        Method = "GET"
        ExpectedResponseTime = 300
    },
    @{
        Name = "Intent Parsing"
        Endpoint = "/api/admin/intent"
        Method = "POST"
        Body = @{
            text = "Привет, как дела?"
            userId = "test-user-001"
        } | ConvertTo-Json
        ExpectedResponseTime = 1000
    }
)

# Performance metrics
$Metrics = @{
    TotalRequests = 0
    SuccessfulRequests = 0
    FailedRequests = 0
    TotalResponseTime = 0
    MinResponseTime = [double]::MaxValue
    MaxResponseTime = 0
    ResponseTimes = @()
    Errors = @()
}

# Function to make HTTP request
function Invoke-TestRequest {
    param(
        [string]$Url,
        [string]$Method,
        [string]$Body = $null
    )
    
    $Stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
    
    try {
        if ($Method -eq "GET") {
            $Response = Invoke-WebRequest -Uri $Url -Headers $Headers -SkipCertificateCheck -TimeoutSec 30
        } else {
            $Response = Invoke-WebRequest -Uri $Url -Method $Method -Headers $Headers -Body $Body -SkipCertificateCheck -TimeoutSec 30
        }
        
        $Stopwatch.Stop()
        $ResponseTime = $Stopwatch.ElapsedMilliseconds
        
        return @{
            Success = $true
            StatusCode = $Response.StatusCode
            ResponseTime = $ResponseTime
            Content = $Response.Content
        }
    }
    catch {
        $Stopwatch.Stop()
        $ResponseTime = $Stopwatch.ElapsedMilliseconds
        
        return @{
            Success = $false
            StatusCode = $_.Exception.Response.StatusCode.value__
            ResponseTime = $ResponseTime
            Error = $_.Exception.Message
        }
    }
}

# Function to run performance test
function Start-PerformanceTest {
    param(
        [string]$ScenarioName,
        [string]$Endpoint,
        [string]$Method,
        [string]$Body = $null,
        [int]$ExpectedResponseTime
    )
    
    Write-Host "🧪 Testing: $ScenarioName" -ForegroundColor Yellow
    
    $ScenarioMetrics = @{
        Name = $ScenarioName
        Requests = 0
        Success = 0
        Failed = 0
        TotalTime = 0
        MinTime = [double]::MaxValue
        MaxTime = 0
        Times = @()
    }
    
    $EndTime = (Get-Date).AddSeconds($TestDuration)
    
    while ((Get-Date) -lt $EndTime) {
        $Url = "$BaseUrl$Endpoint"
        $Result = Invoke-TestRequest -Url $Url -Method $Method -Body $Body
        
        $ScenarioMetrics.Requests++
        $Metrics.TotalRequests++
        
        if ($Result.Success) {
            $ScenarioMetrics.Success++
            $Metrics.SuccessfulRequests++
            
            $ResponseTime = $Result.ResponseTime
            $ScenarioMetrics.TotalTime += $ResponseTime
            $Metrics.TotalResponseTime += $ResponseTime
            
            $ScenarioMetrics.Times += $ResponseTime
            $Metrics.ResponseTimes += $ResponseTime
            
            if ($ResponseTime -lt $ScenarioMetrics.MinTime) {
                $ScenarioMetrics.MinTime = $ResponseTime
            }
            if ($ResponseTime -gt $ScenarioMetrics.MaxTime) {
                $ScenarioMetrics.MaxTime = $ResponseTime
            }
            
            if ($ResponseTime -lt $Metrics.MinResponseTime) {
                $Metrics.MinResponseTime = $ResponseTime
            }
            if ($ResponseTime -gt $Metrics.MaxResponseTime) {
                $Metrics.MaxResponseTime = $ResponseTime
            }
            
            # Performance warning
            if ($ResponseTime -gt $ExpectedResponseTime) {
                Write-Host "⚠️ Slow response: ${ResponseTime}ms (expected: ${ExpectedResponseTime}ms)" -ForegroundColor Yellow
            }
        } else {
            $ScenarioMetrics.Failed++
            $Metrics.FailedRequests++
            $Metrics.Errors += "$ScenarioName: $($Result.Error)"
            Write-Host "❌ Request failed: $($Result.Error)" -ForegroundColor Red
        }
        
        # Rate limiting
        Start-Sleep -Milliseconds (1000 / $RequestsPerSecond)
    }
    
    # Calculate scenario statistics
    $AvgTime = if ($ScenarioMetrics.Success -gt 0) { $ScenarioMetrics.TotalTime / $ScenarioMetrics.Success } else { 0 }
    $SuccessRate = if ($ScenarioMetrics.Requests -gt 0) { ($ScenarioMetrics.Success / $ScenarioMetrics.Requests) * 100 } else { 0 }
    
    Write-Host "📊 $ScenarioName Results:" -ForegroundColor Cyan
    Write-Host "   Requests: $($ScenarioMetrics.Requests)" -ForegroundColor White
    Write-Host "   Success Rate: $([math]::Round($SuccessRate, 2))%" -ForegroundColor White
    Write-Host "   Avg Response Time: $([math]::Round($AvgTime, 2))ms" -ForegroundColor White
    Write-Host "   Min/Max: $($ScenarioMetrics.MinTime)ms / $($ScenarioMetrics.MaxTime)ms" -ForegroundColor White
    
    return $ScenarioMetrics
}

# Function to generate performance report
function Generate-PerformanceReport {
    Write-Host "`n📈 AGI Performance Report" -ForegroundColor Green
    Write-Host "================================" -ForegroundColor Green
    
    # Overall statistics
    $OverallSuccessRate = if ($Metrics.TotalRequests -gt 0) { ($Metrics.SuccessfulRequests / $Metrics.TotalRequests) * 100 } else { 0 }
    $OverallAvgTime = if ($Metrics.SuccessfulRequests -gt 0) { $Metrics.TotalResponseTime / $Metrics.SuccessfulRequests } else { 0 }
    $RequestsPerSecond = if ($TestDuration -gt 0) { $Metrics.TotalRequests / $TestDuration } else { 0 }
    
    Write-Host "Overall Statistics:" -ForegroundColor Yellow
    Write-Host "   Total Requests: $($Metrics.TotalRequests)" -ForegroundColor White
    Write-Host "   Successful: $($Metrics.SuccessfulRequests)" -ForegroundColor White
    Write-Host "   Failed: $($Metrics.FailedRequests)" -ForegroundColor White
    Write-Host "   Success Rate: $([math]::Round($OverallSuccessRate, 2))%" -ForegroundColor White
    Write-Host "   Avg Response Time: $([math]::Round($OverallAvgTime, 2))ms" -ForegroundColor White
    Write-Host "   Min/Max Response Time: $($Metrics.MinResponseTime)ms / $($Metrics.MaxResponseTime)ms" -ForegroundColor White
    Write-Host "   Requests/Second: $([math]::Round($RequestsPerSecond, 2))" -ForegroundColor White
    
    # Performance analysis
    Write-Host "`nPerformance Analysis:" -ForegroundColor Yellow
    
    if ($OverallSuccessRate -ge 99) {
        Write-Host "   ✅ Excellent reliability" -ForegroundColor Green
    } elseif ($OverallSuccessRate -ge 95) {
        Write-Host "   ⚠️ Good reliability" -ForegroundColor Yellow
    } else {
        Write-Host "   ❌ Poor reliability" -ForegroundColor Red
    }
    
    if ($OverallAvgTime -le 500) {
        Write-Host "   ✅ Excellent performance" -ForegroundColor Green
    } elseif ($OverallAvgTime -le 1000) {
        Write-Host "   ⚠️ Good performance" -ForegroundColor Yellow
    } else {
        Write-Host "   ❌ Poor performance" -ForegroundColor Red
    }
    
    if ($RequestsPerSecond -ge 10) {
        Write-Host "   ✅ High throughput" -ForegroundColor Green
    } elseif ($RequestsPerSecond -ge 5) {
        Write-Host "   ⚠️ Moderate throughput" -ForegroundColor Yellow
    } else {
        Write-Host "   ❌ Low throughput" -ForegroundColor Red
    }
    
    # Error analysis
    if ($Metrics.Errors.Count -gt 0) {
        Write-Host "`nError Analysis:" -ForegroundColor Yellow
        $ErrorGroups = $Metrics.Errors | Group-Object
        foreach ($ErrorGroup in $ErrorGroups) {
            Write-Host "   $($ErrorGroup.Name): $($ErrorGroup.Count) occurrences" -ForegroundColor Red
        }
    }
    
    # Recommendations
    Write-Host "`nRecommendations:" -ForegroundColor Yellow
    if ($OverallSuccessRate -lt 99) {
        Write-Host "   🔧 Investigate and fix failed requests" -ForegroundColor White
    }
    if ($OverallAvgTime -gt 1000) {
        Write-Host "   🚀 Optimize response times" -ForegroundColor White
    }
    if ($RequestsPerSecond -lt 5) {
        Write-Host "   ⚡ Improve system throughput" -ForegroundColor White
    }
    
    Write-Host "   📊 Monitor system resources" -ForegroundColor White
    Write-Host "   🔄 Run regular performance tests" -ForegroundColor White
}

# Main execution
Write-Host "⏱️ Test Duration: $TestDuration seconds" -ForegroundColor Cyan
Write-Host "👥 Concurrent Users: $ConcurrentUsers" -ForegroundColor Cyan
Write-Host "📊 Requests/Second: $RequestsPerSecond" -ForegroundColor Cyan

$StartTime = Get-Date
Write-Host "`n🚀 Starting performance tests at $StartTime" -ForegroundColor Green

# Run tests for each scenario
$ScenarioResults = @()
foreach ($Scenario in $TestScenarios) {
    $Result = Start-PerformanceTest -ScenarioName $Scenario.Name -Endpoint $Scenario.Endpoint -Method $Scenario.Method -Body $Scenario.Body -ExpectedResponseTime $Scenario.ExpectedResponseTime
    $ScenarioResults += $Result
}

$EndTime = Get-Date
$TotalDuration = ($EndTime - $StartTime).TotalSeconds

Write-Host "`n✅ Performance testing completed in $([math]::Round($TotalDuration, 2)) seconds" -ForegroundColor Green

# Generate report
Generate-PerformanceReport

# Save results to file
$ReportData = @{
    TestDate = Get-Date
    Duration = $TotalDuration
    Metrics = $Metrics
    ScenarioResults = $ScenarioResults
}

$ReportFile = "agi-performance-report-$(Get-Date -Format 'yyyy-MM-dd_HH-mm-ss').json"
$ReportData | ConvertTo-Json -Depth 10 | Out-File -FilePath $ReportFile -Encoding UTF8

Write-Host "`n📄 Detailed report saved to: $ReportFile" -ForegroundColor Cyan
Write-Host "🎯 AGI Performance Testing Completed!" -ForegroundColor Green 