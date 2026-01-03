import http from 'k6/http';
import { check, sleep } from 'k6';
import { FormData } from 'https://jslib.k6.io/formdata/0.0.2/index.js';
import { randomString, randomItem } from 'https://jslib.k6.io/k6-utils/1.2.0/index.js';
import { getBaseUrl, getEndpointUrl } from './config.js';
import { config } from './config.js';

export function generateUUID() {
  return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {
    const r = Math.random() * 16 | 0;
    const v = c === 'x' ? r : (r & 0x3 | 0x8);
    return v.toString(16);
  });
}

export function generateDocumentTitle() {
  const prefixes = ['Report', 'Document', 'File', 'Memo', 'Contract', 'Invoice', 'Agreement', 'Policy'];
  const suffixes = ['Q1-2024', 'Final', 'Draft', 'Revised', 'Updated', 'Review', 'Approved'];

  return `${randomItem(prefixes)}-${randomString(8)}-${randomItem(suffixes)}`;
}

export function generateDocumentDescription() {
  const descriptions = [
    'Important business document requiring review and approval',
    'Financial report for quarterly analysis and strategic planning',
    'Legal contract document for compliance verification',
    'Technical specification document for development team',
    'Marketing materials for upcoming campaign launch',
    'Customer agreement with terms and conditions',
    'Internal policy document for organizational guidelines',
    'Project documentation for stakeholder review'
  ];

  return randomItem(descriptions);
}

export function generateFileContent(size, contentType = 'text/plain') {
  if (contentType === 'text/plain') {
    // Generate text content more efficiently
    const chars = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789 \n';
    const chunkSize = 1000;
    const chunks = Math.floor(size / chunkSize);
    let content = '';

    // Build in chunks for better performance
    for (let i = 0; i < chunks; i++) {
      let chunk = '';
      for (let j = 0; j < chunkSize; j++) {
        chunk += chars.charAt(Math.floor(Math.random() * chars.length));
      }
      content += chunk;
    }

    // Add remaining characters
    const remaining = size - (chunks * chunkSize);
    for (let i = 0; i < remaining; i++) {
      content += chars.charAt(Math.floor(Math.random() * chars.length));
    }

    return content;
  } else if (contentType === 'application/json') {
    const jsonObj = {
      data: randomString(size - 50),
      timestamp: new Date().toISOString(),
      type: 'test-document'
    };
    return JSON.stringify(jsonObj);
  } else {
    // Generate binary-like content for other types
    const bytes = new Uint8Array(size);
    for (let i = 0; i < size; i++) {
      bytes[i] = Math.floor(Math.random() * 256);
    }
    return String.fromCharCode.apply(null, bytes);
  }
}


export function generateFileName(contentType) {
  const extensions = {
    'text/plain': '.txt',
    'application/pdf': '.pdf',
    'application/json': '.json',
    'image/jpeg': '.jpg',
    'application/octet-stream': '.bin'
  };

  const ext = extensions[contentType] || '.bin';
  return `document-${randomString(10)}${ext}`;
}

/**
 * Create a new document via API
 * @param {string} baseUrl - Base URL of the API
 * @param {object} options - Document options (title, description, fileSize, contentType)
 * @returns {object} Response object
 */
export function createDocument(baseUrl, options = {}) {
  const {
    title = generateDocumentTitle(),
    description = generateDocumentDescription(),
    fileSize = config.testData.fileSizes.small,
    contentType = 'text/plain'
  } = options;

  const fileName = generateFileName(contentType);
  const fileContent = generateFileContent(fileSize, contentType);

  const formData = new FormData();
  formData.append('title', title);
  formData.append('description', description);
  formData.append('file', http.file(fileContent, fileName, contentType));

  const url = `${baseUrl}${config.endpoints.documents}`;
  const params = {
    headers: {
      'Content-Type': 'multipart/form-data; boundary=' + formData.boundary,
    },
    timeout: config.timeouts.http,
  };

  const response = http.post(url, formData.body(), params);

  check(response, {
    'document created successfully': (r) => r.status === 201,
    'response has document ID': (r) => {
      if (r.status === 201) {
        try {
          const body = JSON.parse(r.body);
          return body.id !== undefined;
        } catch (e) {
          return false;
        }
      }
      return false;
    },
  });

  return response;
}

/**
 * Get a document by ID
 * @param {string} baseUrl - Base URL of the API
 * @param {string} documentId - Document UUID
 * @returns {object} Response object
 */
export function getDocument(baseUrl, documentId) {
  const url = `${baseUrl}${config.endpoints.document(documentId)}`;
  const params = { timeout: config.timeouts.http };

  const response = http.get(url, params);

  check(response, {
    'document retrieved successfully': (r) => r.status === 200,
    'response has valid structure': (r) => {
      if (r.status === 200) {
        try {
          const body = JSON.parse(r.body);
          return body.id === documentId && body.title !== undefined;
        } catch (e) {
          return false;
        }
      }
      return false;
    },
  });

  return response;
}

/**
 * List all documents
 * @param {string} baseUrl - Base URL of the API
 * @returns {object} Response object
 */
