# Traceability Matrix — NainNotify

| Requirement ID | Description | Source / Gate | Implementation Component | Test Verification |
| -------------- | ----------- | ------------- | ------------------------ | ----------------- |
| REQ-01 | Asynchronous ingestion with sub-50ms latency | Charter, ADR-001 | `Notificaciones.Api.Controllers.NotificationsController` | `PipelineIntegrationTests.cs` |
| REQ-02 | Dual transport mode (in-memory & RabbitMQ) | ADR-002 | `src/Notificaciones.Api/Program.cs` | `IntegrationTestWebApplicationFactory.cs` |
| REQ-03 | Privacy-by-design & PII masking | SECURITY.md | `Notificaciones.Infrastructure.Telemetry.PrivacyMasker` | `PrivacyMaskerTests.cs` |
| REQ-04 | Honeypot anti-spam defense | Requirements | `NotificationsController.SubmitContactForm` | `SecurityHoneypotIntegrationTests.cs` |
| REQ-05 | Client IP partitioned rate limiting | Requirements | `Program.cs` RateLimiter policies | `RateLimiterIntegrationTests.cs` |
| REQ-06 | Realtime SSE telemetry streaming | ADR-003 | `NotificationStreamBroadcaster`, `/api/v1/Notifications/stream` | `TelemetryStreamIntegrationTests.cs` |
| REQ-07 | Reactive Vue 3 client & Playground | ADR-003 | `Notificaciones.Client/` (port 5378) | `vue-tsc -b` & Vite build |
