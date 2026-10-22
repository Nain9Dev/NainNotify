# Implementation Tasks — Client Visual Redesign

- [x] [A] T-010 Write spec and plan (`spec.md`, `plan.md`). Done when both files exist and name the requirements `REQ-NOTIF-06` to `REQ-NOTIF-12`.
- [x] [A] T-011 Refresh design tokens and base styles in `src/style.css`. Done when buttons, inputs, chips and panels share one token set and reduced motion is honoured.
- [x] [A] T-012 Extract stream state into `composables/useNotifyStream.ts` and add the standalone fallback in `services/api.ts`. Done when the page shows data on localhost with the API stopped and the header reads "Demo local".
- [x] [A] T-013 Pace the standalone engine and mask recipients in `services/mockNotifyEngine.ts`. Done when a sent notification takes about two seconds to reach `Dispatched`.
- [x] [A] T-014 Build `SiteHeader`, `HeroSection`, `SiteFooter`. Done when the hero has one headline, one sentence and two buttons.
- [x] [A] T-015 Build `PipelineFlow` with stage player and KPI strip. Done when each stage highlights for at least 300 ms.
- [x] [A] T-016 Build `NotificationStudio` and `EmailPreview`. Done when a scenario click fills the form and the preview renders without another click.
- [x] [A] T-017 Build `ActivityFeed` grouped by notification. Done when one row per notification shows five stage dots and expands to raw events.
- [x] [A] T-018 Build `TrustStrip`. Done when each guarantee is an icon plus at most four words.
- [x] [A] T-019 Remove superseded components and verify `npm run build`. Done when the build passes with no unused files.
- [x] [A] T-020 Verify responsive layout at 375 / 768 / 1024 px in the browser. Done when no horizontal overflow appears.
- [x] [A] T-021 Update `docs/changelog.md`, `docs/traceability.md` and `README.md` client section.
- [x] [H] T-022 Decide the public domain. Decided 2026-10-22: keep `notify.naindev.com`; `public/CNAME`, CORS origins and README stay as they are.

## Handoff
- Done and verified: T-010 to T-022 (build passes; manual browser pass at three widths).
- Next: nothing pending; changes are uncommitted.
- Blocked: nothing.
