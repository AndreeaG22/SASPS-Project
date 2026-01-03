const fs = require('fs');
const path = require('path');

// Configuration
const RESULTS_DIR = path.join(__dirname, 'results');
const REPORTS_DIR = path.join(__dirname, 'reports');

if (!fs.existsSync(REPORTS_DIR)) {
  fs.mkdirSync(REPORTS_DIR, { recursive: true });
}

/**
 * Find all JSON result files
 */
function findResultFiles() {
  if (!fs.existsSync(RESULTS_DIR)) {
    console.error('❌ Results directory not found!');
    console.log('Please run the tests first using run-all-tests.sh or run-all-tests.ps1');
    process.exit(1);
  }

  const files = fs.readdirSync(RESULTS_DIR).filter(f => f.endsWith('.json'));

  if (files.length === 0) {
    console.error('❌ No result files found!');
    console.log('Please run the tests first using run-all-tests.sh or run-all-tests.ps1');
    process.exit(1);
  }

  return files.map(f => path.join(RESULTS_DIR, f));
}

/**
 * Parse and categorize result files
 */
function categorizeResults(files) {
  const results = {
    activeRecord: {},
    repository: {}
  };

  files.forEach(file => {
    const filename = path.basename(file);
    const data = JSON.parse(fs.readFileSync(file, 'utf8'));

    // Determine implementation and scenario
    let implementation, scenario;

    if (filename.includes('Active-Record')) {
      implementation = 'activeRecord';
    } else if (filename.includes('Repository-UoW')) {
      implementation = 'repository';
    } else {
      return; // Skip unknown files
    }

    // Extract scenario name
    if (filename.includes('smoke-test')) {
      scenario = 'smoke';
    } else if (filename.includes('load-test')) {
      scenario = 'load';
    } else if (filename.includes('stress-test')) {
      scenario = 'stress';
    } else if (filename.includes('scalability-data')) {
      scenario = 'scalabilityData';
    } else if (filename.includes('scalability-users')) {
      scenario = 'scalabilityUsers';
    } else if (filename.includes('soak-test')) {
      scenario = 'soak';
    } else {
      return; // Skip unknown scenarios
    }

    results[implementation][scenario] = data;
  });

  return results;
}

/**
 * Extract key metrics from test data
 */
function extractMetrics(data) {
  const metrics = {};

  if (data.metrics) {
    // HTTP request duration
    if (data.metrics.http_req_duration && data.metrics.http_req_duration.values) {
      const vals = data.metrics.http_req_duration.values;
      metrics.httpDuration = {
        avg: vals.avg,
        median: vals.med,
        p90: vals['p(90)'],
        p95: vals['p(95)'],
        p99: vals['p(99)'],
        max: vals.max
      };
    }

    // Throughput
    if (data.metrics.http_reqs && data.metrics.http_reqs.values) {
      metrics.throughput = {
        count: data.metrics.http_reqs.values.count,
        rate: data.metrics.http_reqs.values.rate
      };
    }

    // Error rate
    if (data.metrics.http_req_failed && data.metrics.http_req_failed.values) {
      metrics.errorRate = data.metrics.http_req_failed.values.rate * 100;
      metrics.failedCount = data.metrics.http_req_failed.values.passes || 0;
    }

    // Operation-specific metrics
    const operations = ['creation', 'retrieval', 'update', 'deletion', 'list'];
    operations.forEach(op => {
      const key = `document_${op}_time`;
      if (data.metrics[key] && data.metrics[key].values) {
        const vals = data.metrics[key].values;
        metrics[op] = {
          avg: vals.avg,
          p95: vals['p(95)'],
          p99: vals['p(99)']
        };
      }
    });
  }

  return metrics;
}

/**
 * Calculate percentage difference
 */
function isNumber(value) {
  return typeof value === 'number' && Number.isFinite(value);
}

/**
 * Calculate percentage difference (positive means Repository is higher than Active Record)
 */
function calculateDifference(ar, repo) {
  if (!isNumber(ar) || !isNumber(repo) || ar === 0) return 'N/A';
  return (((repo - ar) / ar) * 100).toFixed(2);
}

/**
 * Determine winner
 * - lowerIsBetter=true: smaller value wins (e.g., latency, error rate)
 * - lowerIsBetter=false: larger value wins (e.g., throughput)
 */
function determineWinner(ar, repo, lowerIsBetter = true) {
  if (!isNumber(ar) || !isNumber(repo)) return 'N/A';

  if (lowerIsBetter) {
    return ar < repo ? 'Active Record' : 'Repository';
  }
  return ar > repo ? 'Active Record' : 'Repository';
}

