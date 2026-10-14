<script setup lang="ts">
import { ref } from 'vue'
import type {
  DemoNotificationRequest,
  DemoNotificationResponse,
  NotificationPriority,
  NotificationType,
  TemplatePreviewResponse,
} from '../types/contracts'
import {
  triggerDemoNotification,
  renderTemplatePreview,
  submitContactForm,
} from '../services/api'

const emit = defineEmits<{
  (e: 'preview', previewData: TemplatePreviewResponse): void
  (e: 'dispatched', response: DemoNotificationResponse): void
  (e: 'contactSubmitted', msg: string): void
}>()

const activeTab = ref<'demo' | 'contact'>('demo')
const isSubmitting = ref(false)
const errorMessage = ref<string | null>(null)
const successNotice = ref<string | null>(null)

// Demo Form State
const demoForm = ref<DemoNotificationRequest>({
  targetRecipient: 'evaluator@client-corp.com',
  subject: 'Alerta del Sistema: Nodo de Cómputo Rebalanceado',
  content: 'El clúster de producción ha completado la migración de particiones hacia nodos .NET 10 con latencia sub-milisegundo.',
  eventType: 'SystemAlert',
  priority: 'High',
  senderName: 'NainDev Site Reliability Ops',
})

// Presets
interface Preset {
  label: string
  type: NotificationType
  priority: NotificationPriority
  subject: string
  content: string
  recipient: string
  sender: string
}

const presets: Preset[] = [
  {
    label: 'Alerta Crítica de Seguridad',
    type: 'SystemAlert',
    priority: 'High',
    subject: 'ALERTA: Detección de anomalía en pasarela de autenticación',
    content: 'Se detectaron 5 intentos fallidos concurrentes desde IP 198.51.100.42. El filtro perimetral ha bloqueado el tráfico temporalmente.',
    recipient: 'security-ops@enterprise.com',
    sender: 'NainDev Security Guard',
  },
  {
    label: 'Bienvenida Onboarding',
    type: 'WelcomeMessage',
    priority: 'Normal',
    subject: 'Bienvenido al ecosistema NainDev Cloud Platform',
    content: 'Tu cuenta ha sido aprovisionada con éxito. Ya puedes integrar microservicios de telemetría y validación determinista.',
    recipient: 'developer@enterprise.com',
    sender: 'Aitor Nain Mendoza',
  },
  {
    label: 'Benchmark del Broker de Mensajes',
    type: 'TestDemo',
    priority: 'High',
    subject: 'Test de Integración MassTransit + In-Memory Bus',
    content: 'Prueba de validación determinista de enrutamiento y deserialización JSON sin dependencias externas.',
    recipient: 'evaluator@demo-test.com',
    sender: 'Automated Evaluation Suite',
  },
  {
    label: 'Solicitud de Contacto Web',
    type: 'ContactFormSubmission',
    priority: 'High',
    subject: 'Consulta de Arquitectura .NET 10 & Python',
    content: 'Interesados en modernizar un monolito heredado a microservicios desacoplados con arquitectura hexagonal.',
    recipient: 'contact@naindev.com',
    sender: 'Director Técnico TechCorp',
  },
]

function applyPreset(preset: Preset) {
  demoForm.value.eventType = preset.type
  demoForm.value.priority = preset.priority
  demoForm.value.subject = preset.subject
  demoForm.value.content = preset.content
  demoForm.value.targetRecipient = preset.recipient
  demoForm.value.senderName = preset.sender
  errorMessage.value = null
  successNotice.value = `Plantilla "${preset.label}" cargada.`
}

async function handleDispatch() {
  isSubmitting.value = true
  errorMessage.value = null
  successNotice.value = null

  try {
    const res = await triggerDemoNotification(demoForm.value)
    emit('dispatched', res)
    successNotice.value = `Notificación encolada con éxito (Tracking: ${res.assignedTrackingId.slice(0, 8)}...). Receptor: ${res.effectiveRecipient}`
  } catch (err: unknown) {
    errorMessage.value = err instanceof Error ? err.message : 'Error desconocido al despachar'
  } finally {
    isSubmitting.value = false
  }
}

async function handlePreview() {
  isSubmitting.value = true
  errorMessage.value = null

  try {
    const preview = await renderTemplatePreview(demoForm.value)
    emit('preview', preview)
  } catch (err: unknown) {
    errorMessage.value = err instanceof Error ? err.message : 'Error al renderizar vista previa'
  } finally {
    isSubmitting.value = false
  }
}

// Contact Form State
const contactForm = ref({
  senderName: 'Evaluador Técnico',
  senderEmail: 'evaluador@empresa.com',
  subject: 'Consulta sobre el Microservicio de Notificaciones',
  message: 'Quisiera conocer más sobre las garantías de entrega y el aislamiento por MassTransit.',
  website: '', // Honeypot
})

