import http from 'k6/http';
import { check, sleep } from 'k6';
import { Trend, Counter, Rate } from 'k6/metrics';
import { config, getBaseUrl, getTargetName } from './config.js';
import {
  performCRUDCycle,
  createDocument,
  getDocument,
  updateDocument,
  deleteDocument,
  listDocuments,
  waitForAPI
} from './utils.js';

const documentCreationTime = new Trend('document_creation_time');
const documentRetrievalTime = new Trend('document_retrieval_time');
const documentUpdateTime = new Trend('document_update_time');
const documentDeletionTime = new Trend('document_deletion_time');
const documentListTime = new Trend('document_list_time');

const successfulCRUDCycles = new Counter('successful_crud_cycles');
const failedCRUDCycles = new Counter('failed_crud_cycles');
const crudSuccessRate = new Rate('crud_success_rate');

export const options = {
  scenarios: {
    smoke_test: {
      executor: 'ramping-vus',
      startVUs: 0,
      stages: config.stages.smoke,
      gracefulRampDown: '30s',
    },
  },
  thresholds: {
    ...config.thresholds,
    'document_creation_time': ['avg<1000', 'p(95)<2000'],
    'document_retrieval_time': ['avg<500', 'p(95)<1000'],
    'document_update_time': ['avg<800', 'p(95)<1500'],
    'document_deletion_time': ['avg<500', 'p(95)<1000'],
    'document_list_time': ['avg<800', 'p(95)<1500'],
    'crud_success_rate': ['rate>0.95'],
  },
};

export function setup() {
  const baseUrl = getBaseUrl();
  const targetName = getTargetName();

  console.log(`\n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━`);
  console.log(`🧪 Smoke Test - CRUD Operations Baseline`);
  console.log(`━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━`);
  console.log(`📊 Target: ${targetName}`);
  console.log(`🌐 Base URL: ${baseUrl}`);
  console.log(`👥 Virtual Users: 5`);
  console.log(`⏱️  Duration: 2 minutes`);
  console.log(`📦 Document Size: Small (1-10 KB)`);
  console.log(`━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n`);

  console.log('⏳ Waiting for API to be ready...');
  const ready = waitForAPI(baseUrl);
  if (!ready) {
    throw new Error(`API not ready at ${baseUrl}`);
  }
  console.log('API is ready\n');

  return { baseUrl, targetName };
}

export default function(data) {
  const baseUrl = data.baseUrl;

  // Perform complete CRUD cycle
  const metrics = {
    create: documentCreationTime,
    read: documentRetrievalTime,
    update: documentUpdateTime,
    delete: documentDeletionTime,
    list: documentListTime
  };

  const success = performCRUDCycle(baseUrl, metrics);

  if (success) {
    successfulCRUDCycles.add(1);
    crudSuccessRate.add(true);
  } else {
    failedCRUDCycles.add(1);
    crudSuccessRate.add(false);
  }

  // Small delay between iterations
  sleep(1);
}

export function teardown(data) {
  console.log(`\n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━`);
  console.log(`Smoke Test Completed`);
  console.log(`━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n`);
}

export function handleSummary(data) {
  const targetName = __ENV.TARGET === 'repository' ? 'Repository-UoW' : 'Active-Record';
  const timestamp = new Date().toISOString().replace(/[:.]/g, '-').substring(0, 19);
  
  // Ensure results directory exists
  const resultsDir = config.output.resultsDir || './results';
  
  const summary = {
    [`${resultsDir}/${targetName}-smoke-test-${timestamp}.json`]: JSON.stringify(data, null, 2),
    'stdout': generateTextSummary(data, targetName),
  };

  // Also generate a CSV summary
  summary[`${resultsDir}/${targetName}-smoke-test-${timestamp}.csv`] = generateCSVSummary(data);

  return summary;
}

function generateTextSummary(data, targetName) {
  let output = '\n';
  output += '╔══════════════════════════════════════════════════════════════╗\n';
  output += `║         SMOKE TEST RESULTS - ${targetName.padEnd(30)}║\n`;
  output += '╚══════════════════════════════════════════════════════════════╝\n\n';

  if (data.metrics.http_req_duration && data.metrics.http_req_duration.values) {
    const httpDuration = data.metrics.http_req_duration.values;
    output += '📊 HTTP Request Duration:\n';
    output += `   Average: ${httpDuration.avg?.toFixed(2) || 'N/A'} ms\n`;
    output += `   Median:  ${httpDuration.med?.toFixed(2) || 'N/A'} ms\n`;
    output += `   P95:     ${httpDuration['p(95)']?.toFixed(2) || 'N/A'} ms\n`;
    output += `   P99:     ${httpDuration['p(99)']?.toFixed(2) || 'N/A'} ms\n`;
    output += `   Max:     ${httpDuration.max?.toFixed(2) || 'N/A'} ms\n\n`;
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
      output += `   ${op.label.padEnd(8)}: avg=${values.avg?.toFixed(2) || 'N/A'}ms, p95=${values['p(95)']?.toFixed(2) || 'N/A'}ms, p99=${values['p(99)']?.toFixed(2) || 'N/A'}ms\n`;
    }
  });

  output += '\n';

  if (data.metrics.crud_success_rate && data.metrics.crud_success_rate.values) {
    const successRate = (data.metrics.crud_success_rate.values.rate || 0) * 100;
    output += `CRUD Success Rate: ${successRate.toFixed(2)}%\n`;
  }

  if (data.metrics.http_reqs && data.metrics.http_reqs.values) {
    output += `📈 Total Requests: ${data.metrics.http_reqs.values.count || 0}\n`;
    output += `📉 Request Rate: ${(data.metrics.http_reqs.values.rate || 0).toFixed(2)} req/s\n`;
  }

  if (data.metrics.http_req_failed && data.metrics.http_req_failed.values) {
    const failRate = (data.metrics.http_req_failed.values.rate || 0) * 100;
    output += `❌ Failed Requests: ${failRate.toFixed(2)}%\n`;
  }

  output += '\n';
  return output;
}

function generateCSVSummary(data) {
  let csv = 'Metric,Average,Median,P95,P99,Max\n';
  
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
      csv += `${metric},${v.avg?.toFixed(2) || 'N/A'},${v.med?.toFixed(2) || 'N/A'},${v['p(95)']?.toFixed(2) || 'N/A'},${v['p(99)']?.toFixed(2) || 'N/A'},${v.max?.toFixed(2) || 'N/A'}\n`;
    }
  });

  return csv;
}
