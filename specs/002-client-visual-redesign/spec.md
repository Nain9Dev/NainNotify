# Feature Specification: Client Visual Redesign (Studio Layout)

## 1. Context and Problem Statement
The showcase client (`Notificaciones.Client/`) presents the gateway as a dense telemetry console: long paragraphs, technical jargon, a raw event log and a form with eight fields. A prospective customer cannot tell in five seconds what the product does or how to try it. The page also overflows horizontally below 1100px and renders an empty dashboard when the API is not running on localhost.

The redesign turns the page into a guided, low-text experience: choose a scenario, send it, watch it travel through the pipeline and see the rendered email, all above the fold.

## 2. Requirements (EARS)
- `REQ-NOTIF-06`: When the page loads, the client shall show a headline of at most eight words, one supporting sentence and a single primary call to action that scrolls to the studio.
- `REQ-NOTIF-07`: When a visitor clicks a scenario card, the client shall populate the form and render the email preview automatically without any further click.
- `REQ-NOTIF-08`: When a notification is sent, the client shall animate each pipeline stage for at least 300 ms so that the flow is readable even when the real gateway completes in under 50 ms.
- `REQ-NOTIF-09`: When lifecycle events arrive, the activity feed shall group them by notification and show one row per notification with a five-step stage indicator, latency and time.
- `REQ-NOTIF-10`: When the Server-Sent Events connection to the local API fails, the client shall fall back to the standalone engine and label the session as a local demo instead of showing empty metrics.
- `REQ-NOTIF-11`: The layout shall render without horizontal overflow from 360px to 1440px wide, and shall honour `prefers-reduced-motion`.
- `REQ-NOTIF-12`: Every interactive control shall have a visible focus state and an accessible name (WCAG 2.2 AA).

## 3. Non-goals
- Changing the public API contracts or the backend.
- Changing the deployment domain (`CNAME`) or CORS origins.
- Adding a light theme (tokens are prepared, the theme itself is deferred).

## 4. Handoff
- Done: layout, components, standalone fallback, pacing, build verified. See `tasks.md`.
- Domain: the owner confirmed on 2026-10-22 that the public site stays on `notify.naindev.com`.
