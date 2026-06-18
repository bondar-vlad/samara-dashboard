/**
 * Role & access-level model for the child-rights dashboard.
 *
 * Mirrors the nine roles agreed for the platform (code → role → access level →
 * what the main dashboard shows → data-access restriction → category). The UI
 * uses this single source of truth to decide:
 *   • which dashboard *view* to render per role,
 *   • which cascading territory *selectors* to show (e.g. a community/ОТГ
 *     specialist only gets a school dropdown),
 *   • which navigation links and actions are permitted,
 *   • whether personal (non-aggregated) data may be shown.
 *
 * NOTE: this is the presentation-layer access model. Real identity (Дія.Підпис,
 * Трембіта) and server-side enforcement are a separate, later concern — every
 * rule here must also be enforced by the backend before production.
 */

/** Stable role identifiers (kept ASCII; the "Ф-Г"… labels live in i18n). */
export type RoleCode =
  | "F_G" // Фахівець ССД (Громада)
  | "F_M" // Фахівець / Нач. ССД (Місто)
  | "F_O" // Нач. ССД / Аналітик (Область)
  | "N_R" // НСССУ / Регіональний контролер
  | "N_N" // Мінсоцполітики / Глобал-Адмін
  | "B" //   Батьки (мобільний додаток ЦШД)
  | "CHILD" // Дитина (власний кабінет)
  | "M_V" // Міжвідомчий користувач
  | "ADM" // Адміністратор системи
  | "P"; //  Публічний / неавторизований

/** Breadth of the territory a role may see (narrow → wide). */
export type ScopeLevel =
  | "school"
  | "community"
  | "region"
  | "national"
  | "child"
  | "agency"
  | "public";

/** Cascading selector kinds, ordered narrow → wide when combined. */
export type SelectorKind = "region" | "community" | "school" | "agency";

/** Which dashboard surface a role lands on. */
export type DashboardView =
  | "operational" // own cases / children / deadlines (Ф-Г, Ф-М)
  | "analytics" //   territorial roll-up + decision support (Ф-О, Н-Р, Н-Н)
  | "parent" //      a single child's profile (Б)
  | "interagency" // one agency's vertical slice (М-В)
  | "admin" //       users, roles, integrations, audit (Адм)
  | "public"; //     aggregated, de-identified only (П)

/** Fine-grained capabilities checked across the UI. */
export type Permission =
  | "view:operational"
  | "view:analytics"
  | "view:personalData"
  | "view:risks"
  | "view:childProfile"
  | "view:public"
  | "manage:flags"
  | "manage:users"
  | "view:audit"
  | "export:reports";

/** Navigation destinations the header can expose. */
export type NavKey = "home" | "dashboard" | "risks" | "demo" | "access";

export interface RoleDefinition {
  code: RoleCode;
  /** Short badge label, e.g. "Ф-Г" (display only; not localized). */
  badge: string;
  /** Access-level label key under i18n `roles.levels.*`. */
  level: ScopeLevel;
  /** Category key under i18n `roles.categories.*`. */
  category:
    | "specialistCommunity"
    | "specialistCity"
    | "specialistOblast"
    | "nationalService"
    | "globalAdmin"
    | "endUser"
    | "childUser"
    | "interagency"
    | "administrator"
    | "citizen";
  view: DashboardView;
  /** Cascading territory pickers to render for this role (in order). */
  selectors: SelectorKind[];
  /** Personal (non-aggregated) data is visible to this role. */
  canSeePersonalData: boolean;
  permissions: Permission[];
  nav: NavKey[];
  /**
   * Territory the role is pinned to and cannot widen beyond. Resolved against
   * live data at runtime; `null` parts mean "no fixed pin at that level".
   */
  pin: {
    region?: string | null;
    community?: string | null;
    /** For role B (parent): bound to a single child. */
    child?: boolean;
    /** For role M_V: bound to one agency vertical. */
    agency?: boolean;
  };
}

/**
 * Interagency verticals available to the М-В role (Трембіта counterparties).
 * `flagAgency` links a vertical to the `RedFlag.sourceAgency` enum string so the
 * interagency view can show that agency's slice of the cross-agency signals.
 */
export const AGENCIES = [
  { code: "MON", flagAgency: "Education" }, //         МОН
  { code: "MOZ", flagAgency: "Medical" }, //           МОЗ
  { code: "MVS", flagAgency: "JuvenilePolice" }, //    МВС
  { code: "PFU", flagAgency: "SocialServices" }, //    ПФУ
  { code: "MINJUST", flagAgency: null }, //            Мінʼюст
  { code: "MINVET", flagAgency: null }, //             Мінветеранів
] as const;

export type AgencyCode = (typeof AGENCIES)[number]["code"];

export const agencyFlagSource = (code: AgencyCode | null): string | null =>
  AGENCIES.find((a) => a.code === code)?.flagAgency ?? null;

/**
 * Demo territory pins. These match the seeded education data so a scoped role
 * lands on a populated view out of the box.
 */
