<script setup lang="ts">
import { ref } from 'vue'
import SiteHeader from './components/SiteHeader.vue'
import HeroSection from './components/HeroSection.vue'
import PipelineFlow from './components/PipelineFlow.vue'
import NotificationStudio from './components/NotificationStudio.vue'
import ActivityFeed from './components/ActivityFeed.vue'
import TrustStrip from './components/TrustStrip.vue'
import SiteFooter from './components/SiteFooter.vue'
import TemplatePreviewModal from './components/TemplatePreviewModal.vue'
import { useNotifyStream } from './composables/useNotifyStream'
import type { TemplatePreviewResponse } from './types/contracts'

const { snapshot, events, latestEvent, health, status, transport, isStandalone, clearEvents } = useNotifyStream()

const expanded = ref<{ preview: TemplatePreviewResponse; from: string; to: string; subject: string } | null>(null)

function openPreview(payload: { preview: TemplatePreviewResponse; from: string; to: string; subject: string }) {
  expanded.value = payload
}
</script>

<template>
  <div class="app">
    <div class="backdrop" aria-hidden="true">
      <div class="glow glow-a"></div>
      <div class="glow glow-b"></div>
      <div class="grid"></div>
    </div>

    <SiteHeader :status="status" :transport="transport" />

    <main>
      <HeroSection />
      <PipelineFlow :events="events" :latest-event="latestEvent" :snapshot="snapshot" />
      <NotificationStudio :events="events" :standalone="isStandalone" @expand="openPreview" />
      <ActivityFeed :events="events" @clear="clearEvents" />
      <TrustStrip :health="health" />
    </main>

    <SiteFooter />

    <TemplatePreviewModal
      :preview="expanded?.preview ?? null"
      :from="expanded?.from ?? ''"
      :to="expanded?.to ?? ''"
      :subject="expanded?.subject ?? ''"
      @close="expanded = null"
    />
  </div>
</template>

<style scoped>
.app {
  position: relative;
  min-height: 100vh;
  display: flex;
  flex-direction: column;
}

main {
  flex: 1;
  position: relative;
  z-index: 1;
}

.backdrop {
  position: fixed;
  inset: 0;
  z-index: 0;
  pointer-events: none;
  overflow: hidden;
}

.glow {
  position: absolute;
  border-radius: 50%;
  filter: blur(120px);
  opacity: 0.22;
}

.glow-a {
  width: 720px;
  height: 720px;
  top: -320px;
  left: 50%;
  transform: translateX(-50%);
  background: radial-gradient(circle, #38bdf8 0%, rgba(56, 189, 248, 0) 70%);
}

.glow-b {
  width: 640px;
  height: 640px;
  bottom: -300px;
  right: -200px;
  background: radial-gradient(circle, #6366f1 0%, rgba(99, 102, 241, 0) 70%);
  opacity: 0.16;
}

.grid {
  position: absolute;
  inset: 0;
  background-image:
    linear-gradient(rgba(255, 255, 255, 0.025) 1px, transparent 1px),
    linear-gradient(90deg, rgba(255, 255, 255, 0.025) 1px, transparent 1px);
  background-size: 48px 48px;
  mask-image: radial-gradient(ellipse at 50% 0%, #000 20%, transparent 70%);
  -webkit-mask-image: radial-gradient(ellipse at 50% 0%, #000 20%, transparent 70%);
}
</style>
