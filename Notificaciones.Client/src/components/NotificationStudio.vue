<script setup lang="ts">
import { computed, onMounted, onUnmounted, ref, watch } from 'vue'
import type {
  DemoNotificationRequest,
  NotificationLifecycleEvent,
  NotificationPriority,
  NotificationStage,
  NotificationType,
  TemplatePreviewResponse,
} from '../types/contracts'
import { renderTemplatePreview, submitContactForm, triggerDemoNotification } from '../services/api'
import EmailPreview from './EmailPreview.vue'
import StageStepper from './StageStepper.vue'

const props = defineProps<{
  events: NotificationLifecycleEvent[]
  standalone: boolean
}>()

const emit = defineEmits<{
  (e: 'expand', payload: { preview: TemplatePreviewResponse; from: string; to: string; subject: string }): void
}>()

function expandPreview() {
  if (!preview.value) return
  emit('expand', { preview: preview.value, from: previewFrom.value, to: previewTo.value, subject: form.value.subject })
}

type Mode = 'notify' | 'contact'

interface Scenario {
  id: string
  label: string
  hint: string
  icon: 'alert' | 'wave' | 'wrench' | 'flask' | 'mail'
  mode: Mode
  type: NotificationType
  priority: NotificationPriority
  recipient: string
  subject: string
  content: string
  sender: string
}

const SCENARIOS: Scenario[] = [
  {
    id: 'security',
    label: 'Alerta de seguridad',
    hint: 'Prioridad alta',
    icon: 'alert',
    mode: 'notify',
    type: 'SystemAlert',
    priority: 'High',
    recipient: 'seguridad@empresa.com',
    subject: 'Acceso desde un dispositivo nuevo',
    content: 'Hemos detectado un inicio de sesión desde un dispositivo que no reconocemos. Si no has sido tú, revisa tu cuenta ahora.',
    sender: 'Seguridad',
  },
  {
    id: 'welcome',
    label: 'Bienvenida',
    hint: 'Alta de usuario',
    icon: 'wave',
    mode: 'notify',
    type: 'WelcomeMessage',
    priority: 'Normal',
    recipient: 'nuevo.usuario@empresa.com',
    subject: 'Tu cuenta está lista',
    content: 'Bienvenido. Ya puedes empezar a enviar notificaciones desde tu panel.',
    sender: 'Equipo NainNotify',
  },
  {
    id: 'maintenance',
    label: 'Aviso de mantenimiento',
    hint: 'Comunicado',
    icon: 'wrench',
    mode: 'notify',
    type: 'SystemAlert',
    priority: 'Normal',
    recipient: 'clientes@empresa.com',
    subject: 'Mantenimiento programado',
    content: 'El servicio se actualizará el sábado a las 03:00. No esperamos interrupciones.',
    sender: 'Operaciones',
  },
  {
    id: 'test',
    label: 'Prueba técnica',
    hint: 'Ping del pipeline',
    icon: 'flask',
    mode: 'notify',
    type: 'TestDemo',
    priority: 'Low',
    recipient: 'qa@empresa.com',
    subject: 'Ping de prueba',
    content: 'Mensaje de prueba para verificar el recorrido completo.',
    sender: 'QA',
  },
  {
    id: 'contact',
    label: 'Formulario de contacto',
    hint: 'Con honeypot',
    icon: 'mail',
    mode: 'contact',
    type: 'ContactFormSubmission',
    priority: 'High',
    recipient: 'contacto@naindev.com',
    subject: 'Consulta sobre integración',
    content: 'Hola, quiero integrar notificaciones en nuestra plataforma. ¿Podemos hablar?',
    sender: 'Laura Pérez',
  },
]

const selectedId = ref<string>(SCENARIOS[0].id)
const mode = ref<Mode>('notify')

const form = ref({
  recipient: SCENARIOS[0].recipient,
  subject: SCENARIOS[0].subject,
  content: SCENARIOS[0].content,
  sender: SCENARIOS[0].sender,
  senderEmail: 'laura.perez@empresa.com',
  type: SCENARIOS[0].type as NotificationType,
  priority: SCENARIOS[0].priority as NotificationPriority,
})

