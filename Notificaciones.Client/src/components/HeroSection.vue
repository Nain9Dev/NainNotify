<script setup lang="ts">
const REPO_URL = 'https://github.com/Nain9Dev/NainNotify'
</script>

<template>
  <section class="hero" id="top">
    <div class="container hero-inner">
      <span class="section-kicker">Pasarela de notificaciones · .NET 10</span>

      <h1 class="display hero-title">
        Envía. Observa. <span class="gradient-text">Entrega.</span>
      </h1>

      <p class="hero-lede">
        Notificaciones asíncronas con trazabilidad en tiempo real.
        Pruébalo en diez segundos, sin registro.
      </p>

      <div class="hero-actions">
        <a class="btn btn-primary btn-lg" href="#studio">
          Probar ahora
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
            <path d="M12 5v14M5 12l7 7 7-7" />
          </svg>
        </a>
        <a class="btn btn-secondary btn-lg" :href="REPO_URL" target="_blank" rel="noopener">
          Ver el código
        </a>
      </div>

      <ol class="hero-steps" aria-label="Cómo funciona">
        <li><span class="step-num">1</span> Elige un escenario</li>
        <li><span class="step-num">2</span> Envíalo</li>
        <li><span class="step-num">3</span> Míralo llegar</li>
      </ol>
    </div>
  </section>
</template>

<style scoped>
.hero {
  padding: clamp(3rem, 9vw, 6.5rem) 0 clamp(1.5rem, 4vw, 3rem);
  text-align: center;
}

.hero-inner {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 1.25rem;
}

.hero-title {
  font-size: clamp(2.6rem, 7vw, 4.75rem);
  max-width: 14ch;
}

.hero-lede {
  font-size: clamp(1rem, 1.6vw, 1.2rem);
  color: var(--text-2);
  max-width: 44ch;
}

.hero-actions {
  display: flex;
  flex-wrap: wrap;
  justify-content: center;
  gap: 0.75rem;
  margin-top: 0.5rem;
}

.hero-steps {
  list-style: none;
  display: flex;
  flex-wrap: wrap;
  justify-content: center;
  gap: 0.5rem 1.75rem;
  margin-top: 1.25rem;
  font-size: 0.875rem;
  color: var(--text-2);
}

.hero-steps li {
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
}

.step-num {
  display: inline-grid;
  place-items: center;
  width: 22px;
  height: 22px;
  border-radius: 50%;
  font-family: var(--font-mono);
  font-size: 0.72rem;
  font-weight: 600;
  color: var(--accent);
  background: var(--accent-soft);
  border: 1px solid var(--accent-ring);
}
</style>
