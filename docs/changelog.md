# Changelog — NainNotify (Microservicio Notificaciones)

All notable changes to this project are documented in this file following Keep a Changelog and Semantic Versioning.

## [1.0.0] - 2026-10-14
### Added
- Decoupled Vue 3 + TypeScript reactive client (`Notificaciones.Client/`) running on pinned port `5378`.
- Interactive notification playground with 5 built-in email templates and recipient sandboxing.
- Real-time Server-Sent Events (SSE) live lifecycle stream component.
- Scalar OpenAPI reference interactive documentation served on `/docs`.
- Formal governance suite: `CLAUDE.md`, `SECURITY.md`, `ADR-001`, `ADR-002`, `ADR-003`, `changelog.md`, `traceability.md`.
- Port registration in `D:\Development\Tools\dev-ports.json` (Web: `5378`, API: `8120`, RabbitMQ: `5672`/`15672`).

### Fixed
- Pinned gateway local port to 8120 across launch settings and docker configurations.
- Deterministic telemetry uptime and address masking.
- Completed 78-test suite (55 unit + 23 integration) with 100% pass rate.
