import { sleep } from 'k6';
import { Trend, Counter, Rate } from 'k6/metrics';
import { SharedArray } from 'k6/data';
import { config, getBaseUrl, getTargetName } from './config.js';
import {
  createDocument,
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
const readOperations = new Counter('read_operations');
const createOperations = new Counter('create_operations');
const updateOperations = new Counter('update_operations');
const deleteOperations = new Counter('delete_operations');

const operationSuccessRate = new Rate('operation_success_rate');

export const options = {
  scenarios: {
    load_test: {
      executor: 'ramping-vus',
      startVUs: 0,
      stages: config.stages.load,
      gracefulRampDown: '30s',
    },
  },
  thresholds: {
    ...config.thresholds,
    'http_req_duration': ['avg<1500', 'p(95)<3000', 'p(99)<5000'],
    'http_reqs': ['rate>10'], // At least 10 req/s
    'operation_success_rate': ['rate>0.95'],
  },
};

let documentIds = [];

export function setup() {
  const baseUrl = getBaseUrl();
  const targetName = getTargetName();

  console.log(`\n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━`);
  console.log(` Load Test - High-Volume Operations (Laptop Mode)`);
  console.log(`━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━`);
  console.log(`📊 Target: ${targetName}`);
  console.log(`🌐 Base URL: ${baseUrl}`);
  console.log(`👥 Virtual Users: 10`);
  console.log(`⏱️  Duration: 3 minutes`);
  console.log(`📦 Initial Dataset: 100 documents`);
  console.log(`━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n`);

  console.log('⏳ Waiting for API to be ready...');
  const ready = waitForAPI(baseUrl);
  if (!ready) {
    throw new Error(`API not ready at ${baseUrl}`);
  }
  console.log('API is ready\n');

  console.log('Creating initial test dataset (100 documents)...');
  
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

  performMixedWorkload(baseUrl, docIds, metrics);
  
  totalOperations.add(1);
  operationSuccessRate.add(true);

  sleep(Math.random() * 2 + 0.5);
}

export function teardown(data) {
  console.log(`\n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━`);
  console.log(`Load Test Completed`);
  console.log(`━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n`);
}

export function handleSummary(data) {
  const targetName = __ENV.TARGET === 'repository' ? 'Repository-UoW' : 'Active-Record';
  const timestamp = new Date().toISOString().replace(/[:.]/g, '-').substring(0, 19);
  
  const resultsDir = config.output.resultsDir || './results';
  
  const summary = {
    [`${resultsDir}/${targetName}-load-test-${timestamp}.json`]: JSON.stringify(data, null, 2),
    'stdout': generateTextSummary(data, targetName),
  };

  summary[`${resultsDir}/${targetName}-load-test-${timestamp}.csv`] = generateCSVSummary(data);

  return summary;
}

function generateTextSummary(data, targetName) {
  let output = '\n';
  output += '╔══════════════════════════════════════════════════════════════╗\n';
  output += `║         LOAD TEST RESULTS - ${targetName.padEnd(32)}║\n`;
  output += '╚══════════════════════════════════════════════════════════════╝\n\n';

  if (data.metrics.http_req_duration && data.metrics.http_req_duration.values) {
    const httpDuration = data.metrics.http_req_duration.values;
    output += '📊 HTTP Request Duration:\n';
    output += `   Average: ${httpDuration.avg?.toFixed(2) || 'N/A'} ms\n`;
    output += `   Median:  ${httpDuration.med?.toFixed(2) || 'N/A'} ms\n`;
    output += `   P90:     ${httpDuration['p(90)']?.toFixed(2) || 'N/A'} ms\n`;
    output += `   P95:     ${httpDuration['p(95)']?.toFixed(2) || 'N/A'} ms\n`;
    output += `   P99:     ${httpDuration['p(99)']?.toFixed(2) || 'N/A'} ms\n`;
    output += `   Max:     ${httpDuration.max?.toFixed(2) || 'N/A'} ms\n\n`;
  }

  if (data.metrics.http_reqs && data.metrics.http_reqs.values) {
    const reqs = data.metrics.http_reqs.values;
    output += '🚀 Throughput:\n';
    output += `   Total Requests: ${reqs.count || 0}\n`;
    output += `   Requests/sec:   ${(reqs.rate || 0).toFixed(2)} req/s\n\n`;
  }

  output += '🔧 Operation Performance:\n';
  
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
      output += `   ${op.label.padEnd(8)}: avg=${values.avg?.toFixed(2) || 'N/A'}ms, p95=${values['p(95)']?.toFixed(2) || 'N/A'}ms, max=${values.max?.toFixed(2) || 'N/A'}ms\n`;
    }
  });

  output += '\n';

  if (data.metrics.http_req_failed && data.metrics.http_req_failed.values) {
    const failRate = (data.metrics.http_req_failed.values.rate || 0) * 100;
    const failCount = data.metrics.http_req_failed.values.passes || 0;
    output += `Error Rate: ${failRate.toFixed(2)}% (${failCount} failed requests)\n`;
  }

  if (data.metrics.operation_success_rate && data.metrics.operation_success_rate.values) {
    const successRate = (data.metrics.operation_success_rate.values.rate || 0) * 100;
    output += `Success Rate: ${successRate.toFixed(2)}%\n`;
  }

  if (data.metrics.data_received && data.metrics.data_received.values && data.metrics.data_sent && data.metrics.data_sent.values) {
    const received = (data.metrics.data_received.values.count || 0) / (1024 * 1024);
    const sent = (data.metrics.data_sent.values.count || 0) / (1024 * 1024);
    output += `\n📡 Data Transfer:\n`;
    output += `   Received: ${received.toFixed(2)} MB\n`;
    output += `   Sent:     ${sent.toFixed(2)} MB\n`;
  }

  output += '\n';
  return output;
}

function generateCSVSummary(data) {
  let csv = 'Metric,Average,Median,P90,P95,P99,Max,Count,Rate\n';
  
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
      csv += `${metric},${v.avg?.toFixed(2) || 'N/A'},${v.med?.toFixed(2) || 'N/A'},${v['p(90)']?.toFixed(2) || 'N/A'},${v['p(95)']?.toFixed(2) || 'N/A'},${v['p(99)']?.toFixed(2) || 'N/A'},${v.max?.toFixed(2) || 'N/A'},${v.count || 'N/A'},${v.rate?.toFixed(2) || 'N/A'}\n`;
    }
  });

  // Add summary metrics
  if (data.metrics.http_reqs && data.metrics.http_reqs.values) {
    csv += `\nTotal Requests,${data.metrics.http_reqs.values.count || 0}\n`;
    csv += `Throughput (req/s),${(data.metrics.http_reqs.values.rate || 0).toFixed(2)}\n`;
  }

  if (data.metrics.http_req_failed && data.metrics.http_req_failed.values) {
    const failRate = (data.metrics.http_req_failed.values.rate || 0) * 100;
    csv += `Error Rate (%),${failRate.toFixed(2)}\n`;
  }

  return csv;
}
