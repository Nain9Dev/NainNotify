# Runbook — NainNotify

## Assigned Ports & Endpoints

NainNotify is assigned port **8120** (within the `8100-8199` backend allocation defined in `PORTS.md`). The port is pinned across three configuration points that must remain synchronized:

| Environment | Configuration File | Assigned Port / URL |
| :--- | :--- | :--- |
| **Local Debug** | `src/Notificaciones.Api/Properties/launchSettings.json` | `http://localhost:8120` |
| **Container Image** | `Dockerfile` | `EXPOSE 8120`, `ASPNETCORE_URLS=http://+:8120` |
| **Compose Service** | `docker-compose.yml` | `"8120:8120"` |
| **Vue 3 Client** | `Notificaciones.Client/vite.config.ts` | Port `5378` (`strictPort: true`) |

Kestrel is configured to fail fast on startup if port 8120 is claimed by another process. If port collisions occur:

```powershell
netstat -ano | findstr :8120
Get-Process -Id <pid>
```

Related infrastructure ports reserved by this service:
- **5672**: RabbitMQ AMQP broker
- **15672**: RabbitMQ Management UI (`guest` / `guest`)

---

## Startup & Execution Modes

### Mode A: Standalone In-Memory (Zero External Dependencies)

```bash
dotnet run --project src/Notificaciones.Api
```

The default `Development` profile configures `RabbitMq:UseInMemoryTransport=true`, hosting MassTransit consumers in-process. The full asynchronous pipeline operates end-to-end without Docker or an external broker.

- Operations & Telemetry Console: <http://localhost:8120>
- OpenAPI Documentation: <http://localhost:8120/swagger>
- Client Development Server:
  ```bash
  cd Notificaciones.Client
  npm run dev
  ```
  Accessible at <http://localhost:5378>.

To run against a local RabbitMQ broker without Docker:

```bash
dotnet run --project src/Notificaciones.Api --launch-profile "Notificaciones.Api (RabbitMQ)"
```

> **Warning**: Do not enable in-memory transport in multi-instance production environments; messages published in one instance are invisible to consumer workers in other processes.

### Mode B: Clustered Deployment with Docker Compose

```bash
docker-compose up -d --build
```

Starts three containers: `rabbitmq`, `notificaciones-api`, and `notificaciones-worker`. The compose configuration injects `RabbitMq__UseInMemoryTransport=false`, forcing all instances onto the shared AMQP exchange.

- Live Dashboard & OpenAPI: <http://localhost:8120>
- RabbitMQ Management: <http://localhost:15672>
- Worker logs stream:
  ```bash
  docker logs -f notificaciones-worker
  ```

---

## Verification & Health Probes

### Run Automated Test Suite

```bash
dotnet test MicroservicioNotificaciones.sln
```

The integration test suite boots the ASP.NET Core test server against the in-memory bus, covering the five-stage pipeline, recipient sandboxing, rate limiting, honeypot traps, and static file hosting. Zero network dependencies or SMTP credentials are required.

### Dependency Health Check

```bash
curl http://localhost:8120/api/v1/notifications/health
```

Expected response:

```json
{
  "status": "Healthy",
  "transport": "in-memory",
  "bus": "Connected",
  "timestamp": "2026-10-27T13:14:00Z"
}
```

---

## Known Operational Boundaries

1. **In-Memory Telemetry Projection**: The real-time read model is maintained in volatile process memory and resets on service restart. It powers the live operations console and is not an immutable audit ledger.
2. **Instance-Local Projections**: When scaling the API Gateway behind a reverse proxy, each gateway replica streams SSE events observed through its own subscription.
3. **Simulated Dispatch Channel**: In the absence of validated SMTP relay credentials, the delivery adapter tags dispatched emails with channel `simulation` and records delivery without connecting to an external mail server.
4. **Socket-Level Rate Limiting**: Behind a reverse proxy (e.g., NGINX, Cloudflare, Traefik), ensure `ForwardedHeaders:Enabled` is set with configured trusted proxies; otherwise, all clients will share the proxy IP and trigger rate limit exhaustion prematurely.
