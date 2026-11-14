# Implementation Tasks — Installable site (Android and iOS)

- [x] [A] T-035 Write `InstallabilityTests`. Done when the 4 tests fail on the unmodified tree (missing manifest).
- [x] [A] T-036 Render the install icons and add `manifest.webmanifest` and the head tags. Done when the tests pass and `npm run build` copies manifest and icons to `dist/`.
- [x] [A] T-037 Check installability in Chrome on `Notificaciones.Client/dist`. Done when `installabilityErrors` is empty.
- [ ] [H] T-038 After the merge, install the site from Chrome on Android and Safari on iPhone. Done when icon, label and standalone window are as expected (needs the owner's phones).
- [x] [A] T-039 Update requirements, traceability and changelog.

## Handoff
- Done and verified: T-035 to T-037 and T-039 (68 unit and 23 integration tests green, Vite build green).
- Blocked: T-038 is owner-only.
