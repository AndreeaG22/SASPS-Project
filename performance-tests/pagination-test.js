import { sleep } from 'k6';
import { Trend, Counter } from 'k6/metrics';
import { config, getBaseUrl, getTargetName } from './config.js';
import {
  createTestDataset,
  listDocuments,
  getDocument,
  waitForAPI
} from './utils.js';
import { randomItem } from 'https://jslib.k6.io/k6-utils/1.2.0/index.js';

const list50Time = new Trend('list_50_response_time');
const list100Time = new Trend('list_100_response_time');
const list250Time = new Trend('list_250_response_time');
const list500Time = new Trend('list_500_response_time');

const totalListOps = new Counter('total_list_operations');
const totalReadOps = new Counter('total_read_operations');

export const options = {
  scenarios: {
    phase_50: {
      executor: 'constant-vus',
      vus: 5,
      duration: '1m30s',
      startTime: '0s',
      tags: { phase: '50' },
    },
    phase_100: {
      executor: 'constant-vus',
      vus: 5,
      duration: '1m30s',
      startTime: '2m',
      tags: { phase: '100' },
    },
    phase_250: {
      executor: 'constant-vus',
      vus: 5,
      duration: '1m30s',
      startTime: '4m',
      tags: { phase: '250' },
    },
    phase_500: {
      executor: 'constant-vus',
      vus: 5,
      duration: '1m30s',
      startTime: '6m',
      tags: { phase: '500' },
    },
  },
  thresholds: {
    'http_req_duration': ['p(95)<5000'],
    'http_req_failed': ['rate<0.01'],
  },
};

export function setup() {
  const baseUrl = getBaseUrl();
  const targetName = getTargetName();

  console.log(`\n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━`);
  console.log(` Pagination & List Performance Test (Laptop Mode)`);
  console.log(`━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━`);
  console.log(`📊 Target: ${targetName}`);
  console.log(`🌐 Base URL: ${baseUrl}`);
  console.log(`👥 Virtual Users: 5 (per phase)`);
  console.log(`⏱️  Duration: 1.5 minutes per phase`);
  console.log(`📦 Phases: 50 → 100 → 250 → 500 documents`);
  console.log(`━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n`);

  console.log('⏳ Waiting for API to be ready...');
  const ready = waitForAPI(baseUrl);
  if (!ready) {
    throw new Error(`API not ready at ${baseUrl}`);
  }
  console.log('API is ready\n');

  console.log('Phase 1: Creating 50 documents...');
  const docs50 = createTestDataset(baseUrl, 50, {
    fileSize: config.testData.fileSizes.small
  });
  console.log(`Phase 1: Created ${docs50.length} documents\n`);
  sleep(2);

  console.log('Phase 2: Creating 50 additional documents (total: 100)...');
  const docs50more = createTestDataset(baseUrl, 50, {
    fileSize: config.testData.fileSizes.small
  });
  const docs100 = [...docs50, ...docs50more];
  console.log(`Phase 2: Total ${docs100.length} documents\n`);
  sleep(2);

  console.log('Phase 3: Creating 150 additional documents (total: 250)...');
  const docs150 = createTestDataset(baseUrl, 150, {
    fileSize: config.testData.fileSizes.small
  });
  const docs250 = [...docs100, ...docs150];
  console.log(`Phase 3: Total ${docs250.length} documents\n`);
  sleep(2);

  console.log('Phase 4: Creating 250 additional documents (total: 500)...');
  const docs250more = createTestDataset(baseUrl, 250, {
    fileSize: config.testData.fileSizes.small
  });
  const docs500 = [...docs250, ...docs250more];
  console.log(`Phase 4: Total ${docs500.length} documents\n`);

  return {
    baseUrl,
    targetName,
    docs50,
    docs100,
    docs250,
    docs500
  };
}

export default function(data) {
  const baseUrl = data.baseUrl;
  const scenario = __ENV.SCENARIO_NAME || 'unknown';

  let docIds, metric;

  // Select appropriate dataset and metrics based on scenario
  if (scenario === 'phase_50') {
    docIds = data.docs50;
    metric = list50Time;
  } else if (scenario === 'phase_100') {
    docIds = data.docs100;
    metric = list100Time;
  } else if (scenario === 'phase_250') {
    docIds = data.docs250;
    metric = list250Time;
  } else if (scenario === 'phase_500') {
    docIds = data.docs500;
    metric = list500Time;
  } else {
    // Fallback based on iteration
    const elapsed = __ITER * __VU;
    if (elapsed < 200) {
      docIds = data.docs50;
      metric = list50Time;
    } else if (elapsed < 500) {
      docIds = data.docs100;
      metric = list100Time;
    } else if (elapsed < 1000) {
      docIds = data.docs250;
      metric = list250Time;
    } else {
      docIds = data.docs500;
      metric = list500Time;
    }
  }

  // 80% list operations, 20% individual reads
  if (Math.random() < 0.8) {
    // List operation
    const start = Date.now();
    listDocuments(baseUrl);
    if (metric) metric.add(Date.now() - start);
    totalListOps.add(1);
  } else {
    // Individual read
    if (docIds && docIds.length > 0) {
      const docId = randomItem(docIds);
      getDocument(baseUrl, docId);
      totalReadOps.add(1);
    }
  }

  sleep(Math.random() * 1 + 0.3);
}

