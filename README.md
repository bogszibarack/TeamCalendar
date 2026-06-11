# Szabadság Naptár és Ügyeleti Beosztás

Egy könnyen indítható, "modern" webalkalmazást raktam össze, ami simává teszi a csapat szabadságainak kezelését. Közben automatikusan figyeli az ügyeleti beosztásokat és kiszűri az ütközéseket. Nem volt annyira egyszerű az összes konfigurációs és logikai gubancot kibogozni. A végeredmény egy működő, reszponzív rendszer lett.

## Tech Stack
- **Backend:** .NET 8 Web API (Minimal APIs architektúra)
- **Frontend:** Angular 18 (Modern Zoneless változáskövetési architektúra)
- **Adatbázis:** Entity Framework Core In-Memory adatbázissal
- **API Dokumentáció:** Swagger / OpenAPI

---

## Telepítési és indítási útmutató

### Előfeltételek
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Node.js (v18 vagy újabb)](https://nodejs.org/)


### 1. A Backend futtatása (.NET API)
1. Nyiss egy terminált, és lépj be a backend mappába:
   cd backend

2. .NET elindítása
    dotnet run

3. A szerver elindul és itt fog "élni": http://localhost:5259

4. Swagger API dokumentáció + teszt: http://localhost:5259/swagger

5. Második terminál fül és lépj be a frontend mappába
    cd frontend

6. Indítsd el az Angulart
    npx ng serve

7. Nyiss egy böngészőt, itt fog "élni": http://localhost:4200


Főbb döntések és feltételezések
In-Memory Adattárolás: EF Core In-Memory adatbázist használtam, hogy az alkalmazás 100%-ban önellátó legyen, és konfigurációs hibák nélkül azonnal elinduljon.

Zoneless Angular: Az Angular legújabb kísérleti Zoneless architektúráját alkalmaztam a jobb teljesítmény érdekében (ChangeDetectorRef).

Ütközéskezelés: Beépítettem egy olyan logikát, ami azonnal jelzi és letiltja az Approve gombot, ha egy vezető pl. véletlenül olyan szabadságot fogadna el, ami ütközik az adott dolgozó ügyeleti hetével.


Megvalósított opcionális fejlesztések (Extrák)
[x] Szabadság jóváhagyási munkafolyamat: Interaktív Approve/Reject gombok a vezetőnek.

[x] Vizuális ütközés-kiemelés: CSS figyelmeztetések és dinamikus hibaüzenetek a felületen.

[x] REST API dokumentáció: Teljes, azonnal tesztelhető Swagger UI.