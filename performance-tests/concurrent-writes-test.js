import { sleep } from 'k6';
import { Trend, Counter, Rate } from 'k6/metrics';
import { config, getBaseUrl, getTargetName } from './config.js';
import {
  createDocument,
  updateDocument,
  waitForAPI
} from './utils.js';
import { randomItem } from 'https://jslib.k6.io/k6-utils/1.2.0/index.js';

// Custom metrics
const writeOperationTime = new Trend('write_operation_time');
const createSuccessRate = new Rate('create_success_rate');
const updateSuccessRate = new Rate('update_success_rate');
const totalWrites = new Counter('total_write_operations');
const failedWrites = new Counter('failed_write_operations');

// Test configuration (Laptop-friendly)
export const options = {
  scenarios: {
    concurrent_writes: {
      executor: 'constant-vus',
      vus: 10,
      duration: '2m',
    },
  },
  thresholds: {
    'http_req_duration': ['p(95)<3000', 'p(99)<5000'],
    'http_req_failed': ['rate<0.02'],
    'create_success_rate': ['rate>0.98'],
    'write_operation_time': ['avg<1500', 'p(95)<3000'],
  },
};

// Shared array to store created document IDs
let documentIds = [];

// Setup function
export function setup() {
  const baseUrl = getBaseUrl();
  const targetName = getTargetName();

  console.log(`\n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━`);
  console.log(` Concurrent Writes Performance Test (Laptop Mode)`);
  console.log(`━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━`);
  console.log(`📊 Target: ${targetName}`);
  console.log(`🌐 Base URL: ${baseUrl}`);
  console.log(`👥 Virtual Users: 10 (constant)`);
  console.log(`⏱️  Duration: 2 minutes`);
  console.log(`📝 Workload: 90% Create, 10% Update (Write-Heavy)`);
  console.log(`━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n`);

  // Wait for API to be ready
  console.log('⏳ Waiting for API to be ready...');
  const ready = waitForAPI(baseUrl);
  if (!ready) {
    throw new Error(`API not ready at ${baseUrl}`);
  }
  console.log('API is ready\n');

  return {
    baseUrl,
    targetName,
    documentIds: []
  };
}

// Main test function
export default function(data) {
  const baseUrl = data.baseUrl;

  // 90% create, 10% update
  const rand = Math.random();

  if (rand < 0.9) {
    // Create operation with variable file sizes
    const fileSizes = Object.values(config.testData.fileSizes);
    const fileSize = randomItem(fileSizes);

    const start = Date.now();
    const resp = createDocument(baseUrl, { fileSize });
    const duration = Date.now() - start;

    writeOperationTime.add(duration);
    totalWrites.add(1);

    if (resp.status === 201) {
      createSuccessRate.add(true);
      try {
        const newId = JSON.parse(resp.body).id;
        documentIds.push(newId);
      } catch (e) {
        // Ignore parse errors
      }
    } else {
      createSuccessRate.add(false);
      failedWrites.add(1);
    }
  } else {
    // Update operation
    if (documentIds.length > 0) {
      const docId = randomItem(documentIds);
      const start = Date.now();
      const resp = updateDocument(baseUrl, docId);
      const duration = Date.now() - start;

      writeOperationTime.add(duration);
      totalWrites.add(1);

      if (resp.status === 200) {
        updateSuccessRate.add(true);
      } else {
        updateSuccessRate.add(false);
        failedWrites.add(1);
      }
    }
  }

  // Small delay to simulate realistic load
  sleep(Math.random() * 0.5 + 0.2);
}

// Teardown function
export function teardown(data) {
  console.log(`\n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━`);
  console.log(`Concurrent Writes Test Completed`);
  console.log(`━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n`);
}

