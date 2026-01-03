import { sleep } from 'k6';
import { Trend, Counter, Rate } from 'k6/metrics';
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

const totalOperations = new Counter('total_operations');
const failedOperations = new Counter('failed_operations');
const operationSuccessRate = new Rate('operation_success_rate');

export const options = {
  scenarios: {
    stress_test: {
      executor: 'ramping-vus',
      startVUs: 0,
      stages: config.stages.stress,
      gracefulRampDown: '30s',
    },
  },
  thresholds: {
    'http_req_duration': ['p(95)<5000', 'p(99)<10000'], // More lenient for stress test
    'http_req_failed': ['rate<0.1'], // Allow up to 10% failure at peak
  },
};

export function setup() {
  const baseUrl = getBaseUrl();
  const targetName = getTargetName();

  console.log(`\n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━`);
  console.log(` Stress Test - Finding Breaking Point (Laptop Mode)`);
  console.log(`━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━`);
  console.log(`📊 Target: ${targetName}`);
  console.log(`🌐 Base URL: ${baseUrl}`);
  console.log(`👥 Virtual Users: 5 → 25 (ramping)`);
  console.log(`⏱️  Duration: 5 minutes`);
  console.log(`📦 Initial Dataset: 100 documents`);
  console.log(`━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n`);

  console.log('⏳ Waiting for API to be ready...');
  const ready = waitForAPI(baseUrl);
  if (!ready) {
    throw new Error(`API not ready at ${baseUrl}`);
  }
  console.log('API is ready\n');

  console.log('📝 Creating initial test dataset (100 documents)...');
  
  const initialDocIds = createTestDataset(baseUrl, 100, {
    fileSize: config.testData.fileSizes.small
  });

  console.log(`Created ${initialDocIds.length} initial documents\n`);

  return { 
    baseUrl, 
    targetName,
    documentIds: initialDocIds
  };
}

export default function(data) {
  const baseUrl = data.baseUrl;
  const docIds = [...data.documentIds];

  const metrics = {
    create: documentCreationTime,
    read: documentRetrievalTime,
    update: documentUpdateTime,
    delete: documentDeletionTime,
    list: documentListTime
  };

  try {
    performMixedWorkload(baseUrl, docIds, metrics);
    totalOperations.add(1);
    operationSuccessRate.add(true);
  } catch (e) {
    failedOperations.add(1);
    operationSuccessRate.add(false);
  }

  // Variable sleep with shorter delays for stress
  sleep(Math.random() * 1 + 0.2);
}

export function teardown(data) {
  console.log(`\n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━`);
  console.log(`Stress Test Completed`);
  console.log(`━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n`);
}

export function handleSummary(data) {
  const targetName = __ENV.TARGET === 'repository' ? 'Repository-UoW' : 'Active-Record';
  const timestamp = new Date().toISOString().replace(/[:.]/g, '-').substring(0, 19);
  
  const resultsDir = config.output.resultsDir || './results';
  
  const summary = {
    [`${resultsDir}/${targetName}-stress-test-${timestamp}.json`]: JSON.stringify(data, null, 2),
    'stdout': generateTextSummary(data, targetName),
  };

  summary[`${resultsDir}/${targetName}-stress-test-${timestamp}.csv`] = generateCSVSummary(data);

  return summary;
}

