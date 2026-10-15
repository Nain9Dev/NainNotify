import type {
  ContactFormRequest,
  DemoNotificationRequest,
  DemoNotificationResponse,
  HealthStatusResponse,
  NotificationLifecycleEvent,
  NotificationStage,
  NotificationType,
  TelemetrySnapshot,
  TemplatePreviewResponse,
} from '../types/contracts'
import type { SseHandlers } from './api'

const listeners = new Set<SseHandlers>()

const snapshot: TelemetrySnapshot = {
  uptimeSeconds: 18450,
  accepted: 48,
  dispatched: 47,
  failed: 1,
  inFlight: 0,
  successRate: 97.9,
  averageLatencyMs: 18.4,
  p95LatencyMs: 34.2,
  fastestLatencyMs: 9.1,
  renderedBytes: 145200,
  byType: {
    ContactFormSubmission: 22,
    WelcomeMessage: 14,
    SystemAlert: 8,
    TestDemo: 4,
  },
  byChannel: {
    ResendSmtp: 42,
    InAppPush: 5,
  },
  throughputSeries: [4, 6, 8, 5, 9, 7, 11, 8, 12, 10, 14, 9],
  lastEventAt: new Date().toISOString(),
}

const recentEvents: NotificationLifecycleEvent[] = [
  {
    notificationId: 'notif-synth-001',
    stage: 'Dispatched',
    type: 'WelcomeMessage',
    priority: 'Normal',
    recipient: 'maria.garcia@example.es',
    subject: 'Bienvenido a la plataforma de notificaciones',
    source: 'Worker/MassTransit',
    elapsedMilliseconds: 18,
    payloadBytes: 3120,
    attempt: 1,
    channel: 'ResendSmtp',
    detail: 'Mensaje entregado satisfactoriamente al relay SMTP en 18ms.',
    error: null,
    timestamp: new Date(Date.now() - 1000 * 60 * 3).toISOString(),
  },
  {
    notificationId: 'notif-synth-002',
    stage: 'Dispatched',
    type: 'ContactFormSubmission',
    priority: 'High',
    recipient: 'aitor@naindev.com',
    subject: 'Nueva propuesta de modernización técnica .NET',
    source: 'Worker/MassTransit',
    elapsedMilliseconds: 24,
    payloadBytes: 4280,
    attempt: 1,
    channel: 'ResendSmtp',
    detail: 'Lead cualificado procesado y reenviado a la bandeja corporativa.',
    error: null,
    timestamp: new Date(Date.now() - 1000 * 60 * 12).toISOString(),
  },
  {
    notificationId: 'notif-synth-003',
    stage: 'Dispatched',
    type: 'SystemAlert',
    priority: 'High',
    recipient: 'ops@naindev.com',
    subject: 'Alerta: Umbral de consumo de base de datos verificado',
    source: 'Worker/MassTransit',
    elapsedMilliseconds: 12,
    payloadBytes: 1840,
    attempt: 1,
    channel: 'InAppPush',
    detail: 'Alerta operacional distribuida con canal prioritario.',
    error: null,
    timestamp: new Date(Date.now() - 1000 * 60 * 25).toISOString(),
  },
]

function emitEvent(event: NotificationLifecycleEvent) {
  recentEvents.unshift(event)
  if (recentEvents.length > 50) recentEvents.pop()
  snapshot.lastEventAt = event.timestamp
  listeners.forEach((h) => {
    try {
      h.onLifecycle(event)
      h.onMetrics(JSON.parse(JSON.stringify(snapshot)))
    } catch (e) {
      console.warn('Listener error', e)
    }
  })
}

function processPipeline(
  id: string,
  type: NotificationType,
  recipient: string,
  subject: string,
  priority: string,
  bytes: number
) {
  snapshot.accepted++
  snapshot.inFlight++
  snapshot.byType[type] = (snapshot.byType[type] || 0) + 1

  const stages: { stage: NotificationStage; delay: number; detail: string }[] = [
    { stage: 'Accepted', delay: 0, detail: 'Solicitud aceptada y firmada por el Gateway HTTP.' },
    { stage: 'Queued', delay: 10, detail: 'Mensaje publicado en la cola de mensajería (In-Memory / RabbitMQ).' },
    { stage: 'Received', delay: 22, detail: 'Consumidor MassTransit ha recibido y deserializado el payload.' },
    { stage: 'Rendered', delay: 35, detail: 'Plantilla Razor HTML compilada con éxito.' },
    { stage: 'Dispatched', delay: 48, detail: `Despacho completado con éxito a ${recipient} en tiempo récord.` },
  ]

  stages.forEach(({ stage, delay, detail }) => {
    setTimeout(() => {
      if (stage === 'Dispatched') {
        snapshot.inFlight = Math.max(0, snapshot.inFlight - 1)
        snapshot.dispatched++
        snapshot.renderedBytes += bytes
      }
      emitEvent({
        notificationId: id,
        stage,
        type,
        priority,
        recipient,
        subject,
        source: 'Worker/MassTransit',
        elapsedMilliseconds: delay,
        payloadBytes: bytes,
        attempt: 1,
        channel: 'ResendSmtp',
        detail,
        error: null,
        timestamp: new Date().toISOString(),
      })
    }, delay)
  })
}

