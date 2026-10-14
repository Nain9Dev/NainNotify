# ADR-003 — Decoupled Vue 3 + TypeScript Client for NainNotify

- **Status**: Approved
- **Date**: 2026-10-14
- **Deciders**: naindev
- **Scope**: Frontend presentation, real-time SSE observability, and commercial portfolio integration.

## Context
While `Notificaciones.Api` served an operations console using static vanilla HTML/CSS from `wwwroot`, the NainDev commercial portfolio standard requires a modern, reactive single-page application built with Vue 3 and strict TypeScript (aligned with `PolicyOperations.Client` on 5375, `Nain3D.Client` on 5376, `ProspectAI.Client` on 5377).

## Decision
1. **Commercial Brand**: Adopt **NainNotify** (or **NainNotify Gateway**) for the web showcase on `notify.naindev.com` while preserving the repository name `Microservicio-Notificaciones`.
2. **Dedicated SPA**: Create `Notificaciones.Client` using Vite 6, Vue 3 (Composition API), and strict TypeScript pinned to port `5378`.
3. **Real-Time Telemetry**: Connect directly to the `/api/v1/Notifications/stream` Server-Sent Events (SSE) endpoint to visualize live lifecycle transitions (`Enqueued`, `Rendering`, `Dispatched`, `Failed`) with zero polling.
4. **Interactive Playground**: Allow 1-click test dispatches across all 5 built-in templates (`SecurityAlert`, `InvoiceReceipt`, `TeamInvite`, `PasswordReset`, `ContactFormSubmission`).

## Consequences
- Clean separation between frontend presentation and the .NET backend API.
- Port `5378` registered in `D:\Development\Tools\dev-ports.json`.
- Embedded console in `wwwroot` remains intact as fallback.
