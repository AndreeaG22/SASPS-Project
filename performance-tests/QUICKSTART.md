# Start Rapid

### 1. Pornește Aplicațiile
```bash
cd /path/to/sasps
docker-compose up -d
```

### 2. Rulează Testele

**Toate testele:**
```bash
cd performance-tests
./run-all-tests.sh
```

**Doar testele rapide (recomandat prima dată):**
```bash
./run-all-tests.sh --quick
```

**Pe Windows:**
```powershell
.\run-all-tests.ps1 -Quick
```

### 3. Vezi Rezultatele

**Grafana (timp real):**
- Deschide: http://localhost:3000
- Login: admin/admin

**Sau generează rapoarte:**
```bash
node analyze-results.js
```

Rezultate în:
- `results/` - Date brute (JSON/CSV)
- `reports/` - Rapoarte comparație
- Grafana - Vizualizare live

## Teste Disponibile

| Test | Durată | Useri | Ce Testează |
|------|--------|-------|-------------|
| Smoke | 2 min | 5 | Operații CRUD de bază |
| Load | 10 min | 50 | Sarcină mare |
| Stress | 15 min | 10→200 | Găsește limita |
| Scalability-Data | 12 min | 20 | Date în creștere |
| Scalability-Users | 23 min | 5→200 | Useri în creștere |
| Concurrent Writes | 5 min | 30 | Scriere intensivă |
| Pagination | 12 min | 20 | Listare la scară mare |
| Soak | 2 ore | 30 | Stabilitate long-term |

## Comenzi Utile

```bash
# Doar o implementare
./run-all-tests.sh --target AR        # Active Record
./run-all-tests.sh --target REPO      # Repository

# Un singur test
./run-all-tests.sh --scenario smoke

# Verifică că rulează
curl http://localhost:8080/api/documents
```

**API nu răspunde?**
```bash
docker-compose ps
docker-compose restart
```

**Node.js lipsește?**
```bash
node --version  # Minim v14
```

## Documentație Completă

Vezi [README.md](README.md) pentru detalii.