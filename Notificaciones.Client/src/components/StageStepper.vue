<script setup lang="ts">
import { computed } from 'vue'
import type { NotificationStage } from '../types/contracts'

const props = withDefaults(
  defineProps<{
    reached: NotificationStage[]
    failed?: boolean
    size?: 'sm' | 'md'
  }>(),
  { failed: false, size: 'sm' }
)

const ORDER: NotificationStage[] = ['Accepted', 'Queued', 'Received', 'Rendered', 'Dispatched']
const LABEL: Record<NotificationStage, string> = {
  Accepted: 'Aceptada',
  Queued: 'En cola',
  Received: 'Recibida',
  Rendered: 'Renderizada',
  Dispatched: 'Entregada',
  Failed: 'Fallida',
}

const reachedIndex = computed(() => {
  let max = -1
  for (const s of props.reached) {
    const i = ORDER.indexOf(s)
    if (i > max) max = i
  }
  return max
})

const done = computed(() => reachedIndex.value === ORDER.length - 1 || props.failed)

const steps = computed(() =>
  ORDER.map((stage, i) => ({
    stage,
    label: LABEL[stage],
    state:
      props.failed && i > reachedIndex.value
        ? 'failed'
        : i < reachedIndex.value
          ? 'done'
          : i === reachedIndex.value
            ? done.value
              ? 'done'
              : 'current'
            : 'todo',
  }))
)

const summary = computed(() => {
  if (props.failed) return 'Fallida'
  if (reachedIndex.value < 0) return 'Pendiente'
  return LABEL[ORDER[reachedIndex.value]]
})
</script>

<template>
  <div class="stepper" :class="`stepper-${size}`" role="img" :aria-label="`Estado: ${summary}`">
    <template v-for="(step, i) in steps" :key="step.stage">
      <span
        class="dot"
        :class="[`dot-${step.state}`, step.state !== 'todo' ? `stage-${step.stage}` : '']"
        :title="step.label"
      ></span>
      <span v-if="i < steps.length - 1" class="line" :class="{ 'line-done': i < reachedIndex }"></span>
    </template>
  </div>
</template>

<style scoped>
.stepper {
  display: inline-flex;
  align-items: center;
  --dot: 9px;
  --gap: 10px;
}

.stepper-md {
  --dot: 12px;
  --gap: 18px;
}

.dot {
  width: var(--dot);
  height: var(--dot);
  border-radius: 50%;
  background: rgba(255, 255, 255, 0.1);
  border: 1px solid var(--line-strong);
  flex-shrink: 0;
  transition: background var(--motion) var(--ease), box-shadow var(--motion) var(--ease);
}

.dot-done {
  background: currentColor;
  border-color: transparent;
}

.dot-current {
  background: currentColor;
  border-color: transparent;
  box-shadow: 0 0 0 3px color-mix(in srgb, currentColor 30%, transparent);
  animation: step-pulse 1.1s ease-in-out infinite;
}

.dot-failed {
  background: transparent;
  border-color: var(--stage-failed);
}

.line {
  width: var(--gap);
  height: 1px;
  background: var(--line-strong);
  flex-shrink: 0;
}

.line-done {
  background: rgba(255, 255, 255, 0.35);
}

@keyframes step-pulse {
  0%, 100% { box-shadow: 0 0 0 3px color-mix(in srgb, currentColor 30%, transparent); }
  50% { box-shadow: 0 0 0 6px color-mix(in srgb, currentColor 10%, transparent); }
}
</style>
