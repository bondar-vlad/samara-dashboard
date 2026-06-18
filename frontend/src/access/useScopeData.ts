"use client";

import { useMemo } from "react";
import { useSchools } from "@/lib/hooks";
import type { SchoolSummary } from "@/lib/types";
import { AGENCIES } from "./roles";
import { useRole } from "./RoleProvider";

export interface ScopeOption {
  value: string;
  label: string;
}

export interface ScopeData {
  /** Region options the active role may choose from (pinned roles get one). */
  regions: ScopeOption[];
  /** Community options within the effective region. */
  communities: ScopeOption[];
  /** School options within the effective region + community. */
  schools: ScopeOption[];
  /** Agency verticals (for the interagency role). */
  agencies: ScopeOption[];
  /** Resolved territory after applying the role pin + current selection. */
  effective: {
    region: string | null;
    community: string | null;
    schoolId: string | null;
  };
  /** True when a school falls inside the current effective scope. */
  schoolInScope: (schoolId: string | undefined) => boolean;
  /** True when the effective scope narrows below the full national picture. */
  isNarrowed: boolean;
  /** Lookup of school id → summary. */
  schoolById: Map<string, SchoolSummary>;
  isLoading: boolean;
}

const distinct = (values: (string | undefined | null)[]): string[] =>
  [...new Set(values.filter((v): v is string => !!v))].sort((a, b) =>
    a.localeCompare(b, "uk"),
  );

const toOptions = (values: string[]): ScopeOption[] =>
  values.map((v) => ({ value: v, label: v }));

/**
 * Derives the cascading territory option lists for the active role from the
 * live school list, constrained by the role's pin. A community-pinned role
 * (Ф-Г / Ф-М) only ever sees its own community's schools; a region-pinned role
 * (Ф-О) is locked to its oblast but may drill into any community within it;
 * national roles (Н-Р / Н-Н) may pick any region → community → school.
 */
export function useScopeData(): ScopeData {
  const { def, scope } = useRole();
  const schoolsQuery = useSchools();
  const all = useMemo(() => schoolsQuery.data ?? [], [schoolsQuery.data]);

  return useMemo<ScopeData>(() => {
    const schoolById = new Map(all.map((s) => [s.id, s]));

    // Effective region/community honour the role pin first, then the selection.
    const effectiveRegion = def.pin.region ?? scope.region ?? null;
    const effectiveCommunity = def.pin.community ?? scope.community ?? null;
    const effectiveSchoolId = scope.schoolId ?? null;

    const inRegion = (s: SchoolSummary) =>
      !effectiveRegion || s.region === effectiveRegion;
    const inCommunity = (s: SchoolSummary) =>
      !effectiveCommunity || s.community === effectiveCommunity;

    // Region options: a single pinned region, otherwise every known region.
    const regions = def.pin.region
      ? toOptions([def.pin.region])
      : toOptions(distinct(all.map((s) => s.region)));

    // Community options live inside the effective region.
    const communities = def.pin.community
      ? toOptions([def.pin.community])
      : toOptions(distinct(all.filter(inRegion).map((s) => s.community)));

    // School options live inside the effective region + community.
    const schools = all
      .filter((s) => inRegion(s) && inCommunity(s))
      .sort((a, b) => a.name.localeCompare(b.name, "uk"))
      .map((s) => ({ value: s.id, label: s.name }));

    const agencies = AGENCIES.map((a) => ({ value: a.code, label: a.code }));

    const schoolInScope = (schoolId: string | undefined): boolean => {
      if (!schoolId) return false;
      const s = schoolById.get(schoolId);
      if (!s) return false;
      if (effectiveRegion && s.region !== effectiveRegion) return false;
      if (effectiveCommunity && s.community !== effectiveCommunity) return false;
      if (effectiveSchoolId && s.id !== effectiveSchoolId) return false;
      return true;
    };

    return {
      regions,
      communities,
      schools,
      agencies,
      effective: {
        region: effectiveRegion,
        community: effectiveCommunity,
        schoolId: effectiveSchoolId,
      },
      schoolInScope,
      isNarrowed: !!(effectiveRegion || effectiveCommunity || effectiveSchoolId),
      schoolById,
      isLoading: schoolsQuery.isLoading,
    };
  }, [all, def, scope, schoolsQuery.isLoading]);
}
