/** API response types — mirror the Education (5101) and Analysis (5105) services. */

export interface ProfileRef {
  profile: string;
  profileName: string;
  cluster: string;
  clusterName: string;
}

export interface ReformProfile {
  profile: string;
  profileName: string;
  cluster: string;
  clusterName: string;
  direction: string;
}

export interface ReformCluster {
  cluster: string;
  clusterName: string;
  direction: string;
  profiles: ReformProfile[];
}

/** GET /education/api/reference/reform */
export interface ReformReference {
  clusters: ReformCluster[];
}

export interface ProfileChoice {
  declaredProfile: string | null;
  desiredCluster: string | null;
  desiredClusterName: string | null;
  desiredProfiles: ProfileRef[];
  recommendedCluster: string | null;
  recommendedClusterName: string | null;
  recommendedProfiles: ProfileRef[];
  recommendationConfidence: number | null;
  recommendationUpdatedUtc: string | null;
  hasMismatch: boolean;
}

export interface SubjectAverage {
  subject: string;
  average: number;
  count: number;
}

export interface TopicAverage {
  subject: string;
  topic: string;
  average: number;
  count?: number;
}

export interface Attendance {
  total: number;
  present: number;
  excused: number;
  unexcused: number;
  severity: string;
}

/** GET /education/api/students */
export interface StudentListItem {
  id: string;
  fullName: string;
  className: string;
  gradeLevel: number;
  schoolId: string;
  desiredCluster: string | null;
  recommendedCluster: string | null;
  hasProfileMismatch: boolean;
}

/** GET /education/api/students/{id} */
export interface StudentDetail {
  id: string;
  fullName: string;
  gradeLevel: number;
  schoolId: string;
  classId: string;
  className: string;
  profileChoice: ProfileChoice;
  attendance: Attendance;
  subjectAverages: SubjectAverage[];
  topicAverages: TopicAverage[];
}

export type Severity = "Green" | "Yellow" | "Red" | string;

/** GET /analysis/api/red-flags */
export interface RedFlag {
  id: string;
  ruleCode: string;
  scope: string;
  subjectId: string;
  subjectName: string;
  severity: Severity;
  title: string;
  description: string;
  sourceAgency: string;
  targetAudiences: string[];
  recommendedActions: string[];
  aiModel: string;
  detectedAtUtc: string;
  status: string;
}

/** GET /analysis/api/recommendations */
export interface Recommendation {
  id: string;
  scope: string;
  subjectId: string;
  subjectName: string;
  kind: string;
  title: string;
  summary: string;
  rationale: string;
  confidence: number;
  aiModel: string;
  createdAtUtc: string;
}

export interface FitGap {
  area: "subject" | "topic" | string;
  name: string;
  current: number;
  target: number;
  gap: number;
}

export interface ProgramMatch {
  programId: string;
  universityId: string;
  universityName: string;
  programName: string;
  cluster: string;
  clusterName: string;
  fitScore: number;
  meetsThreshold: boolean;
  strengths: string[];
  gaps: FitGap[];
  advice: string[];
}

/** GET /analysis/api/students/{id}/university-fit */
export interface UniversityFit {
  studentId: string;
  recommendedCluster: string | null;
  recommendedClusterName: string | null;
  matches: ProgramMatch[];
}

/** POST /analysis/api/students/{id}/run */
export interface AnalysisRunResult {
  runId: string;
  modelName: string;
  status: string;
  flagsProduced: number;
  recommendationsProduced: number;
  summary: string;
}
