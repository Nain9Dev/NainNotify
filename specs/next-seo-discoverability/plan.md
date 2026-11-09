# Plan — Search and AI-engine discoverability

1. `tests/Notificaciones.UnitTests/Client/SeoDiscoverabilityTests.cs` asserts REQ-NOTIF-013 against `Notificaciones.Client/index.html` and `Notificaciones.Client/public/*`.
2. Add canonical, author link, Open Graph, Twitter card and JSON-LD to `index.html`; extend the description to name the stack.
3. Add `robots.txt`, `sitemap.xml` and `llms.txt` to `Notificaciones.Client/public/`.

No change in Domain, Application, Infrastructure, Worker or API.
