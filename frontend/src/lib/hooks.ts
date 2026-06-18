"use client";

import { useMemo } from "react";
import {
  useQuery,
  useMutation,
  useQueryClient,
} from "@tanstack/react-query";
import { api } from "./api";

export function useStudents() {
  return useQuery({
    queryKey: ["students"],
    queryFn: api.listStudents,
  });
}

/**
 * Picks one stable demo school for the dashboards. Prefers a school that spans both the
 * profile cohort (grade < 11) and the admission cohort (grade 11), so all three widgets have
 * data; ties are broken by size then id, which lands on the Academic Lyceum (grades 9–11).
 */
export function usePrimarySchoolId(): string | undefined {
  const students = useStudents();
  return useMemo(() => {
    const rows = students.data ?? [];
    if (rows.length === 0) return undefined;

    const bySchool = new Map<string, { total: number; hasGrad: boolean; hasJunior: boolean }>();
    for (const s of rows) {
      const e = bySchool.get(s.schoolId) ?? { total: 0, hasGrad: false, hasJunior: false };
      e.total += 1;
      if (s.gradeLevel >= 11) e.hasGrad = true;
      else e.hasJunior = true;
      bySchool.set(s.schoolId, e);
    }

    const entries = [...bySchool.entries()];
    const dual = entries.filter(([, e]) => e.hasGrad && e.hasJunior);
    const pool = dual.length > 0 ? dual : entries;
    pool.sort((a, b) => b[1].total - a[1].total || (a[0] < b[0] ? -1 : 1));
    return pool[0][0];
  }, [students.data]);
}

export function useReformReference() {
  return useQuery({
    queryKey: ["reform-reference"],
    queryFn: api.getReformReference,
    staleTime: Infinity, // reference data, effectively static
  });
}

export function useStudent(id: string | null) {
  return useQuery({
    queryKey: ["student", id],
    queryFn: () => api.getStudent(id as string),
    enabled: !!id,
  });
}

export function useStudentRedFlags(id: string | null) {
  return useQuery({
    queryKey: ["red-flags", id],
    queryFn: () => api.getStudentRedFlags(id as string),
    enabled: !!id,
  });
}

export function useStudentRecommendations(id: string | null) {
  return useQuery({
    queryKey: ["recommendations", id],
    queryFn: () => api.getStudentRecommendations(id as string),
    enabled: !!id,
  });
}

export function useUniversityFit(id: string | null) {
  return useQuery({
    queryKey: ["university-fit", id],
    queryFn: () => api.getUniversityFit(id as string),
    enabled: !!id,
  });
}

// ─── NMT fourth-subject (admission) ──────────────────────────────────────────

export function useNmtSubjects() {
  return useQuery({
    queryKey: ["nmt-subjects"],
    queryFn: api.getNmtSubjects,
    staleTime: Infinity, // reference data
  });
}

export function useSchoolFourthSubjects(schoolId: string | null | undefined) {
  return useQuery({
    queryKey: ["fourth-subject-school", schoolId],
    queryFn: () => api.getSchoolFourthSubjects(schoolId as string),
    enabled: !!schoolId,
  });
}

export function useStudentFourthSubject(id: string | null) {
  return useQuery({
    queryKey: ["fourth-subject", id],
    queryFn: () => api.getStudentFourthSubject(id as string),
    enabled: !!id,
  });
}

// ─── Admission direction (NMT-based profile) ─────────────────────────────────

export function useAdmissionDirections() {
  return useQuery({
    queryKey: ["admission-directions"],
    queryFn: api.getAdmissionDirections,
    staleTime: Infinity, // reference data
  });
}

export function useSchoolDirections(schoolId: string | null | undefined) {
  return useQuery({
    queryKey: ["direction-school", schoolId],
    queryFn: () => api.getSchoolDirections(schoolId as string),
    enabled: !!schoolId,
  });
}