const botSimulationActive = ref(false)

function toggleBotTrap() {
  botSimulationActive.value = !botSimulationActive.value
  contactForm.value.website = botSimulationActive.value ? 'https://spam-bot-crawler.ru' : ''
}

async function handleContactSubmit() {
  isSubmitting.value = true
  errorMessage.value = null
  successNotice.value = null

  try {
    const res = await submitContactForm(contactForm.value)
    if (botSimulationActive.value) {
      successNotice.value = `Filtro Honeypot Activado: La API respondió 202 Accepted para engañar al bot, pero el mensaje fue descartado en silencio por el gateway sin llegar a la cola.`
    } else {
      successNotice.value = `Formulario enviado con éxito (Tracking: ${res.trackingId.slice(0, 8)}...). Procesándose de forma asíncrona.`
    }
    emit('contactSubmitted', res.message)
  } catch (err: unknown) {
    errorMessage.value = err instanceof Error ? err.message : 'Error al enviar formulario'
  } finally {
    isSubmitting.value = false
  }
}
</script>

<template>
  <div class="glass-panel playground-panel">
    <!-- Panel Header with Tabs -->
    <div class="panel-header">
      <div class="header-left">
        <h2 class="display-title panel-title">Consola de Pruebas & Ingesta</h2>
        <p class="panel-desc">
          Dispara eventos asíncronos reales al broker o evalúa la defensa de honeypot antispam.
        </p>
      </div>

      <div class="tabs-nav" role="tablist">
        <button
          type="button"
          class="tab-btn"
          :class="{ 'tab-active': activeTab === 'demo' }"
          @click="activeTab = 'demo'"
        >
          Despacho Interactivo
        </button>
        <button
          type="button"
          class="tab-btn"
          :class="{ 'tab-active': activeTab === 'contact' }"
          @click="activeTab = 'contact'"
        >
          Honeypot Anti-Spam
        </button>
      </div>
    </div>

    <!-- Alert notices -->
    <div v-if="successNotice" class="alert alert-success" role="alert">
      <svg viewBox="0 0 20 20" fill="currentColor" width="16" height="16">
        <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clip-rule="evenodd" />
      </svg>
      <span>{{ successNotice }}</span>
    </div>

    <div v-if="errorMessage" class="alert alert-error" role="alert">
      <svg viewBox="0 0 20 20" fill="currentColor" width="16" height="16">
        <path fill-rule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7 4a1 1 0 11-2 0 1 1 0 012 0zm-1-9a1 1 0 00-1 1v4a1 1 0 102 0V6a1 1 0 00-1-1z" clip-rule="evenodd" />
      </svg>
      <span>{{ errorMessage }}</span>
    </div>

    <!-- Tab 1: Demo Dispatch -->
    <div v-if="activeTab === 'demo'" class="tab-content">
      <!-- Quick Presets -->
      <div class="presets-bar">
        <span class="presets-label">Escenarios Rápidos:</span>
        <div class="presets-list">
          <button
            v-for="p in presets"
            :key="p.label"
            type="button"
            class="preset-chip"
            @click="applyPreset(p)"
          >
            {{ p.label }}
          </button>
        </div>
      </div>

      <form @submit.prevent="handleDispatch" class="form-grid">
        <div class="form-row form-row-2">
          <div class="form-group">
            <label class="form-label" for="eventType">Tipo de Notificación</label>
            <select id="eventType" v-model="demoForm.eventType" class="form-select">
              <option value="SystemAlert">SystemAlert (Alerta de Sistema)</option>
              <option value="WelcomeMessage">WelcomeMessage (Bienvenida)</option>
              <option value="ContactFormSubmission">ContactFormSubmission (Contacto)</option>
              <option value="TestDemo">TestDemo (Prueba de Rendimiento)</option>
            </select>
          </div>

          <div class="form-group">
            <label class="form-label" for="priority">Prioridad de Entrega</label>
            <select id="priority" v-model="demoForm.priority" class="form-select">
              <option value="High">High (Alta - Procesamiento Inmediato)</option>
              <option value="Normal">Normal (Estándar)</option>
              <option value="Low">Low (Baja)</option>
            </select>
          </div>
        </div>

        <div class="form-row form-row-2">
          <div class="form-group">
            <label class="form-label" for="targetRecipient">
              Destinatario
              <span class="sandbox-tag" title="El sandbox redirige correos no autorizados a un buzón seguro">Sandbox Guard</span>
            </label>
            <input
              id="targetRecipient"
              v-model="demoForm.targetRecipient"
              type="email"
              required
              class="form-input"
              placeholder="evaluador@empresa.com"
            />
          </div>

          <div class="form-group">
            <label class="form-label" for="senderName">Remitente / Emisor</label>
            <input
              id="senderName"
              v-model="demoForm.senderName"
              type="text"
              class="form-input"
              placeholder="NainDev Ops"
            />
          </div>
        </div>

        <div class="form-group">
          <label class="form-label" for="subject">Asunto</label>
          <input
            id="subject"
            v-model="demoForm.subject"
            type="text"
            required
            class="form-input"
            placeholder="Asunto de la notificación..."
          />
        </div>

        <div class="form-group">
          <label class="form-label" for="content">Cuerpo del Mensaje</label>
          <textarea
            id="content"
            v-model="demoForm.content"
            rows="3"
            required
            class="form-textarea"
            placeholder="Escribe el contenido o variables de la notificación..."
          ></textarea>
        </div>

        <div class="form-actions">
          <button
            type="button"
            class="btn btn-secondary"
            :disabled="isSubmitting"
            @click="handlePreview"
          >
            <svg viewBox="0 0 20 20" fill="currentColor" width="16" height="16">
              <path d="M10 12a2 2 0 100-4 2 2 0 000 4z" />
              <path fill-rule="evenodd" d="M.458 10C1.732 5.943 5.522 3 10 3s8.268 2.943 9.542 7c-1.274 4.057-5.064 7-9.542 7S1.732 14.057.458 10zM14 10a4 4 0 11-8 0 4 4 0 018 0z" clip-rule="evenodd" />
            </svg>
            Previsualizar Plantilla HTML
          </button>

          <button
            type="submit"
            class="btn btn-primary"
            :disabled="isSubmitting"
          >
            <svg viewBox="0 0 20 20" fill="currentColor" width="16" height="16">
              <path d="M10.894 2.553a1 1 0 00-1.788 0l-7 14a1 1 0 001.169 1.409l5-1.429A1 1 0 009 15.571V11a1 1 0 112 0v4.571a1 1 0 00.725.962l5 1.428a1 1 0 001.17-1.408l-7-14z" />
            </svg>
            {{ isSubmitting ? 'Encolando en Broker...' : 'Despachar al Broker MassTransit' }}
          </button>
        </div>
      </form>
    </div>

    <!-- Tab 2: Honeypot Anti-Spam Demonstration -->
    <div v-if="activeTab === 'contact'" class="tab-content">
      <div class="honeypot-banner">
        <div class="honeypot-info">
          <h4>Arquitectura Anti-Spam Determinista</h4>
          <p>
            El gateway implementa un campo señuelo oculto (<code>Website</code>). Los usuarios humanos nunca lo ven ni lo rellenan. Si un bot crawler lo completa, la API devuelve <code>202 Accepted</code> para no delatar la defensa, pero descarta el mensaje inmediatamente sin saturar el bus ni enviar correos spam.
          </p>
        </div>
        <button
          type="button"
          class="btn"
          :class="botSimulationActive ? 'btn-danger' : 'btn-secondary'"
          @click="toggleBotTrap"
        >
          {{ botSimulationActive ? 'Modo Bot Activo (Campo relleno)' : 'Simular Bot Invasor' }}
        </button>
      </div>

      <form @submit.prevent="handleContactSubmit" class="form-grid">
        <div class="form-row form-row-2">
          <div class="form-group">
            <label class="form-label" for="contactName">Nombre Completo</label>
            <input
              id="contactName"
              v-model="contactForm.senderName"
              type="text"
              required
              class="form-input"
            />
          </div>

          <div class="form-group">
            <label class="form-label" for="contactEmail">Correo de Contacto</label>
            <input
              id="contactEmail"
              v-model="contactForm.senderEmail"
              type="email"
              required
              class="form-input"
            />
          </div>
        </div>

        <div class="form-group">
          <label class="form-label" for="contactSubject">Asunto</label>
          <input
            id="contactSubject"
            v-model="contactForm.subject"
            type="text"
            required
            class="form-input"
          />
        </div>

        <div class="form-group">
          <label class="form-label" for="contactMsg">Mensaje</label>
          <textarea
            id="contactMsg"
            v-model="contactForm.message"
            rows="3"
            required
            class="form-textarea"
          ></textarea>
        </div>

        <!-- Honeypot Field (visible only when simulating bot for demonstration) -->
        <div v-if="botSimulationActive" class="form-group honeypot-field-visible">
          <label class="form-label text-rose">
            CAMPO HONEYPOT OCULTO (Relleno por Bot)
          </label>
          <input
            v-model="contactForm.website"
            type="text"
            class="form-input border-rose font-mono"
            placeholder="http://spam-link.xyz"
          />
        </div>

        <div class="form-actions">
          <button
            type="submit"
            class="btn btn-primary"
            :disabled="isSubmitting"
          >
            Enviar Mensaje por Gateway
          </button>
        </div>
      </form>
    </div>
  </div>
