import { sleep } from 'k6';
import { Trend, Counter, Rate, Gauge } from 'k6/metrics';
import { config, getBaseUrl, getTargetName } from './config.js';
import {
  createTestDataset,
  performMixedWorkload,
  waitForAPI
} from './utils.js';

const documentCreationTime = new Trend('document_creation_time');
const documentRetrievalTime = new Trend('document_retrieval_time');
const documentUpdateTime = new Trend('document_update_time');
const documentDeletionTime = new Trend('document_deletion_time');
const documentListTime = new Trend('document_list_time');

const earlyPhaseResponseTime = new Trend('early_phase_response_time'); // First 15 min
const midPhaseResponseTime = new Trend('mid_phase_response_time');     // 45-60 min
const latePhaseResponseTime = new Trend('late_phase_response_time');   // Last 15 min

const totalOperations = new Counter('total_operations');
const failedOperations = new Counter('failed_operations');
const operationSuccessRate = new Rate('operation_success_rate');

const currentResponseTime = new Gauge('current_response_time');

const testStartTime = Date.now();

export const options = {
  scenarios: {
    soak_test: {
      executor: 'ramping-vus',
      startVUs: 0,
      stages: config.stages.soak,
      gracefulRampDown: '60s',
    },
  },
  thresholds: {
    'http_req_duration': ['avg<2000', 'p(95)<4000', 'p(99)<8000'],
    'http_req_failed': ['rate<0.02'], // Allow up to 2% failure over long duration
    'operation_success_rate': ['rate>0.98'],
  },
};

export function setup() {
  const baseUrl = getBaseUrl();
  const targetName = getTargetName();

  console.log(`\n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━`);
  console.log(`⏰ Soak Test - Endurance & Stability`);
  console.log(`━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━`);
  console.log(`📊 Target: ${targetName}`);
  console.log(`🌐 Base URL: ${baseUrl}`);
  console.log(`👥 Virtual Users: 30 (constant)`);
  console.log(`⏱️  Duration: 2 hours`);
  console.log(`📦 Initial Dataset: 2,000 documents`);
  console.log(`━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━`);
  console.log(`️  WARNING: This test will run for 2 hours!`);
  console.log(`━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n`);

  console.log('⏳ Waiting for API to be ready...');
  const ready = waitForAPI(baseUrl);
  if (!ready) {
    throw new Error(`API not ready at ${baseUrl}`);
  }
  console.log('API is ready\n');

  console.log('Creating initial test dataset (2,000 documents)...');
  console.log('⏳ This may take several minutes...');

  const documentIds = createTestDataset(baseUrl, 2000, {
    fileSize: config.testData.fileSizes.small
  });

  console.log(`Created ${documentIds.length} initial documents\n`);
  console.log(`🚀 Starting soak test... Please wait 2 hours for completion.\n`);

  return {
    baseUrl,
    targetName,
    documentIds,
    startTime: Date.now()
  };
}

export default function(data) {
  const baseUrl = data.baseUrl;
  const docIds = [...data.documentIds];

  const elapsedMinutes = (Date.now() - data.startTime) / 1000 / 60;

  const operationStart = Date.now();

  const metrics = {
    create: documentCreationTime,
    read: documentRetrievalTime,
    update: documentUpdateTime,
    delete: documentDeletionTime,
    list: documentListTime
  };

  try {
    performMixedWorkload(baseUrl, docIds, metrics);

    const operationTime = Date.now() - operationStart;
    currentResponseTime.add(operationTime);

    // Track metrics by phase for degradation analysis
    if (elapsedMinutes <= 15) {
      // Early phase (first 15 minutes)
      earlyPhaseResponseTime.add(operationTime);
    } else if (elapsedMinutes >= 45 && elapsedMinutes <= 60) {
      // Mid phase (45-60 minutes)
      midPhaseResponseTime.add(operationTime);
    } else if (elapsedMinutes >= 105) {
      // Late phase (last 15 minutes)
      latePhaseResponseTime.add(operationTime);
    }

    totalOperations.add(1);
    operationSuccessRate.add(true);
  } catch (e) {
    failedOperations.add(1);
    operationSuccessRate.add(false);
  }

  // Realistic user think time
  sleep(Math.random() * 2 + 1);
}

