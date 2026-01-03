# DocuStore - Teste de Performanță

Suite de teste pentru compararea a două arhitecturi: **Active Record** vs **Repository + Unit of Work**.

## Ce Face?

Măsoară performanța a două implementări ale API-ului DocuStore:
- **Active Record** (port 8080)
- **Repository + Unit of Work** (port 8082)

Testează: viteza de răspuns, throughput, stabilitate, scalabilitate.

## Instalare

### Ce Ai Nevoie

1. **k6** - Tool pentru load testing
   - https://k6.io/docs/getting-started/installation/

2. **Node.js** (v14+) - Pentru analiză rezultate
   - https://nodejs.org/

3. **Docker & Docker Compose** - Pentru aplicații
   - https://docs.docker.com/get-docker/

4. **Minim 8GB RAM** (recomandat 16GB)

### Pași

1. **Pornește serviciile**:
```bash
cd /path/to/sasps
docker-compose up -d
```

2. **Verifică că rulează**:
```bash
curl http://localhost:8080/api/documents
curl http://localhost:8082/api/documents
```

3. **Intră în folder**:
```bash
cd performance-tests
```

## Rulare Teste

### Toate Testele

**Linux/macOS:**
```bash
./run-all-tests.sh
```

**Windows:**
```powershell
.\run-all-tests.ps1
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

| Test | Scop | Durată |
|------|------|--------|
| **Smoke** | Test basic CRUD | 2-3 min |
| **Load** | Sarcină mare (50 useri) | 15 min |
| **Stress** | Găsește limita (până la 200 useri) | 20 min |
| **Scalability Data** | Teste cu 100 → 10,000 documente | 30 min |
| **Scalability Users** | Teste cu 5 → 200 useri | 25 min |
| **Concurrent Writes** | Test scriere intensivă | 7 min |
| **Pagination** | Test listare cu multe date | 35 min |
| **Soak** | Stabilitate 2 ore | 2.5 ore |

## Analiză Rezultate

După teste, rulează:

```bash
node analyze-results.js
```

Generează:
- Raport Markdown cu comparații
- CSV pentru Excel

Rezultatele sunt în `results/` și `reports/`.

## Monitorizare în Timp Real

Accesează **Grafana** pentru grafice live:
- URL: http://localhost:3000
- User/Pass: admin/admin

## Probleme Frecvente

**Conexiune refuzată:**
```bash
# Verifică logurile
docker-compose logs --tail=100
```

**Rate erori mare:**
```bash
# Verifică resursele
docker stats
```

**Debug k6:**
```bash
k6 run --verbose smoke-test.js
```

## Structură

```
performance-tests/
├── config.js              # Configurații
├── utils.js               # Funcții helper
├── smoke-test.js          # Test CRUD basic
├── load-test.js           # Test sarcină mare
├── stress-test.js         # Test limită
├── scalability-*.js       # Teste scalabilitate
├── concurrent-writes-test.js  # Test scriere
├── pagination-test.js     # Test paginare
├── run-all-tests.sh       # Script rulare
├── analyze-results.js     # Analiză rezultate
└── results/               # Rezultate teste
```

## Configurare Personalizată

Editează `config.js` pentru:
- URL-uri
- Praguri performanță
- Durata testelor
- Număr utilizatori

## Metrici Măsurate

- Timp creare document
- Timp citire document
- Timp update document
- Timp ștergere document
- Timp listare documente
- Rate de eroare
- Percentile (p50, p90, p95, p99)