</template>

<style scoped>
.playground-panel {
  padding: 1.5rem;
  margin-top: 1.5rem;
}

.panel-header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 1.5rem;
  margin-bottom: 1.5rem;
}

.panel-title {
  font-size: 1.25rem;
  color: var(--color-text-main);
  margin-bottom: 0.25rem;
}

.panel-desc {
  font-size: 0.85rem;
  color: var(--color-text-dim);
}

.tabs-nav {
  display: flex;
  background: rgba(10, 14, 28, 0.8);
  border: 1px solid var(--color-border);
  padding: 0.25rem;
  border-radius: var(--radius-md);
  gap: 0.25rem;
}

.tab-btn {
  background: transparent;
  border: none;
  color: var(--color-text-dim);
  padding: 0.45rem 0.85rem;
  border-radius: var(--radius-sm);
  font-size: 0.825rem;
  font-weight: 600;
  cursor: pointer;
  transition: all var(--transition-fast);
}

.tab-btn:hover {
  color: var(--color-text-main);
}

.tab-active {
  background: rgba(255, 255, 255, 0.1);
  color: var(--color-text-main);
}

.presets-bar {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  margin-bottom: 1.25rem;
  padding: 0.65rem 0.85rem;
  background: rgba(10, 14, 28, 0.6);
  border: 1px dashed var(--color-border);
  border-radius: var(--radius-md);
  overflow-x: auto;
}

