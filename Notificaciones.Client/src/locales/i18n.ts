import { ref } from 'vue';

export type Locale = 'es' | 'en';

const STORAGE_KEY = 'nain_locale';
const EVENT_NAME = 'nain:locale-change';

function getInitialLocale(): Locale {
  if (typeof window === 'undefined') return 'es';
  const stored = localStorage.getItem(STORAGE_KEY);
  if (stored === 'es' || stored === 'en') return stored;
  return 'es';
}

export const currentLocale = ref<Locale>(getInitialLocale());

export function setLocale(locale: Locale) {
  if (currentLocale.value === locale) return;
  currentLocale.value = locale;
  if (typeof window !== 'undefined') {
    localStorage.setItem(STORAGE_KEY, locale);
    window.dispatchEvent(new CustomEvent(EVENT_NAME, { detail: { locale } }));
  }
}

export function toggleLocale() {
  setLocale(currentLocale.value === 'es' ? 'en' : 'es');
}

if (typeof window !== 'undefined') {
  window.addEventListener(EVENT_NAME, (event: Event) => {
    const custom = event as CustomEvent<{ locale?: Locale }>;
    const target = custom.detail?.locale;
    if (target === 'es' || target === 'en') {
      currentLocale.value = target;
    }
  });
}

export const translations = {
  es: {
    liveRabbitMq: 'En vivo · RabbitMQ',
    liveInMemory: 'En vivo',
    localDemo: 'Demo local',
    connecting: 'Conectando…',
    reconnecting: 'Reconectando…',
    kicker: 'Pasarela de notificaciones · .NET 10',
    heroTitleA: 'Envía. Observa.',
    heroTitleB: 'Entrega.',
    heroLede: 'Notificaciones asíncronas con trazabilidad en tiempo real. Pruébalo en diez segundos, sin registro.',
    tryNow: 'Probar ahora',
    viewCode: 'Ver el código',
    step1: 'Elige un escenario',
    step2: 'Envíalo',
    step3: 'Míralo llegar',
    security: 'Seguridad',
    privacy: 'Privacidad',
    footerProduct: 'un producto de',
  },
  en: {
    liveRabbitMq: 'Live · RabbitMQ',
    liveInMemory: 'Live',
    localDemo: 'Local Demo',
    connecting: 'Connecting…',
    reconnecting: 'Reconnecting…',
    kicker: 'Notification Gateway · .NET 10',
    heroTitleA: 'Send. Observe.',
    heroTitleB: 'Deliver.',
    heroLede: 'Asynchronous notifications with real-time traceability. Try it in ten seconds, no registration needed.',
    tryNow: 'Try Now',
    viewCode: 'View Code',
    step1: 'Pick a scenario',
    step2: 'Dispatch it',
    step3: 'Watch it arrive',
    security: 'Security',
    privacy: 'Privacy',
    footerProduct: 'a product by',
  },
} as const;

export type TranslationKey = keyof typeof translations.es;

export function t(key: TranslationKey): string {
  const loc = currentLocale.value;
  return translations[loc][key] ?? translations.es[key] ?? key;
}
