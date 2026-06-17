"use client";

import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useSyncExternalStore,
  type ReactNode,
} from "react";
import {
  DEFAULT_LANGUAGE,
  STORAGE_KEY,
  resources,
  type Language,
} from "./resources";

type Vars = Record<string, string | number>;

interface I18nValue {
  language: Language;
  setLanguage: (lang: Language) => void;
  t: (key: string, vars?: Vars) => string;
}

const isLanguage = (v: unknown): v is Language => v === "uk" || v === "en";

const readStored = (): Language => {
  try {
    const v = window.localStorage.getItem(STORAGE_KEY);
    return isLanguage(v) ? v : DEFAULT_LANGUAGE;
  } catch {
    return DEFAULT_LANGUAGE;
  }
};

/**
 * Tiny external store for the active language. Using `useSyncExternalStore`
 * avoids calling setState inside an effect.
 *
 * The store always starts at `DEFAULT_LANGUAGE` so the first client render
 * matches the server-rendered HTML (no hydration mismatch). The persisted
 * preference is applied right after mount via `hydrate()`.
 */
const createLangStore = () => {
  let language: Language = DEFAULT_LANGUAGE;
  const listeners = new Set<() => void>();

  return {
    subscribe: (cb: () => void) => {
      listeners.add(cb);
      return () => listeners.delete(cb);
    },
    getSnapshot: () => language,
    getServerSnapshot: () => DEFAULT_LANGUAGE,
    set: (lang: Language) => {
      if (lang === language) return;
      language = lang;
      try {
        window.localStorage.setItem(STORAGE_KEY, lang);
      } catch {
        /* ignore persistence failures (e.g. private mode) */
      }
      listeners.forEach((l) => l());
    },
    hydrate: () => {
      const stored = readStored();
      if (stored === language) return;
      language = stored;
      listeners.forEach((l) => l());
    },
  };
};

const langStore = createLangStore();

const interpolate = (template: string, vars?: Vars): string =>
  vars
    ? template.replace(/\{\{(\w+)\}\}/g, (_, k: string) =>
        k in vars ? String(vars[k]) : `{{${k}}}`,
      )
    : template;

const resolve = (lang: Language, key: string): string | undefined => {
  let node: unknown = resources[lang];
  for (const part of key.split(".")) {
    if (node && typeof node === "object" && part in (node as object)) {
      node = (node as Record<string, unknown>)[part];
    } else {
      return undefined;
    }
  }
  return typeof node === "string" ? node : undefined;
};

const I18nContext = createContext<I18nValue | null>(null);

export const I18nProvider = ({ children }: { children: ReactNode }) => {
  const language = useSyncExternalStore(
    langStore.subscribe,
    langStore.getSnapshot,
    langStore.getServerSnapshot,
  );

  useEffect(() => {
    langStore.hydrate();
  }, []);

  useEffect(() => {
    document.documentElement.lang = language;
  }, [language]);

  const setLanguage = useCallback((lang: Language) => langStore.set(lang), []);

  const t = useCallback(
    (key: string, vars?: Vars) =>
      interpolate(
        resolve(language, key) ?? resolve(DEFAULT_LANGUAGE, key) ?? key,
        vars,
      ),
    [language],
  );

  const value = useMemo<I18nValue>(
    () => ({ language, setLanguage, t }),
    [language, setLanguage, t],
  );

  return <I18nContext.Provider value={value}>{children}</I18nContext.Provider>;
};

export const useTranslation = (): I18nValue => {
  const ctx = useContext(I18nContext);
  if (!ctx) {
    throw new Error("useTranslation must be used within I18nProvider");
  }
  return ctx;
};
