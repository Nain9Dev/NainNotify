# Microservicio de Notificaciones (NainDev)

![.NET 10](https://img.shields.io/badge/.NET-10.0-blueviolet)
![MassTransit](https://img.shields.io/badge/MassTransit-8.3.4-blue)
![RabbitMQ](https://img.shields.io/badge/RabbitMQ-Message%20Broker-ff6600)

Un ecosistema de despacho asíncrono de correos diseñado con **Clean Architecture** y **Domain-Driven Design**. Separa completamente el *Gateway HTTP* del *Worker* de procesamiento para garantizar una latencia inferior a 50ms en la ingesta, incluso con caídas del proveedor SMTP (Resend/Mailtrap).

Este repositorio aplica el flujo **Spec-Driven Development (SDD)**: todo cambio nace en la documentación (`docs/` y `specs/`) antes que en el código.

---

## ⚡ 1-Click Demo (Docker)

Levanta la arquitectura distribuida (Gateway + Worker + RabbitMQ) en tu máquina sin instalar nada más que Docker:

```bash
docker-compose up -d --build
```

- 📡 **Consola y Swagger UI**: [http://localhost:8120](http://localhost:8120) · [/swagger](http://localhost:8120/swagger)
- 🐇 **RabbitMQ Admin**: [http://localhost:15672](http://localhost:15672) *(guest / guest)*
- 📜 **Logs en vivo**: `docker logs -f notificaciones-worker`

> **Puerto asignado: 8120.** Fijado en `launchSettings.json`, `Dockerfile` y
> `docker-compose.yml` segun el registro `D:\Development\PORTS.md`, para no colisionar con
> el resto de proyectos locales. Detalle en [`docs/60-runbook.md`](docs/60-runbook.md).

Haz un `POST` al endpoint `/demo` y observa cómo el Worker encola y procesa la plantilla en milisegundos de forma transparente.

---

## 🏛️ Diseño y Patrones Aplicados

El ecosistema implementa separación física y lógica extrema:

1. **Inmutabilidad y Eventos**: `Notificaciones.Domain` define enumeraciones y `NotificationLifecycleEvent` sin ninguna dependencia a librerías externas.
2. **Motor de Plantillas Resiliente**: Generación de correos Multipart (HTML + Texto) con blindaje VML para clientes difíciles como Outlook.
3. **Privacidad por Diseño**: `PrivacyMasker` asegura que ningún PII (Personal Identifiable Information) se filtre a los logs del clúster.
4. **Modo Híbrido**: Puede correr entero en 1 solo proceso (`UseInMemoryTransport`) o escalar horizontalmente con múltiples workers atacando RabbitMQ.

---

## 🧪 Verificación

```bash
dotnet test MicroservicioNotificaciones.sln
```

78 pruebas, sin dependencias externas. Los tests de integración levantan la aplicación real
sobre el transporte en memoria y cubren el pipeline completo de cinco etapas, el sandbox de
destinatarios, el rate limiting, el honeypot antispam y el servido de la consola. Los
unitarios fijan el escapado de HTML de las plantillas, el enmascarado de direcciones y la
aritmética de la proyección de telemetría.

> **Toda la documentación arquitectónica profunda reside en la carpeta `docs/`.**
