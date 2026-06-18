"use client";

import { useMemo } from "react";
import NextLink from "next/link";
import {
  Card,
  CardContent,
  CardHeader,
  Box,
  Stack,
  Typography,
  Chip,
  LinearProgress,
  CircularProgress,
  Alert,
  Divider,
} from "@mui/material";
import { useDashboardSummary, useRedFlags, useStudents, useSchools } from "@/lib/hooks";
import type { SchoolSummary } from "@/lib/types";
import { severityColor, severityHex } from "@/components/profile/severity";
import { BLUE, ORANGE, GREEN, RED } from "@/theme/colors";
import { useTranslation } from "@/i18n/I18nProvider";

function Kpi({ label, value, color }: { label: string; value: number | string; color: string }) {
  return (
    <Card variant="outlined" sx={{ flex: 1, minWidth: 150 }}>
      <CardContent>
        <Typography variant="h4" sx={{ fontWeight: 800, color }}>
          {value}
        </Typography>
        <Typography variant="body2" color="text.secondary">
          {label}
        </Typography>
      </CardContent>
    </Card>
  );
}

const CRITICAL = new Set(["Red", "Orange"]);

export default function ManagementDashboard() {
  const { t } = useTranslation();
  const summary = useDashboardSummary();
  const flags = useRedFlags({ scope: "Student" });
  const students = useStudents();
  const schools = useSchools();

  const studentSchool = useMemo(() => {
    const m = new Map<string, string>();
    for (const s of students.data ?? []) m.set(s.id, s.schoolId);
    return m;
  }, [students.data]);

  const schoolInfo = useMemo(() => {
    const m = new Map<string, SchoolSummary>();
    for (const s of schools.data ?? []) m.set(s.id, s);
    return m;
  }, [schools.data]);

  const bySchool = useMemo(() => {
    const agg = new Map<string, { name: string; community: string; critical: number; total: number }>();
    for (const f of flags.data ?? []) {
      const schoolId = studentSchool.get(f.subjectId);
      if (!schoolId) continue;
      const info = schoolInfo.get(schoolId);
      const e = agg.get(schoolId) ?? { name: info?.name ?? "—", community: info?.community ?? "", critical: 0, total: 0 };
      e.total += 1;
      if (CRITICAL.has(f.severity)) e.critical += 1;
      agg.set(schoolId, e);
    }
    return [...agg.values()].sort((a, b) => b.critical - a.critical || b.total - a.total);
  }, [flags.data, studentSchool, schoolInfo]);

  const byCommunity = useMemo(() => {
    const agg = new Map<string, { critical: number; total: number }>();
    for (const f of flags.data ?? []) {
      const schoolId = studentSchool.get(f.subjectId);
      const community = schoolId ? schoolInfo.get(schoolId)?.community ?? "—" : "—";
      const e = agg.get(community) ?? { critical: 0, total: 0 };
      e.total += 1;
      if (CRITICAL.has(f.severity)) e.critical += 1;
      agg.set(community, e);
    }
    return [...agg.entries()]
      .map(([community, v]) => ({ community, ...v }))
      .sort((a, b) => b.critical - a.critical || b.total - a.total);
  }, [flags.data, studentSchool, schoolInfo]);

  const ruleCounts = useMemo(() => {
    const m: Record<string, number> = {};
    for (const f of flags.data ?? []) m[f.ruleCode] = (m[f.ruleCode] ?? 0) + 1;
    return m;
  }, [flags.data]);

  const attendance = ruleCounts["EDU-ATTENDANCE"] ?? 0;
  const profileMismatch = ruleCounts["EDU-PROFILE-MISMATCH"] ?? 0;
  const admissionMismatch = (ruleCounts["ADM-NMT4-MISMATCH"] ?? 0) + (ruleCounts["ADM-DIRECTION-MISMATCH"] ?? 0);

  const sev = summary.data?.bySeverity ?? [];
  const maxSev = Math.max(1, ...sev.map((s) => s.count));

  if (summary.isLoading) {
    return (
      <Box sx={{ display: "flex", justifyContent: "center", py: 8 }}>
        <CircularProgress />
      </Box>
    );
  }
  if (summary.isError || !summary.data) {
    return <Alert severity="error">{t("mgmt.loadError")}</Alert>;
  }

  return (
    <Stack spacing={3}>
      <Box>
        <Typography variant="h5" sx={{ fontWeight: 800 }}>
          {t("mgmt.title")}
        </Typography>
        <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5 }}>
          {t("mgmt.subtitle")}
        </Typography>
      </Box>

      <Stack direction="row" spacing={2} useFlexGap sx={{ flexWrap: "wrap" }}>
        <Kpi label={t("mgmt.kpiOpenFlags")} value={summary.data.openFlags} color={RED} />
        <Kpi label={t("mgmt.kpiTotalFlags")} value={summary.data.totalFlags} color={ORANGE} />
        <Kpi label={t("mgmt.kpiRecommendations")} value={summary.data.totalRecommendations} color={BLUE} />
        <Kpi label={t("mgmt.kpiRuns")} value={summary.data.totalRuns} color={GREEN} />
      </Stack>

      <Card variant="outlined">
        <CardHeader title={t("mgmt.bySeverity")} />
        <CardContent>
          <Stack spacing={1.5}>
            {sev.map((s) => (
              <Box key={s.severity}>
                <Stack direction="row" sx={{ justifyContent: "space-between", mb: 0.5 }}>
                  <Chip size="small" color={severityColor(s.severity)} label={t(`severity.${s.severity}`)} />
                  <Typography variant="body2" sx={{ fontWeight: 700 }}>
                    {s.count}
                  </Typography>
                </Stack>
                <LinearProgress
                  variant="determinate"
                  value={(s.count / maxSev) * 100}
                  sx={{
                    height: 8,
                    borderRadius: 4,
                    bgcolor: "#eef0f6",
                    "& .MuiLinearProgress-bar": { bgcolor: severityHex(s.severity) },
                  }}
                />
              </Box>
            ))}
          </Stack>
        </CardContent>
      </Card>

      <Stack direction={{ xs: "column", md: "row" }} spacing={2} sx={{ alignItems: "stretch" }}>
        <Card variant="outlined" sx={{ flex: 1 }}>
          <CardHeader title={t("mgmt.criticalSchools")} subheader={t("mgmt.criticalHint")} />
          <CardContent>
            <Stack spacing={1.25}>
              {bySchool.slice(0, 6).map((s) => (
                <Stack key={s.name} direction="row" sx={{ justifyContent: "space-between", alignItems: "center" }}>
                  <Box>
                    <Typography variant="body2" sx={{ fontWeight: 600 }}>
                      {s.name}
                    </Typography>
                    <Typography variant="caption" color="text.secondary">
                      {s.community}
                    </Typography>
                  </Box>
                  <Chip
                    size="small"
                    color="error"
                    variant="outlined"
                    label={t("mgmt.flagsCount", { critical: s.critical, total: s.total })}
                  />
                </Stack>
              ))}
            </Stack>
          </CardContent>
        </Card>

        <Card variant="outlined" sx={{ flex: 1 }}>
          <CardHeader title={t("mgmt.criticalCommunities")} subheader={t("mgmt.criticalHint")} />
          <CardContent>
            <Stack spacing={1.25}>
              {byCommunity.slice(0, 6).map((c) => (
                <Stack key={c.community} direction="row" sx={{ justifyContent: "space-between", alignItems: "center" }}>
                  <Typography variant="body2" sx={{ fontWeight: 600 }}>
                    {c.community}
                  </Typography>
                  <Chip
                    size="small"
                    color="error"
                    variant="outlined"
                    label={t("mgmt.flagsCount", { critical: c.critical, total: c.total })}
                  />
                </Stack>
              ))}
            </Stack>
          </CardContent>
        </Card>
      </Stack>

      <Card variant="outlined">
        <CardHeader title={t("mgmt.decisionSupport")} subheader={t("mgmt.decisionHint")} />
        <CardContent>
          <Stack spacing={1.5}>
            <Alert severity="warning">{t("mgmt.adviceAttendance", { count: attendance })}</Alert>
            <Alert severity="info">{t("mgmt.adviceProfile", { count: profileMismatch })}</Alert>
            <Alert severity="info">{t("mgmt.adviceAdmission", { count: admissionMismatch })}</Alert>
          </Stack>
        </CardContent>
      </Card>

      <Card variant="outlined">
        <CardHeader title={t("mgmt.recentFlags")} />
        <CardContent>
          <Stack spacing={1} divider={<Divider flexItem />}>
            {summary.data.recentFlags.map((f) => (
              <Stack
                key={f.id}
                direction="row"
                spacing={1}
                component={NextLink}
                href={`/children/${f.subjectId}`}
                sx={{ alignItems: "center", textDecoration: "none", color: "inherit", py: 0.5 }}
              >
                <Chip size="small" color={severityColor(f.severity)} label={t(`severity.${f.severity}`)} />
                <Typography variant="body2" sx={{ fontWeight: 600, flexShrink: 0 }}>
                  {f.subjectName}
                </Typography>
                <Typography variant="body2" color="text.secondary" noWrap>
                  {f.title}
                </Typography>
              </Stack>
            ))}
          </Stack>
        </CardContent>
      </Card>
    </Stack>
  );
}