const botMode = ref(false)
const showAdvanced = ref(false)

function applyScenario(s: Scenario) {
  selectedId.value = s.id
  mode.value = s.mode
  botMode.value = false
  form.value.recipient = s.recipient
  form.value.subject = s.subject
  form.value.content = s.content
  form.value.sender = s.sender
  form.value.type = s.type
  form.value.priority = s.priority
  result.value = null
}

const TYPE_OPTIONS: { value: NotificationType; label: string }[] = [
  { value: 'SystemAlert', label: 'Alerta' },
  { value: 'WelcomeMessage', label: 'Bienvenida' },
  { value: 'ContactFormSubmission', label: 'Contacto' },
  { value: 'TestDemo', label: 'Prueba' },
]

const PRIORITIES: { value: NotificationPriority; label: string }[] = [
  { value: 'High', label: 'Alta' },
  { value: 'Normal', label: 'Normal' },
  { value: 'Low', label: 'Baja' },
]

function toDemoRequest(): DemoNotificationRequest {
  return {
    targetRecipient: form.value.recipient,
    subject: form.value.subject,
    content: form.value.content,
    eventType: mode.value === 'contact' ? 'ContactFormSubmission' : form.value.type,
    priority: mode.value === 'contact' ? 'High' : form.value.priority,
    senderName: form.value.sender,
  }
}

// Live preview, debounced
const preview = ref<TemplatePreviewResponse | null>(null)
const previewLoading = ref(false)
let previewTimer: number | undefined
let previewSeq = 0

function schedulePreview(delay = 350) {
  window.clearTimeout(previewTimer)
  previewTimer = window.setTimeout(refreshPreview, delay)
}

async function refreshPreview() {
  const seq = ++previewSeq
  previewLoading.value = true
  try {
    const res = await renderTemplatePreview(toDemoRequest())
    if (seq === previewSeq) preview.value = res
  } catch {
    // Keep the previous preview; the send button still reports errors.
  } finally {
    if (seq === previewSeq) previewLoading.value = false
  }
}

watch(() => [form.value.subject, form.value.content, form.value.sender, form.value.type, form.value.recipient, mode.value], () => schedulePreview())
onMounted(() => schedulePreview(0))
onUnmounted(() => window.clearTimeout(previewTimer))

// Sending
const sending = ref(false)
const result = ref<{ tone: 'ok' | 'warn' | 'danger'; text: string; trackingId?: string } | null>(null)

async function send() {
  sending.value = true
  result.value = null
  try {
    if (mode.value === 'contact') {
      const res = await submitContactForm({
        senderName: form.value.sender,
        senderEmail: form.value.senderEmail,
        subject: form.value.subject,
        message: form.value.content,
        website: botMode.value ? 'https://spam-crawler.example' : '',
      })
      result.value = botMode.value
        ? { tone: 'warn', text: 'Bot detectado: el gateway respondió 202 y descartó el mensaje en silencio.' }
        : { tone: 'ok', text: 'Enviada. Síguela abajo.', trackingId: res.trackingId }
    } else {
      const res = await triggerDemoNotification(toDemoRequest())
      result.value = { tone: 'ok', text: 'Enviada. Síguela abajo.', trackingId: res.assignedTrackingId }
    }
  } catch (err) {
    result.value = { tone: 'danger', text: err instanceof Error ? err.message : 'No se pudo enviar.' }
  } finally {
    sending.value = false
  }
}

const trackedStages = computed<NotificationStage[]>(() => {
  const id = result.value?.trackingId
  if (!id) return []
  return props.events.filter((e) => e.notificationId === id).map((e) => e.stage)
})

const trackedFailed = computed(() => trackedStages.value.includes('Failed'))
const trackedDone = computed(() => trackedStages.value.includes('Dispatched'))

const previewFrom = computed(() => form.value.sender || 'NainNotify')
const previewTo = computed(() => (mode.value === 'contact' ? 'contacto@naindev.com' : form.value.recipient))
</script>

