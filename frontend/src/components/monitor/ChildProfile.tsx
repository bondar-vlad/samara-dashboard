"use client";

import { useMemo } from "react";
import {
  Card,
  CardHeader,
  CardContent,
  Box,
  Stack,
  Paper,
  Typography,
  Chip,
  CircularProgress,
  Alert,
} from "@mui/material";
import SchoolIcon from "@mui/icons-material/School";
import MedicalServicesIcon from "@mui/icons-material/MedicalServices";
import GavelIcon from "@mui/icons-material/Gavel";
import VolunteerActivismIcon from "@mui/icons-material/VolunteerActivism";
import WarningCard from "@/components/profile/WarningCard";
import AnalysisStatusBar from "@/components/AnalysisStatusBar";
import {
  useStudent,
  useStudentRedFlags,
  useStudentMedicalVisits,
  useStudentSocialCases,
  useClassBullyingReports,
  useSchools,
} from "@/lib/hooks";
import type { RedFlag } from "@/lib/types";
import { severityColor } from "@/components/profile/severity";
import { GREEN } from "@/theme/colors";
import { useTranslation } from "@/i18n/I18nProvider";

function SourceChip({
  icon,
  label,
  caption,
  active,
}: {
  icon: React.ReactNode;
  label: string;
  caption: string;
  active: boolean;
}) {
  return (
    <Paper
      variant="outlined"
      sx={{
        p: 1.5,
        flex: 1,
        minWidth: 150,
        borderColor: active ? GREEN : "divider",
        bgcolor: active ? `${GREEN}0d` : "background.paper",
        opacity: active ? 1 : 0.6,
      }}
    >
      <Stack direction="row" spacing={1} sx={{ alignItems: "center" }}>
        {icon}
        <Box>
          <Typography variant="subtitle2" sx={{ fontWeight: 700 }}>
            {label}
          </Typography>
          <Typography variant="caption" color="text.secondary">
            {caption}
          </Typography>
        </Box>
      </Stack>
    </Paper>
  );
}