function generateTextSummary(data, targetName) {
  let output = '\n';
  output += '╔══════════════════════════════════════════════════════════════╗\n';
  output += `║        STRESS TEST RESULTS - ${targetName.padEnd(31)}║\n`;
  output += '╚══════════════════════════════════════════════════════════════╝\n\n';

  // Overall HTTP metrics with degradation analysis
  if (data.metrics.http_req_duration && data.metrics.http_req_duration.values) {
    const httpDuration = data.metrics.http_req_duration.values;
    output += '📊 HTTP Request Duration (Degradation Analysis):\n';
    output += `   Average: ${httpDuration.avg?.toFixed(2) || 'N/A'} ms\n`;
    output += `   Median:  ${httpDuration.med?.toFixed(2) || 'N/A'} ms\n`;
    output += `   P90:     ${httpDuration['p(90)']?.toFixed(2) || 'N/A'} ms\n`;
    output += `   P95:     ${httpDuration['p(95)']?.toFixed(2) || 'N/A'} ms\n`;
    output += `   P99:     ${httpDuration['p(99)']?.toFixed(2) || 'N/A'} ms\n`;
    output += `   Max:     ${httpDuration.max?.toFixed(2) || 'N/A'} ms\n`;
    
    // Calculate degradation ratio
    if (httpDuration['p(99)'] && httpDuration.avg) {
      const degradation = httpDuration['p(99)'] / httpDuration.avg;
      output += `   \n   ⚠️  Degradation Ratio (P99/Avg): ${degradation.toFixed(2)}x\n`;
      if (degradation > 10) {
        output += `   🔴 SEVERE degradation detected!\n`;
      } else if (degradation > 5) {
        output += `   🟡 Moderate degradation detected\n`;
      } else {
        output += `   🟢 Acceptable degradation\n`;
      }
    }
    output += '\n';
  }

  // Throughput analysis
  if (data.metrics.http_reqs && data.metrics.http_reqs.values) {
    const reqs = data.metrics.http_reqs.values;
    output += '🚀 Throughput:\n';
    output += `   Total Requests: ${reqs.count || 0}\n`;
    output += `   Requests/sec:   ${(reqs.rate || 0).toFixed(2)} req/s\n\n`;
  }

  output += '🔧 Operation Performance Under Stress:\n';
  
  const operations = [
    { key: 'document_creation_time', label: 'Create' },
    { key: 'document_retrieval_time', label: 'Read' },
    { key: 'document_update_time', label: 'Update' },
    { key: 'document_deletion_time', label: 'Delete' },
    { key: 'document_list_time', label: 'List' }
  ];

  operations.forEach(op => {
    if (data.metrics[op.key] && data.metrics[op.key].values) {
      const values = data.metrics[op.key].values;
      const opDegradation = (values['p(99)'] && values.avg) ? values['p(99)'] / values.avg : 0;
      output += `   ${op.label.padEnd(8)}: avg=${values.avg?.toFixed(2) || 'N/A'}ms, p95=${values['p(95)']?.toFixed(2) || 'N/A'}ms, p99=${values['p(99)']?.toFixed(2) || 'N/A'}ms (deg: ${opDegradation.toFixed(1)}x)\n`;
    }
  });

  output += '\n';

  if (data.metrics.http_req_failed && data.metrics.http_req_failed.values) {
    const failRate = (data.metrics.http_req_failed.values.rate || 0) * 100;
    const failCount = data.metrics.http_req_failed.values.passes || 0;
    output += `Error Rate: ${failRate.toFixed(2)}% (${failCount} failed requests)\n`;
    
    if (failRate > 5) {
      output += `   🔴 HIGH error rate - system approaching limits!\n`;
    } else if (failRate > 1) {
      output += `   🟡 Elevated error rate - system under stress\n`;
    } else {
      output += `   🟢 Acceptable error rate\n`;
    }
  }

  if (data.metrics.operation_success_rate && data.metrics.operation_success_rate.values) {
    const successRate = (data.metrics.operation_success_rate.values.rate || 0) * 100;
    output += `Overall Success Rate: ${successRate.toFixed(2)}%\n`;
  }

  output += `\n💥 Breaking Point Assessment:\n`;
  if (data.metrics.http_req_failed && data.metrics.http_req_failed.values && data.metrics.http_req_duration && data.metrics.http_req_duration.values) {
    const failRate = (data.metrics.http_req_failed.values.rate || 0) * 100;
    const p99 = data.metrics.http_req_duration.values['p(99)'] || 0;
    
    if (failRate > 10 || p99 > 10000) {
      output += `   System reached saturation point\n`;
      output += `   Recommended max load: ~150 users\n`;
    } else if (failRate > 5 || p99 > 5000) {
      output += `   System showing stress symptoms\n`;
      output += `   Recommended max load: ~175 users\n`;
    } else {
      output += `   System handled stress well\n`;
      output += `   Can potentially handle >200 users\n`;
    }
  }

  output += '\n';
  return output;
}

function generateCSVSummary(data) {
  let csv = 'Metric,Average,Median,P90,P95,P99,Max,DegradationRatio\n';
  
  const metrics = [
    'http_req_duration',
    'document_creation_time',
    'document_retrieval_time',
    'document_update_time',
    'document_deletion_time',
    'document_list_time'
  ];

  metrics.forEach(metric => {
    if (data.metrics[metric] && data.metrics[metric].values) {
      const v = data.metrics[metric].values;
      const degradation = (v['p(99)'] && v.avg) ? v['p(99)'] / v.avg : 0;
      csv += `${metric},${v.avg?.toFixed(2) || 'N/A'},${v.med?.toFixed(2) || 'N/A'},${v['p(90)']?.toFixed(2) || 'N/A'},${v['p(95)']?.toFixed(2) || 'N/A'},${v['p(99)']?.toFixed(2) || 'N/A'},${v.max?.toFixed(2) || 'N/A'},${degradation.toFixed(2)}\n`;
    }
  });

  csv += `\n`;
  if (data.metrics.http_reqs && data.metrics.http_reqs.values) {
    csv += `Total Requests,${data.metrics.http_reqs.values.count || 0}\n`;
    csv += `Throughput (req/s),${(data.metrics.http_reqs.values.rate || 0).toFixed(2)}\n`;
  }

  if (data.metrics.http_req_failed && data.metrics.http_req_failed.values) {
    const failRate = (data.metrics.http_req_failed.values.rate || 0) * 100;
    csv += `Error Rate (%),${failRate.toFixed(2)}\n`;
  }

  return csv;
}