export function listDocuments(baseUrl) {
  const url = `${baseUrl}${config.endpoints.documents}`;
  const params = { timeout: config.timeouts.http };

  const response = http.get(url, params);

  check(response, {
    'documents listed successfully': (r) => r.status === 200,
    'response has documents array': (r) => {
      if (r.status === 200) {
        try {
          const body = JSON.parse(r.body);
          return Array.isArray(body.documents);
        } catch (e) {
          return false;
        }
      }
      return false;
    },
  });

  return response;
}

/**
 * Update a document's metadata
 * @param {string} baseUrl - Base URL of the API
 * @param {string} documentId - Document UUID
 * @param {object} updates - Update data (title, description)
 * @returns {object} Response object
 */
export function updateDocument(baseUrl, documentId, updates = {}) {
  const {
    title = generateDocumentTitle(),
    description = generateDocumentDescription()
  } = updates;

  const url = `${baseUrl}${config.endpoints.document(documentId)}`;
  const payload = JSON.stringify({ title, description });
  const params = {
    headers: { 'Content-Type': 'application/json' },
    timeout: config.timeouts.http,
  };

  const response = http.put(url, payload, params);

  check(response, {
    'document updated successfully': (r) => r.status === 200,
    'response has updated data': (r) => {
      if (r.status === 200) {
        try {
          const body = JSON.parse(r.body);
          return body.id === documentId;
        } catch (e) {
          return false;
        }
      }
      return false;
    },
  });

  return response;
}

/**
 * Delete a document (soft delete)
 * @param {string} baseUrl - Base URL of the API
 * @param {string} documentId - Document UUID
 * @returns {object} Response object
 */
export function deleteDocument(baseUrl, documentId) {
  const url = `${baseUrl}${config.endpoints.document(documentId)}`;
  const params = { timeout: config.timeouts.http };

  const response = http.del(url, null, params);

  check(response, {
    'document deleted successfully': (r) => r.status === 200,
    'response confirms deletion': (r) => {
      if (r.status === 200) {
        try {
          const body = JSON.parse(r.body);
          return body.message !== undefined || body.deletedAt !== undefined;
        } catch (e) {
          return false;
        }
      }
      return false;
    },
  });

  return response;
}

/**
 * Download a document file
 * @param {string} baseUrl - Base URL of the API
 * @param {string} documentId - Document UUID
 * @returns {object} Response object
 */
export function downloadDocument(baseUrl, documentId) {
  const url = `${baseUrl}${config.endpoints.download(documentId)}`;
  const params = { timeout: config.timeouts.http };

  const response = http.get(url, params);

  check(response, {
    'document downloaded successfully': (r) => r.status === 200,
    'response has file content': (r) => r.status === 200 && r.body.length > 0,
  });

  return response;
}

/**
 * Perform a complete CRUD cycle
 * @param {string} baseUrl - Base URL of the API
 * @param {object} metrics - Custom metrics object to track timings
 * @returns {boolean} Success status
 */
export function performCRUDCycle(baseUrl, metrics = {}) {
  // Create
  const createStart = Date.now();
  const createResp = createDocument(baseUrl);
  if (metrics.create) metrics.create.add(Date.now() - createStart);

  if (createResp.status !== 201) return false;

  const documentId = JSON.parse(createResp.body).id;
  sleep(config.sleepTimes.min);

  // Read
  const readStart = Date.now();
  const readResp = getDocument(baseUrl, documentId);
  if (metrics.read) metrics.read.add(Date.now() - readStart);

  if (readResp.status !== 200) return false;
  sleep(config.sleepTimes.min);

  // Update
  const updateStart = Date.now();
  const updateResp = updateDocument(baseUrl, documentId);
  if (metrics.update) metrics.update.add(Date.now() - updateStart);

  if (updateResp.status !== 200) return false;
  sleep(config.sleepTimes.min);

  // Read again to verify update
  const readAfterUpdateResp = getDocument(baseUrl, documentId);
  if (readAfterUpdateResp.status !== 200) return false;
  sleep(config.sleepTimes.min);

  // Delete
  const deleteStart = Date.now();
  const deleteResp = deleteDocument(baseUrl, documentId);
  if (metrics.delete) metrics.delete.add(Date.now() - deleteStart);

  if (deleteResp.status !== 200) return false;
  sleep(config.sleepTimes.min);

  // List all documents
  const listStart = Date.now();
  const listResp = listDocuments(baseUrl);
  if (metrics.list) metrics.list.add(Date.now() - listStart);

  return listResp.status === 200;
}

/**
 * Perform mixed workload operations based on configured distribution
 * @param {string} baseUrl - Base URL of the API
 * @param {array} existingDocumentIds - Array of existing document IDs for read/update/delete
 * @param {object} metrics - Custom metrics object
 */
