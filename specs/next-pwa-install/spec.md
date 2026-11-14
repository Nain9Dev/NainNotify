# Feature Specification: Installable site (Android and iOS)

## 1. Context and Problem Statement
`https://notify.naindev.com/` is the Vite build of `Notificaciones.Client/`, published to GitHub Pages by `.github/workflows/deploy.yml` (artifact path `Notificaciones.Client/dist`). It publishes no web app manifest, so a phone cannot install it with its own icon and name. The shared contract and the verified browser criteria are in the portfolio's `specs/next-pwa-install/spec.md` (Nain9Dev/nain9dev.github.io); this file applies them to this site.

## 2. Requirements (EARS)
- `REQ-NOTIF-014`: The public page shall link a web app manifest and the iOS home-screen tags, and the deployed site shall serve the manifest and its icons from its root. The site registers no service worker.
  - The manifest declares `id` and `start_url` as `./`, `scope` as `./`, `display` as `standalone`, `lang` as `es`, `dir` as `ltr`, `name` and `short_name` as `NainNotify`, the page's meta description as `description`, and `theme_color` and `background_color` as `#070a12` (the page's `theme-color`). It does not declare `prefer_related_applications`.
  - It declares three PNG icons at their real size: `icons/icon-192.png` and `icons/icon-512.png` with purpose `any`, and `icons/icon-maskable-512.png` with purpose `maskable` (opaque, mark inside the centred circle of radius 40% of the width).
  - The page carries `apple-touch-icon` (`icons/apple-touch-icon.png`, 180 px, opaque), `apple-mobile-web-app-title` equal to `short_name`, `mobile-web-app-capable` and `apple-mobile-web-app-capable` set to `yes`, and `apple-mobile-web-app-status-bar-style` set to `black` (the page is dark).

## 3. Non-goals
- A service worker, offline page, cache or in-page install button (owner decision of 2026-11-13: installable only).
- Changing the visual design or copy of the client.
- The operations console served by the API (`src/Notificaciones.Api/wwwroot/index.html`): it is a separate document, not the published site and not a copy of the client build.

## 4. Edge cases
- The page uses `base: './'`, so the links and manifest paths are relative to the document, like the rest of its assets.
- GitHub Pages serves the files from `public/` as real files at the root of `dist/`; there is no SPA fallback and no Content-Security-Policy on the page, so no policy change is needed.
- The install icons live in `icons/`, outside the hashed `assets/` folder.

## 5. Acceptance criteria
- [x] `InstallabilityTests` fails on the unmodified tree and passes after the change.
- [x] `dotnet build`, `dotnet test` and `npm run build` pass.
- [x] Chrome reports no installability error on `Notificaciones.Client/dist`.
- [ ] After the deploy, the live manifest answers 200 and the owner installs the site on Android and iOS: icon uncropped, label `NainNotify`, window without browser bar.

## 6. Handoff
- Done and verified: manifest, icons, head tags, tests, docs.
- The test project is not run by any workflow (`deploy.yml` only builds the client; `codeql.yml` only scans). The test runs with `dotnet test` locally.
- Next: the owner merges, then installs from a phone (T-038).