function formatNumber(value, decimals = 2, suffix = '') {
  return isNumber(value) ? `${value.toFixed(decimals)}${suffix}` : 'N/A';
}

/**
 * Generate comparison table
 */
function generateComparisonTable(arMetrics, repoMetrics, scenarioName) {
  let table = `\n## ${scenarioName}\n\n`;
  table += '| Metric | Active Record | Repository + UoW | Difference | Winner |\n';
  table += '|--------|--------------|------------------|------------|--------|\n';

  if (arMetrics.httpDuration && repoMetrics.httpDuration) {
    const ar = arMetrics.httpDuration;
    const repo = repoMetrics.httpDuration;

    table += `| Avg Response Time | ${formatNumber(ar.avg, 2, 'ms')} | ${formatNumber(repo.avg, 2, 'ms')} | ${calculateDifference(ar.avg, repo.avg)}% | ${determineWinner(ar.avg, repo.avg)} |\n`;
    table += `| P95 Response Time | ${formatNumber(ar.p95, 2, 'ms')} | ${formatNumber(repo.p95, 2, 'ms')} | ${calculateDifference(ar.p95, repo.p95)}% | ${determineWinner(ar.p95, repo.p95)} |\n`;
    table += `| P99 Response Time | ${formatNumber(ar.p99, 2, 'ms')} | ${formatNumber(repo.p99, 2, 'ms')} | ${calculateDifference(ar.p99, repo.p99)}% | ${determineWinner(ar.p99, repo.p99)} |\n`;
  }

  if (arMetrics.throughput && repoMetrics.throughput) {
    const ar = arMetrics.throughput;
    const repo = repoMetrics.throughput;

    table += `| Throughput | ${formatNumber(ar.rate, 2, ' req/s')} | ${formatNumber(repo.rate, 2, ' req/s')} | ${calculateDifference(ar.rate, repo.rate)}% | ${determineWinner(ar.rate, repo.rate, false)} |\n`;
    table += `| Total Requests | ${isNumber(ar.count) ? ar.count : 'N/A'} | ${isNumber(repo.count) ? repo.count : 'N/A'} | ${calculateDifference(ar.count, repo.count)}% | - |\n`;
  }

  // Error rate
  if (arMetrics.errorRate !== undefined || repoMetrics.errorRate !== undefined) {
    table += `| Error Rate | ${formatNumber(arMetrics.errorRate, 2, '%')} | ${formatNumber(repoMetrics.errorRate, 2, '%')} | ${calculateDifference(arMetrics.errorRate, repoMetrics.errorRate)}% | ${determineWinner(arMetrics.errorRate, repoMetrics.errorRate)} |\n`;
  }

  return table;
}


/**
 * Generate full comparison report
 */
function generateReport(results) {
  const timestamp = new Date().toISOString().replace(/[:.]/g, '-').substring(0, 19);
  const reportFile = path.join(REPORTS_DIR, `comparison-report-${timestamp}.md`);

  let report = '# DocuStore Performance Comparison Report\n\n';
  report += `**Active Record vs Repository + Unit of Work**\n\n`;
  report += `Generated: ${new Date().toISOString()}\n\n`;
  report += '---\n\n';

  report += '## Executive Summary\n\n';
  report += 'This report compares the performance of two architectural patterns:\n';
  report += '- **Active Record**: Business logic embedded in domain models\n';
  report += '- **Repository + Unit of Work**: Separated persistence layer with explicit transactions\n\n';

  // Process each scenario
  const scenarios = [
    { key: 'smoke', name: 'Smoke Test (Baseline CRUD)' },
    { key: 'load', name: 'Load Test (50 Users, 10 Minutes)' },
    { key: 'stress', name: 'Stress Test (10-200 Users)' },
    { key: 'scalabilityData', name: 'Data Volume Scalability' },
    { key: 'scalabilityUsers', name: 'User Concurrency Scalability' },
    { key: 'soak', name: 'Soak Test (2 Hours)' }
  ];

  scenarios.forEach(scenario => {
    if (results.activeRecord[scenario.key] && results.repository[scenario.key]) {
      const arMetrics = extractMetrics(results.activeRecord[scenario.key]);
      const repoMetrics = extractMetrics(results.repository[scenario.key]);

      report += generateComparisonTable(arMetrics, repoMetrics, scenario.name);
      report += '\n';
    }
  });

  // Overall conclusions
  report += '## Overall Conclusions\n\n';
  report += '_Note: Specific conclusions depend on the actual test results._\n\n';
  report += '### Key Findings\n\n';
  report += '- Performance differences observed across various load scenarios\n';
  report += '- Scalability characteristics analyzed for both implementations\n';
  report += '- Stability and endurance evaluated over extended periods\n\n';

  report += '---\n\n';
  report += '## Appendix: Test Configuration\n\n';
  report += '### Test Scenarios\n\n';
  report += '1. **Smoke Test**: 5 users, 2 minutes, baseline CRUD operations\n';
  report += '2. **Load Test**: 50 users, 10 minutes, mixed workload (60% read, 25% create, 10% update, 5% delete)\n';
  report += '3. **Stress Test**: Ramp 10→200 users over 15 minutes\n';
  report += '4. **Data Volume Scalability**: 100→1K→10K documents, 20 users\n';
  report += '5. **User Concurrency Scalability**: 5→200 users in stages, 2K documents\n';
  report += '6. **Soak Test**: 30 users, 2 hours\n\n';

  report += '### Environment\n\n';
  report += '- Active Record: http://localhost:8080\n';
  report += '- Repository + UoW: http://localhost:8082\n';
  report += '- Database: PostgreSQL 16\n';
  report += '- Runtime: .NET 10\n';

  // Write report
  fs.writeFileSync(reportFile, report);
  console.log(`\nComparison report generated: ${reportFile}`);

  return reportFile;
}

