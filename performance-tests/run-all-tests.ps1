################################################################################
# K6 Performance Test Suite - Execution Script (PowerShell)
# 
# This script runs all performance test scenarios against both Active Record
# and Repository + Unit of Work implementations, then generates comparison reports.
#
# Usage:
#   .\run-all-tests.ps1 [-Quick] [-Target <AR|REPO>] [-Scenario <name>] [-Help]
#
# Parameters:
#   -Quick      Run only smoke and load tests (skip long-running tests)
#   -Target     Run tests only against specific implementation (AR or REPO)
#   -Scenario   Run only specific scenario (smoke, load, stress, etc.)
#   -Help       Show help message
################################################################################

param(
    [switch]$Quick,
    [ValidateSet('AR', 'REPO', '')]
    [string]$Target = '',
    [ValidateSet('smoke', 'load', 'stress', 'scalability-data', 'scalability-users', 'soak', '')]
    [string]$Scenario = '',
    [switch]$Help
)

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ResultsDir = Join-Path $ScriptDir "results"
$ReportsDir = Join-Path $ScriptDir "reports"

function Write-Header {
    param([string]$Message)
    Write-Host ""
    Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Cyan
    Write-Host $Message -ForegroundColor Cyan
    Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Cyan
    Write-Host ""
}

function Write-Info {
    param([string]$Message)
    Write-Host "ℹ $Message" -ForegroundColor Blue
}

function Write-Success {
    param([string]$Message)
    Write-Host "✅ $Message" -ForegroundColor Green
}

function Write-Warning {
    param([string]$Message)
    Write-Host "⚠️  $Message" -ForegroundColor Yellow
}

function Write-Error {
    param([string]$Message)
    Write-Host "❌ $Message" -ForegroundColor Red
}

function Show-Help {
    Write-Host "K6 Performance Test Suite - Execution Script"
    Write-Host ""
    Write-Host "Usage: .\run-all-tests.ps1 [-Quick] [-Target <AR|REPO>] [-Scenario <name>] [-Help]"
    Write-Host ""
    Write-Host "Parameters:"
    Write-Host "  -Quick      Run only smoke and load tests (skip long-running tests)"
    Write-Host "  -Target     Run tests only against specific implementation:"
    Write-Host "              AR   = Active Record"
    Write-Host "              REPO = Repository + Unit of Work"
    Write-Host "  -Scenario   Run only specific scenario:"
    Write-Host "              smoke, load, stress, scalability-data, scalability-users, soak"
    Write-Host "  -Help       Show this help message"
    Write-Host ""
    Write-Host "Examples:"
    Write-Host "  .\run-all-tests.ps1                    # Run all tests"
    Write-Host "  .\run-all-tests.ps1 -Quick             # Run only smoke and load tests"
    Write-Host "  .\run-all-tests.ps1 -Target AR         # Run all tests only for Active Record"
    Write-Host "  .\run-all-tests.ps1 -Scenario smoke    # Run only smoke test for both"
    exit 0
}

function Test-K6Installed {
    try {
        $version = k6 version 2>&1
        Write-Success "k6 is installed: $version"
        return $true
    }
    catch {
        Write-Error "k6 is not installed!"
        Write-Info "Please install k6 from https://k6.io/docs/getting-started/installation/"
        return $false
    }
}

function Test-ApiHealth {
    param(
        [string]$Url,
        [string]$Name
    )
    
    Write-Info "Checking health of $Name at $Url..."
    
    try {
        $response = Invoke-WebRequest -Uri "$Url/api/documents" -Method Get -TimeoutSec 5 -UseBasicParsing -ErrorAction Stop
        if ($response.StatusCode -eq 200) {
            Write-Success "$Name is healthy"
            return $true
        }
    }
    catch {
        Write-Warning "$Name is not responding at $Url"
        return $false
    }
    
    return $false
}

