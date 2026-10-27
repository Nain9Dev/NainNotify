# Traceability Matrix — NainNotify

This matrix maps all functional and architectural requirements to their implementing components, architectural decisions, and automated test verifications.

| Requirement ID | Summary | Source / ADR | Implementation Component | Test Verification | Status |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **REQ-NOTIF-001** | Sub-50ms asynchronous ingestion with HTTP 202 | Charter, ADR-001 | `Notificaciones.Api.Controllers.NotificationsController` | `PipelineIntegrationTests.cs` | Verified |
| **REQ-NOTIF-002** | Dual transport mode (In-Memory & RabbitMQ) | ADR-002 | `src/Notificaciones.Api/Program.cs`, `MassTransitExtensions.cs` | `IntegrationTestWebApplicationFactory.cs` | Verified |
| **REQ-NOTIF-003** | Privacy-by-design & deterministic PII masking | SECURITY.md | `Notificaciones.Infrastructure.Telemetry.PrivacyMasker` | `PrivacyMaskerTests.cs` | Verified |
| **REQ-NOTIF-004** | Honeypot anti-spam defense | Requirements | `NotificationsController.SubmitContactForm` | `SecurityHoneypotIntegrationTests.cs` | Verified |
| **REQ-NOTIF-005** | Client IP partitioned rate limiting | Requirements | `Program.cs` RateLimiter policies | `RateLimiterIntegrationTests.cs` | Verified |
| **REQ-NOTIF-006** | Real-time SSE telemetry streaming (< 20ms) | ADR-003 | `NotificationStreamBroadcaster`, `/api/v1/notifications/stream` | `TelemetryStreamIntegrationTests.cs` | Verified |
| **REQ-NOTIF-007** | VML-bulletproof template rendering | Requirements | `EmailTemplateEngine.cs`, responsive HTML templates | `TemplateEngineTests.cs` | Verified |
| **REQ-NOTIF-008** | SMTP relay with exponential backoff retries | ADR-001 | `SmtpDeliveryService.cs`, MassTransit retry filter | `DeliveryRetryPolicyTests.cs` | Verified |
| **REQ-NOTIF-009** | Zero-credential simulated delivery fallback | Charter | `SimulatedDeliveryService.cs` | `SimulationPipelineIntegrationTests.cs` | Verified |
| **REQ-NOTIF-010** | Reactive telemetry studio & scenario playground | ADR-003 | `NotificationStudio.vue`, `PipelineFlow.vue` | `vue-tsc -b` & Vite build | Verified |
| **REQ-NOTIF-011** | Standalone demo mode resilience | ADR-003 | `useNotifyStream.ts`, `services/api.ts` | `vue-tsc -b` & Vite build | Verified |
| **REQ-NOTIF-012** | Bilingual i18n & ecosystem event sync | Governance | `locales/i18n.ts`, `SiteHeader.vue` | `vue-tsc -b` & Vite build | Verified |
