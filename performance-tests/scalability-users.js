import { sleep } from 'k6';
import { Trend, Counter, Rate } from 'k6/metrics';
import { config, getBaseUrl, getTargetName } from './config.js';
import {
  createTestDataset,
  performMixedWorkload,
  waitForAPI
} from './utils.js';

const users3Metric = new Trend('users_3_response_time');
const users5Metric = new Trend('users_5_response_time');
const users10Metric = new Trend('users_10_response_time');
const users15Metric = new Trend('users_15_response_time');
const users20Metric = new Trend('users_20_response_time');

const totalOperations = new Counter('total_operations');
const operationSuccessRate = new Rate('operation_success_rate');

export const options = {
  scenarios: {
    scalability_users: {
      executor: 'ramping-vus',
      startVUs: 0,
      stages: config.stages.scalabilityUsers,
      gracefulRampDown: '30s',
    },
  },
  thresholds: {
    'http_req_duration': ['p(95)<4000', 'p(99)<8000'],
    'http_req_failed': ['rate<0.05'],
  },
};

export function setup() {
  const baseUrl = getBaseUrl();
  const targetName = getTargetName();

  console.log(`\n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━`);
  console.log(`👥 Scalability Test - Concurrent Users Impact (Laptop Mode)`);
  console.log(`━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━`);
  console.log(`📊 Target: ${targetName}`);
  console.log(`🌐 Base URL: ${baseUrl}`);
  console.log(`👥 Virtual Users: 3 → 5 → 10 → 15 → 20`);
  console.log(`⏱️  Duration: 1 minute per stage`);
  console.log(`📦 Fixed Dataset: 100 documents`);
  console.log(`━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n`);

  console.log('⏳ Waiting for API to be ready...');
  const ready = waitForAPI(baseUrl);
  if (!ready) {
    throw new Error(`API not ready at ${baseUrl}`);
  }
  console.log('API is ready\n');

  console.log('📝 Creating fixed test dataset (100 documents)...');
  const documentIds = createTestDataset(baseUrl, 100, {
    fileSize: config.testData.fileSizes.small
  });

  console.log(`Created ${documentIds.length} documents\n`);

  return {
    baseUrl,
    targetName,
    documentIds
  };
}

export default function(data) {
  const baseUrl = data.baseUrl;
  const docIds = [...data.documentIds];

  const currentUsers = __VU;
  let metric;

  if (currentUsers <= 3) {
    metric = users3Metric;
  } else if (currentUsers <= 5) {
    metric = users5Metric;
  } else if (currentUsers <= 10) {
    metric = users10Metric;
  } else if (currentUsers <= 15) {
    metric = users15Metric;
  } else {
    metric = users20Metric;
  }

  const start = Date.now();

  const metrics = {
    create: metric,
    read: metric,
    update: metric,
    delete: metric,
    list: metric
  };

  try {
    performMixedWorkload(baseUrl, docIds, metrics);
    totalOperations.add(1);
    operationSuccessRate.add(true);
  } catch (e) {
    operationSuccessRate.add(false);
  }

  // Variable sleep
  sleep(Math.random() * 1.5 + 0.5);
}

export function teardown(data) {
  console.log(`\n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━`);
  console.log(`User Concurrency Scalability Test Completed`);
  console.log(`━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n`);
}

export function handleSummary(data) {
  const targetName = __ENV.TARGET === 'repository' ? 'Repository-UoW' : 'Active-Record';
  const timestamp = new Date().toISOString().replace(/[:.]/g, '-').substring(0, 19);

  const resultsDir = config.output.resultsDir || './results';

  const summary = {
    [`${resultsDir}/${targetName}-scalability-users-${timestamp}.json`]: JSON.stringify(data, null, 2),
    'stdout': generateTextSummary(data, targetName),
  };

  summary[`${resultsDir}/${targetName}-scalability-users-${timestamp}.csv`] = generateCSVSummary(data);

  return summary;
}