export function useStudentDirection(id: string | null) {
  return useQuery({
    queryKey: ["direction", id],
    queryFn: () => api.getStudentDirection(id as string),
    enabled: !!id,
  });
}

// ─── 10th-grade profile choice (reform cluster) ─────────────────────────────

export function useSchoolProfileChoices(schoolId: string | null | undefined) {
  return useQuery({
    queryKey: ["profile-choice-school", schoolId],
    queryFn: () => api.getSchoolProfileChoices(schoolId as string),
    enabled: !!schoolId,
  });
}

export function useStudentProfileChoice(id: string | null) {
  return useQuery({
    queryKey: ["profile-choice", id],
    queryFn: () => api.getStudentProfileChoice(id as string),
    enabled: !!id,
  });
}

export function useStudentAnalysisStatus(id: string | null) {
  return useQuery({
    queryKey: ["analysis-status", id],
    queryFn: () => api.getStudentAnalysisStatus(id as string),
    enabled: !!id,
  });
}

// ─── Management dashboard + risk monitor + cross-agency profile ──────────────────

export function useDashboardSummary() {
  return useQuery({ queryKey: ["dashboard-summary"], queryFn: api.getDashboardSummary });
}

export function useRedFlags(params: { severity?: string; scope?: string; subjectId?: string; status?: string } = {}) {
  return useQuery({
    queryKey: ["red-flags-list", params],
    queryFn: () => api.listRedFlags(params),
  });
}

export function useSchools() {
  return useQuery({ queryKey: ["schools"], queryFn: api.listSchools, staleTime: Infinity });
}

export function useStudentMedicalVisits(id: string | null) {
  return useQuery({
    queryKey: ["medical-visits", id],
    queryFn: () => api.getStudentMedicalVisits(id as string),
    enabled: !!id,
  });
}

export function useStudentSocialCases(id: string | null) {
  return useQuery({
    queryKey: ["social-cases", id],
    queryFn: () => api.getStudentSocialCases(id as string),
    enabled: !!id,
  });
}

export function useClassBullyingReports(classId: string | null | undefined) {
  return useQuery({
    queryKey: ["bullying-reports", classId],
    queryFn: () => api.getClassBullyingReports(classId as string),
    enabled: !!classId,
  });
}

/**
 * AI improvement plan ("що підтягнути") toward the pupil's chosen profile/direction.
 * Fetched on demand (it calls the LLM) — pass `enabled` from a button click. Does not retry,
 * and the backend itself returns `available:false` rather than erroring when no model is set.
 */
export function useStudentImprovementPlan(id: string | null, enabled: boolean) {
  return useQuery({
    queryKey: ["improvement-plan", id],
    queryFn: () => api.getStudentImprovementPlan(id as string),
    enabled: enabled && !!id,
    staleTime: 5 * 60 * 1000,
    retry: false,
  });
}

/** Acknowledge or resolve a red flag, then refresh the flag lists and dashboard. */
export function useFlagAction() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, action }: { id: string; action: "acknowledge" | "resolve" }) =>
      action === "resolve" ? api.resolveFlag(id) : api.acknowledgeFlag(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["red-flags"] });
      qc.invalidateQueries({ queryKey: ["red-flags-list"] });
      qc.invalidateQueries({ queryKey: ["dashboard-summary"] });
    },
  });
}

/** Runs analysis for a student, then refreshes everything that depends on it. */
export function useRunAnalysis() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => api.runStudentAnalysis(id),
    onSuccess: (_data, id) => {
      qc.invalidateQueries({ queryKey: ["students"] });
      qc.invalidateQueries({ queryKey: ["student", id] });
      qc.invalidateQueries({ queryKey: ["red-flags", id] });
      qc.invalidateQueries({ queryKey: ["recommendations", id] });
      qc.invalidateQueries({ queryKey: ["university-fit", id] });
      qc.invalidateQueries({ queryKey: ["analysis-status", id] });
    },
  });
}
