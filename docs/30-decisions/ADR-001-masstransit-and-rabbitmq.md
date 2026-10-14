# ADR-001 — Asynchronous Notification Ingestion via MassTransit and RabbitMQ

- **Status**: Approved
- **Date**: 2026-10-14
- **Deciders**: naindev
- **Scope**: Messaging middleware, ingestion latency, and decoupling of delivery providers.

## Context
Directly invoking SMTP providers (e.g. Resend, Mailtrap, SendGrid) inside HTTP request handlers causes unbounded response latency (200ms - 3000ms) and catastrophic failure when the third-party mail provider suffers transient network downtime.

## Decision
Adopt **MassTransit 8.3** as the messaging abstraction layer over **RabbitMQ**:
1. The HTTP Gateway (`Notificaciones.Api`) validates the payload and publishes a `NotificationEvent` to the broker in under 50ms, returning `202 Accepted` immediately.
2. The asynchronous consumer (`NotificationConsumer`) in `Notificaciones.Worker` handles templating, retry policies with exponential backoff, and SMTP delivery.
3. Decouple domain events from third-party transport mechanisms.

## Consequences
- Guaranteed sub-50ms ingestion latency for client applications.
- Resilience against SMTP provider outages with message durability in RabbitMQ queues.
- Outbox pattern compatibility for transactional consistency with relational databases.
