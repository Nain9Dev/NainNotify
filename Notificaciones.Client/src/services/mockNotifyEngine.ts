import type {
  ContactFormRequest,
  ContactFormResponse,
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
import { maskEmail } from '../utils/format'

/**
 * Standalone engine used when no gateway is reachable (GitHub Pages or the API is down).
 * Stages are paced so the pipeline is readable; reported latencies model the real gateway.
 */

const listeners = new Set<SseHandlers>()
const bootedAt = Date.now()

const snapshot: TelemetrySnapshot = {
  uptimeSeconds: 0,
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

function seedEvent(
  id: string,
  type: NotificationType,
  priority: string,
  recipient: string,
  subject: string,
  elapsed: number,
  bytes: number,
  channel: string,
  minutesAgo: number
): NotificationLifecycleEvent {
  return {
    notificationId: id,
    stage: 'Dispatched',
    type,
    priority,
    recipient: maskEmail(recipient),
    subject,
    source: 'Worker/MassTransit',
    elapsedMilliseconds: elapsed,
    payloadBytes: bytes,
    attempt: 1,
    channel,
    detail: 'Entregado.',
    error: null,
    timestamp: new Date(Date.now() - minutesAgo * 60_000).toISOString(),
  }
}

const recentEvents: NotificationLifecycleEvent[] = [
  seedEvent('a1f4c9e2', 'WelcomeMessage', 'Normal', 'maria.garcia@example.es', 'Bienvenida a la plataforma', 18, 3120, 'ResendSmtp', 3),
  seedEvent('b7d02e41', 'ContactFormSubmission', 'High', 'aitor@naindev.com', 'Propuesta de modernización .NET', 24, 4280, 'ResendSmtp', 12),
  seedEvent('c93e7a10', 'SystemAlert', 'High', 'ops@naindev.com', 'Umbral de base de datos alcanzado', 12, 1840, 'InAppPush', 25),
]

function snapshotCopy(): TelemetrySnapshot {
  snapshot.uptimeSeconds = 18_450 + Math.floor((Date.now() - bootedAt) / 1000)
  return JSON.parse(JSON.stringify(snapshot))
}

function emit(event: NotificationLifecycleEvent) {
  recentEvents.unshift(event)
  if (recentEvents.length > 60) recentEvents.pop()
  snapshot.lastEventAt = event.timestamp
  const metrics = snapshotCopy()
  listeners.forEach((h) => {
    try {
      h.onLifecycle(event)
      h.onMetrics(metrics)
    } catch (e) {
      console.warn('Listener error', e)
    }
  })
}

const STAGE_PLAN: { stage: NotificationStage; at: number; latency: number; detail: string }[] = [
  { stage: 'Accepted', at: 0, latency: 1.4, detail: 'Petición validada por el gateway.' },
  { stage: 'Queued', at: 420, latency: 4.2, detail: 'Publicada en el bus de mensajes.' },
  { stage: 'Received', at: 900, latency: 8.9, detail: 'Consumida por el worker.' },
  { stage: 'Rendered', at: 1500, latency: 15.6, detail: 'Plantilla HTML renderizada.' },
  { stage: 'Dispatched', at: 2100, latency: 22.3, detail: 'Entregada al canal de salida.' },
]

function jitter(base: number): number {
  return Math.round((base + (Math.random() - 0.5) * base * 0.3) * 10) / 10
}

function runPipeline(
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
  const masked = maskEmail(recipient)
  const total = jitter(STAGE_PLAN[STAGE_PLAN.length - 1].latency)

  STAGE_PLAN.forEach(({ stage, at, latency, detail }, index) => {
    window.setTimeout(() => {
      const elapsed = index === STAGE_PLAN.length - 1 ? total : jitter(latency)
      if (stage === 'Dispatched') {
        snapshot.inFlight = Math.max(0, snapshot.inFlight - 1)
        snapshot.dispatched++
        snapshot.renderedBytes += bytes
        snapshot.averageLatencyMs = Math.round(((snapshot.averageLatencyMs * (snapshot.dispatched - 1) + total) / snapshot.dispatched) * 10) / 10
        snapshot.p95LatencyMs = Math.max(snapshot.p95LatencyMs, Math.round(total * 1.35 * 10) / 10)
        snapshot.fastestLatencyMs = Math.min(snapshot.fastestLatencyMs, total)
        snapshot.successRate = Math.round((snapshot.dispatched / snapshot.accepted) * 1000) / 10
        snapshot.byChannel.ResendSmtp = (snapshot.byChannel.ResendSmtp || 0) + 1
      }
      emit({
        notificationId: id,
        stage,
        type,
        priority,
        recipient: masked,
        subject,
        source: index === 0 ? 'Api/Gateway' : 'Worker/MassTransit',
        elapsedMilliseconds: elapsed,
        payloadBytes: bytes,
        attempt: 1,
        channel: 'ResendSmtp',
        detail,
        error: null,
        timestamp: new Date().toISOString(),
      })
    }, at)
  })
}

function newId(): string {
  return crypto.randomUUID().replace(/-/g, '').slice(0, 8)
}

function escapeHtml(value: string): string {
  return value
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;')
}

const TYPE_LABEL: Record<NotificationType, string> = {
  SystemAlert: 'Alerta del sistema',
  WelcomeMessage: 'Bienvenida',
  ContactFormSubmission: 'Contacto',
  TestDemo: 'Prueba',
}

const TYPE_COLOR: Record<NotificationType, string> = {
  SystemAlert: '#f59e0b',
  WelcomeMessage: '#38bdf8',
  ContactFormSubmission: '#6366f1',
  TestDemo: '#34d399',
}

export const mockNotifyEngine = {
  fetchHealth(): HealthStatusResponse {
    return {
      service: 'NainNotify',
      version: '2.0.0',
      architecture: 'Clean Architecture (standalone engine)',
      status: 'Healthy',
      transport: 'in-memory',
      workerInProcess: true,
      instanceId: 'standalone',
      uptimeSeconds: snapshotCopy().uptimeSeconds,
      checkDurationMs: 2,
      dependencies: {
        bus: { status: 'Healthy', durationMs: 1 },
        smtp: { status: 'Healthy', durationMs: 1 },
      },
      timestamp: new Date().toISOString(),
    }
  },

  fetchMetrics(): { snapshot: TelemetrySnapshot; listeners: number } {
    return { snapshot: snapshotCopy(), listeners: listeners.size || 1 }
  },

  fetchActivity(): NotificationLifecycleEvent[] {
    return JSON.parse(JSON.stringify(recentEvents))
  },

  triggerDemo(req: DemoNotificationRequest): DemoNotificationResponse {
    const id = newId()
    const bytes = Math.max(1200, req.content.length * 8)
    runPipeline(id, req.eventType, req.targetRecipient, req.subject, req.priority, bytes)
    return {
      status: 'Enqueued',
      broker: 'in-memory',
      assignedTrackingId: id,
      targetType: req.eventType,
      effectiveRecipient: maskEmail(req.targetRecipient),
      sandboxed: true,
      sandboxNotice: 'Demo local: nada sale de tu navegador.',
    }
  },

  submitContact(req: ContactFormRequest): ContactFormResponse {
    const id = newId()
    if (req.website && req.website.trim().length > 0) {
      // Honeypot filled: the gateway answers 202 but drops the message silently.
      return { status: 'Accepted', message: 'Recibido.', trackingId: id, timestamp: new Date().toISOString() }
    }
    const bytes = 2400 + req.message.length * 4
    runPipeline(id, 'ContactFormSubmission', req.senderEmail, req.subject, 'High', bytes)
    return { status: 'Enqueued', message: 'Recibido.', trackingId: id, timestamp: new Date().toISOString() }
  },

  renderPreview(req: DemoNotificationRequest): TemplatePreviewResponse {
    const color = TYPE_COLOR[req.eventType] ?? '#38bdf8'
    const subject = escapeHtml(req.subject || 'Sin asunto')
    const body = escapeHtml(req.content || '').replace(/\n/g, '<br>')
    const sender = escapeHtml(req.senderName || 'NainNotify')
    const html = `<!doctype html><html><head><meta charset="utf-8"><meta name="viewport" content="width=device-width"></head>
<body style="margin:0;background:#f3f4f6;font-family:Arial,Helvetica,sans-serif;">
<table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="background:#f3f4f6;padding:24px 12px;">
<tr><td align="center">
<table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="max-width:560px;background:#ffffff;border-radius:12px;overflow:hidden;border:1px solid #e5e7eb;">
<tr><td style="height:6px;background:${color};"></td></tr>
<tr><td style="padding:28px 32px 8px 32px;">
<div style="font-size:11px;letter-spacing:1px;text-transform:uppercase;color:${color};font-weight:700;">${TYPE_LABEL[req.eventType] ?? req.eventType}</div>
<h1 style="margin:10px 0 0 0;font-size:22px;line-height:1.3;color:#111827;">${subject}</h1>
</td></tr>
<tr><td style="padding:12px 32px 28px 32px;font-size:15px;line-height:1.6;color:#374151;">${body}</td></tr>
<tr><td style="padding:0 32px 28px 32px;">
<a href="#" style="display:inline-block;background:${color};color:#ffffff;text-decoration:none;font-weight:700;font-size:14px;padding:12px 20px;border-radius:8px;">Ver detalles</a>
</td></tr>
<tr><td style="padding:16px 32px;border-top:1px solid #e5e7eb;font-size:12px;color:#6b7280;">${sender} · Enviado con NainNotify</td></tr>
</table>
</td></tr>
</table>
</body></html>`

    return {
      html,
      plainText: `${req.subject}\n\n${req.content}\n\n--\n${req.senderName || 'NainNotify'}`,
      bytes: html.length,
      renderMilliseconds: 4,
      type: req.eventType,
    }
  },

  connectStream(handlers: SseHandlers): () => void {
    listeners.add(handlers)
    handlers.onStatusChange('standalone')
    handlers.onHydrate({
      snapshot: snapshotCopy(),
      recent: JSON.parse(JSON.stringify(recentEvents)),
      listeners: listeners.size,
    })
    return () => {
      listeners.delete(handlers)
    }
  },
}
