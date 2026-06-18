import type {
  StudentListItem,
  StudentDetail,
  RedFlag,
  Recommendation,
  UniversityFit,
  AnalysisRunResult,
  ReformReference,
  NmtSubject,
  SchoolFourthSubjects,
  StudentFourthSubject,
  AdmissionDirection,
  SchoolDirections,
  StudentDirection,
  SchoolProfileChoices,
  StudentProfileChoice,
} from "./types";

/**
 * All backend calls go through the Next rewrite proxy (`/bff/*` -> gateway),
 * keeping the browser same-origin. The gateway strips the service prefix:
 *   /bff/education/api/... -> education
 *   /bff/analysis/api/...  -> analysis
 */
const EDU = "/bff/education/api";
const ANL = "/bff/analysis/api";

async function getJson<T>(url: string): Promise<T> {
  const res = await fetch(url, { headers: { Accept: "application/json" } });
  if (!res.ok) {
    throw new Error(`GET ${url} failed: ${res.status} ${res.statusText}`);
  }
  return res.json() as Promise<T>;
}

async function postJson<T>(url: string): Promise<T> {
  const res = await fetch(url, {
    method: "POST",
    headers: { Accept: "application/json", "Content-Length": "0" },
  });
  if (!res.ok) {
    throw new Error(`POST ${url} failed: ${res.status} ${res.statusText}`);
  }
  return res.json() as Promise<T>;
}

export const api = {
  listStudents: () => getJson<StudentListItem[]>(`${EDU}/students`),

  getReformReference: () => getJson<ReformReference>(`${EDU}/reference/reform`),

  getStudent: (id: string) => getJson<StudentDetail>(`${EDU}/students/${id}`),

  getStudentRedFlags: (id: string) =>
    getJson<RedFlag[]>(`${ANL}/red-flags?subjectId=${id}&scope=Student`),

  getStudentRecommendations: (id: string) =>
    getJson<Recommendation[]>(`${ANL}/recommendations?subjectId=${id}`),

  getUniversityFit: (id: string, take = 5) =>
    getJson<UniversityFit>(
      `${ANL}/analysis/students/${id}/university-fit?take=${take}`,
    ),

  runStudentAnalysis: (id: string) =>
    postJson<AnalysisRunResult>(`${ANL}/analysis/students/${id}/run`),

  // NMT fourth-subject (admission)
  getNmtSubjects: () => getJson<NmtSubject[]>(`${ANL}/admission/nmt-subjects`),

  getSchoolFourthSubjects: (schoolId: string) =>
    getJson<SchoolFourthSubjects>(
      `${ANL}/admission/schools/${schoolId}/fourth-subject-students`,
    ),

  getStudentFourthSubject: (id: string) =>
    getJson<StudentFourthSubject>(`${ANL}/admission/students/${id}/fourth-subject`),

  // Admission direction (NMT-based profile)
  getAdmissionDirections: () =>
    getJson<AdmissionDirection[]>(`${ANL}/admission/directions`),

  getSchoolDirections: (schoolId: string) =>
    getJson<SchoolDirections>(
      `${ANL}/admission/schools/${schoolId}/direction-students`,
    ),

  getStudentDirection: (id: string) =>
    getJson<StudentDirection>(`${ANL}/admission/students/${id}/direction`),

  // 10th-grade profile choice (reform cluster)
  getSchoolProfileChoices: (schoolId: string) =>
    getJson<SchoolProfileChoices>(`${ANL}/profile/schools/${schoolId}/students`),

  getStudentProfileChoice: (id: string) =>
    getJson<StudentProfileChoice>(`${ANL}/profile/students/${id}`),
};
