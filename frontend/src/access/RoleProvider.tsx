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
  DEFAULT_ROLE,
  ROLES,
  getRole,
  hasPermission,
  parseRoleCode,
  type AgencyCode,
  type NavKey,
  type Permission,
  type RoleCode,
  type RoleDefinition,
} from "./roles";

const STORAGE_KEY = "samara.role";

/** Active territory / agency selection within the bounds of the current role. */
export interface Scope {
  region: string | null;
  community: string | null;
  schoolId: string | null;
  agency: AgencyCode | null;
}

interface SessionState {
  role: RoleCode;
  scope: Scope;
}

interface RoleContextValue extends SessionState {
  def: RoleDefinition;
  setRole: (role: RoleCode) => void;
  setRegion: (region: string | null) => void;
  setCommunity: (community: string | null) => void;
  setSchool: (schoolId: string | null) => void;
  setAgency: (agency: AgencyCode | null) => void;
  can: (perm: Permission) => boolean;
  canNav: (nav: NavKey) => boolean;
}

/** Initial scope derived from a role's territorial pin. */
const scopeForRole = (role: RoleCode): Scope => {
  const { pin } = ROLES[role];
  return {
    region: pin.region ?? null,
    community: pin.community ?? null,
    schoolId: null,
    agency: pin.agency ? "MON" : null,
  };
};

const defaultState: SessionState = {
  role: DEFAULT_ROLE,
  scope: scopeForRole(DEFAULT_ROLE),
};

/**
 * SSR-safe external store for the active role + scope (same pattern as the i18n
 * language store). It always starts at `defaultState` so the first client render
 * matches the server HTML; the persisted role is applied after mount via
 * `hydrate()`. Scope is intentionally NOT persisted — it resets per session.
 */
const createRoleStore = () => {
  let state: SessionState = defaultState;
  const listeners = new Set<() => void>();

  const emit = () => listeners.forEach((l) => l());

  const persist = (role: RoleCode) => {
    try {
      window.localStorage.setItem(STORAGE_KEY, role);
    } catch {
      /* ignore (private mode etc.) */
    }
  };

  return {
    subscribe: (cb: () => void) => {
      listeners.add(cb);
      return () => listeners.delete(cb);
    },
    getSnapshot: () => state,
    getServerSnapshot: () => defaultState,

    setRole: (role: RoleCode) => {
      if (role === state.role) return;
      state = { role, scope: scopeForRole(role) };
      persist(role);
      emit();
    },
    patchScope: (patch: Partial<Scope>) => {
      state = { role: state.role, scope: { ...state.scope, ...patch } };
      emit();
    },
    hydrate: () => {
      const stored = parseRoleCode(
        (() => {
          try {
            return window.localStorage.getItem(STORAGE_KEY);
          } catch {
            return null;
          }
        })(),
      );
      if (!stored || stored === state.role) return;
      state = { role: stored, scope: scopeForRole(stored) };
      emit();
    },
  };
};

const roleStore = createRoleStore();

const RoleContext = createContext<RoleContextValue | null>(null);

export function RoleProvider({ children }: { children: ReactNode }) {
  const state = useSyncExternalStore(
    roleStore.subscribe,
    roleStore.getSnapshot,
    roleStore.getServerSnapshot,
  );

  // Apply the persisted role once on mount (post-hydration).
  useEffect(() => {
    roleStore.hydrate();
  }, []);

  const setRole = useCallback((role: RoleCode) => roleStore.setRole(role), []);

  // Cascading resets: changing a wider level clears the narrower ones.
  const setRegion = useCallback(
    (region: string | null) =>
      roleStore.patchScope({ region, community: null, schoolId: null }),
    [],
  );
  const setCommunity = useCallback(
    (community: string | null) =>
      roleStore.patchScope({ community, schoolId: null }),
    [],
  );
  const setSchool = useCallback(
    (schoolId: string | null) => roleStore.patchScope({ schoolId }),
    [],
  );
  const setAgency = useCallback(
    (agency: AgencyCode | null) => roleStore.patchScope({ agency }),
    [],
  );

  const value = useMemo<RoleContextValue>(() => {
    const def = getRole(state.role);
    return {
      ...state,
      def,
      setRole,
      setRegion,
      setCommunity,
      setSchool,
      setAgency,
      can: (perm) => hasPermission(state.role, perm),
      canNav: (nav) => def.nav.includes(nav),
    };
  }, [state, setRole, setRegion, setCommunity, setSchool, setAgency]);

  return <RoleContext.Provider value={value}>{children}</RoleContext.Provider>;
}

/** Access the active role, its scope and the scope mutators. */
export function useRole(): RoleContextValue {
  const ctx = useContext(RoleContext);
  if (!ctx) throw new Error("useRole must be used within a RoleProvider");
  return ctx;
}
