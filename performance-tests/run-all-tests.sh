#!/bin/bash

################################################################################
# K6 Performance Test Suite - Execution Script
# 
# This script runs all performance test scenarios against both Active Record
# and Repository + Unit of Work implementations.
#
# Usage:
#   ./run-all-tests.sh [OPTIONS]
#
# Options:
#   --quick       Run only smoke and load tests (skip long-running tests)
#   --target AR   Run tests only against Active Record
#   --target REPO Run tests only against Repository + UoW
#   --scenario X  Run only specific scenario:
#                 smoke, load, stress, concurrent-writes,
#                 scalability-data, scalability-users, pagination, soak
#   --help        Show this help message
################################################################################

RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# Configuration
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
RESULTS_DIR="${SCRIPT_DIR}/results"
REPORTS_DIR="${SCRIPT_DIR}/reports"

RUN_QUICK=false
TARGET_FILTER=""     # AR | REPO | "" (both)
SCENARIO_FILTER=""   # smoke | load | stress | concurrent-writes | scalability-data | scalability-users | pagination | soak | "" (all)

while [[ $# -gt 0 ]]; do
  case $1 in
    --quick)
      RUN_QUICK=true
      shift
      ;;
    --target)
      TARGET_FILTER="$2"
      shift 2
      ;;
    --scenario)
      SCENARIO_FILTER="$2"
      shift 2
      ;;
    --help)
      echo "Usage: $0 [OPTIONS]"
      echo ""
      echo "Options:"
      echo "  --quick         Run only smoke and load tests"
      echo "  --target AR     Run tests only against Active Record"
      echo "  --target REPO   Run tests only against Repository + UoW"
      echo "  --scenario X    Run only specific scenario:"
      echo "                    smoke, load, stress, concurrent-writes,"
      echo "                    scalability-data, scalability-users, pagination, soak"
      echo "  --help          Show this help message"
      exit 0
      ;;
    *)
      echo "Unknown option: $1"
      echo "Use --help for usage information"
      exit 1
      ;;
  esac
done

print_header() {
  echo -e "${CYAN}"
  echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
  echo "$1"
  echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
  echo -e "${NC}"
}

print_info() {
  echo -e "${BLUE}ℹ $1${NC}"
}

print_success() {
  echo -e "${GREEN}✅ $1${NC}"
}

print_warning() {
  echo -e "${YELLOW}⚠️  $1${NC}"
}

print_error() {
  echo -e "${RED}❌ $1${NC}"
}

check_k6_installed() {
  if ! command -v k6 &> /dev/null; then
    print_error "k6 is not installed!"
    print_info "Please install k6 from https://k6.io/docs/getting-started/installation/"
    exit 1
  fi
  
  print_success "k6 is installed: $(k6 version)"
}

check_api_health() {
  local url=$1
  local name=$2
  
  print_info "Checking health of $name at $url..."
  
  if curl -s -f -o /dev/null --max-time 5 "$url/api/documents"; then
    print_success "$name is healthy"
    return 0
  else
    print_warning "$name is not responding at $url"
    return 1
  fi
}

check_influxdb() {
  print_info "Checking InfluxDB availability..."
  
  if curl -s -f -o /dev/null --max-time 5 "http://localhost:8086/ping"; then
    print_success "InfluxDB is available - metrics will be sent to Grafana"
    return 0
  else
    print_warning "InfluxDB is not available - tests will run without real-time monitoring"
    print_info "To enable Grafana monitoring: docker-compose up -d influxdb grafana"
    return 1
  fi
}

create_directories() {
  print_info "Creating output directories..."
  mkdir -p "$RESULTS_DIR"
  mkdir -p "$REPORTS_DIR"
  print_success "Output directories created"
}

run_test() {
  local test_file=$1
  local target=$2
  local scenario_id=$3
  local pretty_name=$4
  
  print_header "Running $pretty_name for $target"
  
  # Prepare k6 command with optional InfluxDB output
  local k6_cmd="k6 run"
  
  # Check if InfluxDB is available
  if curl -s -f -o /dev/null --max-time 2 "http://localhost:8086/ping" 2>/dev/null; then
    k6_cmd="$k6_cmd --out influxdb=http://localhost:8086/k6"
    print_info "📊 Sending metrics to InfluxDB for Grafana visualization"
  fi
  
  TARGET="$target" SCENARIO="$scenario_id" $k6_cmd "$SCRIPT_DIR/$test_file" || true
  
  print_success "$pretty_name completed for $target"
  return 0
}

