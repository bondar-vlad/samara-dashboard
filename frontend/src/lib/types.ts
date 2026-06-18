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

// ─── NMT fourth-subject (admission) ──────────────────────────────────────────

/** GET /analysis/api/admission/nmt-subjects */
export interface NmtSubject {
  subject: string;
  name: string;
  mandatory: boolean;
  fourthSubjectOption: boolean;
}

export interface RankedSubject {
  subject: string;
  subjectName: string;
  score: number;
  evidenceCount: number;
}

/** GET /analysis/api/admission/students/{id}/fourth-subject */
export interface StudentFourthSubject {
  studentId: string;
  chosenSubject: string | null;
  chosenSubjectName: string | null;
  recommendedSubject: string | null;
  recommendedSubjectName: string | null;
  hasChoice: boolean;
  isMatch: boolean;
  ranked: RankedSubject[];
  rationale: string;
}

export interface FourthSubjectStudent {
  studentId: string;
  fullName: string;
  className: string;
  chosenSubject: string | null;
  recommendedSubject: string | null;
  isMatch: boolean;
}

export interface FourthSubjectDistribution {
  subject: string;
  subjectName: string;
  chosenCount: number;
  recommendedCount: number;
}

/** GET /analysis/api/admission/schools/{id}/fourth-subject-students */
export interface SchoolFourthSubjects {
  students: FourthSubjectStudent[];
  distribution: FourthSubjectDistribution[];
}

// ─── Admission direction (NMT-based profile) ─────────────────────────────────

export interface DirectionSpecialty {
  code: string;
  name: string;
}

/** GET /analysis/api/admission/directions */
export interface AdmissionDirection {
  code: string;
  name: string;
  branchOfKnowledge: string;
  relatedCluster: string;
  relatedClusterName: string;
  nmtCoefficients: Record<string, number>;
  keySubjects: string[];
  keyTopics: string[];
  specialties: DirectionSpecialty[];
}

export interface RankedDirection {
  directionCode: string;
  directionName: string;
  competitiveScore: number | null;
  topicFit: number;
  combinedScore: number;
}

/** GET /analysis/api/admission/students/{id}/direction */
export interface StudentDirection {
  studentId: string;
  desiredDirectionCode: string | null;
  desiredDirectionName: string | null;
  recommendedDirectionCode: string | null;
  recommendedDirectionName: string | null;
  hasChoice: boolean;
  isMatch: boolean;
  nmtScores: Record<string, number>;
  ranked: RankedDirection[];
  rationale: string;
}

export interface DirectionStudent {
  studentId: string;
  fullName: string;
  className: string;
  desiredDirectionCode: string | null;
  recommendedDirectionCode: string | null;
  isMatch: boolean;
}

export interface DirectionDistribution {
  directionCode: string;
  directionName: string;
  chosenCount: number;
  recommendedCount: number;
}

/** GET /analysis/api/admission/schools/{id}/direction-students */
export interface SchoolDirections {
  students: DirectionStudent[];
  distribution: DirectionDistribution[];
}

// ─── 10th-grade profile choice (cluster) ─────────────────────────────────────

/** A pupil row in the profile-choice list: desired vs recommended reform cluster. */
export interface ProfileChoiceStudent {
  studentId: string;
  fullName: string;
  className: string;
  desiredCluster: string | null;
  desiredClusterName: string | null;
  recommendedCluster: string | null;
  recommendedClusterName: string | null;
  hasChoice: boolean;
  isMatch: boolean;
}

export interface ProfileChoiceDistribution {
  cluster: string;
  clusterName: string;
  chosenCount: number;
  recommendedCount: number;
}

/** GET /analysis/api/profile/schools/{id}/students */
export interface SchoolProfileChoices {
  students: ProfileChoiceStudent[];
  distribution: ProfileChoiceDistribution[];
}

export interface ClusterScore {
  cluster: string;
  clusterName: string;
  score: number;
}

/** GET /analysis/api/profile/students/{id} */
export interface StudentProfileChoice {
  studentId: string;
  desiredCluster: string | null;
  desiredClusterName: string | null;
  desiredProfiles: ProfileRef[];
  recommendedCluster: string | null;
  recommendedClusterName: string | null;
  recommendedProfiles: ProfileRef[];
  hasChoice: boolean;
  isMatch: boolean;
  confidence: number | null;
  ranked: ClusterScore[];
  rationale: string;
}
