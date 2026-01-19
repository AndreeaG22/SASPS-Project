# DocuStore
Andreea-Gabriela Matei - EGOV-1B
Ioan Teodorescu - EGOV-1B

O privire de ansamblu peste cele doua implementari pentru solutia DocuStore care compara **Active Record** cu **Repository + Unit of Work**. Ambele fac acelasi lucru: administreaza documente (CRUD), versiuni, tag-uri si search.

## 1. Structura Proiectului & Stratificarea

- **Module:** Document, Versioning, Tagging, MetadataIndexing (plus utilitare comune) in ambele variante `docustore-activerecord/` si `docustore-repoUow/`.
- **Straturi pe fiecare modul:**
  - **API:** Endpoint-uri minimal API grupate pe resursa (de exemplu `/api/documents`, `/api/versions`, `/api/tags`, `/api/search`). Swagger e pornit prin `DocuStore.Gateway`.
  - **Application:** Comenzi si interogari MediatR plus validatori. Coordoneaza actiunile din domain si pune la punct evenimente intre module.
  - **Domain:** 
    - *Active Record:* Entitatile mostenesc `ActiveRecordBase`, au logica de salvare in baza de date si iau dependentele printr-un `ServiceLocator` static (`Document.Domain/Entities/DocumentEntity.cs`).
    - *Repository+UoW:* Entitatile din domain sunt obiecte simple fara sa stie de baza de date; contractele pentru salvare stau in `Document.Application/Interfaces`.
  - **Infrastructure:** DbContexts EF (Entity Framework) Core, migratii si configurare per modul. Repository+UoW pune repositories si units of work; Active Record configureaza in principal DbContexts si porneste service locators in `DocuStore.Gateway/Program.cs`.

## 2. Endpoint-uri & Design API

- **Documents (administram CRUD si download pentru documente):**
  - `POST /api/documents` — face un document cu metadata si fisier incarcat
  - `GET /api/documents` — listeaza toate documentele (fara paginare deocamdata)
  - `GET /api/documents/{id}` — ia metadata unui singur document
  - `PUT /api/documents/{id}` — schimba titlul/descrierea
  - `DELETE /api/documents/{id}` — sterge soft un document
  - `GET /api/documents/{id}/download` — ia fisierul versiunii curente

- **Versions (urmarim si promovam versiunile documentelor):**
  - `POST /api/versions` — pune o versiune noua pentru un document
  - `GET /api/versions/document/{docId}` — listeaza istoricul versiunilor pentru un document
  - `PUT /api/versions/document/{docId}/set-current` — promoveaza o versiune ca fiind curenta
  - `GET /api/versions/document/{docId}/version/{n}/download` — ia o versiune specifica

- **Tagging (organizam documentele cu tag-uri):**
  - `POST /api/tags` — face un tag
  - `GET /api/tags` — listeaza tag-urile (optional cu numarul de documente)
  - `POST /api/tags/documents/{docId}/tags` — pune un tag la un document
  - `DELETE /api/tags/documents/{docId}/tags/{tagId}` — scoate un tag de pe un document
  - `GET /api/tags/documents/{docId}/tags` — listeaza tag-urile de pe un document
  - `GET /api/tags/{tagId}/documents` — listeaza documentele cu un anumit tag
  - `GET /api/tags/documents?tagIds=a,b` — listeaza documentele care au toate tag-urile specificate

- **Search (cautare indexata peste documente):**
  - `GET /api/search/documents` — cautare dupa cuvinte cheie/data/creator cu paginare si sortare
  - `POST /api/search/reindex` — rebuilduieste indexul de cautare pentru toate documentele

- **REST API Design:**
  - Controller-ele sunt exprimate ca minimal APIs; Repository+UoW tine compozitia strict in host builder, pe cand Active Record porneste si service locators

## 3. Pattern-uri Folosite

- **Comune ambelor:** Mapping intre DTO-uri (API) si domain, MediatR pentru procesarea request-urilor, validatori FluentValidation pe comenzi/interogari, si o arhitectura modulara monolitica stratificata per context delimitat.

