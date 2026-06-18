"use client";

import { useMemo } from "react";
import NextLink from "next/link";
import {
  Alert,
  Box,
  Button,
  Card,
  CardContent,
  CardHeader,
  Chip,
  CircularProgress,
  Divider,
  LinearProgress,
  Stack,
  Typography,
} from "@mui/material";
import OpenInNewIcon from "@mui/icons-material/OpenInNew";
import AdminPanelSettingsIcon from "@mui/icons-material/AdminPanelSettings";
import { useRole } from "@/access/RoleProvider";
import { agencyFlagSource, ROLE_ORDER } from "@/access/roles";
import { useDashboardSummary, useRedFlags, useStudents } from "@/lib/hooks";
import { severityColor, severityHex } from "@/components/profile/severity";
import { BLUE, ORANGE, GREEN } from "@/theme/colors";
import AccessBanner from "@/components/access/AccessBanner";
import ManagementDashboard from "./ManagementDashboard";
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

/** Public portal: aggregated, de-identified indicators only — no personal data. */
function PublicDashboard() {
  const { t } = useTranslation();
  const summary = useDashboardSummary();

  if (summary.isLoading) {
    return (
      <Box sx={{ display: "flex", justifyContent: "center", py: 8 }}>
        <CircularProgress />
      </Box>
    );
  }
  if (summary.isError || !summary.data) return <Alert severity="error">{t("mgmt.loadError")}</Alert>;

  const sev = summary.data.bySeverity;
  const maxSev = Math.max(1, ...sev.map((s) => s.count));

  return (
    <Stack spacing={3}>
      <Box>
        <Typography variant="h5" sx={{ fontWeight: 800 }}>
          {t("access.publicTitle")}
        </Typography>
        <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5 }}>
          {t("access.publicSubtitle")}
        </Typography>
      </Box>

      <Alert severity="info">{t("access.publicNotice")}</Alert>

      <Stack direction="row" spacing={2} useFlexGap sx={{ flexWrap: "wrap" }}>
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
    </Stack>
  );
}

/** Parent app: a single bound child (via Дія.Підпис) — own child only. */
function ParentDashboard() {
  const { t } = useTranslation();
  const { role } = useRole();
  const students = useStudents();
  const child = students.data?.[0];
  const isChild = role === "CHILD";

  return (
    <Stack spacing={3}>
      <Box>
        <Typography variant="h5" sx={{ fontWeight: 800 }}>
          {t(isChild ? "access.childTitle" : "access.parentTitle")}
        </Typography>
      </Box>
      <Alert severity="info">{t(isChild ? "access.childNotice" : "access.parentNotice")}</Alert>
      <Card variant="outlined">
        <CardContent>
          {students.isLoading ? (
            <Box sx={{ display: "flex", justifyContent: "center", py: 4 }}>
              <CircularProgress />
            </Box>
          ) : child ? (
            <Stack spacing={1.5}>
              <Typography variant="h6" sx={{ fontWeight: 700 }}>
                {child.fullName}
              </Typography>
              <Typography variant="body2" color="text.secondary">
                {child.className}
              </Typography>
              <Button
                component={NextLink}
                href={`/children/${child.id}`}
                variant="contained"
                startIcon={<OpenInNewIcon />}
                sx={{ alignSelf: "flex-start" }}
              >
                {t("access.openChild")}
              </Button>
            </Stack>
          ) : (
            <Alert severity="warning">{t("access.noChild")}</Alert>
          )}
        </CardContent>
      </Card>
    </Stack>
  );
}

/** Interagency user: one agency's vertical slice of the cross-agency signals. */
function InteragencyDashboard() {
  const { t } = useTranslation();
  const { scope } = useRole();
  const flags = useRedFlags({ scope: "Student" });
  const source = agencyFlagSource(scope.agency);

  const vertical = useMemo(
    () => (source ? (flags.data ?? []).filter((f) => f.sourceAgency === source) : []),
    [flags.data, source],
  );

  const byAgency = useMemo(() => {
    const m: Record<string, number> = {};
    for (const f of flags.data ?? []) m[f.sourceAgency] = (m[f.sourceAgency] ?? 0) + 1;
    return m;
  }, [flags.data]);

  return (
    <Stack spacing={3}>
      <Box>
        <Typography variant="h5" sx={{ fontWeight: 800 }}>
          {t("access.interagencyTitle", { agency: scope.agency ?? "—" })}
        </Typography>
      </Box>
      <Alert severity="info">{t("access.interagencyNotice")}</Alert>

      <Stack direction="row" spacing={1} useFlexGap sx={{ flexWrap: "wrap" }}>
        {Object.entries(byAgency).map(([agency, count]) => (
          <Chip
            key={agency}
            label={`${agency}: ${count}`}
            color={agency === source ? "primary" : "default"}
            variant={agency === source ? "filled" : "outlined"}
          />
        ))}
      </Stack>

      <Card variant="outlined">
        <CardHeader title={t("childProfile.signals")} />
        <CardContent>
          {flags.isLoading ? (
            <Box sx={{ display: "flex", justifyContent: "center", py: 4 }}>
              <CircularProgress />
            </Box>
          ) : vertical.length === 0 ? (
            <Alert severity="info">{t("access.scopedEmpty")}</Alert>
          ) : (
            <Stack spacing={1} divider={<Divider flexItem />}>
              {vertical.slice(0, 12).map((f) => (
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
          )}
        </CardContent>
      </Card>
    </Stack>
  );
}

/** System administrator: users, roles, integrations — links to the role matrix. */
function AdminDashboard() {
  const { t } = useTranslation();
  return (
    <Stack spacing={3}>
      <Box>
        <Typography variant="h5" sx={{ fontWeight: 800 }}>
          {t("access.adminTitle")}
        </Typography>
        <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5 }}>
          {t("access.adminSubtitle")}
        </Typography>
      </Box>

      <Stack direction="row" spacing={2} useFlexGap sx={{ flexWrap: "wrap" }}>
        <Kpi label={t("access.adminRolesCount", { count: ROLE_ORDER.length })} value={ROLE_ORDER.length} color={BLUE} />
      </Stack>

      <Card variant="outlined">
        <CardHeader title={t("access.adminUsers")} subheader={t("access.adminUsersHint")} />
        <CardContent>
          <Button
            component={NextLink}
            href="/access"
            variant="contained"
            startIcon={<AdminPanelSettingsIcon />}
          >
            {t("access.openAccessMatrix")}
          </Button>
        </CardContent>
      </Card>
    </Stack>
  );
}

/**
 * Routes the active role to the dashboard surface it is entitled to, with the
 * access banner on top. Territorial roles (operational / analytics) reuse the
 * scope-aware management dashboard.
 */
export default function RoleDashboard() {
  const { def } = useRole();

  const body = () => {
    switch (def.view) {
      case "public":
        return <PublicDashboard />;
      case "parent":
        return <ParentDashboard />;
      case "interagency":
        return <InteragencyDashboard />;
      case "admin":
        return <AdminDashboard />;
      default:
        return <ManagementDashboard />;
    }
  };

  return (
    <Stack spacing={3}>
      <AccessBanner />
      {body()}
    </Stack>
  );
}
