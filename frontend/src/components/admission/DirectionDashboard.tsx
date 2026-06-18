"use client";

import { useMemo, useState } from "react";
import { useRouter } from "next/navigation";
import {
  Card,
  Box,
  Stack,
  Typography,
  Chip,
  CircularProgress,
  Alert,
  Table,
  TableHead,
  TableBody,
  TableRow,
  TableCell,
  TableContainer,
  TablePagination,
  Paper,
} from "@mui/material";
import WarningAmberIcon from "@mui/icons-material/WarningAmber";
import CheckCircleOutlineIcon from "@mui/icons-material/CheckCircleOutlined";
import ChartFrame from "@/components/charts/ChartFrame";
import GroupedBarChart, {
  type GroupedDatum,
  type SeriesDef,
} from "@/components/charts/GroupedBarChart";
import { usePrimarySchoolId, useAdmissionDirections, useSchoolDirections } from "@/lib/hooks";
import { SERIES_DESIRED, SERIES_RECOMMENDED, YELLOW, GREEN } from "@/theme/colors";
import { useTranslation } from "@/i18n/I18nProvider";

function Legend({ series }: { series: SeriesDef[] }) {
  return (
    <Stack direction="row" spacing={2} sx={{ flexWrap: "wrap", mb: 1 }} useFlexGap>
      {series.map((s) => (
        <Stack key={s.key} direction="row" spacing={0.75} sx={{ alignItems: "center" }}>
          <Box sx={{ width: 12, height: 12, borderRadius: "3px", bgcolor: s.color }} />
          <Typography variant="body2" color="text.secondary">
            {s.name}
          </Typography>
        </Stack>
      ))}
    </Stack>
  );
}

export default function DirectionDashboard() {
  const router = useRouter();
  const { t } = useTranslation();
  const schoolId = usePrimarySchoolId();
  const directions = useAdmissionDirections();
  const school = useSchoolDirections(schoolId);
  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(10);

  const series: SeriesDef[] = useMemo(
    () => [
      { key: "chosen", name: t("common.seriesChosen"), color: SERIES_DESIRED },
      { key: "recommended", name: t("common.seriesRecommended"), color: SERIES_RECOMMENDED },
    ],
    [t],
  );

  const nameOf = useMemo(() => {
    const map = new Map<string, string>();
    for (const d of directions.data ?? []) map.set(d.code, d.name);
    return (code: string | null) => (code ? map.get(code) ?? code : "—");
  }, [directions.data]);

  const chartData: GroupedDatum[] = useMemo(() => {
    return (school.data?.distribution ?? [])
      .filter((d) => d.chosenCount > 0 || d.recommendedCount > 0)
      .map((d) => ({
        label: d.directionName,
        values: { chosen: d.chosenCount, recommended: d.recommendedCount },
      }));
  }, [school.data]);

  const rows = school.data?.students ?? [];
  const notChosen = rows.filter((s) => !s.desiredDirectionCode).length;
  const mismatches = rows.filter((s) => s.desiredDirectionCode && !s.isMatch).length;

  const pageCount = Math.max(1, Math.ceil(rows.length / rowsPerPage));
  const safePage = Math.min(page, pageCount - 1);
  const pagedRows = rows.slice(safePage * rowsPerPage, safePage * rowsPerPage + rowsPerPage);

  if (school.isLoading || !schoolId) {
    return (
      <Box sx={{ display: "flex", justifyContent: "center", py: 8 }}>
        <CircularProgress />
      </Box>
    );
  }
  if (school.isError) {
    return <Alert severity="error">{t("direction.loadError")}</Alert>;
  }

  return (
    <Stack spacing={3}>
      <ChartFrame
        title={t("direction.chartTitle")}
        subheader={t("direction.chartSub")}
        filename="admission-direction"
      >
        <Legend series={series} />
        {chartData.length > 0 ? (
          <GroupedBarChart data={chartData} series={series} />
        ) : (
          <Alert severity="info">{t("common.noChartData")}</Alert>
        )}
      </ChartFrame>

      <Box>
        <Stack
          direction="row"
          spacing={1}
          sx={{ alignItems: "center", flexWrap: "wrap", mb: 0.5 }}
          useFlexGap
        >
          <Typography variant="h6" sx={{ fontWeight: 700 }}>
            {t("direction.studentsTitle")}
          </Typography>
          <Chip size="small" label={t("common.notChosenCount", { count: notChosen })} variant="outlined" />
          <Chip size="small" color="warning" variant="outlined" label={t("common.mismatchesCount", { count: mismatches })} />
        </Stack>
        <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
          {t("direction.clickHint")}
        </Typography>

        <Card variant="outlined">
          <TableContainer component={Paper} elevation={0}>
            <Table size="small">
              <TableHead>
                <TableRow>
                  <TableCell>{t("table.student")}</TableCell>
                  <TableCell>{t("table.class")}</TableCell>
                  <TableCell>{t("table.choice")}</TableCell>
                  <TableCell>{t("table.recommendation")}</TableCell>
                  <TableCell align="center">{t("table.status")}</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {pagedRows.map((s) => {
                  const mismatch = !!s.desiredDirectionCode && !s.isMatch;
                  return (
                    <TableRow
                      key={s.studentId}
                      hover
                      onClick={() => router.push(`/students/${s.studentId}`)}
                      sx={{ cursor: "pointer", bgcolor: mismatch ? `${YELLOW}14` : undefined }}
                    >
                      <TableCell sx={{ fontWeight: 600 }}>{s.fullName}</TableCell>
                      <TableCell>{s.className}</TableCell>
                      <TableCell>
                        {s.desiredDirectionCode ? nameOf(s.desiredDirectionCode) : "—"}
                      </TableCell>
                      <TableCell>{nameOf(s.recommendedDirectionCode)}</TableCell>
                      <TableCell align="center">
                        {!s.desiredDirectionCode ? (
                          <Chip size="small" label={t("table.notChosen")} variant="outlined" />
                        ) : mismatch ? (
                          <Chip
                            size="small"
                            color="warning"
                            variant="outlined"
                            icon={<WarningAmberIcon />}
                            label={t("table.mismatch")}
                          />
                        ) : (
                          <CheckCircleOutlineIcon sx={{ color: GREEN, fontSize: 20 }} />
                        )}
                      </TableCell>
                    </TableRow>
                  );
                })}
              </TableBody>
            </Table>
          </TableContainer>
          <TablePagination
            component="div"
            count={rows.length}
            page={safePage}
            onPageChange={(_e, p) => setPage(p)}
            rowsPerPage={rowsPerPage}
            onRowsPerPageChange={(e) => {
              setRowsPerPage(parseInt(e.target.value, 10));
              setPage(0);
            }}
            rowsPerPageOptions={[10, 25, 50]}
            labelRowsPerPage={t("table.rowsPerPage")}
            labelDisplayedRows={({ from, to, count }) =>
              t("table.displayedRows", { from, to, count })
            }
          />
        </Card>
      </Box>
    </Stack>
  );
}
