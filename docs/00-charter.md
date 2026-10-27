# Charter — NainNotify

## Mission
NainNotify is an asynchronous, event-driven notification gateway and worker engine built on .NET 10, Clean Architecture, and MassTransit. It completely isolates customer-facing HTTP transactions from volatile external I/O (SMTP relays, third-party messaging APIs, and network latency), guaranteeing deterministic sub-50ms ingestion while ensuring resilient, retryable background dispatch and zero-loss message delivery.

## Strategic Objectives
1. **Low-Latency Non-Blocking Ingestion**: Accept inbound notifications via REST endpoints in under 50ms, returning HTTP `202 Accepted` immediately upon dispatching messages to the internal event bus.
2. **Provider Fault Resilience & Zero Message Loss**: Decouple notification dispatch from upstream external providers (SMTP/MailKit, Webhooks) using MassTransit message topologies with exponential backoff retries and Dead Letter Exchanges (DLX).
3. **Privacy-by-Design & Zero-PII Leakage**: Automatically mask personally identifiable information (PII) at the ingestion boundary before publishing telemetry, persisting logs, or streaming events to browser clients.
4. **Dual Transport Portability**: Support zero-dependency single-process in-memory operation for rapid local evaluation and lightweight demos, alongside full enterprise RabbitMQ clustering for production scale.
5. **Real-Time Operational Observability**: Stream end-to-end domain lifecycle state transitions (Submitted, Queued, Processing, Dispatched, Delivered, Failed) directly to connected frontends via Server-Sent Events (SSE).

## Architectural Invariants
- **Domain Purity**: `Notificaciones.Domain` contains pure domain events, entities, value objects, and enumeration models with zero dependencies on frameworks, databases, or network protocols.
- **Transport Agnosticism**: Application use cases interact solely with MassTransit port abstractions (`IPublishEndpoint`, `ISendEndpoint`), remaining oblivious to whether the underlying transport is an in-memory loopback or a clustered RabbitMQ broker.
- **Fail-Safe Dispatch Simulation**: When external SMTP credentials are not configured, the engine safely routes emails through a simulated delivery pipeline rather than failing or blocking, providing full lifecycle events for portfolio demonstration.
- **Edge Security Defense**: Active rate limiting by client IP and honeypot field traps defend against automated email abuse and denial-of-service attempts.
