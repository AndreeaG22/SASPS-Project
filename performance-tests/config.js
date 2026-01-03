/**
 * K6 Performance Testing Configuration
 */

export const config = {
  activeRecord: {
    baseUrl: 'http://localhost:8080',
    name: 'Active Record',
    port: 8080
  },
  repository: {
    baseUrl: 'http://localhost:8082',
    name: 'Repository + UoW',
    port: 8082
  },

  endpoints: {
    documents: '/api/documents',
    document: (id) => `/api/documents/${id}`,
    download: (id) => `/api/documents/${id}/download`
  },

  // Global thresholds for all tests
  thresholds: {
    http_req_duration: ['p(95)<2000', 'p(99)<3000'], // 95% under 2s, 99% under 3s
    http_req_failed: ['rate<0.01'], // Error rate under 1%
    http_reqs: ['rate>1'], // Minimum throughput 1 req/s
  },

  // Scenario-specific stages (Laptop-friendly configuration)
  stages: {
    // Smoke test - 3 users, 1 minute
    smoke: [
      { duration: '20s', target: 3 },
      { duration: '30s', target: 3 },
      { duration: '10s', target: 0 }
    ],

    // Load test - 10 users, 3 minutes
    load: [
      { duration: '30s', target: 10 },
      { duration: '2m', target: 10 },
      { duration: '30s', target: 0 }
    ],

    // Stress test - ramp from 5 to 25 users over 5 minutes
    stress: [
      { duration: '1m', target: 5 },
      { duration: '1m', target: 10 },
      { duration: '1m', target: 15 },
      { duration: '1m', target: 20 },
      { duration: '30s', target: 25 },
      { duration: '30s', target: 0 }
    ],

    scalabilityData: [
      { duration: '30s', target: 5 },
      { duration: '1m', target: 5 },
      { duration: '30s', target: 0 }
    ],

    scalabilityUsers: [
      { duration: '1m', target: 3 },
      { duration: '1m', target: 5 },
      { duration: '1m', target: 10 },
      { duration: '1m', target: 15 },
      { duration: '1m', target: 20 },
      { duration: '30s', target: 0 }
    ],

    // Soak test - 5 users, 10 minutes
    soak: [
      { duration: '1m', target: 5 },
      { duration: '8m', target: 5 },
      { duration: '1m', target: 0 }
    ]
  },

  // Test data configuration
  testData: {
    fileSizes: {
      tiny: 1024, // 1 KB
      small: 10 * 1024, // 10 KB
      medium: 100 * 1024, // 100 KB
      large: 1024 * 1024, // 1 MB
      xlarge: 5 * 1024 * 1024 // 5 MB
    },

    // Content types
    contentTypes: [
      'text/plain',
      'application/pdf',
      'application/json',
      'image/jpeg',
      'application/octet-stream'
    ],

    workloadDistribution: {
      read: 0.60, // 60%
      create: 0.25, // 25%
      update: 0.10, // 10%
      delete: 0.05 // 5%
    },

    datasetSizes: {
      small: 50,
      medium: 100,
      large: 250,
      xlarge: 500
    }
  },

  sleepTimes: {
    min: 0.5,
    max: 2,
    default: 1
  },

  // Timeout settings
  timeouts: {
    http: '60s',
    default: '30s'
  },

  // Output configuration
  output: {
    resultsDir: './results',
    reportsDir: './reports',
    jsonSummary: true,
    htmlReport: true,
    csvExport: true,
    influxdb: {
      url: 'http://localhost:8086/k6',
      db: 'k6',
      enabled: true
    },
    grafana: {
      url: 'http://localhost:3000',
      enabled: true
    }
  }
};

export function getBaseUrl() {
  const target = __ENV.TARGET || 'activeRecord';
  return target === 'repository' ? config.repository.baseUrl : config.activeRecord.baseUrl;
}

export function getTargetName() {
  const target = __ENV.TARGET || 'activeRecord';
  return target === 'repository' ? config.repository.name : config.activeRecord.name;
}

export function getEndpointUrl(endpoint) {
  return `${getBaseUrl()}${endpoint}`;
}

export const BASE_URL = getBaseUrl();

export const HEADERS = config?.headers ?? {
  'Content-Type': 'application/json',
};

export const TARGET_ENV = config?.targetEnv ?? config?.env ?? getTargetName();

export default config;
