"use client";

import { useMemo, useState } from "react";
import { useRouter } from "next/navigation";
import {
  Card,
  Box,
  Stack,
  Typography,
  Chip,
  TextField,
  MenuItem,
  Table,
  TableHead,
  TableBody,
  TableRow,
  TableCell,
  TableContainer,
  TablePagination,
  Paper,
  CircularProgress,
  Alert,
} from "@mui/material";
import { useRedFlags, useDashboardSummary } from "@/lib/hooks";
import { severityColor } from "@/components/profile/severity";
import { useTranslation } from "@/i18n/I18nProvider";

const SEV_RANK: Record<string, number> = { Red: 0, Orange: 1, Yellow: 2, Green: 3 };
const SEVERITIES = ["Red", "Orange", "Yellow"];
const STATUSES = ["Open", "Acknowledged", "Resolved"];

export default function RiskMonitor() {
  const { t } = useTranslation();
  const router = useRouter();
  const [severity, setSeverity] = useState("");
  const [status, setStatus] = useState("Open");
  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(15);

  const flags = useRedFlags({ severity: severity || undefined, status: status || undefined });
  const summary = useDashboardSummary();

  const counts = useMemo(() => {
    const m: Record<string, number> = {};
    for (const s of summary.data?.bySeverity ?? []) m[s.severity] = s.count;
    return m;
  }, [summary.data]);

  const rows = useMemo(() => {
    const list = [...(flags.data ?? [])];
    list.sort(
      (a, b) =>
        (SEV_RANK[a.severity] ?? 9) - (SEV_RANK[b.severity] ?? 9) ||
        b.detectedAtUtc.localeCompare(a.detectedAtUtc),
    );
    return list;
  }, [flags.data]);

  const pageCount = Math.max(1, Math.ceil(rows.length / rowsPerPage));
  const safePage = Math.min(page, pageCount - 1);
  const paged = rows.slice(safePage * rowsPerPage, safePage * rowsPerPage + rowsPerPage);

  return (
    <Stack spacing={3}>
      <Box>
        <Typography variant="h5" sx={{ fontWeight: 800 }}>
          {t("risks.title")}
        </Typography>
        <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5 }}>
          {t("risks.subtitle")}
        </Typography>
      </Box>

      <Stack direction="row" spacing={1} useFlexGap sx={{ flexWrap: "wrap", alignItems: "center" }}>
        {SEVERITIES.map((s) => (
          <Chip
            key={s}
            color={severityColor(s)}
            variant="outlined"
            label={`${t(`severity.${s}`)}: ${counts[s] ?? 0}`}
          />
        ))}
        <Box sx={{ flexGrow: 1 }} />
        <TextField
          select
          size="small"
          label={t("risks.filterSeverity")}
          value={severity}
          onChange={(e) => {
            setSeverity(e.target.value);
            setPage(0);
          }}
          sx={{ minWidth: 160 }}
        >
          <MenuItem value="">{t("risks.all")}</MenuItem>
          {SEVERITIES.map((s) => (
            <MenuItem key={s} value={s}>
              {t(`severity.${s}`)}
            </MenuItem>
          ))}
        </TextField>
        <TextField
          select
          size="small"
          label={t("risks.filterStatus")}
          value={status}
          onChange={(e) => {
            setStatus(e.target.value);
            setPage(0);
          }}
          sx={{ minWidth: 160 }}
        >
          <MenuItem value="">{t("risks.all")}</MenuItem>
          {STATUSES.map((s) => (
            <MenuItem key={s} value={s}>
              {t(`warningCard.status${s}`)}
            </MenuItem>
          ))}
        </TextField>
      </Stack>

      {flags.isLoading ? (
        <Box sx={{ display: "flex", justifyContent: "center", py: 8 }}>
          <CircularProgress />
        </Box>
      ) : flags.isError ? (
        <Alert severity="error">{t("risks.loadError")}</Alert>
      ) : rows.length === 0 ? (
        <Alert severity="success">{t("risks.empty")}</Alert>
      ) : (
        <Card variant="outlined">
          <TableContainer component={Paper} elevation={0}>
            <Table size="small">
              <TableHead>
                <TableRow>
                  <TableCell>{t("risks.colSeverity")}</TableCell>
                  <TableCell>{t("risks.colTitle")}</TableCell>
                  <TableCell>{t("table.student")}</TableCell>
                  <TableCell>{t("risks.colRule")}</TableCell>
                  <TableCell>{t("risks.colDetected")}</TableCell>
                  <TableCell align="center">{t("table.status")}</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {paged.map((f) => {
                  const isStudent = f.scope === "Student";
                  return (
                    <TableRow
                      key={f.id}
                      hover={isStudent}
                      onClick={() => isStudent && router.push(`/children/${f.subjectId}`)}
                      sx={{ cursor: isStudent ? "pointer" : "default" }}
                    >
                      <TableCell>
                        <Chip size="small" color={severityColor(f.severity)} label={t(`severity.${f.severity}`)} />
                      </TableCell>
                      <TableCell sx={{ fontWeight: 600 }}>{f.title}</TableCell>
                      <TableCell>{f.subjectName}</TableCell>
                      <TableCell>
                        <Typography variant="caption" color="text.secondary">
                          {f.ruleCode}
                        </Typography>
                      </TableCell>
                      <TableCell>{new Date(f.detectedAtUtc).toLocaleDateString()}</TableCell>
                      <TableCell align="center">
                        <Typography variant="caption">{t(`warningCard.status${f.status}`)}</Typography>
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
            rowsPerPageOptions={[15, 30, 50]}
            labelRowsPerPage={t("table.rowsPerPage")}
            labelDisplayedRows={({ from, to, count }) => t("table.displayedRows", { from, to, count })}
          />
        </Card>
      )}
    </Stack>
  );
}
