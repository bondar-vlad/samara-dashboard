"use client";

import { useMemo } from "react";
import { useRouter } from "next/navigation";
import {
  Card,
  CardHeader,
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
  Paper,
} from "@mui/material";
import WarningAmberIcon from "@mui/icons-material/WarningAmber";
import CheckCircleOutlineIcon from "@mui/icons-material/CheckCircleOutlined";
import ChartFrame from "@/components/charts/ChartFrame";
import GroupedBarChart, {
  type GroupedDatum,
  type SeriesDef,
} from "@/components/charts/GroupedBarChart";
import { useStudents, useNmtSubjects, useSchoolFourthSubjects } from "@/lib/hooks";
import { SERIES_DESIRED, SERIES_RECOMMENDED, YELLOW, GREEN } from "@/theme/colors";

const SERIES: SeriesDef[] = [
  { key: "chosen", name: "Вибір учнів", color: SERIES_DESIRED },
  { key: "recommended", name: "Рекомендація системи", color: SERIES_RECOMMENDED },
];

function Legend() {
  return (
    <Stack direction="row" spacing={2} sx={{ flexWrap: "wrap", mb: 1 }} useFlexGap>
      {SERIES.map((s) => (
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

export default function FourthSubjectDashboard() {
  const router = useRouter();
  const students = useStudents();
  const nmt = useNmtSubjects();
  const schoolId = students.data?.[0]?.schoolId;
  const school = useSchoolFourthSubjects(schoolId);

  const nameOf = useMemo(() => {
    const map = new Map<string, string>();
    for (const s of nmt.data ?? []) map.set(s.subject, s.name);
    return (subject: string | null) => (subject ? map.get(subject) ?? subject : "—");
  }, [nmt.data]);

  // Chart: chosen vs recommended count per fourth-subject option (skip empty).
  const chartData: GroupedDatum[] = useMemo(() => {
    return (school.data?.distribution ?? [])
      .filter((d) => d.chosenCount > 0 || d.recommendedCount > 0)
      .map((d) => ({
        label: d.subjectName,
        values: { chosen: d.chosenCount, recommended: d.recommendedCount },
      }));
  }, [school.data]);

  const rows = school.data?.students ?? [];
  const notChosen = rows.filter((s) => !s.chosenSubject).length;
  const mismatches = rows.filter((s) => s.chosenSubject && !s.isMatch).length;

  if (students.isLoading || school.isLoading) {
    return (
      <Box sx={{ display: "flex", justifyContent: "center", py: 8 }}>
        <CircularProgress />
      </Box>
    );
  }
  if (students.isError || school.isError) {
    return <Alert severity="error">Не вдалося завантажити дані про четвертий предмет НМТ.</Alert>;
  }

  return (
    <Stack spacing={3}>
      <ChartFrame
        title="Четвертий предмет НМТ: вибір учнів проти рекомендації"
        subheader="Кількість учнів за предметом на вибір — що обрали учні та що рекомендує система"
        filename="nmt-fourth-subject"
      >
        <Legend />
        {chartData.length > 0 ? (
          <GroupedBarChart data={chartData} series={SERIES} />
        ) : (
          <Alert severity="info">Немає даних для графіка.</Alert>
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
            Учні: четвертий предмет
          </Typography>
          <Chip size="small" label={`Не обрали: ${notChosen}`} variant="outlined" />
          <Chip size="small" color="warning" variant="outlined" label={`Розбіжностей: ${mismatches}`} />
        </Stack>
        <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
          Натисніть на учня, щоб відкрити детальний аналіз четвертого предмета.
        </Typography>

        <Card variant="outlined">
          <TableContainer component={Paper} elevation={0}>
            <Table size="small">
              <TableHead>
                <TableRow>
                  <TableCell>Учень</TableCell>
                  <TableCell>Клас</TableCell>
                  <TableCell>Вибір учня</TableCell>
                  <TableCell>Рекомендація системи</TableCell>
                  <TableCell align="center">Статус</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {rows.map((s) => {
                  const mismatch = !!s.chosenSubject && !s.isMatch;
                  return (
                    <TableRow
                      key={s.studentId}
                      hover
                      onClick={() => router.push(`/admission/students/${s.studentId}`)}
                      sx={{ cursor: "pointer", bgcolor: mismatch ? `${YELLOW}14` : undefined }}
                    >
                      <TableCell sx={{ fontWeight: 600 }}>{s.fullName}</TableCell>
                      <TableCell>{s.className}</TableCell>
                      <TableCell>{s.chosenSubject ? nameOf(s.chosenSubject) : "—"}</TableCell>
                      <TableCell>{nameOf(s.recommendedSubject)}</TableCell>
                      <TableCell align="center">
                        {!s.chosenSubject ? (
                          <Chip size="small" label="Не обрано" variant="outlined" />
                        ) : mismatch ? (
                          <Chip
                            size="small"
                            color="warning"
                            variant="outlined"
                            icon={<WarningAmberIcon />}
                            label="Розбіжність"
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
        </Card>
      </Box>
    </Stack>
  );
}
