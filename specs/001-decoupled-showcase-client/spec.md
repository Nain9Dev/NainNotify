# Feature Specification: Decoupled Vue 3 Client for NainNotify

## 1. Context and Problem Statement
NainNotify is an enterprise .NET 10 asynchronous notification microservice. While its embedded console in `wwwroot` serves basic testing, building an interactive commercial showcase aligned with `naindev.com` requires a modern Vue 3 + TypeScript single-page application with dark glassmorphism styling, real-time Server-Sent Events (SSE) telemetry, and an interactive template testing playground.

## 2. Requirements (EARS)
- `REQ-NOTIF-01`: The system shall provide a decoupled Vue 3 + TypeScript SPA hosted on pinned local port `5378`.
- `REQ-NOTIF-02`: The client shall establish an EventSource connection to `/api/v1/Notifications/stream` to receive real-time domain lifecycle events without polling.
- `REQ-NOTIF-03`: The client shall offer an interactive notification dispatcher allowing instant testing of all 5 built-in templates (`SecurityAlert`, `InvoiceReceipt`, `TeamInvite`, `PasswordReset`, `ContactFormSubmission`).
- `REQ-NOTIF-04`: The client shall display a live KPI dashboard showing dispatched count, in-flight count, failed count, and p95 latency.
- `REQ-NOTIF-05`: The client shall allow live preview of rendered email templates (HTML and Text multipart).