// Teardown function
export function teardown(data) {
  console.log(`\n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━`);
  console.log(`Pagination Performance Test Completed`);
  console.log(`━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n`);
}

// Custom summary handler
export function handleSummary(data) {
  const targetName = __ENV.TARGET === 'repository' ? 'Repository-UoW' : 'Active-Record';
  const timestamp = new Date().toISOString().replace(/[:.]/g, '-').substring(0, 19);

  const resultsDir = config.output.resultsDir || './results';

  const summary = {
    [`${resultsDir}/${targetName}-pagination-test-${timestamp}.json`]: JSON.stringify(data, null, 2),
    'stdout': generateTextSummary(data, targetName),
  };

  summary[`${resultsDir}/${targetName}-pagination-test-${timestamp}.csv`] = generateCSVSummary(data);

  return summary;
}

function generateTextSummary(data, targetName) {
  let output = '\n';
  output += '╔══════════════════════════════════════════════════════════════╗\n';
  output += `║   PAGINATION PERFORMANCE - ${targetName.padEnd(30)}║\n`;
  output += '╚══════════════════════════════════════════════════════════════╝\n\n';

  output += '📊 List Performance by Dataset Size:\n\n';

  // Phase comparison
  const phases = [
    { name: 'Phase 1 (1K docs)', key: 'list_1k_response_time' },
    { name: 'Phase 2 (5K docs)', key: 'list_5k_response_time' },
    { name: 'Phase 3 (10K docs)', key: 'list_10k_response_time' },
    { name: 'Phase 4 (20K docs)', key: 'list_20k_response_time' }
  ];

  phases.forEach(phase => {
    if (data.metrics[phase.key] && data.metrics[phase.key].values) {
      const vals = data.metrics[phase.key].values;
      output += `  ${phase.name}:\n`;
      output += `    avg=${vals.avg?.toFixed(2)}ms, p95=${vals['p(95)']?.toFixed(2)}ms, p99=${vals['p(99)']?.toFixed(2)}ms\n\n`;
    }
  });

  output += '📈 Scalability Analysis:\n';

  if (data.metrics.list_1k_response_time && data.metrics.list_20k_response_time) {
    const list1k = data.metrics.list_1k_response_time.values.avg;
    const list20k = data.metrics.list_20k_response_time.values.avg;
    const scalingFactor = list20k / list1k;
    const dataFactor = 20; // 20x more data

    output += `  Response time scaling (20K/1K): ${scalingFactor?.toFixed(2)}x\n`;
    output += `  Data volume increase: ${dataFactor}x\n`;
    output += `  Efficiency: ${(dataFactor / scalingFactor).toFixed(2)}x\n`;

    if (scalingFactor < dataFactor * 0.3) {
      output += `  🟢 Excellent scalability - sublinear growth\n`;
    } else if (scalingFactor < dataFactor * 0.7) {
      output += `  🟡 Good scalability - near-sublinear\n`;
    } else if (scalingFactor < dataFactor) {
      output += `  🟠 Moderate scalability - linear growth\n`;
    } else {
      output += `  🔴 Poor scalability - superlinear growth\n`;
    }
  }

  // Overall stats
  output += '\n📊 Overall Statistics:\n';
  if (data.metrics.total_list_operations) {
    output += `  Total List Operations: ${data.metrics.total_list_operations?.values?.count || 0}\n`;
  }
  if (data.metrics.total_read_operations) {
    output += `  Total Read Operations: ${data.metrics.total_read_operations?.values?.count || 0}\n`;
  }

  if (data.metrics.http_req_failed) {
    const failRate = (data.metrics.http_req_failed.values?.rate || 0) * 100;
    output += `  Error Rate: ${failRate?.toFixed(2)}%\n`;
  }

  output += '\n';
  return output;
}

function generateCSVSummary(data) {
  let csv = 'Phase,DocumentCount,Average_ms,Median_ms,P95_ms,P99_ms,Max_ms\n';

  const phases = [
    { name: '1000', count: 1000, key: 'list_1k_response_time' },
    { name: '5000', count: 5000, key: 'list_5k_response_time' },
    { name: '10000', count: 10000, key: 'list_10k_response_time' },
    { name: '20000', count: 20000, key: 'list_20k_response_time' }
  ];

  phases.forEach(phase => {
    if (data.metrics[phase.key] && data.metrics[phase.key].values) {
      const v = data.metrics[phase.key].values;
      csv += `Phase ${phase.name},${phase.count},${v.avg?.toFixed(2)},${v.med?.toFixed(2)},${v['p(95)']?.toFixed(2)},${v['p(99)']?.toFixed(2)},${v.max?.toFixed(2)}\n`;
    }
  });

  return csv;
}