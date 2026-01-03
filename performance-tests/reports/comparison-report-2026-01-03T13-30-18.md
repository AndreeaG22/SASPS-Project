# DocuStore Performance Comparison Report

**Active Record vs Repository + Unit of Work**

Generated: 2026-01-03T13:30:18.209Z

---

## Executive Summary

This report compares the performance of two architectural patterns:
- **Active Record**: Business logic embedded in domain models
- **Repository + Unit of Work**: Separated persistence layer with explicit transactions


## Smoke Test (Baseline CRUD)

| Metric | Active Record | Repository + UoW | Difference | Winner |
|--------|--------------|------------------|------------|--------|
| Avg Response Time | 5.18ms | 4.95ms | -4.41% | Repository |
| P95 Response Time | 8.79ms | 7.82ms | -11.06% | Repository |
| P99 Response Time | N/A | N/A | N/A% | N/A |
| Throughput | 3.80 req/s | 3.80 req/s | 0.08% | Repository |
| Total Requests | 229 | 229 | 0.00% | - |
| Error Rate | 0.00% | 0.00% | N/A% | Repository |


## Load Test (50 Users, 10 Minutes)

| Metric | Active Record | Repository + UoW | Difference | Winner |
|--------|--------------|------------------|------------|--------|
| Avg Response Time | 6.16ms | 6.45ms | 4.77% | Active Record |
| P95 Response Time | 19.49ms | 28.96ms | 48.56% | Active Record |
| P99 Response Time | N/A | N/A | N/A% | N/A |
| Throughput | 2.65 req/s | 2.50 req/s | -5.47% | Active Record |
| Total Requests | 503 | 489 | -2.78% | - |
| Error Rate | 0.80% | 0.20% | -74.28% | Repository |


## Stress Test (10-200 Users)

| Metric | Active Record | Repository + UoW | Difference | Winner |
|--------|--------------|------------------|------------|--------|
| Avg Response Time | 10.29ms | 8.42ms | -18.19% | Repository |
| P95 Response Time | 39.50ms | 30.77ms | -22.09% | Repository |
| P99 Response Time | N/A | N/A | N/A% | N/A |
| Throughput | 3.01 req/s | 3.69 req/s | 22.56% | Repository |
| Total Requests | 990 | 1170 | 18.18% | - |
| Error Rate | 0.91% | 1.37% | 50.43% | Active Record |


## Data Volume Scalability

| Metric | Active Record | Repository + UoW | Difference | Winner |
|--------|--------------|------------------|------------|--------|
| Avg Response Time | 3.70ms | 3.88ms | 4.71% | Active Record |
| P95 Response Time | 7.48ms | 7.71ms | 3.15% | Active Record |
| P99 Response Time | N/A | N/A | N/A% | N/A |
| Throughput | 4.67 req/s | 4.75 req/s | 1.78% | Repository |
| Total Requests | 2027 | 2060 | 1.63% | - |
| Error Rate | 0.00% | 0.00% | N/A% | Repository |


## User Concurrency Scalability

| Metric | Active Record | Repository + UoW | Difference | Winner |
|--------|--------------|------------------|------------|--------|
| Avg Response Time | 8.08ms | 8.40ms | 3.93% | Active Record |
| P95 Response Time | 31.71ms | 33.45ms | 5.50% | Active Record |
| P99 Response Time | N/A | N/A | N/A% | N/A |
| Throughput | 2.36 req/s | 2.30 req/s | -2.66% | Active Record |
| Total Requests | 818 | 798 | -2.44% | - |
| Error Rate | 0.37% | 1.13% | 207.52% | Active Record |

## Overall Conclusions

_Note: Specific conclusions depend on the actual test results._

### Key Findings

- Performance differences observed across various load scenarios
- Scalability characteristics analyzed for both implementations
- Stability and endurance evaluated over extended periods

---

## Appendix: Test Configuration

### Test Scenarios

1. **Smoke Test**: 5 users, 2 minutes, baseline CRUD operations
2. **Load Test**: 50 users, 10 minutes, mixed workload (60% read, 25% create, 10% update, 5% delete)
3. **Stress Test**: Ramp 10→200 users over 15 minutes
4. **Data Volume Scalability**: 100→1K→10K documents, 20 users
5. **User Concurrency Scalability**: 5→200 users in stages, 2K documents
6. **Soak Test**: 30 users, 2 hours

### Environment

- Active Record: http://localhost:8080
- Repository + UoW: http://localhost:8082
- Database: PostgreSQL 16
- Runtime: .NET 10
