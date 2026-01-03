import { sleep } from 'k6';
import { Trend, Counter } from 'k6/metrics';
import { config, getBaseUrl, getTargetName } from './config.js';
import {
  createTestDataset,
  getDocument,
  listDocuments,
  waitForAPI
} from './utils.js';
import { randomItem } from 'https://jslib.k6.io/k6-utils/1.2.0/index.js';

const phase50ReadTime = new Trend('phase_50_read_time');
const phase50ListTime = new Trend('phase_50_list_time');
const phase100ReadTime = new Trend('phase_100_read_time');
const phase100ListTime = new Trend('phase_100_list_time');
const phase250ReadTime = new Trend('phase_250_read_time');
const phase250ListTime = new Trend('phase_250_list_time');

const totalReads = new Counter('total_read_operations');
const totalLists = new Counter('total_list_operations');

export const options = {
  scenarios: {
    phase_50: {
      executor: 'constant-vus',
      vus: 5,
      duration: '2m',
      startTime: '0s',
      tags: { phase: '50' },
    },
    phase_100: {
      executor: 'constant-vus',
      vus: 5,
      duration: '2m',
      startTime: '2m30s',
      tags: { phase: '100' },
    },
    phase_250: {
      executor: 'constant-vus',
      vus: 5,
      duration: '2m',
      startTime: '5m',
      tags: { phase: '250' },
    },
  },
  thresholds: {
    'http_req_duration': ['p(95)<3000'],
    'http_req_failed': ['rate<0.01'],
  },
};

let currentPhase = '50';

export function setup() {
  const baseUrl = getBaseUrl();
  const targetName = getTargetName();

  console.log(`\n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━`);
  console.log(` Scalability Test - Data Volume Impact (Laptop Mode)`);
  console.log(`━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━`);
  console.log(`📊 Target: ${targetName}`);
  console.log(`🌐 Base URL: ${baseUrl}`);
  console.log(`👥 Virtual Users: 5 (per phase)`);
  console.log(`⏱️  Duration: 2 minutes per phase`);
  console.log(`📦 Phases: 50 → 100 → 250 documents`);
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

  return {
    baseUrl,
    targetName,
    docs50,
    docs100,
    docs250
  };
}

// Main test function
export default function(data) {
  const baseUrl = data.baseUrl;
  const scenario = __ENV.SCENARIO_NAME || 'unknown';

  let docIds;
  let readMetric, listMetric;

  // Select appropriate dataset and metrics based on scenario
  if (scenario === 'phase_50') {
    docIds = data.docs50;
    readMetric = phase50ReadTime;
    listMetric = phase50ListTime;
  } else if (scenario === 'phase_100') {
    docIds = data.docs100;
    readMetric = phase100ReadTime;
    listMetric = phase100ListTime;
  } else if (scenario === 'phase_250') {
    docIds = data.docs250;
    readMetric = phase250ReadTime;
    listMetric = phase250ListTime;
  } else {
    const elapsed = __ITER * __VU;
    if (elapsed < 500) {
      docIds = data.docs50;
      readMetric = phase50ReadTime;
      listMetric = phase50ListTime;
    } else if (elapsed < 1000) {
      docIds = data.docs100;
      readMetric = phase100ReadTime;
      listMetric = phase100ListTime;
    } else {
      docIds = data.docs250;
      readMetric = phase250ReadTime;
      listMetric = phase250ListTime;
    }
  }

  // 70% reads, 30% lists
  if (Math.random() < 0.7 && docIds && docIds.length > 0) {
    // Perform read operation
    const docId = randomItem(docIds);
    const start = Date.now();
    getDocument(baseUrl, docId);
    if (readMetric) readMetric.add(Date.now() - start);
    totalReads.add(1);
  } else {
    // Perform list operation
    const start = Date.now();
    listDocuments(baseUrl);
    if (listMetric) listMetric.add(Date.now() - start);
    totalLists.add(1);
  }

  sleep(Math.random() * 1 + 0.5);
}

export function teardown(data) {
  console.log(`\n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━`);
  console.log(`Data Volume Scalability Test Completed`);
  console.log(`━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n`);
}

export function handleSummary(data) {
  const targetName = __ENV.TARGET === 'repository' ? 'Repository-UoW' : 'Active-Record';
  const timestamp = new Date().toISOString().replace(/[:.]/g, '-').substring(0, 19);

  const resultsDir = config.output.resultsDir || './results';

  const summary = {
    [`${resultsDir}/${targetName}-scalability-data-${timestamp}.json`]: JSON.stringify(data, null, 2),
    'stdout': generateTextSummary(data, targetName),
  };

  summary[`${resultsDir}/${targetName}-scalability-data-${timestamp}.csv`] = generateCSVSummary(data);

  return summary;
}