- **Specifice Active Record:**
  - Entitatile din domain se ocupa singure de salvare si interogari (ex. `Save`, `Find`, `All`) si publica direct evenimente dupa ce scriu in baza de date
  - Dependentele sunt luate printr-un `ServiceLocator` static, ceea ce leaga strans logica de business de infrastructura si de stare globala statica
  - Exemplu (`docustore-activerecord/src/Document/Document.Domain/Entities/DocumentEntity.cs`):
    ```csharp
    public async Task UploadAndSave(byte[] fileContent, CancellationToken ct = default)
    {
        FilePathOnDisk = await GetService<IFileStorageService>()
            .CreateDocumentFolderAsync(this.Id, this.FileName, ct);
        await Save(ct); // scrie prin DbContext din ServiceLocator
        await GetService<IEventPublisher>().PublishAsync(new DocumentCreatedEvent(...), ct);
    }
    ```

- **Specifice Repository + Unit of Work:**
  - Contractele pentru salvare (`IDocumentRepository`, `IUnitOfWork`) si helperii pentru tranzactii sunt injectate; domain-ul nu stie nimic despre baza de date
  - Handler-ele din application pun la punct repositories si publicarea evenimentelor:
    ```csharp
    public async Task<DocumentDto> Handle(CreateDocumentCommand request, CancellationToken ct)
    {
        var document = DocumentEntity.Create(...);
        var path = await _fileStorageService.CreateDocumentFolderAsync(document.Id, document.FileName, ct);
        document.SetFileInfo(path, request.FileContent.Length);
        await _unitOfWork.Documents.AddAsync(document, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        await _eventPublisher.PublishAsync(new DocumentCreatedEvent(...), ct);
        return new DocumentDto(...);
    }
    ```
  - Accesul la baza de date e centralizat in repositories; UnitOfWork da limite optionale de tranzactie

## 4. Anti-Pattern-uri & Probleme de Cod

- **Active Record:**
  - `ServiceLocator` static + `DocumentDbContextProvider` in codul de domain leaga prea strans logica de business de EF si de stare globala, face testarea grea si urmarirea dependentelor complicata
  - Metodele din domain amesteca salvarea, orchestrarea si validarea; evenimentele sunt publicate in afara unei limite explicite de tranzactie
  - Interogari precum `All/Where/Count` ruleaza direct din entitati, incurajeaza imprastierea logicii peste domain in loc sa treaca prin servicii de aplicatie/repositories
  - Testele au nevoie de o instanta live de PostgreSQL; pica cand baza de date Docker nu e disponibila (vezi §6 Strategia de Testare)

- **Repository + UoW:**
  - Abstractia extra pune cod repetitiv si crestere pe curba de invatare
  - UnitOfWork da metode de tranzactie, dar majoritatea handler-elor doar apeleaza `SaveChangesAsync`, deci workflow-urile cross-aggregate pot inca sa nu aiba coordonare tranzactionala explicita

## 5. Alegeri Arhitecturale

**Bune:**
- Limite clare intre module pentru Document, Versioning, Tagging si Search
- Suprafata API consistenta cu Swagger, CORS si handler-e bazate pe MediatR
- Integrare event-driven (ex. `DocumentCreatedEvent` alimenteaza versioning/indexing) incurajeaza decuplarea intre module
- Varianta Repository+UoW separa curat responsabilitatile si e foarte usor de testat (teste pure de domain, infrastructura mockata)

**Discutabile:**
- Service locator-ul din Active Record si accesul static la DbContext pun dependente ascunse si fac testarea unitara grea; salvarea si regulile de business sunt impreuna
- Lipsa paginarii pe endpoint-urile de listare documente ar putea afecta performanta la scara mare
- Publicarea evenimentelor nu e invelita intr-o tranzactie in nicio varianta; esuari dupa scrieri in baza de date pot lasa modulele downstream inconsistente
- Warning-uri de versiuni de pachete in infrastructura Repository (EF vs Npgsql release candidate) ar putea deveni o problema de mentenanta daca raman nerezolvate

