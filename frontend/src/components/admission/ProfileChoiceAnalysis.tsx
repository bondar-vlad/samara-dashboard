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
  Divider,
  LinearProgress,
  CircularProgress,
  Alert,
} from "@mui/material";
import WarningAmberIcon from "@mui/icons-material/WarningAmber";
import CheckCircleOutlineIcon from "@mui/icons-material/CheckCircleOutlined";
import WarningCard from "@/components/profile/WarningCard";
import ImprovementPlan from "@/components/profile/ImprovementPlan";
import { useStudentProfileChoice, useStudentRedFlags, useStudentAnalysisStatus } from "@/lib/hooks";
import type { RedFlag } from "@/lib/types";
import { BLUE, ORANGE, YELLOW, GREEN } from "@/theme/colors";
import { useTranslation } from "@/i18n/I18nProvider";

function ChoiceBox({
  title,
  subtitle,
  value,
  accent,
  highlight,
  chips,
}: {
  title: string;
  subtitle: string;
  value: string | null;
  accent: string;
  highlight: boolean;
  chips?: string[];
}) {
  return (
    <Paper
      variant="outlined"
      sx={{
        flex: 1,
        p: 2,
        borderColor: highlight ? accent : "divider",
        borderWidth: highlight ? 2 : 1,
        bgcolor: highlight ? `${accent}0d` : "background.paper",
      }}
    >
      <Stack direction="row" spacing={1} sx={{ alignItems: "center", mb: 0.5 }}>
        <Box sx={{ width: 10, height: 10, borderRadius: "50%", bgcolor: accent }} />
        <Typography variant="subtitle2" sx={{ fontWeight: 700 }}>
          {title}
        </Typography>
      </Stack>
      <Typography variant="caption" color="text.secondary">
        {subtitle}
      </Typography>
      <Typography variant="h6" sx={{ fontWeight: 700, mt: 1 }}>
        {value ?? "—"}
      </Typography>
      {chips && chips.length > 0 && (
        <Stack direction="row" spacing={0.75} sx={{ flexWrap: "wrap", mt: 1 }} useFlexGap>
          {chips.map((c) => (
            <Chip key={c} size="small" variant="outlined" label={c} />
          ))}
        </Stack>
      )}
    </Paper>
  );
}

