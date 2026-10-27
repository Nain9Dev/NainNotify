<picture>
  <source media="(prefers-color-scheme: dark)" srcset="docs/assets/banner-dark.svg">
  <source media="(prefers-color-scheme: light)" srcset="docs/assets/banner-light.svg">
  <img src="docs/assets/banner-dark.svg" alt="NainNotify - Pasarela de Notificaciones Asíncronas en la Nube" width="100%">
</picture>

<div align="center">

[![Versión en Inglés](https://img.shields.io/badge/Version-English-blue?style=flat-square)](README.md)
[![Idioma: Español](https://img.shields.io/badge/Idioma-Espa%C3%B1ol-red?style=flat-square)](README.es.md)
[![Demostración en Vivo](https://img.shields.io/badge/Demo%20en%20Vivo-notify.naindev.com-38bdf8?style=flat-square&logo=google-chrome&logoColor=white)](https://notify.naindev.com/)
[![CodeQL](https://github.com/Nain9Dev/NainNotify/actions/workflows/codeql.yml/badge.svg)](https://github.com/Nain9Dev/NainNotify/actions/workflows/codeql.yml)
[![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4?style=flat-square&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![Pruebas de Arquitectura](https://img.shields.io/badge/Architecture%20Tests-NetArchTest%20enforced-80FF00?style=flat-square&logo=checkmarx&logoColor=black)](tests/Notificaciones.UnitTests/Architecture/ArchitectureTests.cs)
[![Abrir en GitHub Codespaces](https://img.shields.io/badge/Open%20in-Codespaces-1f2937?style=flat-square&logo=github&logoColor=white)](https://codespaces.new/Nain9Dev/NainNotify)
[![Vue 3](https://img.shields.io/badge/Vue-3.5-4FC08D?style=flat-square&logo=vuedotjs&logoColor=white)](https://vuejs.org/)
[![TypeScript](https://img.shields.io/badge/TypeScript-5.7-3178C6?style=flat-square&logo=typescript&logoColor=white)](https://www.typescriptlang.org/)
[![Pruebas Superadas](https://img.shields.io/badge/Tests-81%20passing-80FF00?style=flat-square&logo=checkmarx&logoColor=black)](tests)
[![Licencia: MIT](https://img.shields.io/badge/License-MIT-informational?style=flat-square)](LICENSE)

</div>

NainNotify es una pasarela de notificaciones dirigida por eventos y motor worker construido con .NET 10, Clean Architecture y MassTransit. Desacopla la ingestión HTTP de alto rendimiento del procesamiento en segundo plano, la renderización de correos electrónicos (HTML responsivo con compatibilidad VML para Outlook) y la ejecución de proveedores de entrega, mientras transmite los eventos de ciclo de vida del dominio directamente a un cliente de telemetría reactivo Vue 3 + TypeScript mediante Server-Sent Events (SSE).

---

## Visión General de la Arquitectura

```mermaid
graph TB
    subgraph ClientLayer["Capa de Cliente Frontend"]
        VueClient["Cliente Vue 3 + TypeScript<br/><code>Puerto 5378 / notify.naindev.com</code><br/><i>Estudio Guiado y Pipeline en Vivo</i>"]
    end

    subgraph GatewayLayer["Capa API Gateway (:8120)"]
        ApiGateway["ASP.NET Core API Gateway<br/><code>Rate Limiter y Honeypot Anti-Spam</code>"]
        PrivacyMasker["PrivacyMasker<br/><i>Enmascaramiento Cero-PII en el Borde</i>"]
        SSEHub["Canales y Hub SSE<br/><i>Transmisión de Telemetría en Tiempo Real</i>"]
    end

    subgraph BusLayer["Transporte de Bus de Eventos y Mensajería"]
        MassTransit["Bus MassTransit<br/><i>Modo Dual: En Memoria / RabbitMQ</i>"]
        ExQueue[("Exchange NotificationEvent<br/>y Flujos de Ciclo de Vida")]
    end

    subgraph WorkerLayer["Motor de Ejecución Worker"]
        WorkerEngine["Motor Worker en Segundo Plano<br/><i>Consumidores MassTransit</i>"]
        TemplateEngine["EmailTemplateEngine<br/><i>HTML con Blindaje VML y RFC 2822</i>"]
        SmtpRelay["Dispatcher SMTP y<br/>Adaptador de Simulación"]
    end

    VueClient -->|"HTTP POST (Ingesta)"| ApiGateway
    ApiGateway --> PrivacyMasker
    PrivacyMasker -->|"Publicar Evento"| MassTransit
    MassTransit --> ExQueue
    ExQueue -->|"Consumir Lote"| WorkerEngine
    WorkerEngine --> TemplateEngine
    TemplateEngine --> SmtpRelay
    WorkerEngine -->|"Transiciones de Estado<br/>(Aceptado &rarr; Despachado)"| MassTransit
    MassTransit -->|"Enviar Ciclo de Vida"| SSEHub
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

## Gobernanza de Puertos

Todos los puertos de escucha están asignados de manera determinista de acuerdo con `dev-ports.json`:

| Servicio / Componente | Protocolo / Transporte | Puerto | URL / Ruta |
|:---|:---|:---|:---|
| **Cliente NainNotify (Vue 3)** | HTTP (Vite Dev / Preview) | `5378` | `http://localhost:5378` |
| **API Gateway** | HTTP (ASP.NET Core) | `8120` | `http://localhost:8120` |
| **Scalar Interactive API Docs** | HTTP (OpenAPI 3.0) | `8120` | `http://localhost:8120/docs` |
| **Swagger UI** | HTTP | `8120` | `http://localhost:8120/swagger` |
| **Flujo Telemetría SSE** | HTTP EventSource | `8120` | `http://localhost:8120/api/v1/Notifications/stream` |
| **Health Check y Diagnósticos** | HTTP | `8120` | `http://localhost:8120/api/v1/Notifications/health` |
| **Panel de Control RabbitMQ** | HTTP (Docker Compose) | `15672` | `http://localhost:15672` |

---

## Inicio Rápido en 60 Segundos

### 1. Ejecutar el API Gateway y Worker de .NET

Por defecto, el gateway se ejecuta en modo en-memoria sin dependencias externas:

```bash
dotnet run --project src/Notificaciones.Api
```

El gateway escucha en `http://localhost:8120`.

### 2. Ejecutar el Cliente Frontend de Demostración

En un terminal independiente:

```bash
cd Notificaciones.Client
npm install
npm run dev
```

Abre `http://localhost:5378` para iniciar el estudio interactivo guiado.

### 3. Ejecutar con RabbitMQ Distribuido (Docker)

Para ejecutar la topología multi-contenedor (API Gateway, Worker y RabbitMQ):

```bash
docker-compose up -d --build
```

---

## Matriz de Verificación y Pruebas

Ejecuta la suite de pruebas automatizadas a través de todas las capas arquitectónicas:

```bash
# Ejecutar pruebas unitarias y de integración (81 pruebas en total)
dotnet test MicroservicioNotificaciones.sln

# Verificación estricta de tipos TypeScript y compilación del frontend
cd Notificaciones.Client
npm run build
```

---

## Patrones de Diseño Clave y Garantías

1. **Cero Filtración de PII (`PrivacyMasker`)**: Todas las direcciones de destinatarios que entran a los flujos de telemetría públicos se enmascaran en el perímetro (ej. `a***r@naindev.com`).
2. **Defensa Honeypot Anti-Spam**: Los bots que completan campos ocultos reciben respuestas HTTP `202 Accepted` normales para evitar ser alertados, pero su carga útil se descarta antes de llegar a la cola.
3. **Server-Sent Events Desacoplados (SSE)**: El API Gateway mantiene canales de difusión delimitados con latidos de keepalive, transmitiendo cambios de estado con latencia inferior a 20ms.
4. **Plantillas Blindadas con VML**: La generación de correos utiliza Vector Markup Language para garantizar una visualización impecable en clientes Microsoft Outlook de escritorio y web.