export function performMixedWorkload(baseUrl, existingDocumentIds, metrics = {}) {
  const rand = Math.random();
  const dist = config.testData.workloadDistribution;

  if (rand < dist.read) {
    if (existingDocumentIds.length > 0) {
      const docId = randomItem(existingDocumentIds);
      const start = Date.now();
      getDocument(baseUrl, docId);
      if (metrics.read) metrics.read.add(Date.now() - start);
    } else {
      const start = Date.now();
      listDocuments(baseUrl);
      if (metrics.list) metrics.list.add(Date.now() - start);
    }
  } else if (rand < dist.read + dist.create) {
    const start = Date.now();
    const resp = createDocument(baseUrl, {
      fileSize: randomItem(Object.values(config.testData.fileSizes))
    });
    if (metrics.create) metrics.create.add(Date.now() - start);

    // Add new document ID to the pool
    if (resp.status === 201) {
      try {
        const newId = JSON.parse(resp.body).id;
        existingDocumentIds.push(newId);
      } catch (e) {
        // Ignore parse errors
      }
    }
  } else if (rand < dist.read + dist.create + dist.update) {
    if (existingDocumentIds.length > 0) {
      const docId = randomItem(existingDocumentIds);
      const start = Date.now();
      updateDocument(baseUrl, docId);
      if (metrics.update) metrics.update.add(Date.now() - start);
    }
  } else {
    if (existingDocumentIds.length > 0) {
      const index = Math.floor(Math.random() * existingDocumentIds.length);
      const docId = existingDocumentIds[index];
      const start = Date.now();
      const resp = deleteDocument(baseUrl, docId);
      if (metrics.delete) metrics.delete.add(Date.now() - start);

      // Remove from pool if successful
      if (resp.status === 200) {
        existingDocumentIds.splice(index, 1);
      }
    }
  }

  sleep(Math.random() * (config.sleepTimes.max - config.sleepTimes.min) + config.sleepTimes.min);
}

/**
 * Wait for API to be ready
 */
export function waitForAPI(baseUrl, maxRetries = 10) {
  for (let i = 0; i < maxRetries; i++) {
    try {
      const response = http.get(`${baseUrl}${config.endpoints.documents}`, {
        timeout: '5s'
      });
      if (response.status === 200) {
        return true;
      }
    } catch (e) {
      // Ignore errors
    }
    sleep(2);
  }
  return false;
}

/**
 * Create test dataset with optimized batching and progress reporting
 */
export function createTestDataset(baseUrl, count, options = {}) {
  const documentIds = [];
  const batchDelay = options.batchDelay || 0.1;
  const batchSize = 50; // Report progress every 50 documents

  console.log(`Creating ${count} documents...`);

  for (let i = 0; i < count; i++) {
    const resp = createDocument(baseUrl, {
      ...options,
      fileSize: options.fileSize || randomItem(Object.values(config.testData.fileSizes))
    });

    if (resp.status === 201) {
      try {
        const docId = JSON.parse(resp.body).id;
        documentIds.push(docId);
      } catch (e) {
        console.log(`⚠️  Failed to parse response for document ${i + 1}`);
      }
    } else {
      console.log(`❌ Failed to create document ${i + 1} (status: ${resp.status})`);
    }

    // Progress reporting
    if ((i + 1) % batchSize === 0) {
      const progress = ((i + 1) / count * 100).toFixed(1);
      console.log(`   Progress: ${i + 1}/${count} documents (${progress}%)`);
      sleep(batchDelay);
    }
  }

  console.log(`✅ Successfully created ${documentIds.length}/${count} documents`);

  return documentIds;
}

/**
 * Custom summary handler for k6 tests
 */
export function customSummary(data) {
  const targetName = __ENV.TARGET === 'repository' ? 'Repository-UoW' : 'Active-Record';
  const timestamp = new Date().toISOString().replace(/[:.]/g, '-');

  return {
    [`${config.output.resultsDir}/${targetName}-${__ENV.SCENARIO || 'test'}-${timestamp}.json`]: JSON.stringify(data, null, 2),
    stdout: textSummary(data, { indent: ' ', enableColors: true }),
  };
}

/**
 * Simple text summary for console output
 */
function textSummary(data, options = {}) {
  const indent = options.indent || '';
  const enableColors = options.enableColors || false;

  let summary = '\n' + indent + '✓ Test Summary\n';
  summary += indent + '================\n\n';

  if (data.metrics) {
    summary += indent + 'Metrics:\n';
    for (const [name, metric] of Object.entries(data.metrics)) {
      if (metric.values) {
        summary += indent + `  ${name}:\n`;
        summary += indent + `    avg: ${metric.values.avg?.toFixed(2) || 'N/A'}\n`;
        summary += indent + `    p95: ${metric.values['p(95)']?.toFixed(2) || 'N/A'}\n`;
        summary += indent + `    p99: ${metric.values['p(99)']?.toFixed(2) || 'N/A'}\n`;
      }
    }
  }

  return summary;
}

export default {
  generateUUID,
  generateDocumentTitle,
  generateDocumentDescription,
  generateFileContent,
  generateFileName,
  createDocument,
  getDocument,
  listDocuments,
  updateDocument,
  deleteDocument,
  downloadDocument,
  performCRUDCycle,
  performMixedWorkload,
  waitForAPI,
  createTestDataset,
  customSummary
};