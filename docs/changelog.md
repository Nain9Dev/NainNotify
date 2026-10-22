# Changelog — NainNotify (Microservicio Notificaciones)

All notable changes to this project are documented in this file following Keep a Changelog and Semantic Versioning.

## [1.1.0] - 2026-10-22
### Changed
- Client redesigned as a guided studio: short hero with one call to action, animated pipeline with a compact KPI strip, one-click scenarios, live email preview and an activity feed grouped per notification (`specs/002-client-visual-redesign/`).
- Pipeline stages are played with a minimum dwell time so the flow stays readable when the real gateway completes in under 50 ms.
- Standalone engine paces stages over about two seconds and masks recipients like the server-side `PrivacyMasker`.

### Fixed
- On `localhost:5378` with the gateway stopped, the client now falls back to the standalone engine instead of rendering empty metrics.
- Horizontal overflow below 1100 px removed; layout verified at 375, 768 and 1024 px.

### Removed
- `HeaderNav`, `TelemetryCards`, `PipelineTrack`, `NotificationPlayground`, `LiveEventStream` and `PrivacyNotice` components, superseded by the studio components.

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
