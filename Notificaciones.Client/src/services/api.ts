import type {
  DemoNotificationRequest,
  DemoNotificationResponse,
  ContactFormRequest,
  HealthStatusResponse,
  NotificationLifecycleEvent,
  StreamHydrationPayload,
  TelemetrySnapshot,
  TemplatePreviewResponse,
} from '../types/contracts'

const API_BASE = '/api/v1/Notifications'

export async function fetchHealth(): Promise<HealthStatusResponse> {
  const res = await fetch(`${API_BASE}/health`)
  if (!res.ok) {
    throw new Error(`Health check returned status ${res.status}`)
  }
  return res.json()
}

export async function fetchMetrics(): Promise<{ snapshot: TelemetrySnapshot; listeners: number }> {
  const res = await fetch(`${API_BASE}/metrics`)
  if (!res.ok) {
    throw new Error(`Metrics endpoint returned status ${res.status}`)
  }
  return res.json()
}

export async function fetchActivity(limit = 40): Promise<NotificationLifecycleEvent[]> {
  const res = await fetch(`${API_BASE}/activity?limit=${limit}`)
  if (!res.ok) {
    throw new Error(`Activity endpoint returned status ${res.status}`)
  }
  return res.json()
}

export async function triggerDemoNotification(
  payload: DemoNotificationRequest
): Promise<DemoNotificationResponse> {
  const res = await fetch(`${API_BASE}/demo`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(payload),
  })
  if (!res.ok) {
    const errorBody = await res.json().catch(() => null)
    throw new Error(errorBody?.detail || errorBody?.message || `Dispatch failed with status ${res.status}`)
  }
  return res.json()
}

export async function submitContactForm(
  payload: ContactFormRequest
): Promise<{ status: string; message: string; trackingId: string; timestamp: string }> {
  const res = await fetch(`${API_BASE}/contact`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(payload),
  })
  if (!res.ok) {
    const errorBody = await res.json().catch(() => null)
    throw new Error(errorBody?.detail || errorBody?.message || `Contact submission failed: status ${res.status}`)
  }
  return res.json()
}

export async function renderTemplatePreview(
  payload: DemoNotificationRequest
): Promise<TemplatePreviewResponse> {
  const res = await fetch(`${API_BASE}/preview`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(payload),
  })
  if (!res.ok) {
    const errorBody = await res.json().catch(() => null)
    throw new Error(errorBody?.detail || errorBody?.message || `Preview failed: status ${res.status}`)
  }
  return res.json()
}

export type SseConnectionStatus = 'connecting' | 'connected' | 'disconnected' | 'error'

export interface SseHandlers {
  onHydrate: (payload: StreamHydrationPayload) => void
  onLifecycle: (event: NotificationLifecycleEvent) => void
  onMetrics: (snapshot: TelemetrySnapshot) => void
  onStatusChange: (status: SseConnectionStatus) => void
}

export function connectNotificationStream(handlers: SseHandlers): () => void {
  let eventSource: EventSource | null = null
  let isClosedManually = false
  let reconnectTimer: ReturnType<typeof setTimeout> | null = null

  function initConnection() {
    if (isClosedManually) return

    handlers.onStatusChange('connecting')
    eventSource = new EventSource(`${API_BASE}/stream`)

    eventSource.onopen = () => {
      handlers.onStatusChange('connected')
    }

    eventSource.addEventListener('hydrate', (e: MessageEvent) => {
      try {
        const data: StreamHydrationPayload = JSON.parse(e.data)
        handlers.onHydrate(data)
      } catch (err) {
        console.error('Failed to parse hydrate SSE payload', err)
      }
    })

    eventSource.addEventListener('lifecycle', (e: MessageEvent) => {
      try {
        const data: NotificationLifecycleEvent = JSON.parse(e.data)
        handlers.onLifecycle(data)
      } catch (err) {
        console.error('Failed to parse lifecycle SSE payload', err)
      }
    })

    eventSource.addEventListener('metrics', (e: MessageEvent) => {
      try {
        const data: TelemetrySnapshot = JSON.parse(e.data)
        handlers.onMetrics(data)
      } catch (err) {
        console.error('Failed to parse metrics SSE payload', err)
      }
    })

    eventSource.onerror = () => {
      handlers.onStatusChange('disconnected')
      if (eventSource) {
        eventSource.close()
        eventSource = null
      }
      if (!isClosedManually) {
        reconnectTimer = setTimeout(initConnection, 3000)
      }
    }
  }

  initConnection()

  return () => {
    isClosedManually = true
    if (reconnectTimer) clearTimeout(reconnectTimer)
    if (eventSource) {
      eventSource.close()
      eventSource = null
    }
  }
}
