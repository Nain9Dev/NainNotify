# ADR-002 — Dual-Transport Architecture for Zero-Dependency Demos

- **Status**: Approved
- **Date**: 2026-10-14
- **Deciders**: naindev
- **Scope**: Local development, test execution, and portfolio showcases.

## Context
Running full integration tests or presenting a live portfolio demonstration typically requires installing Docker and booting up RabbitMQ containers, creating operational friction for quick technical evaluations.

## Decision
Support dual-mode transport configuration via `RabbitMq:UseInMemoryTransport`:
1. **Single-Process Demo Mode**: When `UseInMemoryTransport=true`, MassTransit uses its in-memory bus and co-hosts `NotificationConsumer` inside `Notificaciones.Api`. The full pipeline (ingestion -> message -> template -> delivery simulation -> telemetry SSE) functions with zero dependencies.
2. **Distributed Production Mode**: When `UseInMemoryTransport=false`, `Notificaciones.Api` publishes to RabbitMQ on port `5672`, and `Notificaciones.Worker` processes messages in an isolated process.

## Consequences
- Integration tests execute entirely in memory in under 2 seconds without external mocks.
- Developers can review the system with a single `dotnet run` command.
- Production and Docker deployments switch seamlessly to RabbitMQ via environment variables.