const DNIPRO = "Дніпропетровська область";
const SAMARA = "Самарська громада";

export const ROLES: Record<RoleCode, RoleDefinition> = {
  F_G: {
    code: "F_G",
    badge: "Ф-Г",
    level: "community",
    category: "specialistCommunity",
    view: "operational",
    selectors: ["school"],
    canSeePersonalData: true,
    permissions: [
      "view:operational",
      "view:risks",
      "view:childProfile",
      "view:personalData",
      "manage:flags",
    ],
    nav: ["dashboard", "risks"],
    pin: { region: DNIPRO, community: SAMARA },
  },

  F_M: {
    code: "F_M",
    badge: "Ф-М",
    level: "community",
    category: "specialistCity",
    view: "operational",
    selectors: ["school"],
    canSeePersonalData: true,
    permissions: [
      "view:operational",
      "view:analytics",
      "view:risks",
      "view:childProfile",
      "view:personalData",
      "manage:flags",
      "export:reports",
    ],
    nav: ["dashboard", "risks"],
    pin: { region: DNIPRO, community: SAMARA },
  },

  F_O: {
    code: "F_O",
    badge: "Ф-О",
    level: "region",
    category: "specialistOblast",
    view: "analytics",
    selectors: ["community", "school"],
    canSeePersonalData: true,
    permissions: [
      "view:operational",
      "view:analytics",
      "view:risks",
      "view:childProfile",
      "view:personalData",
      "manage:flags",
      "export:reports",
    ],
    nav: ["dashboard", "risks"],
    pin: { region: DNIPRO, community: null },
  },

  N_R: {
    code: "N_R",
    badge: "Н-Р",
    level: "national",
    category: "nationalService",
    view: "analytics",
    selectors: ["region", "community", "school"],
    canSeePersonalData: true,
    permissions: [
      "view:analytics",
      "view:risks",
      "view:childProfile",
      "view:personalData",
      "manage:flags",
      "manage:users",
      "export:reports",
    ],
    nav: ["dashboard", "risks", "access"],
    pin: { region: null, community: null },
  },

  N_N: {
    code: "N_N",
    badge: "Н-Н",
    level: "national",
    category: "globalAdmin",
    view: "analytics",
    selectors: ["region", "community", "school"],
    canSeePersonalData: true,
    permissions: [
      "view:analytics",
      "view:risks",
      "view:childProfile",
      "view:personalData",
      "manage:flags",
      "export:reports",
    ],
    nav: ["dashboard", "risks", "access"],
    pin: { region: null, community: null },
  },

  B: {
    code: "B",
    badge: "Б",
    level: "child",
    category: "endUser",
    view: "parent",
    selectors: [],
    canSeePersonalData: true,
    permissions: ["view:childProfile", "view:personalData"],
    nav: ["dashboard"],
    pin: { child: true },
  },

  CHILD: {
    code: "CHILD",
    badge: "Д",
    level: "child",
    category: "childUser",
    view: "parent",
    selectors: [],
    canSeePersonalData: true,
    permissions: ["view:childProfile", "view:personalData"],
    nav: ["dashboard"],
    pin: { child: true },
  },

  M_V: {
    code: "M_V",
    badge: "М-В",
    level: "agency",
    category: "interagency",
    view: "interagency",
    selectors: ["agency", "region"],
    canSeePersonalData: true,
    permissions: ["view:analytics", "view:risks", "view:personalData"],
    nav: ["dashboard", "risks"],
    pin: { agency: true, region: null },
  },

  ADM: {
    code: "ADM",
    badge: "Адм",
    level: "national",
    category: "administrator",
    view: "admin",
    selectors: [],
    canSeePersonalData: false,
    permissions: ["manage:users", "view:audit"],
    nav: ["dashboard", "access"],
    pin: {},
  },

  P: {
    code: "P",
    badge: "П",
    level: "public",
    category: "citizen",
    view: "public",
    selectors: [],
    canSeePersonalData: false,
    permissions: ["view:public"],
    nav: ["dashboard"],
    pin: {},
  },
};

/** Display order used by the role switcher and the access reference page. */
export const ROLE_ORDER: RoleCode[] = [
  "F_G",
  "F_M",
  "F_O",
  "N_R",
  "N_N",
  "B",
  "CHILD",
  "M_V",
  "ADM",
  "P",
];

/** Role the app starts in. Області-level analyst showcases the cascading
 *  community → school selectors on first load while keeping data populated. */
export const DEFAULT_ROLE: RoleCode = "F_O";

export const getRole = (code: RoleCode): RoleDefinition => ROLES[code];

export const hasPermission = (code: RoleCode, perm: Permission): boolean =>
  ROLES[code].permissions.includes(perm);

export const canSeeNav = (code: RoleCode, nav: NavKey): boolean =>
  ROLES[code].nav.includes(nav);

const isRoleCode = (v: unknown): v is RoleCode =>
  typeof v === "string" && v in ROLES;

export const parseRoleCode = (v: unknown): RoleCode | null =>
  isRoleCode(v) ? v : null;