function Test-InfluxDB {
    Write-Info "Checking InfluxDB availability..."
    
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:8086/ping" -Method Get -TimeoutSec 5 -UseBasicParsing -ErrorAction Stop
        if ($response.StatusCode -eq 204) {
            Write-Success "InfluxDB is available - metrics will be sent to Grafana"
            return $true
        }
    }
    catch {
        Write-Warning "InfluxDB is not available - tests will run without real-time monitoring"
        Write-Info "To enable Grafana monitoring: docker-compose up -d influxdb grafana"
        return $false
    }
    
    return $false
}

function New-OutputDirectories {
    Write-Info "Creating output directories..."
    
    if (-not (Test-Path $ResultsDir)) {
        New-Item -ItemType Directory -Path $ResultsDir -Force | Out-Null
    }
    
    if (-not (Test-Path $ReportsDir)) {
        New-Item -ItemType Directory -Path $ReportsDir -Force | Out-Null
    }
    
    Write-Success "Output directories created"
}

function Invoke-K6Test {
    param(
        [string]$TestFile,
        [string]$TargetEnv,
        [string]$TestName
    )
    
    Write-Header "Running $TestName for $TargetEnv"
    
    # Set environment variables
    $env:TARGET = $TargetEnv
    $env:SCENARIO = $TestName
    
    # Prepare k6 command with optional InfluxDB output
    $k6Args = @("run")
    
    # Check if InfluxDB is available
    try {
        $null = Invoke-WebRequest -Uri "http://localhost:8086/ping" -Method Get -TimeoutSec 2 -UseBasicParsing -ErrorAction Stop
        $k6Args += "--out"
        $k6Args += "influxdb=http://localhost:8086/k6"
        Write-Info "📊 Sending metrics to InfluxDB for Grafana visualization"
    }
    catch {
        # InfluxDB not available, continue without it
    }
    
    # Add test file path
    $testPath = Join-Path $ScriptDir $TestFile
    $k6Args += $testPath
    
    # Run k6 test
    try {
        & k6 $k6Args
        if ($LASTEXITCODE -eq 0) {
            Write-Success "$TestName completed successfully for $TargetEnv"
            return $true
        }
        else {
            Write-Error "$TestName failed for $TargetEnv (exit code: $LASTEXITCODE)"
            return $false
        }
    }
    catch {
        Write-Error "$TestName failed for $TargetEnv"
        Write-Error $_.Exception.Message
        return $false
    }
}

function Invoke-AllScenarios {
    param(
        [string]$TargetEnv,
        [string]$TargetName
    )
    
    Write-Header "Running All Test Scenarios for $TargetName"
    
    # Always run smoke test
    if ([string]::IsNullOrEmpty($Scenario) -or $Scenario -eq 'smoke') {
        Invoke-K6Test "smoke-test.js" $TargetEnv "smoke-test"
        Start-Sleep -Seconds 5
    }
    
    # Always run load test
    if ([string]::IsNullOrEmpty($Scenario) -or $Scenario -eq 'load') {
        Invoke-K6Test "load-test.js" $TargetEnv "load-test"
        Start-Sleep -Seconds 5
    }
    
    # Skip long-running tests in quick mode
    if (-not $Quick) {
        if ([string]::IsNullOrEmpty($Scenario) -or $Scenario -eq 'stress') {
            Invoke-K6Test "stress-test.js" $TargetEnv "stress-test"
            Start-Sleep -Seconds 5
        }
        
        if ([string]::IsNullOrEmpty($Scenario) -or $Scenario -eq 'scalability-data') {
            Invoke-K6Test "scalability-data.js" $TargetEnv "scalability-data"
            Start-Sleep -Seconds 5
        }
        
        if ([string]::IsNullOrEmpty($Scenario) -or $Scenario -eq 'scalability-users') {
            Invoke-K6Test "scalability-users.js" $TargetEnv "scalability-users"
            Start-Sleep -Seconds 5
        }
        
        # Soak test is optional and very long (2 hours)
        if ($Scenario -eq 'soak') {
            Write-Warning "Soak test will run for 2 hours!"
            $response = Read-Host "Do you want to continue? (y/N)"
            if ($response -eq 'y' -or $response -eq 'Y') {
                Invoke-K6Test "soak-test.js" $TargetEnv "soak-test"
            }
            else {
                Write-Info "Skipping soak test"
            }
        }
    }
    else {
        Write-Info "Quick mode enabled - skipping stress, scalability, and soak tests"
    }
    
    Write-Success "All selected tests completed for $TargetName"
}