<template>
  <section class="section studio-section" id="studio" aria-labelledby="studio-title">
    <div class="container">
      <div class="section-head">
        <h2 class="section-title" id="studio-title">Pruébalo</h2>
        <span v-if="standalone" class="muted small">Demo local: nada sale de tu navegador.</span>
        <span v-else class="muted small">Los destinatarios externos se redirigen a un buzón seguro.</span>
      </div>

      <div class="panel studio">
        <!-- Left: choose + adjust + send -->
        <div class="compose">
          <div class="step">
            <h3 class="step-title"><span class="step-num">1</span> Elige un escenario</h3>
            <div class="scenarios" role="list">
              <button
                v-for="s in SCENARIOS"
                :key="s.id"
                type="button"
                class="scenario"
                :class="{ 'is-selected': s.id === selectedId }"
                role="listitem"
                :aria-pressed="s.id === selectedId"
                @click="applyScenario(s)"
              >
                <span class="scenario-icon" aria-hidden="true">
                  <svg v-if="s.icon === 'alert'" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.9" stroke-linecap="round" stroke-linejoin="round"><path d="M12 9v4M12 17h.01" /><path d="M10.3 3.9L2.4 17.5A2 2 0 0 0 4.1 20.5h15.8a2 2 0 0 0 1.7-3L13.7 3.9a2 2 0 0 0-3.4 0z" /></svg>
                  <svg v-else-if="s.icon === 'wave'" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.9" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="8" r="4" /><path d="M4 21a8 8 0 0 1 16 0" /></svg>
                  <svg v-else-if="s.icon === 'wrench'" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.9" stroke-linecap="round" stroke-linejoin="round"><path d="M14.7 6.3a4 4 0 0 0 5 5L22 14l-8 8-2.7-2.7a4 4 0 0 0-5-5L3 11l8-8 3.7 3.3z" /></svg>
                  <svg v-else-if="s.icon === 'flask'" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.9" stroke-linecap="round" stroke-linejoin="round"><path d="M9 3h6M10 3v6L4.5 19a1.5 1.5 0 0 0 1.3 2.2h12.4a1.5 1.5 0 0 0 1.3-2.2L14 9V3" /></svg>
                  <svg v-else viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.9" stroke-linecap="round" stroke-linejoin="round"><rect x="3" y="5" width="18" height="14" rx="2.5" /><path d="M3 8l9 6 9-6" /></svg>
                </span>
                <span class="scenario-text">
                  <span class="scenario-label">{{ s.label }}</span>
                  <span class="scenario-hint">{{ s.hint }}</span>
                </span>
              </button>
            </div>
          </div>

          <form class="step" @submit.prevent="send">
            <h3 class="step-title"><span class="step-num">2</span> Ajusta el mensaje</h3>

            <div class="fields">
              <template v-if="mode === 'contact'">
                <div class="row-2">
                  <label class="field">
                    <span class="field-label">Tu nombre</span>
                    <input v-model="form.sender" class="input" type="text" required autocomplete="off" />
                  </label>
                  <label class="field">
                    <span class="field-label">Tu correo</span>
                    <input v-model="form.senderEmail" class="input" type="email" required autocomplete="off" />
                  </label>
                </div>
              </template>
              <label v-else class="field">
                <span class="field-label">Para</span>
                <input v-model="form.recipient" class="input" type="email" required autocomplete="off" placeholder="nombre@empresa.com" />
              </label>

              <label class="field">
                <span class="field-label">Asunto</span>
                <input v-model="form.subject" class="input" type="text" required maxlength="120" />
              </label>

              <label class="field">
                <span class="field-label">Mensaje</span>
                <textarea v-model="form.content" class="textarea" rows="3" required maxlength="2000"></textarea>
              </label>

              <div v-if="mode === 'contact'" class="honeypot">
                <label class="switch">
                  <input v-model="botMode" type="checkbox" />
                  <span class="switch-track" aria-hidden="true"></span>
                  <span>Simular un bot</span>
                </label>
                <input
                  v-if="botMode"
                  class="input mono trap"
                  type="text"
                  value="https://spam-crawler.example"
                  readonly
                  aria-label="Campo oculto rellenado por el bot"
                />
              </div>

              <details v-else class="advanced" :open="showAdvanced" @toggle="showAdvanced = ($event.target as HTMLDetailsElement).open">
                <summary>Más opciones</summary>
                <div class="advanced-body">
                  <div class="field">
                    <span class="field-label">Tipo</span>
                    <div class="chips" role="group" aria-label="Tipo">
                      <button v-for="t in TYPE_OPTIONS" :key="t.value" type="button" class="chip" :aria-pressed="form.type === t.value" @click="form.type = t.value">{{ t.label }}</button>
                    </div>
                  </div>
                  <div class="field">
                    <span class="field-label">Prioridad</span>
                    <div class="chips" role="group" aria-label="Prioridad">
                      <button v-for="p in PRIORITIES" :key="p.value" type="button" class="chip" :aria-pressed="form.priority === p.value" @click="form.priority = p.value">{{ p.label }}</button>
                    </div>
                  </div>
                  <label class="field">
                    <span class="field-label">Remitente</span>
                    <input v-model="form.sender" class="input" type="text" maxlength="60" />
                  </label>
                </div>
              </details>
            </div>

            <div class="send-row">
              <h3 class="step-title"><span class="step-num">3</span> Envía</h3>
              <button type="submit" class="btn btn-primary btn-lg send-btn" :disabled="sending">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M22 2L11 13" /><path d="M22 2l-7 20-4-9-9-4 20-7z" /></svg>
                {{ sending ? 'Enviando…' : 'Enviar' }}
              </button>
            </div>

            <div v-if="result" class="result" :class="`tone-${result.tone}`" role="status" aria-live="polite">
              <template v-if="result.trackingId">
                <StageStepper :reached="trackedStages" :failed="trackedFailed" size="md" />
                <span class="result-text">
                  {{ trackedFailed ? 'Falló la entrega.' : trackedDone ? 'Entregada.' : 'En camino…' }}
                  <span class="mono muted">#{{ result.trackingId.slice(0, 8) }}</span>
                </span>
              </template>
              <span v-else class="result-text">{{ result.text }}</span>
            </div>
          </form>
        </div>

        <!-- Right: live preview -->
        <div class="preview-col">
          <h3 class="step-title preview-title">Así lo recibirán</h3>
          <EmailPreview
            :preview="preview"
            :loading="previewLoading"
            :from="previewFrom"
            :to="previewTo"
            :subject="form.subject"
            @expand="expandPreview"
          />
        </div>
      </div>
    </div>
  </section>