// Custom summary handler
export function handleSummary(data) {
  const targetName = __ENV.TARGET === 'repository' ? 'Repository-UoW' : 'Active-Record';
  const timestamp = new Date().toISOString().replace(/[:.]/g, '-').substring(0, 19);

  const resultsDir = config.output.resultsDir || './results';

  const summary = {
    [`${resultsDir}/${targetName}-concurrent-writes-${timestamp}.json`]: JSON.stringify(data, null, 2),
    'stdout': generateTextSummary(data, targetName),
  };

  summary[`${resultsDir}/${targetName}-concurrent-writes-${timestamp}.csv`] = generateCSVSummary(data);

  return summary;
}

function generateTextSummary(data, targetName) {
  let output = '\n';
  output += '╔══════════════════════════════════════════════════════════════╗\n';
  output += `║   CONCURRENT WRITES TEST - ${targetName.padEnd(30)}║\n`;
  output += '╚══════════════════════════════════════════════════════════════╝\n\n';

  // Write operation metrics
  if (data.metrics.write_operation_time) {
    const writeTime = data.metrics.write_operation_time.values;
    output += '📝 Write Operation Performance:\n';
    output += `   Average: ${writeTime.avg?.toFixed(2)} ms\n`;
    output += `   Median:  ${writeTime.med?.toFixed(2)} ms\n`;
    output += `   P95:     ${writeTime['p(95)']?.toFixed(2)} ms\n`;
    output += `   P99:     ${writeTime['p(99)']?.toFixed(2)} ms\n`;
    output += `   Max:     ${writeTime.max?.toFixed(2)} ms\n\n`;
  }

  // Throughput
  if (data.metrics.total_write_operations) {
    const totalWrites = data.metrics.total_write_operations?.values?.count || 0
    const duration = (data.state.testRunDurationMs || 300000) / 1000;
    const writesPerSec = totalWrites / duration;

    output += '   Write Throughput:\n';
    output += `   Total Writes: ${totalWrites}\n`;
    output += `   Writes/sec:   ${writesPerSec?.toFixed(2)}\n\n`;
  }

  // Success rates
  if (data.metrics.create_success_rate) {
    const createRate = (data.metrics.create_success_rate.values?.rate || 0) * 100;
    output += `Create Success Rate: ${createRate?.toFixed(2)}%\n`;
  }

  if (data.metrics.update_success_rate && data.metrics.update_success_rate.values) {
    const updateRate = (data.metrics.update_success_rate.values?.rate || 0) * 100;
    output += `Update Success Rate: ${updateRate?.toFixed(2)}%\n`;
  }

  // Failed writes
  if (data.metrics.failed_write_operations) {
    const failed = data.metrics.failed_write_operations?.values?.count || 0;
    output += `\nFailed Writes: ${failed}\n`;
  }

  // Overall HTTP metrics
  if (data.metrics.http_req_failed) {
    const failRate = (data.metrics.http_req_failed.values?.rate || 0) * 100;
    output += `Overall Error Rate: ${failRate?.toFixed(2)}%\n`;
  }

  output += '\n';
  return output;
}

function generateCSVSummary(data) {
  let csv = 'Metric,Value\n';

  if (data.metrics.write_operation_time) {
    const v = data.metrics.write_operation_time.values;
    csv += `Write_Avg_ms,${v.avg?.toFixed(2)}\n`;
    csv += `Write_P95_ms,${v['p(95)']?.toFixed(2)}\n`;
    csv += `Write_P99_ms,${v['p(99)']?.toFixed(2)}\n`;
  }

  if (data.metrics.total_write_operations) {
    csv += `Total_Writes,${data.metrics.total_write_operations?.values?.count || 0}\n`;
  }

  if (data.metrics.create_success_rate) {
    csv += `Create_Success_Rate,${((data.metrics.create_success_rate.values?.rate || 0) * 100).toFixed(2)}\n`;
  }

  if (data.metrics.http_req_failed) {
    csv += `Error_Rate,${((data.metrics.http_req_failed.values?.rate || 0) * 100).toFixed(2)}\n`;
  }

  return csv;
}