function generateTextSummary(data, targetName) {
  let output = '\n';
  output += '╔══════════════════════════════════════════════════════════════╗\n';
  output += `║   SCALABILITY (DATA) RESULTS - ${targetName.padEnd(27)}║\n`;
  output += '╚══════════════════════════════════════════════════════════════╝\n\n';

  output += '📊 Performance vs Data Volume:\n\n';

  // Phase comparison
  const phases = [
    { name: 'Phase 1 (100 docs)', readKey: 'phase_100_read_time', listKey: 'phase_100_list_time' },
    { name: 'Phase 2 (1K docs)', readKey: 'phase_1k_read_time', listKey: 'phase_1k_list_time' },
    { name: 'Phase 3 (10K docs)', readKey: 'phase_10k_read_time', listKey: 'phase_10k_list_time' }
  ];

  phases.forEach(phase => {
    output += `  ${phase.name}:\n`;

    if (data.metrics[phase.readKey] && data.metrics[phase.readKey].values) {
      const readVals = data.metrics[phase.readKey].values;
      output += `    Read:  avg=${readVals.avg?.toFixed(2)}ms, p95=${readVals['p(95)']?.toFixed(2)}ms, p99=${readVals['p(99)']?.toFixed(2)}ms\n`;
    }

    if (data.metrics[phase.listKey] && data.metrics[phase.listKey].values) {
      const listVals = data.metrics[phase.listKey].values;
      output += `    List:  avg=${listVals.avg?.toFixed(2)}ms, p95=${listVals['p(95)']?.toFixed(2)}ms, p99=${listVals['p(99)']?.toFixed(2)}ms\n`;
    }

    output += '\n';
  });

  // Scalability analysis
  output += '📈 Scalability Analysis:\n';

  if (data.metrics.phase_100_list_time && data.metrics.phase_10k_list_time) {
    const list100 = data.metrics.phase_100_list_time.values.avg;
    const list10k = data.metrics.phase_10k_list_time.values.avg;
    const scalingFactor = list10k / list100;

    output += `  List operation scaling factor (10K/100): ${scalingFactor?.toFixed(2)}x\n`;

    if (scalingFactor < 2) {
      output += `  🟢 Excellent scalability - sublinear growth\n`;
    } else if (scalingFactor < 5) {
      output += `  🟡 Good scalability - near-linear growth\n`;
    } else if (scalingFactor < 10) {
      output += `  🟠 Moderate scalability - some degradation\n`;
    } else {
      output += `  🔴 Poor scalability - significant degradation\n`;
    }
  }

  output += '\n📊 Overall Statistics:\n';
  if (data.metrics.http_reqs) {
    output += `  Total Requests: ${data.metrics.http_reqs?.values?.count || 0}\n`;
    output += `  Throughput: ${data.metrics.http_reqs.values?.rate?.toFixed(2)} req/s\n`;
  }

  if (data.metrics.http_req_failed) {
    const failRate = (data.metrics.http_req_failed.values?.rate || 0) * 100;
    output += `  Error Rate: ${failRate?.toFixed(2)}%\n`;
  }

  output += '\n';
  return output;
}

function generateCSVSummary(data) {
  let csv = 'Phase,DocumentCount,Operation,Average,Median,P95,P99,Max\n';

  const phases = [
    { name: '100', count: 100, readKey: 'phase_100_read_time', listKey: 'phase_100_list_time' },
    { name: '1000', count: 1000, readKey: 'phase_1k_read_time', listKey: 'phase_1k_list_time' },
    { name: '10000', count: 10000, readKey: 'phase_10k_read_time', listKey: 'phase_10k_list_time' }
  ];

  phases.forEach(phase => {
    if (data.metrics[phase.readKey] && data.metrics[phase.readKey].values) {
      const v = data.metrics[phase.readKey].values;
      csv += `Phase ${phase.name},${phase.count},Read,${v.avg?.toFixed(2)},${v.med?.toFixed(2)},${v['p(95)']?.toFixed(2)},${v['p(99)']?.toFixed(2)},${v.max?.toFixed(2)}\n`;
    }

    if (data.metrics[phase.listKey] && data.metrics[phase.listKey].values) {
      const v = data.metrics[phase.listKey].values;
      csv += `Phase ${phase.name},${phase.count},List,${v.avg?.toFixed(2)},${v.med?.toFixed(2)},${v['p(95)']?.toFixed(2)},${v['p(99)']?.toFixed(2)},${v.max?.toFixed(2)}\n`;
    }
  });

  return csv;
}