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
import { mockNotifyEngine } from './mockNotifyEngine'

const API_BASE = '/api/v1/Notifications'

export async function fetchHealth(): Promise<HealthStatusResponse> {
  try {
    const res = await fetch(`${API_BASE}/health`)
    if (res.ok) return await res.json()
  } catch {
    // Standalone fallback
  }
  return mockNotifyEngine.fetchHealth()
}

export async function fetchMetrics(): Promise<{ snapshot: TelemetrySnapshot; listeners: number }> {
  try {
    const res = await fetch(`${API_BASE}/metrics`)
    if (res.ok) return await res.json()
  } catch {
    // Standalone fallback
  }
  return mockNotifyEngine.fetchMetrics()
}

export async function fetchActivity(limit = 40): Promise<NotificationLifecycleEvent[]> {
  try {
    const res = await fetch(`${API_BASE}/activity?limit=${limit}`)
    if (res.ok) return await res.json()
  } catch {
    // Standalone fallback
  }
  return mockNotifyEngine.fetchActivity().slice(0, limit)
}

export async function triggerDemoNotification(
  payload: DemoNotificationRequest
): Promise<DemoNotificationResponse> {
  try {
    const res = await fetch(`${API_BASE}/demo`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload),
    })
    if (res.ok) return await res.json()
  } catch {
    // Standalone fallback
  }
  return mockNotifyEngine.triggerDemo(payload)
}

export async function submitContactForm(
  payload: ContactFormRequest
): Promise<{ status: string; message: string; trackingId: string; timestamp: string }> {
  try {
    const res = await fetch(`${API_BASE}/contact`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload),
    })
    if (res.ok) return await res.json()
  } catch {
    // Standalone fallback
  }
  return mockNotifyEngine.submitContact(payload)
}

export async function renderTemplatePreview(
  payload: DemoNotificationRequest
): Promise<TemplatePreviewResponse> {
  try {
    const res = await fetch(`${API_BASE}/preview`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload),
    })
    if (res.ok) return await res.json()
  } catch {
    // Standalone fallback
  }
  return mockNotifyEngine.renderPreview(payload)
}

export type SseConnectionStatus = 'connecting' | 'connected' | 'disconnected' | 'error'

export interface SseHandlers {
  onHydrate: (payload: StreamHydrationPayload) => void
  onLifecycle: (event: NotificationLifecycleEvent) => void
  onMetrics: (snapshot: TelemetrySnapshot) => void
  onStatusChange: (status: SseConnectionStatus) => void
}

export function connectNotificationStream(handlers: SseHandlers): () => void {
  if (window.location.hostname === 'localhost' && window.location.port === '5378') {
    let eventSource: EventSource | null = null
    let isClosedManually = false

    function initConnection() {
      if (isClosedManually) return
      handlers.onStatusChange('connecting')
      eventSource = new EventSource(`${API_BASE}/stream`)

      eventSource.addEventListener('hydrate', (e: MessageEvent) => {
        try {
          handlers.onHydrate(JSON.parse(e.data))
        } catch (err) {
          console.error('Failed to parse hydrate payload', err)
        }
      })

      eventSource.addEventListener('lifecycle', (e: MessageEvent) => {
        try {
          handlers.onLifecycle(JSON.parse(e.data))
        } catch (err) {
          console.error('Failed to parse lifecycle payload', err)
        }
      })

      eventSource.addEventListener('metrics', (e: MessageEvent) => {
        try {
          handlers.onMetrics(JSON.parse(e.data))
        } catch (err) {
          console.error('Failed to parse metrics payload', err)
        }
      })

      eventSource.onopen = () => {
        handlers.onStatusChange('connected')
      }

      eventSource.onerror = () => {
        handlers.onStatusChange('error')
        if (eventSource) {
          eventSource.close()
          eventSource = null
        }
      }
    }

    initConnection()
    return () => {
      isClosedManually = true
      if (eventSource) {
        eventSource.close()
        eventSource = null
      }
    }
  }

  return mockNotifyEngine.connectStream(handlers)
}
