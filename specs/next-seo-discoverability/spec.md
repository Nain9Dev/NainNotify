# Feature Specification: Search and AI-engine discoverability

## 1. Context and Problem Statement
`https://notify.naindev.com/` is the Vite build of `Notificaciones.Client/`, published to GitHub Pages by `.github/workflows/deploy.yml` (artifact path `Notificaciones.Client/dist`). The page had no canonical URL, Twitter card, structured data, sitemap, `robots.txt` or `llms.txt`.

## 2. Requirements (EARS)
- `REQ-NOTIF-013`: The public page shall declare one canonical URL, a 70 to 160 character description, Open Graph and Twitter card metadata and `SoftwareApplication` JSON-LD naming its author, and the deployed site shall serve `robots.txt` (advertising the sitemap), `sitemap.xml` and `llms.txt` from its root.

## 3. Non-goals
- Changing the visual design, copy or components of the client.
- Restructuring the single-page app: the static HTML has only the `#app` mount point and no `<h1>`; the headline is rendered by Vue.

## 4. Handoff
- Done: metadata, JSON-LD and discovery files; `SeoDiscoverabilityTests` first red then green. See `tasks.md`.
- The discovery files live in `Notificaciones.Client/public/`, which Vite copies verbatim to `dist/`. The unit tests check the sources; the deployed output is checked by the pull request's build.