</template>

<style scoped>
.small {
  font-size: 0.85rem;
}

.studio {
  display: grid;
  grid-template-columns: minmax(0, 1fr) minmax(0, 1.05fr);
  gap: 0;
  overflow: hidden;
}

.compose {
  padding: clamp(1.1rem, 2.5vw, 1.75rem);
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
  border-right: 1px solid var(--line);
}

.preview-col {
  padding: clamp(1.1rem, 2.5vw, 1.75rem);
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
  background: rgba(255, 255, 255, 0.015);
}

.step {
  display: flex;
  flex-direction: column;
  gap: 0.85rem;
}

.step-title {
  display: flex;
  align-items: center;
  gap: 0.55rem;
  font-size: 0.95rem;
  font-weight: 600;
}

.step-num {
  display: inline-grid;
  place-items: center;
  width: 22px;
  height: 22px;
  border-radius: 50%;
  font-family: var(--font-mono);
  font-size: 0.72rem;
  color: var(--accent);
  background: var(--accent-soft);
  border: 1px solid var(--accent-ring);
}

.scenarios {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 0.6rem;
}

.scenario {
  display: flex;
  align-items: center;
  gap: 0.7rem;
  padding: 0.7rem 0.8rem;
  text-align: left;
  border-radius: var(--radius-sm);
  border: 1px solid var(--line);
  background: var(--bg-inset);
  cursor: pointer;
  transition:
    border-color var(--motion-fast) var(--ease),
    background var(--motion-fast) var(--ease),
    transform var(--motion-fast) var(--ease);
}

.scenario:hover {
  border-color: var(--line-strong);
  transform: translateY(-1px);
}

.scenario.is-selected {
  border-color: var(--accent-ring);
  background: var(--accent-soft);
}

