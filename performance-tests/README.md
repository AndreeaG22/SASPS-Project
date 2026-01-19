# DocuStore - Teste de Performanta

Suite de teste pentru compararea a doua arhitecturi: **Active Record** vs **Repository + Unit of Work**.

## Ce Face?

Masoara performanta a doua implementari ale API-ului DocuStore:
- **Active Record** (port 8080)
- **Repository + Unit of Work** (port 8082)

Testeaza: viteza de raspuns, throughput, stabilitate, scalabilitate.

## Instalare

### Ce Ai Nevoie

1. **k6** - Tool pentru load testing
   - https://k6.io/docs/getting-started/installation/

2. **Node.js** (v14+) - Pentru analiza rezultate
   - https://nodejs.org/

3. **Docker & Docker Compose** - Pentru aplicatii
   - https://docs.docker.com/get-docker/

4. **Minim 8GB RAM** 

### Pasi

1. **Porneste serviciile**:
```bash
cd /path/to/sasps
docker-compose up -d
```

2. **Verifica ca ruleaza**:
```bash
curl http://localhost:8080/api/documents
curl http://localhost:8082/api/documents
```

3. **Intra în folder**:
```bash
cd performance-tests
```

## Rulare Teste

### Toate Testele

**Linux/macOS:**
```bash
./run-all-tests.sh
```

### Mod Rapid (doar testele importante)

```bash
./run-all-tests.sh --quick
```

### Test Individual

```bash
./run-all-tests.sh --scenario smoke
./run-all-tests.sh --scenario load
```

### Doar o Implementare

```bash
./run-all-tests.sh --target AR        # Doar Active Record
./run-all-tests.sh --target REPO      # Doar Repository
```

## Tipuri de Teste

| Test | Scop | Durata |
|------|------|--------|
| **Smoke** | Test basic CRUD | 2-3 min |
| **Load** | Sarcina mare (50 useri) | 15 min |
| **Stress** | Gaseste limita (pâna la 200 useri) | 20 min |
| **Scalability Data** | Teste cu 100 → 10,000 documente | 30 min |
| **Scalability Users** | Teste cu 5 → 200 useri | 25 min |
| **Concurrent Writes** | Test scriere intensiva | 7 min |
| **Pagination** | Test listare cu multe date | 35 min |

## Analiza Rezultate

Dupa teste, ruleaza:

```bash
node analyze-results.js
```

Genereaza:
- Raport Markdown cu comparatii
- CSV pentru Excel

Rezultatele sunt în `results/` si `reports/`.

## Monitorizare în Timp Real

Acceseaza **Grafana** pentru grafice live:
- URL: http://localhost:3000
- User/Pass: admin/admin

## Probleme Frecvente

**Conexiune refuzata:**
```bash
# Verifica logurile
docker-compose logs --tail=100
```

**Rate erori mare:**
```bash
# Verifica resursele
docker stats
```

**Debug k6:**
```bash
k6 run --verbose smoke-test.js
```

## Structura

```
performance-tests/
├── config.js              # Configuratii
├── utils.js               # Functii helper
├── smoke-test.js          # Test CRUD basic
├── load-test.js           # Test sarcina mare
├── stress-test.js         # Test limita
├── scalability-*.js       # Teste scalabilitate
├── concurrent-writes-test.js  # Test scriere
├── pagination-test.js     # Test paginare
├── run-all-tests.sh       # Script rulare
├── analyze-results.js     # Analiza rezultate
└── results/               # Rezultate teste
```

## Configurare Personalizata

Editeaza `config.js` pentru:
- URL-uri
- Praguri performanta
- Durata testelor
- Numar utilizatori

## Metrici Masurate

- Timp creare document
- Timp citire document
- Timp update document
- Timp stergere document
- Timp listare documente
- Rate de eroare
- Percentile (p50, p90, p95, p99)