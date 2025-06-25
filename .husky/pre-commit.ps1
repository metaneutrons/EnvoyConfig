#!/usr/bin/env pwsh
# Pre-commit hook for EnvoyConfig

# Verify that all files have LGPL-3.0 headers
Write-Host "📝 Checking for LGPL-3.0 headers..." -ForegroundColor Cyan
$missingHeaders = Get-ChildItem -Path ./EnvoyConfig -Include *.cs -Recurse -File |
Where-Object { $_.FullName -notlike "*/obj/*" -and $_.FullName -notlike "*/bin/*" } |
Where-Object { $_.Name -ne "EnvoyConfig.AssemblyInfo.cs" -and $_.Name -ne "EnvoyConfig.GlobalUsings.g.cs" } |
Where-Object { (Get-Content $_.FullName -Raw) -notlike "*GNU General Public License*" } |
Select-Object -ExpandProperty FullName

if ($missingHeaders) {
    Write-Host "❌ ERROR: The following files are missing LGPL-3.0 headers:" -ForegroundColor Red
    $missingHeaders | ForEach-Object { Write-Host "   $_" -ForegroundColor Red }
    Write-Host ""
    Write-Host "Please add LGPL-3.0 headers to these files before committing." -ForegroundColor Red
    exit 1
}

# Check for shell scripts (.sh files) in the staged files
Write-Host "🔍 Checking for shell script files..." -ForegroundColor Cyan
$shellScripts = git diff --cached --name-only --diff-filter=ACM | Select-String -Pattern '\.sh$'

if ($shellScripts) {
    Write-Host "❌ ERROR: Shell script files detected in commit:" -ForegroundColor Red
    $shellScripts | ForEach-Object { Write-Host "   $_" -ForegroundColor Red }
    Write-Host "This project only supports PowerShell scripts (.ps1). Please convert shell scripts to PowerShell or remove them." -ForegroundColor Red
    exit 1
}

# Run formatting and style checks but don't block commits
Write-Host "📋 Running code style verification..." -ForegroundColor Cyan
dotnet build EnvoyConfig/EnvoyConfig.csproj /p:TreatWarningsAsErrors=false

# Check for potential secrets
Write-Host "🔐 Checking for potential secrets or credentials..." -ForegroundColor Cyan
# Define pattern with double quotes and proper escaping
$secretsPattern = "(password|secret|key|token|credential).*=.*[0-9a-zA-Z]{16,}"
$secrets = Get-ChildItem -Path ./EnvoyConfig -Include *.cs, *.json, *.xml -Recurse -File |
Select-String -Pattern $secretsPattern

if ($secrets) {
    Write-Host "⚠️ Potential secrets or credentials found:" -ForegroundColor Yellow
    $secrets | ForEach-Object { Write-Host "   $_" -ForegroundColor Yellow }
    Write-Host "Please verify these are not actual secrets before committing." -ForegroundColor Yellow
    exit 1
}

# Success message
Write-Host "✅ Pre-commit checks passed!" -ForegroundColor Green
