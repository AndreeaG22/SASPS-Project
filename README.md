# DocuStore - Active Record vs Repository Pattern
### Studiu comparativ privind eficiența și mentenabilitatea modelelor de persistență *Active Record* și *Repository Pattern* într-un sistem de gestionare a documentelor.

Matei Andreea-Gabriela, Teodorescu Ioan - EGOV-1  

---

Aplicația **DocuStore** implementează un API backend pentru gestionarea documentelor:
- creare document (titlu, descriere)
- adăugare versiuni (content hash)
- etichete (tags)
- listare / căutare

**Stack:** .NET 10 · PostgreSQL · Docker Compose  
**Testare:** Swagger · Postman · k6 

Se compară două variante:
- **A. Active Record** – logica de persistență inclusă în model (`document.Save()`).
- **B. Repository + Unit of Work** – separare clară între business logic și acces la date.

---

### Rulare proiect

```
docker-compose up -d
```

### Update container pentru incarcarea schimbarilor
```
docker-compose up --build docustore-gateway
```
