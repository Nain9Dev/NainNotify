# Requirements Specification — NainNotify

## Overview
This document specifies the functional and non-functional requirements for the NainNotify Asynchronous Cloud Notification Gateway, formulated adhering to Easy Approach to Requirements Syntax (EARS).

---

## Ingestion & Pipeline Requirements

### REQ-NOTIF-001: Asynchronous Ingestion Latency
- **Type**: Ubiquitous / Performance
- **Statement**: The API Gateway shall process inbound notification HTTP POST requests and return HTTP `202 Accepted` with a unique notification correlation ID in under 50ms (p95), deferring rendering and dispatch to background consumers.

### REQ-NOTIF-002: Dual Transport Mode
- **Type**: State-driven
- **Statement**: While running with `RabbitMq:UseInMemoryTransport=true`, the system shall host consumers in-process using MassTransit's in-memory bus; when set to `false`, the system shall publish messages to and consume from an external RabbitMQ message broker.

### REQ-NOTIF-003: Privacy-by-Design & PII Masking
- **Type**: Ubiquitous / Security
- **Statement**: The system shall mask recipient email addresses (retaining only domain and masked prefix, e.g., `a***r@naindev.com`) and purge sensitive payload fields before recording telemetry, writing log messages, or emitting SSE streams.

### REQ-NOTIF-004: Honeypot Anti-Spam Defense
- **Type**: Event-driven / Security
- **Statement**: When an inbound notification submission contains a populated hidden honeypot form field (`_gotcha`), the API Gateway shall reject the submission with HTTP `400 Bad Request` or silently discard it without enqueuing it to the bus.

### REQ-NOTIF-005: Client IP Partitioned Rate Limiting
- **Type**: Event-driven / Security
- **Statement**: When a client IP address exceeds the configured sliding rate limit window (e.g., 20 requests per minute), the API Gateway shall reject subsequent requests with HTTP `429 Too Many Requests` and a standard `Retry-After` header.

### REQ-NOTIF-006: Real-Time SSE Telemetry Streaming
- **Type**: Event-driven / Observability
- **Statement**: When domain events occur throughout the notification lifecycle (`Accepted`, `Enqueued`, `Rendering`, `Dispatched`, `Delivered`, `Failed`), the system shall stream structured JSON events to all connected clients over Server-Sent Events (`/api/v1/notifications/stream`) with sub-20ms latency.

---

## Consumer & Rendering Requirements

### REQ-NOTIF-007: VML-Bulletproof Template Rendering
- **Type**: Ubiquitous
- **Statement**: The worker engine shall compile notification templates into RFC 2822 compliant, dual-mode responsive HTML containing Vector Markup Language (VML) fallbacks for legacy Microsoft Outlook desktop clients.

### REQ-NOTIF-008: SMTP Relay with Resilient Retries
- **Type**: Event-driven / Reliability
- **Statement**: When dispatching emails via external SMTP relays, the consumer shall execute exponential backoff retries (initial delay 2s, factor 2, max 5 attempts) upon encountering transient network or socket faults.

### REQ-NOTIF-009: Simulated Delivery Fallback
- **Type**: State-driven
- **Statement**: While external SMTP credentials (`Smtp:Host`) are empty or unset, the worker shall route notifications through the simulation dispatch adapter, emitting successful lifecycle progression events with channel tag `simulation`.

---

## Client & Interaction Requirements

### REQ-NOTIF-010: Reactive Telemetry Studio & Playground
- **Type**: Ubiquitous
- **Statement**: The frontend client (`Notificaciones.Client`) shall render an interactive studio enabling users to trigger pre-configured notification scenarios (Transactional, Security Alert, Welcome Onboarding) and observe pipeline transitions live.

### REQ-NOTIF-011: Standalone Demo Mode Resilience
- **Type**: State-driven
- **Statement**: While the API Gateway or SSE stream is unreachable, the client shall operate in standalone demo mode, generating simulated synthetic pipeline events to demonstrate UI flows without crashing.

### REQ-NOTIF-012: Internationalization & Ecosystem Alignment
- **Type**: Ubiquitous / Accessibility
- **Statement**: The frontend client shall provide bilingual Spanish/English localization with reactive language toggling (`[ES | EN]`), synchronize with the `nain:locale-change` custom event from `<nain-ecosystem-bar>`, and meet WCAG 2.2 Level AA accessibility standards.
