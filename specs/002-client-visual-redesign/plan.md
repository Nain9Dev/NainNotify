# Plan — Client Visual Redesign

## Page structure (top to bottom)
1. `SiteHeader` — brand, one live/demo status pill, links to API docs and GitHub.
2. `HeroSection` — short headline, one sentence, primary CTA (scroll to studio), secondary CTA (API).
3. `PipelineFlow` — five pipeline nodes with animated wires plus a compact KPI strip (sent, in flight, p95, success). Plays incoming stages with a minimum dwell time.
4. `NotificationStudio` — the usable core. Two columns:
   - Left: scenario cards (one click), a short form (recipient, subject, message), advanced options folded, primary "Enviar" button.
   - Right: `EmailPreview` rendered automatically (debounced) inside an email-client frame, with "Ampliar" opening `TemplatePreviewModal`.
   - Contact scenario reuses the same column and exposes the honeypot toggle.
5. `ActivityFeed` — one row per notification with a five-dot stage stepper; a row expands to the raw lifecycle events.
6. `TrustStrip` — four guarantees as icon + three-word label, health probes as pills.
7. `SiteFooter`.

## State
- `composables/useNotifyStream.ts` owns snapshot, events, health, connection status and the standalone fallback. `App.vue` only wires components.
- `services/api.ts` gains the SSE fallback: on `EventSource.onerror` before any `open`, it closes the source and connects the standalone engine, reporting status `standalone`.
- `services/mockNotifyEngine.ts` paces stages (0 / 400 / 900 / 1500 / 2100 ms) and masks recipients like `PrivacyMasker` does server-side.

## Design tokens (`style.css`)
- Single accent (cyan to indigo), semantic stage colours unchanged, neutral ink surfaces, 8px radius scale, two shadows.
- Fonts already loaded: Outfit (display), Inter (text), JetBrains Mono (numbers).
- Motion: `--motion` durations collapse to 0 under `prefers-reduced-motion`.

## Verification
- `npm run build` (vue-tsc + vite) passes.
- Manual pass in the in-app browser at 1280px, 768px and 375px: no horizontal scroll, scenario click renders preview, send animates pipeline and appends an activity row.
