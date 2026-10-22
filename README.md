<picture>
  <source media="(prefers-color-scheme: dark)" srcset="docs/assets/banner-dark.svg">
  <source media="(prefers-color-scheme: light)" srcset="docs/assets/banner-light.svg">
  <img src="docs/assets/banner-dark.svg" alt="NainNotify - Asynchronous Cloud Notification Gateway" width="100%">
</picture>

<div align="center">

[![Live Demo](https://img.shields.io/badge/Live%20Demo-notify.naindev.com-38bdf8?style=flat-square&logo=google-chrome&logoColor=white)](https://notify.naindev.com/)
[![CodeQL](https://github.com/Nain9Dev/NainNotify/actions/workflows/codeql.yml/badge.svg)](https://github.com/Nain9Dev/NainNotify/actions/workflows/codeql.yml)
[![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4?style=flat-square&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![Architecture Tests](https://img.shields.io/badge/Architecture%20Tests-NetArchTest%20enforced-80FF00?style=flat-square&logo=checkmarx&logoColor=black)](tests/Notificaciones.UnitTests/Architecture/ArchitectureTests.cs)
[![Open in GitHub Codespaces](https://img.shields.io/badge/Open%20in-Codespaces-1f2937?style=flat-square&logo=github&logoColor=white)](https://codespaces.new/Nain9Dev/NainNotify)
[![Vue 3](https://img.shields.io/badge/Vue-3.5-4FC08D?style=flat-square&logo=vuedotjs&logoColor=white)](https://vuejs.org/)
[![TypeScript](https://img.shields.io/badge/TypeScript-5.7-3178C6?style=flat-square&logo=typescript&logoColor=white)](https://www.typescriptlang.org/)
[![Tests Passing](https://img.shields.io/badge/Tests-81%20passing-80FF00?style=flat-square&logo=checkmarx&logoColor=black)](tests)
[![License: MIT](https://img.shields.io/badge/License-MIT-informational?style=flat-square)](LICENSE)

</div>

NainNotify is an event-driven notification gateway and worker engine built with .NET 10, Clean Architecture, and MassTransit. It decouples high-throughput HTTP ingestion from background processing, email rendering (responsive HTML with VML Outlook bulletproofing), and delivery provider execution, while streaming end-to-end domain lifecycle events directly to a reactive Vue 3 + TypeScript telemetry client via Server-Sent Events (SSE).

---

## Architecture Overview

```mermaid
graph TB
    subgraph ClientLayer["Frontend Client Layer"]
        VueClient["Vue 3 + TypeScript Client<br/><code>Port 5378 / notify.naindev.com</code><br/><i>Guided Studio &amp; Live Pipeline</i>"]
    end

    subgraph GatewayLayer["API Gateway Layer (:8120)"]
        ApiGateway["ASP.NET Core API Gateway<br/><code>Rate Limiter &amp; Honeypot Anti-Spam</code>"]
        PrivacyMasker["PrivacyMasker<br/><i>Zero-PII Masking at Edge</i>"]
        SSEHub["SSE Hub &amp; Channels<br/><i>Real-time Telemetry Broadcast</i>"]
    end

    subgraph BusLayer["Messaging &amp; Event Bus Transport"]
        MassTransit["MassTransit Bus<br/><i>Dual-Mode: In-Memory / RabbitMQ</i>"]
        ExQueue[("NotificationEvent Exchange<br/>&amp; Lifecycle Streams")]
    end

    subgraph WorkerLayer["Worker Execution Engine"]
        WorkerEngine["Background Worker Engine<br/><i>MassTransit Consumers</i>"]
        TemplateEngine["EmailTemplateEngine<br/><i>VML-Bulletproof HTML &amp; RFC 2822</i>"]
        SmtpRelay["SMTP Dispatcher &amp;<br/>Delivery Simulation Adapter"]
    end

    VueClient -->|"HTTP POST (Ingest)"| ApiGateway
    ApiGateway --> PrivacyMasker
    PrivacyMasker -->|"Publish Event"| MassTransit
    MassTransit --> ExQueue
    ExQueue -->|"Consume Batch"| WorkerEngine
    WorkerEngine --> TemplateEngine
    TemplateEngine --> SmtpRelay
    WorkerEngine -->|"Lifecycle State Transitions<br/>(Accepted &rarr; Dispatched)"| MassTransit
    MassTransit -->|"Push Lifecycle"| SSEHub
    SSEHub -->|"Server-Sent Events (SSE &lt; 20ms)"| VueClient

    classDef client fill:#0047AB,stroke:#38BDF8,stroke-width:2px,color:#FFFFFF;
    classDef gateway fill:#002B66,stroke:#80FF00,stroke-width:2px,color:#FFFFFF;
    classDef bus fill:#060D1A,stroke:#38BDF8,stroke-width:2px,color:#FFFFFF;
    classDef worker fill:#0A1629,stroke:#94A3B8,stroke-width:2px,color:#FFFFFF;

    class VueClient client;
    class ApiGateway,PrivacyMasker,SSEHub gateway;
    class MassTransit,ExQueue bus;
    class WorkerEngine,TemplateEngine,SmtpRelay worker;
```

---

## Port Governance

All listening ports are allocated deterministically in accordance with `dev-ports.json`:

| Service / Component | Protocol / Transport | Port | URL / Path |
|:---|:---|:---|:---|
| **NainNotify Client (Vue 3)** | HTTP (Vite Dev / Preview) | `5378` | `http://localhost:5378` |
| **API Gateway** | HTTP (ASP.NET Core) | `8120` | `http://localhost:8120` |
| **Scalar Interactive API Docs** | HTTP (OpenAPI 3.0) | `8120` | `http://localhost:8120/docs` |
| **Swagger UI** | HTTP | `8120` | `http://localhost:8120/swagger` |
| **SSE Telemetry Stream** | HTTP EventSource | `8120` | `http://localhost:8120/api/v1/Notifications/stream` |
| **Health Check & Diagnostics** | HTTP | `8120` | `http://localhost:8120/api/v1/Notifications/health` |
| **RabbitMQ Management** | HTTP (Docker Compose) | `15672` | `http://localhost:15672` |

---

## 60-Second Quick Start

### 1. Run the .NET API Gateway & Worker

By default, the gateway runs in zero-dependency in-memory mode, requiring no external databases or message brokers:

```bash
dotnet run --project src/Notificaciones.Api
```

The gateway listens on `http://localhost:8120`.

### 2. Run the Showcase Frontend Client

In a separate terminal:

```bash
cd Notificaciones.Client
npm install
npm run dev
```

Open `http://localhost:5378` to launch the guided studio: pick a scenario, send it, watch it travel through the pipeline and see the rendered email. With the gateway stopped the client runs on its standalone in-browser engine (same experience as the GitHub Pages deployment).

### 3. Run with Distributed RabbitMQ (Docker)

To run the distributed multi-container topology (API Gateway, Worker, and RabbitMQ):

```bash
docker-compose up -d --build
```

---

## Verification & Testing Matrix

Run the automated test suite across all architectural layers:

```bash
# Execute unit and integration tests (78 tests total)
dotnet test MicroservicioNotificaciones.sln

# Type-check and build the frontend client
cd Notificaciones.Client
npm run build
```

### Test Suite Composition

| Test Layer | Framework | Count | Scope |
|:---|:---|:---|:---|
| **Notificaciones.UnitTests** | xUnit | 55 | `PrivacyMasker`, `EmailTemplateEngine` (HTML escaping, VML bulletproofing), and `InMemoryNotificationTelemetryStore` arithmetic. |
| **Notificaciones.IntegrationTests** | xUnit + WebApplicationFactory | 23 | In-memory MassTransit bus, end-to-end 5-stage lifecycle transitions, sandbox recipient redirection, rate limiter policies, honeypot anti-spam, and SSE streaming. |
| **Notificaciones.Client** | vue-tsc + Vite | 1 | TypeScript strict template verification, contract alignment, production bundle generation. |

---

## Key Design Patterns & Guarantees

1. **Zero-PII Telemetry Leakage (`PrivacyMasker`)**: All recipient addresses entering public telemetry streams and event stores are masked at the edge (e.g., `e*******r@c*********p.com`).
2. **Honeypot Anti-Spam Gate**: Automated bots completing hidden input fields are met with standard `202 Accepted` responses to prevent detection, while their payloads are dropped before enqueuing.
3. **Decoupled Server-Sent Events (SSE)**: The API Gateway maintains bounded broadcast channels with keepalive heartbeats, broadcasting state changes without poll overhead.
4. **VML Bulletproof Email Layouts**: HTML email generation utilizes Vector Markup Language fallbacks, ensuring correct multi-column rendering across Microsoft Outlook desktop and web clients.

---

## Repository Structure

```
.
├── src/
│   ├── Notificaciones.Api/             # ASP.NET Core gateway, rate-limiting, SSE broadcast
│   ├── Notificaciones.Application/     # MassTransit consumers, email template rendering
│   ├── Notificaciones.Domain/          # Core events, NotificationLifecycleEvent contracts
│   ├── Notificaciones.Infrastructure/  # Smtp client, configuration loaders, serialization
│   └── Notificaciones.Worker/          # Standalone background worker entry point
├── Notificaciones.Client/              # Decoupled Vue 3 + TypeScript showcase client (port 5378)
├── tests/
│   ├── Notificaciones.UnitTests/       # 55 unit tests
│   └── Notificaciones.IntegrationTests/# 23 integration tests
├── docs/                               # Architecture decisions (ADRs), changelog, traceability
└── specs/                              # Spec-Driven Development specifications and task plans
```