.scenario-icon {
  width: 34px;
  height: 34px;
  flex-shrink: 0;
  display: grid;
  place-items: center;
  border-radius: 9px;
  background: rgba(255, 255, 255, 0.05);
  color: var(--text-2);
}

.scenario.is-selected .scenario-icon {
  background: var(--accent-gradient);
  color: #04101c;
}

.scenario-icon svg {
  width: 18px;
  height: 18px;
}

.scenario-text {
  display: flex;
  flex-direction: column;
  line-height: 1.2;
  min-width: 0;
}

.scenario-label {
  font-size: 0.86rem;
  font-weight: 600;
}

.scenario-hint {
  font-size: 0.72rem;
  color: var(--text-3);
}

.fields {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.row-2 {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 0.75rem;
}

.advanced summary {
  cursor: pointer;
  font-size: 0.82rem;
  color: var(--text-2);
  list-style: none;
  display: inline-flex;
  align-items: center;
  gap: 0.35rem;
  padding: 0.25rem 0;
}

.advanced summary::-webkit-details-marker {
  display: none;
}

.advanced summary::before {
  content: '';
  width: 6px;
  height: 6px;
  border-right: 1.5px solid currentColor;
  border-bottom: 1.5px solid currentColor;
  transform: rotate(-45deg);
  transition: transform var(--motion-fast) var(--ease);
}

.advanced[open] summary::before {
  transform: rotate(45deg);
}

.advanced-body {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
  padding-top: 0.75rem;
}

.chips {
  display: flex;
  flex-wrap: wrap;
  gap: 0.4rem;
}

.honeypot {
  display: flex;
  flex-direction: column;
  gap: 0.6rem;
}

.switch {
  display: inline-flex;
  align-items: center;
  gap: 0.6rem;
  font-size: 0.85rem;
  cursor: pointer;
  user-select: none;
}

.switch input {
  position: absolute;
  opacity: 0;
  width: 1px;
  height: 1px;
}

.switch-track {
  width: 36px;
  height: 20px;
  border-radius: 10px;
  background: rgba(255, 255, 255, 0.12);
  position: relative;
  transition: background var(--motion-fast) var(--ease);
}

.switch-track::after {
  content: '';
  position: absolute;
  top: 2px;
  left: 2px;
  width: 16px;
  height: 16px;
  border-radius: 50%;
  background: #fff;
  transition: transform var(--motion-fast) var(--ease);
}

.switch input:checked + .switch-track {
  background: var(--warn);
}

.switch input:checked + .switch-track::after {
  transform: translateX(16px);
}

.switch input:focus-visible + .switch-track {
  outline: 2px solid var(--accent);
  outline-offset: 2px;
}

.trap {
  border-color: rgba(251, 191, 36, 0.5);
  color: var(--warn);
  font-size: 0.82rem;
}

.send-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 1rem;
  margin-top: 0.25rem;
}

.send-btn {
  min-width: 160px;
}

.result {
  display: flex;
  align-items: center;
  gap: 0.9rem;
  padding: 0.75rem 0.9rem;
  border-radius: var(--radius-sm);
  border: 1px solid var(--line);
  background: var(--bg-inset);
  font-size: 0.875rem;
  animation: fade-up var(--motion) var(--ease-out) both;
}

.result.tone-ok { border-color: rgba(52, 211, 153, 0.3); }
.result.tone-warn { border-color: rgba(251, 191, 36, 0.4); color: var(--warn); }
.result.tone-danger { border-color: rgba(244, 63, 94, 0.4); color: var(--danger); }

.result-text {
  display: inline-flex;
  gap: 0.5rem;
  align-items: baseline;
}

.preview-title {
  margin-bottom: 0.25rem;
}

@media (max-width: 960px) {
  .studio {
    grid-template-columns: 1fr;
  }
  .compose {
    border-right: 0;
    border-bottom: 1px solid var(--line);
  }
}

@media (max-width: 480px) {
  .scenarios,
  .row-2 {
    grid-template-columns: 1fr;
  }
  .send-row {
    flex-direction: column;
    align-items: stretch;
  }
  .send-btn {
    width: 100%;
  }
}
</style>
