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
import { useStudentDirection, useAdmissionDirections } from "@/lib/hooks";
import ImprovementPlan from "@/components/profile/ImprovementPlan";
import { BLUE, ORANGE, YELLOW, GREEN } from "@/theme/colors";
import { useTranslation } from "@/i18n/I18nProvider";

function ChoiceBox({
  title,
  subtitle,
  value,
  accent,
  highlight,
}: {
  title: string;
  subtitle: string;
  value: string | null;
  accent: string;
  highlight: boolean;
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
    </Paper>
  );
}

export default function DirectionAnalysis({ studentId }: { studentId: string }) {
  const { t } = useTranslation();
  const { data, isLoading, isError } = useStudentDirection(studentId);
  const directions = useAdmissionDirections();

  const recInfo = useMemo(() => {
    if (!data?.recommendedDirectionCode) return undefined;
    return directions.data?.find((d) => d.code === data.recommendedDirectionCode);
  }, [directions.data, data]);

  const nmtEntries = Object.entries(data?.nmtScores ?? {});
  const mismatch = !!data?.hasChoice && !data?.isMatch;

  return (
    <Card variant="outlined">
      <CardHeader
        title={t("direction.cardTitle")}
        subheader={t("direction.cardSub")}
      />
      <CardContent>
        {isLoading ? (
          <Box sx={{ display: "flex", justifyContent: "center", py: 4 }}>
            <CircularProgress />
          </Box>
        ) : isError || !data ? (
          <Alert severity="error">{t("direction.analysisLoadError")}</Alert>
        ) : (
          <Stack spacing={2}>
            <Stack direction={{ xs: "column", sm: "row" }} spacing={2} sx={{ alignItems: "stretch" }}>
              <ChoiceBox
                title={t("direction.studentChoice")}
                subtitle={data.hasChoice ? t("direction.chosenDirection") : t("direction.notChosenYet")}
                value={data.desiredDirectionName}
                accent={ORANGE}
                highlight={mismatch}
              />
              <ChoiceBox
                title={t("direction.systemRec")}
                subtitle={t("direction.byNmtAndTopics")}
                value={data.recommendedDirectionName}
                accent={BLUE}
                highlight={mismatch}
              />
            </Stack>

            {data.recommendedDirectionCode && (
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
                    ? t("direction.bannerNotChosen")
                    : mismatch
                      ? t("direction.bannerMismatch")
                      : t("direction.bannerMatch")}
                </Typography>
              </Stack>
            )}

            {/* AI "what to pull up" tool — shown when the pupil keeps a mismatched choice. */}
            {mismatch && <ImprovementPlan studentId={studentId} />}

            {/* NMT scores, if submitted */}
            {nmtEntries.length > 0 && (
              <Box>
                <Typography variant="overline" color="text.secondary">
                  {t("direction.nmtScores")}
                </Typography>
                <Stack direction="row" spacing={1} sx={{ flexWrap: "wrap", mt: 0.5 }} useFlexGap>
                  {nmtEntries.map(([subj, score]) => (
                    <Chip key={subj} size="small" variant="outlined" label={`${subj}: ${score}`} />
                  ))}
                </Stack>
              </Box>
            )}

            {/* rationale */}
            {data.rationale && (
              <Paper variant="outlined" sx={{ p: 2, bgcolor: "#fafafa", borderStyle: "dashed" }}>
                <Typography variant="overline" color="text.secondary">
                  {t("direction.howComputed")}
                </Typography>
                <Typography variant="body2" sx={{ mt: 0.5 }}>
                  {data.rationale}
                </Typography>
              </Paper>
            )}

            {/* recommended direction details */}
            {recInfo && (
              <Paper variant="outlined" sx={{ p: 2 }}>
                <Typography variant="subtitle2" sx={{ fontWeight: 700 }}>
                  {recInfo.name}{" "}
                  <Typography component="span" variant="caption" color="text.secondary">
                    {t("direction.profileLabel", { name: recInfo.relatedClusterName })}
                  </Typography>
                </Typography>
                {recInfo.keySubjects.length > 0 && (
                  <Box sx={{ mt: 1 }}>
                    <Typography variant="caption" color="text.secondary">
                      {t("direction.keySubjects")}
                    </Typography>
                    <Typography variant="caption">{recInfo.keySubjects.join(", ")}</Typography>
                  </Box>
                )}
                {recInfo.specialties.length > 0 && (
                  <Stack direction="row" spacing={1} sx={{ flexWrap: "wrap", mt: 1 }} useFlexGap>
                    {recInfo.specialties.map((sp) => (
                      <Chip key={sp.code} size="small" label={`${sp.code} ${sp.name}`} />
                    ))}
                  </Stack>
                )}
              </Paper>
            )}

            {/* ranked directions */}
            {data.ranked.length > 0 && (
              <Box>
                <Divider sx={{ mb: 1.5 }}>
                  <Typography variant="overline" color="text.secondary">
                    {t("direction.rankingTitle")}
                  </Typography>
                </Divider>
                <Stack spacing={1.25}>
                  {data.ranked.map((r) => {
                    const isRec = r.directionCode === data.recommendedDirectionCode;
                    return (
                      <Box key={r.directionCode}>
                        <Stack
                          direction="row"
                          sx={{ justifyContent: "space-between", alignItems: "baseline" }}
                        >
                          <Typography variant="body2" sx={{ fontWeight: isRec ? 700 : 500 }}>
                            {r.directionName}
                            {isRec && (
                              <Chip size="small" label={t("common.recommendedTag")} color="primary" sx={{ ml: 1, height: 18 }} />
                            )}
                          </Typography>
                          <Typography variant="body2" color="text.secondary">
                            {(r.combinedScore * 100).toFixed(0)}%
                            {r.competitiveScore != null && ` · ${t("direction.nmtShort")} ${r.competitiveScore.toFixed(0)}`}
                            {` · ${t("direction.topicsShort")} `}
                            {r.topicFit.toFixed(1)}
                          </Typography>
                        </Stack>
                        <LinearProgress
                          variant="determinate"
                          value={Math.min(100, r.combinedScore * 100)}
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
          </Stack>
        )}
      </CardContent>
    </Card>
  );
}
