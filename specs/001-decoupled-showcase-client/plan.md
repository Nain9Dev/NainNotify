# Technical Implementation Plan: NainNotify Client

## 1. Architecture & Port Governance
1. Confirm port `5378` registration in `D:\Development\Tools\dev-ports.json`.
2. Configure `vite.config.ts` pinning port `5378` (`strictPort: true`) with reverse proxy for `/api` and `/health` to `http://localhost:8120`.

## 2. Component Design
1. `HeaderNav.vue`: Branding (`NainNotify Gateway`), broker status pill (`in-memory` vs `rabbitmq`), SSE stream connection state, and OpenAPI links.
2. `TelemetryCards.vue`: KPI cards for dispatched notifications, in-flight jobs, p95 latency, and success percentage.
3. `NotificationPlayground.vue`: Fast 1-click test triggers, custom email recipient input, priority dropdown, and honeypot validation demo.
4. `LiveEventStream.vue`: Real-time SSE lifecycle events console (`Enqueued`, `Rendering`, `Dispatched`, `Failed`) with animated event pills.
5. `TemplatePreviewModal.vue`: Multipart email viewer for reviewing rendered HTML and plain-text output.
6. `PrivacyNotice.vue`: Information card on `PrivacyMasker` and sandbox policies.

## 3. Verification & Build
1. Build client with `npm run build` (`vue-tsc -b && vite build`).
2. Run full .NET test suite `dotnet test MicroservicioNotificaciones.sln` (78 tests).