export function teardown(data) {
  const durationMinutes = (Date.now() - data.startTime) / 1000 / 60;

  console.log(`\n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━`);
  console.log(`Soak Test Completed`);
  console.log(`   Total Duration: ${durationMinutes?.toFixed(1)} minutes`);
  console.log(`━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n`);
}

export function handleSummary(data) {
  const targetName = __ENV.TARGET === 'repository' ? 'Repository-UoW' : 'Active-Record';
  const timestamp = new Date().toISOString().replace(/[:.]/g, '-').substring(0, 19);

  const resultsDir = config.output.resultsDir || './results';

  const summary = {
    [`${resultsDir}/${targetName}-soak-test-${timestamp}.json`]: JSON.stringify(data, null, 2),
    'stdout': generateTextSummary(data, targetName),
  };

  summary[`${resultsDir}/${targetName}-soak-test-${timestamp}.csv`] = generateCSVSummary(data);

  return summary;
}

function generateTextSummary(data, targetName) {
  let output = '\n';
  output += '╔══════════════════════════════════════════════════════════════╗\n';
  output += `║        SOAK TEST RESULTS - ${targetName.padEnd(32)}║\n`;
  output += '╚══════════════════════════════════════════════════════════════╝\n\n';

  // Overall duration
  if (data.state && data.state.testRunDurationMs) {
    const durationHours = data.state.testRunDurationMs / 1000 / 60 / 60;
    output += `⏱️  Test Duration: ${durationHours?.toFixed(2)} hours\n\n`;
  }

  // Overall HTTP metrics
  if (data.metrics.http_req_duration) {
    const httpDuration = data.metrics.http_req_duration.values;
    output += '📊 Overall HTTP Request Duration:\n';
    output += `   Average: ${httpDuration.avg?.toFixed(2)} ms\n`;
    output += `   Median:  ${httpDuration.med?.toFixed(2)} ms\n`;
    output += `   P95:     ${httpDuration['p(95)']?.toFixed(2)} ms\n`;
    output += `   P99:     ${httpDuration['p(99)']?.toFixed(2)} ms\n`;
    output += `   Max:     ${httpDuration.max?.toFixed(2)} ms\n\n`;
  }

  // Degradation analysis (comparing early vs late phases)
  output += '📉 Degradation Analysis (Early vs Late Phase):\n';

  const earlyPhase = data.metrics.early_phase_response_time;
  const midPhase = data.metrics.mid_phase_response_time;
  const latePhase = data.metrics.late_phase_response_time;

  if (earlyPhase && earlyPhase.values) {
    output += `   Early Phase (0-15 min):   avg=${earlyPhase.values.avg?.toFixed(2)}ms, p95=${earlyPhase.values['p(95)']?.toFixed(2)}ms\n`;
  }

  if (midPhase && midPhase.values) {
    output += `   Mid Phase (45-60 min):    avg=${midPhase.values.avg?.toFixed(2)}ms, p95=${midPhase.values['p(95)']?.toFixed(2)}ms\n`;
  }

  if (latePhase && latePhase.values) {
    output += `   Late Phase (105-120 min): avg=${latePhase.values.avg?.toFixed(2)}ms, p95=${latePhase.values['p(95)']?.toFixed(2)}ms\n`;
  }

  if (earlyPhase && latePhase && earlyPhase.values && latePhase.values) {
    const degradationFactor = latePhase.values.avg / earlyPhase.values.avg;
    const degradationPercent = ((degradationFactor - 1) * 100);

    output += `\n   Degradation Factor: ${degradationFactor?.toFixed(2)}x\n`;
    output += `   Performance Change: ${degradationPercent > 0 ? '+' : ''}${degradationPercent?.toFixed(2)}%\n`;

    if (degradationFactor < 1.1) {
      output += `   🟢 Excellent stability - minimal degradation (<10%)\n`;
    } else if (degradationFactor < 1.3) {
      output += `   🟡 Good stability - acceptable degradation (<30%)\n`;
    } else if (degradationFactor < 1.5) {
      output += `   🟠 Moderate stability - noticeable degradation (30-50%)\n`;
    } else {
      output += `   🔴 Poor stability - significant degradation (>50%)\n`;
      output += `   ️  Possible memory leak or resource exhaustion!\n`;
    }
  }

  output += '\n';

  // Throughput and volume
  if (data.metrics.http_reqs) {
    const reqs = data.metrics.http_reqs.values;
    output += '🚀 Throughput & Volume:\n';
    output += `   Total Requests: ${reqs.count}\n`;
    output += `   Average Rate:   ${reqs.rate?.toFixed(2)} req/s\n\n`;
  }

  // Error accumulation
  if (data.metrics.http_req_failed) {
    const failRate = (data.metrics.http_req_failed.values?.rate || 0) * 100;
    const failCount = data.metrics.http_req_failed.values.passes || 0;
    output += `❌ Error Analysis:\n`;
    output += `   Total Errors: ${failCount}\n`;
    output += `   Error Rate:   ${failRate?.toFixed(2)}%\n`;

    if (failRate < 1) {
      output += `   🟢 Excellent error rate - system stable\n`;
    } else if (failRate < 2) {
      output += `   🟡 Acceptable error rate - minor issues\n`;
    } else {
      output += `   🔴 High error rate - stability concerns\n`;
    }
  }

  if (data.metrics.operation_success_rate) {
    const successRate = (data.metrics.operation_success_rate.values?.rate || 0) * 100;
    output += `\nOverall Success Rate: ${successRate?.toFixed(2)}%\n`;
  }

  output += `\n🏥 System Stability Assessment:\n`;
  const isStable =
      (!data.metrics.http_req_failed || data.metrics.http_req_failed?.values?.rate || 0< 0.02) &&
      (!earlyPhase || !latePhase || !earlyPhase.values || !latePhase.values || (latePhase.values.avg / earlyPhase.values.avg) < 1.3);

  if (isStable) {
    output += `   System is STABLE for long-term operation\n`;
    output += `   Recommended for production deployment\n`;
  } else {
    output += `   ️  System shows stability concerns\n`;
    output += `   Further investigation recommended before production\n`;
  }

  output += '\n';
  return output;
}