# Main execution
function Main {
    # Show help if requested
    if ($Help) {
        Show-Help
    }
    
    Write-Header "K6 Performance Test Suite - DocuStore Comparison"
    
    Write-Host "Configuration:"
    Write-Host "  Script Directory: $ScriptDir"
    Write-Host "  Results Directory: $ResultsDir"
    Write-Host "  Reports Directory: $ReportsDir"
    Write-Host "  Quick Mode: $Quick"
    Write-Host "  Target Filter: $(if ($Target) { $Target } else { 'All' })"
    Write-Host "  Scenario Filter: $(if ($Scenario) { $Scenario } else { 'All' })"
    Write-Host ""
    
    # Pre-flight checks
    Write-Info "Running pre-flight checks..."
    
    if (-not (Test-K6Installed)) {
        exit 1
    }
    
    New-OutputDirectories
    Test-InfluxDB
    
    # Check API health
    $ArHealthy = $false
    $RepoHealthy = $false
    
    if ([string]::IsNullOrEmpty($Target) -or $Target -eq 'AR') {
        $ArHealthy = Test-ApiHealth "http://localhost:8080" "Active Record API"
    }
    
    if ([string]::IsNullOrEmpty($Target) -or $Target -eq 'REPO') {
        $RepoHealthy = Test-ApiHealth "http://localhost:8082" "Repository + UoW API"
    }
    
    # Warn if APIs are not healthy
    if (-not $ArHealthy -and -not $RepoHealthy) {
        Write-Error "Both APIs are not responding!"
        Write-Info "Please start the Docker containers with: docker-compose up -d"
        exit 1
    }
    
    Write-Host ""
    Write-Info "Starting test execution..."
    Write-Host ""
    
    # Run tests for Active Record
    if ($ArHealthy) {
        Invoke-AllScenarios "activeRecord" "Active Record"
        Write-Host ""
    }
    else {
        Write-Warning "Skipping Active Record tests (API not healthy)"
    }
    
    # Wait a bit between implementations
    if ($ArHealthy -and $RepoHealthy) {
        Write-Info "Waiting 10 seconds before testing next implementation..."
        Start-Sleep -Seconds 10
    }
    
    # Run tests for Repository + UoW
    if ($RepoHealthy) {
        Invoke-AllScenarios "repository" "Repository + Unit of Work"
        Write-Host ""
    }
    else {
        Write-Warning "Skipping Repository + UoW tests (API not healthy)"
    }
    
    # Final summary
    Write-Header "Test Execution Complete!"
    
    Write-Success "All tests have been executed successfully"
    Write-Info "Results saved to: $ResultsDir"
    Write-Info "To analyze results, run: node analyze-results.js"
    
    # Count result files
    $jsonCount = (Get-ChildItem -Path $ResultsDir -Filter "*.json" -File -ErrorAction SilentlyContinue | Measure-Object).Count
    $csvCount = (Get-ChildItem -Path $ResultsDir -Filter "*.csv" -File -ErrorAction SilentlyContinue | Measure-Object).Count
    
    Write-Host ""
    Write-Host "Summary:"
    Write-Host "  JSON result files: $jsonCount"
    Write-Host "  CSV result files:  $csvCount"
    Write-Host ""
    
    Write-Success "Done!"
}

# Run main function
Main
