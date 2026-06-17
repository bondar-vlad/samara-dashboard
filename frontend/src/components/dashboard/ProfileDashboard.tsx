"use client";

import { useMemo } from "react";
import { useRouter } from "next/navigation";
import {
  Card,
  CardContent,
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
import GroupedBarChart, {
  type GroupedDatum,
  type SeriesDef,
} from "@/components/charts/GroupedBarChart";
import { useStudents, useReformReference } from "@/lib/hooks";
import type { StudentListItem } from "@/lib/types";

const DESIRED = "#3b6ea5";
const RECOMMENDED = "#0a0a0a";

const SERIES: SeriesDef[] = [
  { key: "desired", name: "Бажання учнів (анкетування)", color: DESIRED },
  { key: "recommended", name: "Рекомендація системи", color: RECOMMENDED },
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

function ClusterTable({
  clusterName,
  students,
  nameOf,
  onOpen,
}: {
  clusterName: string;
  students: StudentListItem[];
  nameOf: (cluster: string | null) => string;
  onOpen: (id: string) => void;
}) {
  const mismatchCount = students.filter((s) => s.hasProfileMismatch).length;
  return (
    <Card variant="outlined">
      <CardHeader
        title={
          <Typography variant="subtitle1" sx={{ fontWeight: 700 }}>
            {clusterName}
          </Typography>
        }
        subheader={`${students.length} учн. · ${mismatchCount} з розбіжністю`}
      />
      <TableContainer component={Paper} elevation={0}>
        <Table size="small">
          <TableHead>
            <TableRow>
              <TableCell>Учень</TableCell>
              <TableCell>Клас</TableCell>
              <TableCell>Рекомендація системи</TableCell>
              <TableCell align="center">Статус</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {students.map((s) => (
              <TableRow
                key={s.id}
                hover
                onClick={() => onOpen(s.id)}
                sx={{
                  cursor: "pointer",
                  bgcolor: s.hasProfileMismatch ? "#c778000d" : undefined,
                }}
              >
                <TableCell sx={{ fontWeight: 600 }}>{s.fullName}</TableCell>
                <TableCell>{s.className}</TableCell>
                <TableCell>{nameOf(s.recommendedCluster)}</TableCell>
                <TableCell align="center">
                  {s.hasProfileMismatch ? (
                    <Chip
                      size="small"
                      color="warning"
                      variant="outlined"
                      icon={<WarningAmberIcon />}
                      label="Розбіжність"
                    />
                  ) : (
                    <CheckCircleOutlineIcon sx={{ color: "#2e7d54", fontSize: 20 }} />
                  )}
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>
    </Card>
  );
}

export default function ProfileDashboard() {
  const router = useRouter();
  const students = useStudents();
  const reform = useReformReference();

  const nameOf = useMemo(() => {
    const map = new Map<string, string>();
    for (const c of reform.data?.clusters ?? []) map.set(c.cluster, c.clusterName);
    return (cluster: string | null) =>
      cluster ? map.get(cluster) ?? cluster : "—";
  }, [reform.data]);

  // Aggregate desired vs recommended counts per cluster (union of clusters seen).
  const chartData: GroupedDatum[] = useMemo(() => {
    const rows = students.data ?? [];
    const clusters = new Set<string>();
    for (const s of rows) {
      if (s.desiredCluster) clusters.add(s.desiredCluster);
      if (s.recommendedCluster) clusters.add(s.recommendedCluster);
    }
    return Array.from(clusters).map((c) => ({
      label: nameOf(c),
      values: {
        desired: rows.filter((s) => s.desiredCluster === c).length,
        recommended: rows.filter((s) => s.recommendedCluster === c).length,
      },
    }));
  }, [students.data, nameOf]);

  // Group students into per-cluster tables by their DESIRED cluster (survey view).
  const grouped = useMemo(() => {
    const rows = students.data ?? [];
    const m = new Map<string, StudentListItem[]>();
    for (const s of rows) {
      const key = s.desiredCluster ?? "—";
      if (!m.has(key)) m.set(key, []);
      m.get(key)!.push(s);
    }
    return Array.from(m.entries()).sort((a, b) => b[1].length - a[1].length);
  }, [students.data]);

  const totalMismatch = (students.data ?? []).filter(
    (s) => s.hasProfileMismatch,
  ).length;

  if (students.isLoading || reform.isLoading) {
    return (
      <Box sx={{ display: "flex", justifyContent: "center", py: 8 }}>
        <CircularProgress />
      </Box>
    );
  }
  if (students.isError) {
    return <Alert severity="error">Не вдалося завантажити дані учнів.</Alert>;
  }

  return (
    <Stack spacing={3}>
      <Card variant="outlined">
        <CardHeader
          title="Профілі: рекомендація системи проти бажання учнів"
          subheader="Кількість дітей за кластерами профілів — що рекомендує система та що обрали учні в анкетуванні"
          action={
            <Stack direction="row" spacing={1} sx={{ alignItems: "center" }}>
              <Chip size="small" label={`Усього учнів: ${students.data?.length ?? 0}`} />
              <Chip
                size="small"
                color="warning"
                variant="outlined"
                label={`Розбіжностей: ${totalMismatch}`}
              />
            </Stack>
          }
        />
        <CardContent>
          <Legend />
          {chartData.length > 0 ? (
            <GroupedBarChart data={chartData} series={SERIES} />
          ) : (
            <Alert severity="info">
              Немає даних для графіка. Запустіть аналіз для учнів.
            </Alert>
          )}
        </CardContent>
      </Card>

      <Box>
        <Typography variant="h6" sx={{ fontWeight: 700, mb: 0.5 }}>
          Учні за обраним профілем
        </Typography>
        <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
          Натисніть на учня, щоб відкрити детальний аналіз і маркери.
        </Typography>
        <Stack spacing={3}>
          {grouped.map(([cluster, list]) => (
            <ClusterTable
              key={cluster}
              clusterName={nameOf(cluster)}
              students={list}
              nameOf={nameOf}
              onOpen={(id) => router.push(`/students/${id}`)}
            />
          ))}
        </Stack>
      </Box>
    </Stack>
  );
}