function generateCSVSummary(data) {
  let csv = 'Metric,Average,Median,P90,P95,P99,Max\n';

  const metrics = [
    'http_req_duration',
    'early_phase_response_time',
    'mid_phase_response_time',
    'late_phase_response_time',
    'document_creation_time',
    'document_retrieval_time',
    'document_update_time',
    'document_deletion_time',
    'document_list_time'
  ];

  metrics.forEach(metric => {
    if (data.metrics[metric] && data.metrics[metric].values) {
      const v = data.metrics[metric].values;
      csv += `${metric},${v.avg?.toFixed(2)},${v.med?.toFixed(2)},${v['p(90)']?.toFixed(2)},${v['p(95)']?.toFixed(2)},${v['p(99)']?.toFixed(2)},${v.max?.toFixed(2)}\n`;
    }
  });

  csv += `\n`;
  if (data.metrics.http_reqs) {
    csv += `Total Requests,${data.metrics.http_reqs?.values?.count || 0}\n`;
    csv += `Throughput (req/s),${data.metrics.http_reqs.values.rate?.toFixed(2)}\n`;
  }

  if (data.metrics.http_req_failed) {
    const failRate = (data.metrics.http_req_failed.values?.rate || 0) * 100;
    const failCount = data.metrics.http_req_failed.values.passes || 0;
    csv += `Error Rate (%),${failRate?.toFixed(2)}\n`;
    csv += `Total Errors,${failCount}\n`;
  }

  // Degradation analysis
  if (data.metrics.early_phase_response_time && data.metrics.late_phase_response_time) {
    const early = data.metrics.early_phase_response_time.values.avg;
    const late = data.metrics.late_phase_response_time.values.avg;
    const degradation = ((late / early - 1) * 100);
    csv += `Degradation (%),${degradation?.toFixed(2)}\n`;
  }

  return csv;
}