export default function ChildProfile({ studentId }: { studentId: string }) {
  const { t } = useTranslation();
  const student = useStudent(studentId);
  const flags = useStudentRedFlags(studentId);
  const visits = useStudentMedicalVisits(studentId);
  const cases = useStudentSocialCases(studentId);
  const reports = useClassBullyingReports(student.data?.classId);
  const schools = useSchools();

  const schoolName = useMemo(
    () => schools.data?.find((s) => s.id === student.data?.schoolId)?.name ?? "",
    [schools.data, student.data],
  );

  const eduRecords = useMemo(
    () => (student.data?.subjectAverages ?? []).reduce((a, s) => a + s.count, 0),
    [student.data],
  );

  const dedupedFlags = useMemo(() => {
    const byCode = new Map<string, RedFlag>();
    for (const f of flags.data ?? []) {
      const prev = byCode.get(f.ruleCode);
      if (!prev || f.detectedAtUtc > prev.detectedAtUtc) byCode.set(f.ruleCode, f);
    }
    return Array.from(byCode.values()).sort((a, b) => b.detectedAtUtc.localeCompare(a.detectedAtUtc));
  }, [flags.data]);

  if (student.isLoading) {
    return (
      <Box sx={{ display: "flex", justifyContent: "center", py: 8 }}>
        <CircularProgress />
      </Box>
    );
  }
  if (student.isError || !student.data) {
    return <Alert severity="error">{t("childProfile.loadError")}</Alert>;
  }

  const s = student.data;
  const visitList = visits.data ?? [];
  const caseList = cases.data ?? [];
  const reportList = reports.data ?? [];

  return (
    <Stack spacing={2}>
      <AnalysisStatusBar studentId={studentId} />

      {/* Identity */}
      <Card variant="outlined">
        <CardContent>
          <Stack direction="row" spacing={1} useFlexGap sx={{ alignItems: "center", flexWrap: "wrap" }}>
            <Typography variant="h5" sx={{ fontWeight: 800 }}>
              {s.fullName}
            </Typography>
            <Chip size="small" label={t("childProfile.gradeChip", { grade: s.gradeLevel })} />
            <Chip size="small" variant="outlined" label={s.className} />
            {schoolName && <Chip size="small" variant="outlined" label={schoolName} />}
            <Box sx={{ flexGrow: 1 }} />
            <Chip
              size="small"
              color={severityColor(s.attendance.severity)}
              label={t("childProfile.unexcused", { count: s.attendance.unexcused })}
            />
          </Stack>
        </CardContent>
      </Card>

      {/* Data sources */}
      <Card variant="outlined">
        <CardHeader title={t("childProfile.dataSources")} subheader={t("childProfile.dataSourcesHint")} />
        <CardContent>
          <Stack direction="row" spacing={1.5} useFlexGap sx={{ flexWrap: "wrap" }}>
            <SourceChip
              icon={<SchoolIcon color="primary" />}
              label={t("childProfile.sourceEducation")}
              caption={t("childProfile.recordsCount", { count: eduRecords })}
              active
            />
            <SourceChip
              icon={<MedicalServicesIcon sx={{ color: "#e85f2b" }} />}
              label={t("childProfile.sourceMedical")}
              caption={visitList.length ? t("childProfile.recordsCount", { count: visitList.length }) : t("childProfile.noData")}
              active={visitList.length > 0}
            />
            <SourceChip
              icon={<GavelIcon sx={{ color: "#7a869a" }} />}
              label={t("childProfile.sourceJuvenile")}
              caption={reportList.length ? t("childProfile.recordsCount", { count: reportList.length }) : t("childProfile.noData")}
              active={reportList.length > 0}
            />
            <SourceChip
              icon={<VolunteerActivismIcon sx={{ color: "#3a57e8" }} />}
              label={t("childProfile.sourceSocial")}
              caption={caseList.length ? t("childProfile.recordsCount", { count: caseList.length }) : t("childProfile.noData")}
              active={caseList.length > 0}
            />
          </Stack>
        </CardContent>
      </Card>

      {/* Cross-agency signals */}
      {(visitList.length > 0 || caseList.length > 0 || reportList.length > 0) && (
        <Card variant="outlined">
          <CardHeader title={t("childProfile.signals")} />
          <CardContent>
            <Stack spacing={2}>
              {caseList.length > 0 && (
                <Box>
                  <Typography variant="overline" color="text.secondary">
                    {t("childProfile.socialCases")}
                  </Typography>
                  <Stack spacing={1} sx={{ mt: 0.5 }}>
                    {caseList.map((c) => (
                      <Paper key={c.id} variant="outlined" sx={{ p: 1.5 }}>
                        <Stack direction="row" spacing={1} sx={{ alignItems: "center", mb: 0.5 }}>
                          <Chip size="small" color={severityColor(c.severity)} label={t(`severity.${c.severity}`)} />
                          <Typography variant="caption" color="text.secondary">
                            {c.sourceAgency} · {new Date(c.openedOn).toLocaleDateString()} · {c.status}
                          </Typography>
                        </Stack>
                        <Typography variant="body2">{c.reason}</Typography>
                      </Paper>
                    ))}
                  </Stack>
                </Box>
              )}

              {visitList.length > 0 && (
                <Box>
                  <Typography variant="overline" color="text.secondary">
                    {t("childProfile.medicalVisits")}
                  </Typography>
                  <Stack spacing={1} sx={{ mt: 0.5 }}>
                    {visitList.map((v) => (
                      <Paper key={v.id} variant="outlined" sx={{ p: 1.5 }}>
                        <Stack direction="row" spacing={1} sx={{ alignItems: "baseline", flexWrap: "wrap" }} useFlexGap>
                          <Typography variant="body2" sx={{ fontWeight: 600 }}>
                            {v.conditionCategory}
                          </Typography>
                          <Typography variant="caption" color="text.secondary">
                            {new Date(v.date).toLocaleDateString()}
                            {v.note ? ` · ${v.note}` : ""}
                          </Typography>
                        </Stack>
                      </Paper>
                    ))}
                  </Stack>
                </Box>
              )}

              {reportList.length > 0 && (
                <Box>
                  <Typography variant="overline" color="text.secondary">
                    {t("childProfile.bullyingReports")}
                  </Typography>
                  <Stack spacing={1} sx={{ mt: 0.5 }}>
                    {reportList.map((r) => (
                      <Paper key={r.id} variant="outlined" sx={{ p: 1.5 }}>
                        <Stack direction="row" spacing={1} sx={{ alignItems: "center", mb: 0.5 }}>
                          <Chip size="small" color={severityColor(r.severity)} label={t(`severity.${r.severity}`)} />
                          <Typography variant="caption" color="text.secondary">
                            {new Date(r.filedOn).toLocaleDateString()}
                          </Typography>
                        </Stack>
                        <Typography variant="body2">{r.summary}</Typography>
                      </Paper>
                    ))}
                  </Stack>
                </Box>
              )}
            </Stack>
          </CardContent>
        </Card>
      )}

      {/* Markers (red flags) */}
      <Card variant="outlined">
        <CardHeader title={t("childProfile.markers")} subheader={t("childProfile.markersHint")} />
        <CardContent>
          {flags.isLoading ? (
            <CircularProgress size={20} />
          ) : dedupedFlags.length > 0 ? (
            <Stack spacing={2}>
              {dedupedFlags.map((flag) => (
                <WarningCard key={flag.id} flag={flag} />
              ))}
            </Stack>
          ) : (
            <Alert severity="success">{t("comparison.noMarkers")}</Alert>
          )}
        </CardContent>
      </Card>
    </Stack>
  );
}