/**
 * Generate CSV summary
 */
function generateCSVSummary(results) {
  const timestamp = new Date().toISOString().replace(/[:.]/g, '-').substring(0, 19);
  const csvFile = path.join(REPORTS_DIR, `comparison-summary-${timestamp}.csv`);

  let csv = 'Scenario,Metric,ActiveRecord,Repository,Difference(%),Winner\n';

  const scenarios = [
    { key: 'smoke', name: 'Smoke' },
    { key: 'load', name: 'Load' },
    { key: 'stress', name: 'Stress' },
    { key: 'scalabilityData', name: 'ScalabilityData' },
    { key: 'scalabilityUsers', name: 'ScalabilityUsers' },
    { key: 'soak', name: 'Soak' }
  ];

  scenarios.forEach(scenario => {
    if (results.activeRecord[scenario.key] && results.repository[scenario.key]) {
      const arMetrics = extractMetrics(results.activeRecord[scenario.key]);
      const repoMetrics = extractMetrics(results.repository[scenario.key]);

      if (arMetrics.httpDuration && repoMetrics.httpDuration) {
        const ar = arMetrics.httpDuration.avg;
        const repo = repoMetrics.httpDuration.avg;
        csv += `${scenario.name},AvgResponseTime,${isNumber(ar) ? ar.toFixed(2) : 'N/A'},${isNumber(repo) ? repo.toFixed(2) : 'N/A'},${calculateDifference(ar, repo)},${determineWinner(ar, repo)}\n`;
      }

      if (arMetrics.throughput && repoMetrics.throughput) {
        const ar = arMetrics.throughput.rate;
        const repo = repoMetrics.throughput.rate;
        csv += `${scenario.name},Throughput,${isNumber(ar) ? ar.toFixed(2) : 'N/A'},${isNumber(repo) ? repo.toFixed(2) : 'N/A'},${calculateDifference(ar, repo)},${determineWinner(ar, repo, false)}\n`;
      }
    }
  });

  fs.writeFileSync(csvFile, csv);
  console.log(`CSV summary generated: ${csvFile}`);

  return csvFile;
}


/**
 * Main function
 */
function main() {
  console.log('\n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━');
  console.log('📊 Performance Test Results Analysis');
  console.log('━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n');

  // Find and categorize result files
  console.log('🔍 Searching for result files...');
  const files = findResultFiles();
  console.log(`Found ${files.length} result file(s)\n`);

  console.log('📁 Categorizing results...');
  const results = categorizeResults(files);

  const arCount = Object.keys(results.activeRecord).length;
  const repoCount = Object.keys(results.repository).length;

  console.log(`   Active Record scenarios: ${arCount}`);
  console.log(`   Repository + UoW scenarios: ${repoCount}\n`);

  if (arCount === 0 || repoCount === 0) {
    console.log('⚠️  Warning: Results found for only one implementation');
    console.log('   Run tests for both implementations to generate comparison\n');
  }

  // Generate reports
  console.log('Generating comparison reports...');
  const reportFile = generateReport(results);
  const csvFile = generateCSVSummary(results);

  console.log('\n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━');
  console.log('Analysis Complete!');
  console.log('━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n');
  console.log(`View the markdown report: ${reportFile}`);
  console.log(`📊 View the CSV summary: ${csvFile}\n`);
}

if (require.main === module) {
  main();
}

module.exports = {
  findResultFiles,
  categorizeResults,
  extractMetrics,
  generateReport,
  generateCSVSummary
};