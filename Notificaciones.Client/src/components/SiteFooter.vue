<script setup lang="ts">
const REPO_URL = 'https://github.com/Nain9Dev/NainNotify'
const year = new Date().getFullYear()
</script>

<template>
  <footer class="site-footer">
    <div class="container footer-row">
      <span class="muted">© {{ year }} NainNotify · un producto de <a class="link" href="https://www.naindev.com" target="_blank" rel="noopener">NainDev</a></span>
      <nav class="footer-links" aria-label="Enlaces">
        <a class="link" :href="REPO_URL" target="_blank" rel="noopener">GitHub</a>
        <a class="link" :href="`${REPO_URL}/blob/main/LICENSE`" target="_blank" rel="noopener">MIT</a>
        <a class="link" :href="`${REPO_URL}/blob/main/SECURITY.md`" target="_blank" rel="noopener">Seguridad</a>
      </nav>
    </div>
  </footer>
</template>

<style scoped>
.site-footer {
  border-top: 1px solid var(--line);
  padding: 1.5rem 0 2.5rem;
  font-size: 0.82rem;
}

.footer-row {
  display: flex;
  flex-wrap: wrap;
  justify-content: space-between;
  gap: 0.75rem 1.5rem;
}

.footer-links {
  display: flex;
  gap: 1.25rem;
}

.link {
  color: var(--text-2);
  transition: color var(--motion-fast) var(--ease);
}

.link:hover {
  color: var(--text);
}
</style>