.presets-label {
  font-size: 0.775rem;
  font-weight: 600;
  color: var(--color-text-muted);
  white-space: nowrap;
}

.presets-list {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.preset-chip {
  background: rgba(255, 255, 255, 0.05);
  border: 1px solid var(--color-border);
  color: var(--color-text-main);
  font-size: 0.775rem;
  font-weight: 500;
  padding: 0.25rem 0.65rem;
  border-radius: var(--radius-full);
  cursor: pointer;
  white-space: nowrap;
  transition: all var(--transition-fast);
}

.preset-chip:hover {
  background: rgba(56, 189, 248, 0.15);
  border-color: var(--accent-cyan);
  color: var(--accent-cyan);
}

.form-grid {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.form-row {
  display: grid;
  gap: 1rem;
}

.form-row-2 {
  grid-template-columns: 1fr 1fr;
}

.sandbox-tag {
  font-size: 0.675rem;
  background: rgba(245, 158, 11, 0.15);
  color: var(--accent-amber);
  padding: 0.1rem 0.4rem;
  border-radius: 4px;
  border: 1px solid rgba(245, 158, 11, 0.3);
  margin-left: 0.35rem;
  font-weight: 600;
}

.form-actions {
  display: flex;
  align-items: center;
  justify-content: flex-end;
  gap: 0.75rem;
  margin-top: 0.5rem;
}

.alert {
  display: flex;
  align-items: center;
  gap: 0.65rem;
  padding: 0.75rem 1rem;
  border-radius: var(--radius-md);
  font-size: 0.85rem;
  margin-bottom: 1.25rem;
}

.alert-success {
  background: rgba(52, 211, 153, 0.12);
  border: 1px solid rgba(52, 211, 153, 0.3);
  color: #34d399;
}

.alert-error {
  background: rgba(244, 63, 94, 0.12);
  border: 1px solid rgba(244, 63, 94, 0.3);
  color: #f43f5e;
}

.honeypot-banner {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 1rem;
  background: rgba(15, 21, 42, 0.6);
  border: 1px solid var(--color-border);
  padding: 1rem 1.25rem;
  border-radius: var(--radius-md);
  margin-bottom: 1.25rem;
}

.honeypot-info h4 {
  font-size: 0.95rem;
  color: var(--color-text-main);
  margin-bottom: 0.25rem;
}

.honeypot-info p {
  font-size: 0.825rem;
  color: var(--color-text-muted);
  line-height: 1.4;
}

.honeypot-info code {
  font-family: var(--font-mono);
  background: rgba(255, 255, 255, 0.08);
  padding: 0.1rem 0.3rem;
  border-radius: 4px;
}

.btn-danger {
  background: rgba(244, 63, 94, 0.2);
  border: 1px solid var(--accent-rose);
  color: var(--accent-rose);
}

.honeypot-field-visible {
  background: rgba(244, 63, 94, 0.08);
  border: 1px dashed var(--accent-rose);
  padding: 0.75rem;
  border-radius: var(--radius-md);
}

.border-rose {
  border-color: var(--accent-rose) !important;
}

.text-rose {
  color: var(--accent-rose) !important;
}

@media (max-width: 768px) {
  .panel-header {
    flex-direction: column;
  }
  .form-row-2 {
    grid-template-columns: 1fr;
  }
  .honeypot-banner {
    flex-direction: column;
    align-items: flex-start;
  }
}
</style>