## 6. Strategia de Testare

### Teste Unitare & Domain
- **Repository+UoW:** 
  - 97 teste scrise, 86 ruleaza cu succes
  - ~0.6 secunde timp total de rulare
  - Teste rapide si izolate (ex. `Document.Domain.Tests/Entities/DocumentEntityTests.cs`)
  - Handler-e din application cu repositories/evenimente mockate
  - Baza de date InMemory noua per test → comportament determinist
  - Fara infrastructura externa, disposal sigur al contextului
  
- **Active Record:** 
  - 85 teste scrise, 18 eliminate (21%), 67 ramase
  - ~2.2 secunde timp de rulare
  - Teste in stil de integrare impotriva PostgreSQL (`docustore-activerecord/tests/...`)
  - Au nevoie de Docker + PostgreSQL + migratii aplicate
  - Rularea locala curenta a picat pentru ca Postgres era inaccesibil (`Connection refused` pe `127.0.0.1:5432`)
  - Izolare bazata pe tranzactii, dar citirile nu sunt izolate → stare partajata intre teste
  - Provideri statici de DbContext → memory leaks, consum ~3× mai mare de memorie
  - Database locks si timeout-uri din cauza contextelor statice partajate

### Code Coverage (Cobertura)
- **Repository+UoW:**
  - Line coverage: ~5.5% → ~52.1% (depinde de scope-ul analizat)
  - Branch coverage: 91.66% (cel mai bun, in module cu logica de business) → 7.54% (la nivel de sistem)
  - Coverage relevant, scade treptat pe masura ce aplicatia creste
  - Testabilitate ridicata pentru logica decizionala
  
- **Active Record:**
  - Line coverage: ~15.5% → ~41.7% (pare mai mare dar nu spune nimic despre logica)
  - Branch coverage: 61.76% (cel mai bun, doar pe scope foarte restrans) → 2.46% (aproape toata logica ramane netestata)
  - Coverage se prabuseste rapid cand scope-ul creste
  - Logica ascunsa in metode globale (All, Where, Count) nu poate fi testata izolat

### Metrici de Calitate
- **Repository+UoW:** Cu 46% mai putina cyclomatic complexity, cu 56% mai putin class coupling, aderenta la SOLID
- **Active Record:** Incalcari SRP si DIP, metode complexe si strans cuplate

### Performance
Suite-uri k6 in `performance-tests/` cu rezumate generate (`run-all-tests.sh` / `analyze-results.js`). Scenariile acopera:
- Smoke (baseline CRUD: 5 users, 2 minute)
- Load (50 users, 10 minute, mixed workload)
- Stress (ramp 10→200 users, 15 minute)
- Data volume scalability (100→1K→10K documente, 20 users)
- User concurrency scalability (5→200 users in etape, 2K documente)
- Soak (30 users, 2 ore)
- Pagination si scrieri concurente

## 7. Metrici Comparative

Date din raportul k6 (`performance-tests/reports/comparison-report-2026-01-03T13-30-18.md`):

### Smoke Test (Baseline CRUD - 5 users, 2 minute)
| Metrica | Active Record | Repository + UoW | Diferenta | Castigator |
|---------|---------------|------------------|-----------|------------|
| Timp mediu raspuns | 5.18ms | 4.95ms | -4.41% | Repository |
| P95 | 8.79ms | 7.82ms | -11.06% | Repository |
| Throughput | 3.80 req/s | 3.80 req/s | +0.08% | Repository |
| Total requests | 229 | 229 | - | - |
| Rate erori | 0.00% | 0.00% | - | Repository |

**Takeaway:** Repository putin mai rapid, 0% erori in ambele.

