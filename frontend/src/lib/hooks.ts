"use client";

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