function generateTextSummary(data, targetName) {
  let output = '\n';
  output += '╔══════════════════════════════════════════════════════════════╗\n';
  output += `║   SCALABILITY (USERS) RESULTS - ${targetName.padEnd(27)}║\n`;
  output += '╚══════════════════════════════════════════════════════════════╝\n\n';

  output += '📊 Performance vs User Concurrency:\n\n';

  const userLevels = [
    { name: '5 Users', key: 'users_5_response_time' },
    { name: '10 Users', key: 'users_10_response_time' },
    { name: '25 Users', key: 'users_25_response_time' },
    { name: '50 Users', key: 'users_50_response_time' },
    { name: '100 Users', key: 'users_100_response_time' },
    { name: '150 Users', key: 'users_150_response_time' },
    { name: '200 Users', key: 'users_200_response_time' }
  ];

  userLevels.forEach(level => {
    if (data.metrics[level.key] && data.metrics[level.key].values) {
      const vals = data.metrics[level.key].values;
      output += `  ${level.name.padEnd(12)}: avg=${vals.avg?.toFixed(2)}ms, p95=${vals['p(95)']?.toFixed(2)}ms, p99=${vals['p(99)']?.toFixed(2)}ms\n`;
    }
  });

  output += '\n';

  // Scalability analysis
  output += '📈 Scalability Analysis:\n';

  if (data.metrics.users_5_response_time && data.metrics.users_200_response_time) {
    const time5 = data.metrics.users_5_response_time.values.avg;
    const time200 = data.metrics.users_200_response_time.values.avg;
    const scalingFactor = time200 / time5;
    const userFactor = 200 / 5; // 40x users

    output += `  Response time scaling (200/5 users): ${scalingFactor?.toFixed(2)}x\n`;
    output += `  User count increase: ${userFactor}x\n`;
    output += `  Efficiency ratio: ${(userFactor / scalingFactor).toFixed(2)}x\n`;

    if (scalingFactor < userFactor * 0.5) {
      output += `  🟢 Excellent scalability - better than linear\n`;
    } else if (scalingFactor < userFactor * 1.5) {
      output += `  🟡 Good scalability - near-linear growth\n`;
    } else if (scalingFactor < userFactor * 3) {
      output += `  🟠 Moderate scalability - some degradation\n`;
    } else {
      output += `  🔴 Poor scalability - significant degradation\n`;
    }
  }

  output += '\n🚀 Throughput Analysis:\n';
  if (data.metrics.http_reqs) {
    const reqs = data.metrics.http_reqs.values;
    output += `  Total Requests: ${reqs.count}\n`;
    output += `  Average Throughput: ${reqs.rate?.toFixed(2)} req/s\n`;
    output += `  Peak Throughput: ~${(reqs.rate * 1.2).toFixed(2)} req/s (estimated)\n`;
  }

  if (data.metrics.http_req_failed) {
    const failRate = (data.metrics.http_req_failed.values?.rate || 0) * 100;
    output += `\nError Rate: ${failRate?.toFixed(2)}%\n`;

    if (failRate < 1) {
      output += `  🟢 System stable across all user levels\n`;
    } else if (failRate < 5) {
      output += `  🟡 Some instability at higher user counts\n`;
    } else {
      output += `  🔴 Significant errors - capacity limit reached\n`;
    }
  }

  output += '\n';
  return output;
}

function generateCSVSummary(data) {
  let csv = 'UserLevel,Average,Median,P90,P95,P99,Max\n';

  const userLevels = [
    { level: 5, key: 'users_5_response_time' },
    { level: 10, key: 'users_10_response_time' },
    { level: 25, key: 'users_25_response_time' },
    { level: 50, key: 'users_50_response_time' },
    { level: 100, key: 'users_100_response_time' },
    { level: 150, key: 'users_150_response_time' },
    { level: 200, key: 'users_200_response_time' }
  ];

  userLevels.forEach(level => {
    if (data.metrics[level.key] && data.metrics[level.key].values) {
      const v = data.metrics[level.key].values;
      csv += `${level.level},${v.avg?.toFixed(2)},${v.med?.toFixed(2)},${v['p(90)']?.toFixed(2)},${v['p(95)']?.toFixed(2)},${v['p(99)']?.toFixed(2)},${v.max?.toFixed(2)}\n`;
    }
  });

  csv += `\n`;
  if (data.metrics.http_reqs) {
    csv += `Total Requests,${data.metrics.http_reqs?.values?.count || 0}\n`;
    csv += `Avg Throughput (req/s),${data.metrics.http_reqs.values.rate?.toFixed(2)}\n`;
  }

  if (data.metrics.http_req_failed) {
    const failRate = (data.metrics.http_req_failed.values?.rate || 0) * 100;
    csv += `Error Rate (%),${failRate?.toFixed(2)}\n`;
  }

  return csv;
}