### Load Test (50 users, 10 minute, mixed workload)
| Metrica | Active Record | Repository + UoW | Diferenta | Castigator |
|---------|---------------|------------------|-----------|------------|
| Timp mediu raspuns | 6.45ms | 6.16ms | +4.77% | Repository |
| P95 | 28.96ms | 19.49ms | +48.56% | Repository |
| Throughput | 2.50 req/s | 2.65 req/s | -5.47% | Repository |
| Total requests | 503 | 489 | -2.78% | - |
| Rate erori | 0.80% | 0.20% | -74.28% | Repository |

**Takeaway:** Repo mai rapid dar AR mai putin fiabil (0.80% erori vs 0.20%).

### Stress Test (ramp 10→200 users, 15 minute)
| Metrica | Active Record | Repository + UoW | Diferenta | Castigator |
|---------|---------------|------------------|-----------|------------|
| Timp mediu raspuns | 10.29ms | 8.42ms | -18.19% | Repository |
| P95 | 39.50ms | 30.77ms | -22.09% | Repository |
| Throughput | 3.01 req/s | 3.69 req/s | +22.56% | Repository |
| Total requests | 990 | 1170 | +18.18% | - |
| Rate erori | 1.37% | 0.91% | +50.43% | Repository |

**Takeaway:** Repository castiga clar sub stress.

### Data Volume Scalability (100→1K→10K documente, 20 users)
| Metrica | Active Record | Repository + UoW | Diferenta | Castigator |
|---------|---------------|------------------|-----------|------------|
| Timp mediu raspuns | 3.70ms | 3.88ms | +4.71% | Active Record |
| P95 | 7.48ms | 7.71ms | +3.15% | Active Record |
| Throughput | 4.67 req/s | 4.75 req/s | +1.78% | Repository |
| Total requests | 2027 | 2060 | +1.63% | - |
| Rate erori | 0.00% | 0.00% | - | Repository |

**Takeaway:** Performanta aproape identica, 0% erori in ambele.

### User Concurrency Scalability (5→200 users in etape, 2K documente)
| Metrica | Active Record | Repository + UoW | Diferenta | Castigator |
|---------|---------------|------------------|-----------|------------|
| Timp mediu raspuns | 8.08ms | 8.40ms | +3.93% | Active Record |
| P95 | 31.71ms | 33.45ms | +5.50% | Active Record |
| Throughput | 2.36 req/s | 2.30 req/s | -2.66% | Active Record |
| Total requests | 818 | 798 | -2.44% | - |
| Rate erori | 1.13% | 0.37% | +207.52% | Repository |

**Takeaway:** Ambele au diferente mic (>6%), dar Repo are rate de eroare mai stabile.

### Concluzie Generala
Active Record castiga la unele cifre de latenta sub sarcina normala, dar Repository+UoW arata rate de eroare mai stabile in scenarii critice (smoke/load) si performanta mai buna sub stress.

## 8. Rezumatul Compromisurilor

- **Active Record**
  - Flow simplu (entitatile isi administreaza singure salvarea), mai putine straturi de inteles, latenta putin mai mica in unele rulari
  - Cuplare stansa la EF si service locator, mai greu de testat unitar, consistenta evenimentelor depinde de starea implicita a DbContext, modificarile de scalare risca editari transversale

- **Repository + Unit of Work**
  - Separarea responsabilitatilor, testabilitate inalta, limite de tranzactie mai clare, repositories centralizeaza interogarile. Potrivit pentru cerinte in evolutie
  - Mai mult cod repetitiv (interfete, UoW, configurare DI) si o curba de invatare mai abrupta; are nevoie de disciplina pentru folosirea tranzactiilor peste handler-e

**Pe scurt:** ambele implementari servesc acelasi API, dar Repository + UoW sustine mai bine schimbarile, testarea si claritatea tranzactionala, in timp ce Active Record sacrifica aceste calitati pentru un model mental mai simplu si castiguri marginale de latenta in scenarii selectate. Alege in functie de prioritate: viteza de livrare initiala (Active Record) sau mentenabilitate si corectitudine pe termen lung (Repository + UoW).

### Rulare proiect

```
docker-compose up -d
```

### Update container pentru incarcarea schimbarilor
```
docker-compose up --build docustore-gateway / docustore-gateway-repo
```
