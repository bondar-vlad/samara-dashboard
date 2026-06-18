"use client";

import { useState } from "react";
import {
  Paper,
  Stack,
  Box,
  Typography,
  Button,
  Alert,
  Chip,
  LinearProgress,
  CircularProgress,
} from "@mui/material";
import AutoAwesomeIcon from "@mui/icons-material/AutoAwesome";
import RefreshIcon from "@mui/icons-material/Refresh";
import { useStudentImprovementPlan } from "@/lib/hooks";
import { ORANGE, BLUE, GREEN } from "@/theme/colors";
import { useTranslation } from "@/i18n/I18nProvider";

/**
 * AI "what to pull up" tool. When a pupil keeps a choice the data flags as a mismatch, this
 * fetches an OpenAI-generated plan of which subjects/topics to improve toward that chosen goal.
 * The plan is AI-only: if no model is connected the API returns `available:false` and we show a
 * clear "AI model not connected" notice instead of inventing advice.
 */
export default function ImprovementPlan({ studentId }: { studentId: string }) {
  const { t } = useTranslation();
  const [enabled, setEnabled] = useState(false);
  const { data, isLoading, isError, refetch, isFetching } = useStudentImprovementPlan(
    studentId,
    enabled,
  );

  const notConnected = isError || (!!data && !data.available);
  const showPlan = !!data && data.available && data.hasChoice;

  return (
    <Paper variant="outlined" sx={{ p: 2, borderStyle: "dashed", borderColor: ORANGE }}>
      <Stack spacing={1.5}>
        <Stack direction="row" spacing={1} sx={{ alignItems: "center" }}>
          <AutoAwesomeIcon sx={{ color: ORANGE }} />
          <Typography variant="subtitle2" sx={{ fontWeight: 700 }}>
            {t("improvement.title")}
          </Typography>
        </Stack>
        <Typography variant="body2" color="text.secondary">
          {t("improvement.subtitle")}
        </Typography>

        {!enabled && (
          <Button
            variant="contained"
            startIcon={<AutoAwesomeIcon />}
            onClick={() => setEnabled(true)}
            sx={{ alignSelf: "flex-start" }}
          >
            {t("improvement.generate")}
          </Button>
        )}

        {enabled && (isLoading || isFetching) && (
          <Stack direction="row" spacing={1} sx={{ alignItems: "center" }}>
            <CircularProgress size={20} />
            <Typography variant="body2" color="text.secondary">
              {t("improvement.loading")}
            </Typography>
          </Stack>
        )}

        {enabled && !isFetching && notConnected && (
          <Alert severity="info">{t("improvement.aiNotConnected")}</Alert>
        )}

        {enabled && !isFetching && data && data.available && !data.hasChoice && (
          <Alert severity="info">{t("improvement.noChoice")}</Alert>
        )}

        {enabled && !isFetching && showPlan && (
          <Stack spacing={2}>
            {data.summary && <Typography variant="body2">{data.summary}</Typography>}

            <Box>
              <Typography variant="overline" color="text.secondary">
                {t("improvement.itemsTitle", { target: data.targetName })}
              </Typography>
              <Stack spacing={1.5} sx={{ mt: 0.5 }}>
                {data.items.map((item) => {
                  const pct = Math.min(100, (item.current / item.target) * 100);
                  return (
                    <Box key={`${item.area}:${item.name}`}>
                      <Stack
                        direction="row"
                        sx={{ justifyContent: "space-between", alignItems: "baseline" }}
                      >
                        <Typography variant="body2" sx={{ fontWeight: 600 }}>
                          {item.area === "topic" && (
                            <Typography component="span" variant="caption" color="text.secondary">
                              {t("improvement.topic")}:{" "}
                            </Typography>
                          )}
                          {item.name}
                        </Typography>
                        <Typography variant="caption" color="text.secondary">
                          {item.current.toFixed(1)} → {item.target.toFixed(0)}
                          <Chip
                            size="small"
                            label={`${t("improvement.gap")} ${item.gap.toFixed(1)}`}
                            sx={{ ml: 1, height: 18 }}
                          />
                        </Typography>
                      </Stack>
                      <LinearProgress
                        variant="determinate"
                        value={pct}
                        sx={{
                          height: 8,
                          borderRadius: 4,
                          mt: 0.5,
                          bgcolor: "#eef0f6",
                          "& .MuiLinearProgress-bar": { bgcolor: pct >= 83 ? GREEN : ORANGE },
                        }}
                      />
                      {item.advice && (
                        <Typography variant="body2" sx={{ mt: 0.5 }}>
                          {item.advice}
                        </Typography>
                      )}
                    </Box>
                  );
                })}
              </Stack>
            </Box>

            {data.steps.length > 0 && (
              <Box>
                <Typography variant="overline" color="text.secondary">
                  {t("improvement.stepsTitle")}
                </Typography>
                <Stack spacing={0.5} sx={{ mt: 0.5 }}>
                  {data.steps.map((step, i) => (
                    <Stack key={i} direction="row" spacing={1} sx={{ alignItems: "flex-start" }}>
                      <Box sx={{ color: BLUE, fontWeight: 800, lineHeight: 1.5 }}>•</Box>
                      <Typography variant="body2">{step}</Typography>
                    </Stack>
                  ))}
                </Stack>
              </Box>
            )}

            <Stack direction="row" spacing={1} sx={{ alignItems: "center", justifyContent: "space-between" }}>
              <Typography variant="caption" color="text.secondary">
                {t("improvement.modelTag", { model: data.modelName })}
              </Typography>
              <Button size="small" startIcon={<RefreshIcon />} onClick={() => refetch()}>
                {t("improvement.regenerate")}
              </Button>
            </Stack>
          </Stack>
        )}
      </Stack>
    </Paper>
  );
}
