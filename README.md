# DocuStore - Active Record vs Repository Pattern
### Studiu comparativ privind eficiența și mentenabilitatea modelelor de persistență *Active Record* și *Repository Pattern* într-un sistem de gestionare a documentelor.

Matei Andreea-Gabriela, Teodorescu Ioan - EGOV-1  

---

Aplicația **DocuStore** implementează un API backend pentru gestionarea documentelor:
- creare document (titlu, descriere)
- adăugare versiuni (content hash)
- etichete (tags)
- listare / căutare

**Stack:** .NET 9 · PostgreSQL · Docker Compose  
**Testare:** Swagger · Postman · k6 

Se compară două variante:
- **A. Active Record** – logica de persistență inclusă în model (`document.Save()`).
- **B. Repository + Unit of Work** – separare clară între business logic și acces la date.

---

### Rulare proiect

```
dotnet run --project docustore-activerecord/src/DocuStore.AR.API/DocuStore.AR.API.csproj
dotnet run --project docustore-repository-uow/src/DocuStore.Api/DocuStore.Api.csproj
```
