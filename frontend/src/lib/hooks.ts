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
    },
  });
}
