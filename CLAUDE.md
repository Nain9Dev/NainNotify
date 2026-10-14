# NainNotify — Developer Guidelines

## Architecture Overview
NainNotify is an enterprise-grade asynchronous notification and messaging gateway within the NainDev ecosystem.
- **Backend**: .NET 10 Clean Architecture (`src/Notificaciones.*`) running on port `8120`.
- **Frontend**: Vue 3 + TypeScript reactive client (`Notificaciones.Client/`) running on port `5378`.
- **Messaging Engine**: MassTransit 8.3 with dual-transport capability:
  - Single-process zero-dependency execution via `UseInMemoryTransport`.
  - Horizontal enterprise scaling with RabbitMQ AMQP broker (`5672` / `15672`).
- **Real-Time Telemetry**: Server-Sent Events (SSE) streaming domain lifecycle events (`NotificationLifecycleEvent`) directly to the browser.
- **Privacy & Security**: Built-in `PrivacyMasker` ensuring zero PII enters application logs, honeypot anti-spam defense, and IP-partitioned rate limiting.

## Development Commands

### Backend (.NET 10)
```powershell
# Run full test suite (78 tests: 55 unit + 23 integration)
dotnet test MicroservicioNotificaciones.sln

# Run API in development (in-memory single process on port 8120)
dotnet run --project src/Notificaciones.Api

# Run API connected to external RabbitMQ
dotnet run --project src/Notificaciones.Api --launch-profile "Notificaciones.Api (RabbitMQ)"

# Run distributed cluster via Docker Compose
docker-compose up -d --build
```

### Frontend (Vue 3 + TypeScript)
```powershell
cd Notificaciones.Client
npm install
npm run dev      # Serves on http://localhost:5378
npm run build    # Type-checks with vue-tsc and builds production bundle
```

## Boundaries & Ports
- Client SPA: `http://localhost:5378`
- REST Gateway & OpenAPI Scalar Docs: `http://localhost:8120/docs`
- Swagger UI: `http://localhost:8120/swagger`
- RabbitMQ Admin: `http://localhost:15672` (guest / guest)
- AMQP Broker: `localhost:5672`
