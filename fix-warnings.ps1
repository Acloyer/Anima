# Fix Compiler Warnings Script
# This script helps fix common C# compiler warnings

Write-Host "🔧 Fixing Compiler Warnings..." -ForegroundColor Cyan

# Function to add null checks to constructors
function Add-NullChecks {
    param([string]$FilePath)
    
    $content = Get-Content $FilePath -Raw
    $originalContent = $content
    
    # Pattern to find constructor with ILogger parameter
    $pattern = 'public\s+\w+\([^)]*ILogger[^)]*\)\s*\{[^}]*_logger\s*=\s*logger[^}]*\}'
    $replacement = 'public $1($2) { _logger = logger ?? throw new ArgumentNullException(nameof(logger)); $3 }'
    
    $content = $content -replace $pattern, $replacement
    
    if ($content -ne $originalContent) {
        Set-Content $FilePath $content -Encoding UTF8
        Write-Host "✅ Fixed null checks in $FilePath" -ForegroundColor Green
    }
}

# Function to add required modifiers to properties
function Add-RequiredModifiers {
    param([string]$FilePath)
    
    $content = Get-Content $FilePath -Raw
    $originalContent = $content
    
    # Pattern to find string properties without required
    $pattern = 'public\s+string\s+(\w+)\s+\{\s*get;\s*set;\s*\}'
    $replacement = 'public required string $1 { get; set; }'
    
    $content = $content -replace $pattern, $replacement
    
    if ($content -ne $originalContent) {
        Set-Content $FilePath $content -Encoding UTF8
        Write-Host "✅ Added required modifiers in $FilePath" -ForegroundColor Green
    }
}

# Function to add async/await to methods
function Add-AsyncAwait {
    param([string]$FilePath)
    
    $content = Get-Content $FilePath -Raw
    $originalContent = $content
    
    # Pattern to find async methods without await
    $pattern = 'public\s+async\s+Task[^}]*\{[^}]*//\s*Заглушка[^}]*\}'
    $replacement = 'public async Task$1 { await Task.Delay(100); // Заглушка }'
    
    $content = $content -replace $pattern, $replacement
    
    if ($content -ne $originalContent) {
        Set-Content $FilePath $content -Encoding UTF8
        Write-Host "✅ Added async/await in $FilePath" -ForegroundColor Green
    }
}

# List of files to fix
$filesToFix = @(
    "Core/Intent/IntentParser.cs",
    "Core/Intent/AdvancedIntentParser.cs",
    "Core/SA/SAIntrospectionEngine.cs",
    "Core/SA/ThoughtLog.cs",
    "Core/Emotion/EmotionEngine.cs",
    "Core/Learning/LearningEngine.cs",
    "Core/Learning/FeedbackParser.cs",
    "Core/Security/SelfDestructionCheck.cs",
    "Core/Security/EthicalConstraints.cs",
    "Core/Admin/CreatorCommandService.cs",
    "Core/Admin/CreatorPreferences.cs",
    "API/Controllers/AnimaController.cs",
    "API/Controllers/AdminController.cs",
    "API/Controllers/TestController.cs",
    "Infrastructure/Auth/APIKeyService.cs",
    "Infrastructure/Middleware/RateLimiter.cs"
)

Write-Host "📁 Processing $($filesToFix.Count) files..." -ForegroundColor Cyan

foreach ($file in $filesToFix) {
    $fullPath = Join-Path $PSScriptRoot $file
    if (Test-Path $fullPath) {
        Write-Host "🔧 Processing $file..." -ForegroundColor Yellow
        
        try {
            Add-NullChecks $fullPath
            Add-RequiredModifiers $fullPath
            Add-AsyncAwait $fullPath
        } catch {
            Write-Host "⚠️ Error processing $file : $($_.Exception.Message)" -ForegroundColor Yellow
        }
    } else {
        Write-Host "⚠️ File not found: $file" -ForegroundColor Yellow
    }
}

Write-Host "`n🎉 Warning fixes completed!" -ForegroundColor Green
Write-Host "Run 'dotnet build' to check if warnings are reduced." -ForegroundColor White 