# Runbook

## Puerto asignado

Este servicio ocupa el **8120** (banda de backends `8100-8199` del registro
`D:\Development\PORTS.md`). El puerto está fijado en tres sitios y los tres deben coincidir:

| Punto | Fichero | Valor |
| :--- | :--- | :--- |
| Ejecución local | `src/Notificaciones.Api/Properties/launchSettings.json` | `http://localhost:8120` |
| Contenedor | `Dockerfile` | `EXPOSE 8120`, `ASPNETCORE_URLS=http://+:8120` |
| Publicación | `docker-compose.yml` | `"8120:8120"` |

Kestrel **falla al arrancar** si el puerto está ocupado, no deriva a otro en silencio como
hace Vite. Ese error es la señal buena: no lo esquives cambiando de puerto, averigua quién
lo tiene ocupado.

```powershell
netstat -ano | findstr :8120
Get-Process -Id <pid>
```

Puertos de infraestructura que este proyecto también reserva: **5672** (AMQP) y **15672**
(panel de RabbitMQ). No los reasignes a otro proyecto.

---

## Arranque

### Opción A — Un solo proceso (in-memory)

```bash
dotnet run --project src/Notificaciones.Api
```

El perfil `Development` activa `RabbitMq:UseInMemoryTransport`, que sustituye RabbitMQ por el
transporte en memoria de MassTransit y aloja el consumidor en el propio gateway. El pipeline
completo funciona de extremo a extremo sin Docker ni broker.

- Consola: <http://localhost:8120>
- OpenAPI: <http://localhost:8120/swagger>

Para probar contra RabbitMQ real sin Docker, usa el segundo perfil:

```bash
dotnet run --project src/Notificaciones.Api --launch-profile "Notificaciones.Api (RabbitMQ)"
```

> No habilites el transporte en memoria en un despliegue con varias instancias: los mensajes
> nunca salen del proceso que los publicó.

### Opción B — Clúster completo con Docker

```bash
docker-compose up -d --build
```

Levanta tres contenedores: RabbitMQ, API Gateway y Worker. El clúster fija
`RabbitMq__UseInMemoryTransport=false` por variable de entorno, así que usa el broker real
aunque el perfil sea `Development`.

- Consola y OpenAPI: <http://localhost:8120>
- Panel de RabbitMQ: <http://localhost:15672> (`guest` / `guest`)

Logs del worker:

```bash
docker logs -f notificaciones-worker
```

---

## Verificación

```bash
dotnet test MicroservicioNotificaciones.sln
```

La suite de integración levanta la aplicación real sobre el transporte en memoria: cubre el
pipeline de cinco etapas, el sandbox de destinatarios, el rate limiting, el honeypot y el
servido de la consola. No necesita broker ni credenciales SMTP.

Comprobación manual del estado de las dependencias:

```bash
curl http://localhost:8120/api/v1/notifications/health
```

---

## Límites conocidos

- **La telemetría es volátil.** El read model vive en memoria y se reinicia con el proceso.
  Alimenta una consola de operaciones, no un registro de auditoría.
- **Cada instancia del gateway ve su propia proyección.** Con varias réplicas detrás de un
  balanceador, cada una reporta lo que ha observado ella.
- **Sin credenciales SMTP el envío se simula.** El canal se reporta como `simulation`, nunca
  como una entrega real.
- **Rate limiting por IP de socket.** Detrás de un proxy inverso hay que habilitar
  `ForwardedHeaders:Enabled` declarando los proxies de confianza; si no, todos los visitantes
  comparten la IP del proxy y agotan el límite entre todos.