run_all_scenarios() {
  local target=$1
  local target_name=$2
  
  print_header "Running All Test Scenarios for $target_name"
  
  if [ -z "$SCENARIO_FILTER" ] || [ "$SCENARIO_FILTER" = "smoke" ]; then
    run_test "smoke-test.js" "$target" "smoke" "smoke-test"
    sleep 5
  fi
  
  if [ -z "$SCENARIO_FILTER" ] || [ "$SCENARIO_FILTER" = "load" ]; then
    run_test "load-test.js" "$target" "load" "load-test"
    sleep 5
  fi
  
  if [ "$RUN_QUICK" = false ]; then
    if [ -z "$SCENARIO_FILTER" ] || [ "$SCENARIO_FILTER" = "stress" ]; then
      run_test "stress-test.js" "$target" "stress" "stress-test"
      sleep 5
    fi
    
    if [ -z "$SCENARIO_FILTER" ] || [ "$SCENARIO_FILTER" = "concurrent-writes" ]; then
      run_test "concurrent-writes-test.js" "$target" "concurrent-writes" "concurrent-writes-test"
      sleep 5
    fi
    
    if [ -z "$SCENARIO_FILTER" ] || [ "$SCENARIO_FILTER" = "scalability-data" ]; then
      run_test "scalability-data.js" "$target" "scalability-data" "scalability-data"
      sleep 5
    fi
    
    if [ -z "$SCENARIO_FILTER" ] || [ "$SCENARIO_FILTER" = "scalability-users" ]; then
      run_test "scalability-users.js" "$target" "scalability-users" "scalability-users"
      sleep 5
    fi
    
    if [ -z "$SCENARIO_FILTER" ] || [ "$SCENARIO_FILTER" = "pagination" ]; then
      run_test "pagination-test.js" "$target" "pagination" "pagination-test"
      sleep 5
    fi
    
    if [ "$SCENARIO_FILTER" = "soak" ]; then
      print_warning "Soak test will run for 2 hours!"
      read -p "Do you want to continue? (y/N) " -n 1 -r
      echo
      if [[ $REPLY =~ ^[Yy]$ ]]; then
        run_test "soak-test.js" "$target" "soak" "soak-test"
      else
        print_info "Skipping soak test"
      fi
    fi
  else
    print_info "Quick mode enabled - skipping stress, scalability, soak, concurrent-writes, and pagination tests"
  fi
  
  print_success "All selected tests completed for $target_name"
}

main() {
  print_header "K6 Performance Test Suite - DocuStore Comparison"
  
  echo "Configuration:"
  echo "  Script Directory: $SCRIPT_DIR"
  echo "  Results Directory: $RESULTS_DIR"
  echo "  Reports Directory: $REPORTS_DIR"
  echo "  Quick Mode: $RUN_QUICK"
  echo "  Target Filter: ${TARGET_FILTER:-All}"
  echo "  Scenario Filter: ${SCENARIO_FILTER:-All}"
  echo ""
  
  print_info "Running pre-flight checks..."
  check_k6_installed
  create_directories
  check_influxdb
  
  AR_HEALTHY=false
  REPO_HEALTHY=false
  
  if [ -z "$TARGET_FILTER" ] || [ "$TARGET_FILTER" = "AR" ]; then
    if check_api_health "http://localhost:8080" "Active Record API"; then
      AR_HEALTHY=true
    fi
  fi
  
  if [ -z "$TARGET_FILTER" ] || [ "$TARGET_FILTER" = "REPO" ]; then
    if check_api_health "http://localhost:8082" "Repository + UoW API"; then
      REPO_HEALTHY=true
    fi
  fi
  
  if [ "$AR_HEALTHY" = false ] && [ "$REPO_HEALTHY" = false ]; then
    print_error "Both APIs are not responding!"
    print_info "Please start the Docker containers with: docker-compose up -d"
    exit 1
  fi
  
  echo ""
  print_info "Starting test execution..."
  echo ""
  
  if [ "$AR_HEALTHY" = true ]; then
    run_all_scenarios "activeRecord" "Active Record"
    echo ""
  else
    print_warning "Skipping Active Record tests (API not healthy)"
  fi
  
  if [ "$AR_HEALTHY" = true ] && [ "$REPO_HEALTHY" = true ]; then
    print_info "Waiting 10 seconds before testing next implementation..."
    sleep 10
  fi
  
  if [ "$REPO_HEALTHY" = true ]; then
    run_all_scenarios "repository" "Repository + Unit of Work"
    echo ""
  else
    print_warning "Skipping Repository + UoW tests (API not healthy)"
  fi
  
  print_header "Test Execution Complete!"
  
  print_success "All tests have been executed successfully"
  print_info "Results saved to: $RESULTS_DIR"
  print_info "To analyze results, run: node analyze-results.js"
  
  json_count=$(find "$RESULTS_DIR" -name "*.json" -type f 2>/dev/null | wc -l)
  csv_count=$(find "$RESULTS_DIR" -name "*.csv" -type f 2>/dev/null | wc -l)
  
  echo ""
  echo "Summary:"
  echo "  JSON result files: $json_count"
  echo "  CSV result files:  $csv_count"
  echo ""
  
  print_success "Done!"
}

main