export default function ProfileChoiceAnalysis({ studentId }: { studentId: string }) {
  const { t } = useTranslation();
  const { data, isLoading, isError } = useStudentProfileChoice(studentId);
  const flags = useStudentRedFlags(studentId);
  const analyzed = useStudentAnalysisStatus(studentId).data?.analyzed ?? false;

  const maxScore = 12;
  const mismatch = !!data?.hasChoice && !data?.isMatch;

  // The same student can accumulate duplicate flags across runs; keep the newest per code.
  const dedupedFlags = useMemo(() => {
    const byCode = new Map<string, RedFlag>();
    for (const f of flags.data ?? []) {
      const prev = byCode.get(f.ruleCode);
      if (!prev || f.detectedAtUtc > prev.detectedAtUtc) byCode.set(f.ruleCode, f);
    }
    return Array.from(byCode.values()).sort((a, b) =>
      b.detectedAtUtc.localeCompare(a.detectedAtUtc),
    );
  }, [flags.data]);

  return (
    <Card variant="outlined">
      <CardHeader title={t("profileChoice.cardTitle")} subheader={t("profileChoice.cardSub")} />
      <CardContent>
        {isLoading ? (
          <Box sx={{ display: "flex", justifyContent: "center", py: 4 }}>
            <CircularProgress />
          </Box>
        ) : isError || !data ? (
          <Alert severity="error">{t("profileChoice.analysisLoadError")}</Alert>
        ) : (
          <Stack spacing={2}>
            <Stack direction={{ xs: "column", sm: "row" }} spacing={2} sx={{ alignItems: "stretch" }}>
              <ChoiceBox
                title={t("profileChoice.studentChoice")}
                subtitle={data.hasChoice ? t("profileChoice.chosenCluster") : t("profileChoice.notChosenYet")}
                value={data.desiredClusterName}
                accent={ORANGE}
                highlight={mismatch}
                chips={data.desiredProfiles.map((p) => p.profileName)}
              />
              <ChoiceBox
                title={t("profileChoice.systemRec")}
                subtitle={
                  data.confidence != null
                    ? t("profileChoice.confidence", { pct: Math.round(data.confidence * 100) })
                    : t("profileChoice.byGradesAndTopics")
                }
                value={data.recommendedClusterName}
                accent={BLUE}
                highlight={mismatch}
                chips={data.recommendedProfiles.map((p) => p.profileName)}
              />
            </Stack>

            {data.recommendedCluster && (
              <Stack
                direction="row"
                spacing={1}
                sx={{
                  alignItems: "center",
                  p: 1.5,
                  borderRadius: 2,
                  border: 1,
                  bgcolor: !data.hasChoice ? "transparent" : mismatch ? `${YELLOW}14` : `${GREEN}14`,
                  borderColor: !data.hasChoice ? "divider" : mismatch ? YELLOW : GREEN,
                }}
              >
                {mismatch ? (
                  <WarningAmberIcon sx={{ color: YELLOW }} />
                ) : data.hasChoice ? (
                  <CheckCircleOutlineIcon sx={{ color: GREEN }} />
                ) : (
                  <WarningAmberIcon sx={{ color: "text.disabled" }} />
                )}
                <Typography variant="body2">
                  {!data.hasChoice
                    ? t("profileChoice.bannerNotChosen")
                    : mismatch
                      ? t("profileChoice.bannerMismatch")
                      : t("profileChoice.bannerMatch")}
                </Typography>
              </Stack>
            )}

            {/* AI "what to pull up" tool — shown when the pupil keeps a mismatched choice. */}
            {mismatch && <ImprovementPlan studentId={studentId} />}

            {/* rationale */}
            {data.rationale && (
              <Paper variant="outlined" sx={{ p: 2, bgcolor: "#fafafa", borderStyle: "dashed" }}>
                <Typography variant="overline" color="text.secondary">
                  {t("profileChoice.howComputed")}
                </Typography>
                <Typography variant="body2" sx={{ mt: 0.5 }}>
                  {data.rationale}
                </Typography>
              </Paper>
            )}

            {/* ranked clusters */}
            {data.ranked.length > 0 && (
              <Box>
                <Divider sx={{ mb: 1.5 }}>
                  <Typography variant="overline" color="text.secondary">
                    {t("profileChoice.rankingTitle")}
                  </Typography>
                </Divider>
                <Stack spacing={1.25}>
                  {data.ranked.map((r) => {
                    const isRec = r.cluster === data.recommendedCluster;
                    return (
                      <Box key={r.cluster}>
                        <Stack direction="row" sx={{ justifyContent: "space-between", alignItems: "baseline" }}>
                          <Typography variant="body2" sx={{ fontWeight: isRec ? 700 : 500 }}>
                            {r.clusterName}
                            {isRec && (
                              <Chip size="small" label={t("common.recommendedTag")} color="primary" sx={{ ml: 1, height: 18 }} />
                            )}
                          </Typography>
                          <Typography variant="body2" color="text.secondary">
                            {r.score.toFixed(1)}
                          </Typography>
                        </Stack>
                        <LinearProgress
                          variant="determinate"
                          value={Math.min(100, (r.score / maxScore) * 100)}
                          sx={{
                            height: 8,
                            borderRadius: 4,
                            mt: 0.5,
                            bgcolor: "#eef0f6",
                            "& .MuiLinearProgress-bar": { bgcolor: isRec ? BLUE : ORANGE },
                          }}
                        />
                      </Box>
                    );
                  })}
                </Stack>
              </Box>
            )}

            {/* markers (red flags) */}
            <Box>
              <Divider sx={{ mb: 2 }}>
                <Typography variant="overline" color="text.secondary">
                  {t("comparison.markersDivider")}
                </Typography>
              </Divider>
              {flags.isLoading ? (
                <CircularProgress size={20} />
              ) : dedupedFlags.length > 0 ? (
                <Stack spacing={2}>
                  {dedupedFlags.map((flag) => (
                    <WarningCard
                      key={flag.id}
                      flag={flag}
                      defaultExpanded={flag.ruleCode === "EDU-PROFILE-MISMATCH"}
                    />
                  ))}
                </Stack>
              ) : !analyzed ? (
                <Alert severity="info">{t("analysis.notAnalyzedShort")}</Alert>
              ) : (
                <Alert severity="success">{t("comparison.noMarkers")}</Alert>
              )}
            </Box>
          </Stack>
        )}
      </CardContent>
    </Card>
  );
}
