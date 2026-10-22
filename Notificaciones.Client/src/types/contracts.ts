export type NotificationStage =
  | 'Accepted'
  | 'Queued'
  | 'Received'
  | 'Rendered'
  | 'Dispatched'
  | 'Failed'

export type NotificationType =
  | 'ContactFormSubmission'
  | 'WelcomeMessage'
  | 'SystemAlert'
  | 'TestDemo'

export type NotificationPriority = 'High' | 'Normal' | 'Low'

export interface NotificationLifecycleEvent {
  notificationId: string
  stage: NotificationStage
  type: NotificationType
  priority: string
  recipient: string
  subject: string
  source: string
  elapsedMilliseconds: number
  payloadBytes: number
  attempt: number
  channel: string
  detail: string
  error: string | null
  timestamp: string
}

export interface TelemetrySnapshot {
  uptimeSeconds: number
  accepted: number
  dispatched: number
  failed: number
  inFlight: number
  successRate: number
  averageLatencyMs: number
  p95LatencyMs: number
  fastestLatencyMs: number
  renderedBytes: number
  byType: Record<string, number>
  byChannel: Record<string, number>
  throughputSeries: number[]
  lastEventAt: string | null
}

export interface HealthDependency {
  status: string
  description?: string
  durationMs: number
}

export interface HealthStatusResponse {
  service: string
  version: string
  architecture: string
  status: string
  transport: string
  workerInProcess: boolean
  instanceId: string
  uptimeSeconds: number
  checkDurationMs: number
  dependencies: Record<string, HealthDependency>
  timestamp: string
}

export interface DemoNotificationRequest {
  targetRecipient: string
  subject: string
  content: string
  eventType: NotificationType
  priority: NotificationPriority
  senderName?: string
}

export interface DemoNotificationResponse {
  status: string
  broker: string
  assignedTrackingId: string
  targetType: string
  effectiveRecipient: string
  sandboxed: boolean
  sandboxNotice?: string | null
}

export interface ContactFormRequest {
  senderName: string
  senderEmail: string
  subject: string
  message: string
  website?: string
}

export interface ContactFormResponse {
  status: string
  message: string
  trackingId: string
  timestamp: string
}

export interface TemplatePreviewResponse {
  html: string
  plainText: string
  bytes: number
  renderMilliseconds: number
  type: string
}

export interface StreamHydrationPayload {
  snapshot: TelemetrySnapshot
  recent: NotificationLifecycleEvent[]
  listeners: number
}
