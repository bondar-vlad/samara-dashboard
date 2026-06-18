"use client";

import {
  Alert,
  Box,
  Button,
  Chip,
  CircularProgress,
  Stack,
} from "@mui/material";
import ReplayIcon from "@mui/icons-material/Replay";
import PlayArrowIcon from "@mui/icons-material/PlayArrow";
import CheckCircleOutlineIcon from "@mui/icons-material/CheckCircleOutlined";
import { useStudentAnalysisStatus, useRunAnalysis } from "@/lib/hooks";
import { useTranslation } from "@/i18n/I18nProvider";

/**
 * Shows whether the pupil has been analysed and lets the user (re)run the analysis.
 * Makes "not analysed yet" visibly different from "analysed, no risks found", so an
 * empty markers list is never mistaken for missing data.
 */
export default function AnalysisStatusBar({ studentId }: { studentId: string }) {
  const { t } = useTranslation();
  const status = useStudentAnalysisStatus(studentId);
  const runAnalysis = useRunAnalysis();

  const analyzed = status.data?.analyzed ?? false;
  const pending = runAnalysis.isPending;

  const formatted = status.data?.lastAnalyzedUtc
    ? new Date(status.data.lastAnalyzedUtc).toLocaleString()
    : "";

  const runButton = (
    <Button
      variant={analyzed ? "outlined" : "contained"}
      color="primary"
      size="small"
      disabled={pending}
      startIcon={
        pending ? (
          <CircularProgress size={16} color="inherit" />
        ) : analyzed ? (
          <ReplayIcon />
        ) : (
          <PlayArrowIcon />
        )
      }
      onClick={() => runAnalysis.mutate(studentId)}
    >
      {analyzed ? t("analysis.reanalyze") : t("analysis.runAnalysis")}
    </Button>
  );

  // Until the status is known, render nothing rather than flash a misleading state.
  if (status.isLoading) {
    return null;
  }

  if (!analyzed) {
    return (
      <Alert severity="info" action={runButton} sx={{ alignItems: "center" }}>
        {t("analysis.notAnalyzed")}
      </Alert>
    );
  }

  return (
    <Stack direction="row" spacing={1} useFlexGap sx={{ alignItems: "center", flexWrap: "wrap" }}>
      <Chip
        size="small"
        color="success"
        variant="outlined"
        icon={<CheckCircleOutlineIcon />}
        label={formatted ? t("analysis.analyzedAt", { date: formatted }) : t("analysis.analyzed")}
      />
      <Box sx={{ flexGrow: 1 }} />
      {runButton}
    </Stack>
  );
}