export const mockNotifyEngine = {
  fetchHealth(): HealthStatusResponse {
    return {
      service: 'NainNotify Microservice',
      version: '2.0.0',
      architecture: 'Hexagonal Architecture (In-Memory Bus)',
      status: 'Healthy',
      transport: 'In-Memory Bus',
      workerInProcess: true,
      instanceId: 'nainnotify-standalone-cluster',
      uptimeSeconds: snapshot.uptimeSeconds,
      checkDurationMs: 2,
      dependencies: {
        rabbitmq: { status: 'Healthy (In-Memory Engine)', durationMs: 1 },
        smtp: { status: 'Healthy (Sandboxed)', durationMs: 1 },
      },
      timestamp: new Date().toISOString(),
    }
  },

  fetchMetrics(): { snapshot: TelemetrySnapshot; listeners: number } {
    return {
      snapshot: JSON.parse(JSON.stringify(snapshot)),
      listeners: listeners.size || 1,
    }
  },

  fetchActivity(): NotificationLifecycleEvent[] {
    return JSON.parse(JSON.stringify(recentEvents))
  },

  triggerDemo(req: DemoNotificationRequest): DemoNotificationResponse {
    const id = `notif-${crypto.randomUUID().substring(0, 8)}`
    const bytes = Math.max(1200, req.content.length * 8)
    processPipeline(id, req.eventType, req.targetRecipient, req.subject, req.priority, bytes)

    return {
      status: 'Enqueued',
      broker: 'In-Memory Bus',
      assignedTrackingId: id,
      targetType: req.eventType,
      effectiveRecipient: req.targetRecipient,
      sandboxed: true,
      sandboxNotice: 'Modo seguro demostrativo: Despacho completado en memoria sin coste ni dependencias externas.',
    }
  },

  submitContact(req: ContactFormRequest): { status: string; message: string; trackingId: string; timestamp: string } {
    const id = `contact-${crypto.randomUUID().substring(0, 8)}`
    const bytes = 2400 + req.message.length * 4
    processPipeline(id, 'ContactFormSubmission', req.senderEmail, req.subject, 'High', bytes)

    return {
      status: 'Enqueued',
      message: 'Mensaje de contacto recibido y enrutado a la cola de despacho prioritario.',
      trackingId: id,
      timestamp: new Date().toISOString(),
    }
  },

  renderPreview(req: DemoNotificationRequest): TemplatePreviewResponse {
    const html = `<div style="font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; background: #0f172a; color: #f8fafc; border-radius: 8px; padding: 24px; border: 1px solid rgba(255,255,255,0.1);">
  <div style="border-bottom: 1px solid #334155; padding-bottom: 16px; margin-bottom: 20px;">
    <h2 style="color: #38bdf8; margin: 0 0 6px 0;">${req.subject}</h2>
    <span style="font-size: 12px; color: #94a3b8;">Tipo: ${req.eventType} · Prioridad: ${req.priority}</span>
  </div>
  <div style="line-height: 1.6; color: #cbd5e1; font-size: 14px;">
    ${req.content.replace(/\n/g, '<br>')}
  </div>
  <div style="margin-top: 24px; padding-top: 16px; border-top: 1px solid #334155; font-size: 11px; color: #64748b;">
    Despachado por NainNotify · Microservicio Hexagonal .NET 10 & MassTransit
  </div>
</div>`

    return {
      html,
      plainText: `${req.subject}\n\n${req.content}\n\n--\nNainNotify`,
      bytes: html.length,
      renderMilliseconds: 4,
      type: req.eventType,
    }
  },

  connectStream(handlers: SseHandlers): () => void {
    listeners.add(handlers)
    handlers.onStatusChange('connected')
    handlers.onHydrate({
      snapshot: JSON.parse(JSON.stringify(snapshot)),
      recent: JSON.parse(JSON.stringify(recentEvents)),
      listeners: listeners.size,
    })

    return () => {
      listeners.delete(handlers)
    }
  },
}
