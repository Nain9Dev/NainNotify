# Plan — Installable site (Android and iOS)

1. `tests/Notificaciones.UnitTests/Client/InstallabilityTests.cs` asserts REQ-NOTIF-014: manifest members and names, icons present at the declared size (PNG header), opaque maskable and touch icons (decoded alpha channel, no image library), head tags of `Notificaciones.Client/index.html`.
2. Add `Notificaciones.Client/public/manifest.webmanifest` and `Notificaciones.Client/public/icons/`. Vite copies `public/` unchanged to the root of `dist/`, which the deploy workflow uploads. No workflow change.
3. Add the manifest link, `apple-touch-icon` and the iOS meta tags to `Notificaciones.Client/index.html`.

## Icons

Rendered from the brand mark `Notificaciones.Client/public/favicon.svg`:

```
node make-install-icons.mjs --source Notificaciones.Client/public/favicon.svg --out Notificaciones.Client/public/icons --bg "#05070D"
```

`#05070D` is the tile colour of the mark; the maskable icon scales the mark to 0.56 so it stays inside the safe zone, and the Apple icon uses scale 1.

## Decisions
- No service worker, cache or offline page (owner decision of 2026-11-13). Adding one later needs its own spec and ADR.
- The status bar style is `black` because the page is dark.
- The API console under `src/Notificaciones.Api/wwwroot/` is untouched: it is a different page from the published client.

No change in Domain, Application, Infrastructure, Worker or API.
