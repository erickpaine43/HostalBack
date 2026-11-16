# Hostal Vista Azul — API

Backend for a small hotel's management system: rooms, guests, reservations and housekeeping staff. Built with **ASP.NET Core 8**, **Entity Framework Core** and **SQL Server**, containerized with **Docker**.

Frontend: [HostalFront](https://github.com/erickpaine43/HostalFront) · live at https://hostal-front.vercel.app

## Tech stack

- ASP.NET Core 8 Web API, C#
- Entity Framework Core 8 (SQL Server, code-first migrations)
- Docker multi-stage build + docker-compose with health check
- Swagger / OpenAPI
- Global error-handling middleware

## Features

- **Reservations**: create, update, cancel, **check-in** and **change room** on an active stay
- **Rooms**: update status, mark as out of service, assign/unassign housekeepers
- **Guests** and **housekeepers**: full CRUD
- **Queries**: available rooms for a date range, rooms per housekeeper, currently active guests
- **Audit trail**: important operations write a `Traza` record (operation, table, record id, details, timestamp), exposed through `GET /api/consultas/auditoria-trazas`
- `GET /health` endpoint used by the container health check

## Project structure

```
Controllers/   Reservas, Habitaciones, Clientes, AmasDeLlaves, Consultas
Modelos/       EF Core entities
Data/          DbContext
Dto/           Request/response contracts
Middlewares/   ErrorHandlingMiddleware (consistent JSON errors + logging)
Migrations/    EF Core migrations
```

## Running locally

Requirements: .NET 8 SDK and SQL Server.

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=...;Database=hostal_db;..."
dotnet ef database update
dotnet run
```

### With Docker

The connection string is read from the environment; put it in a `.env` file that is **not** committed:

```bash
# .env
DB_CONNECTION_STRING=Server=...;Database=hostal_db;User Id=...;Password=...;TrustServerCertificate=True;
```

```bash
docker compose up --build
```

The API listens on http://localhost:8080 and Swagger is at `/swagger`.
