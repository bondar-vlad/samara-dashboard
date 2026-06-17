"use client";

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
import { useStudentFourthSubject } from "@/lib/hooks";
import { BLUE, ORANGE, YELLOW, GREEN } from "@/theme/colors";

function ChoiceBox({
  title,
  subtitle,
  subject,
  accent,
  highlight,
}: {
  title: string;
  subtitle: string;
  subject: string | null;
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
        {subject ?? "—"}
      </Typography>
    </Paper>
  );
}

export default function FourthSubjectAnalysis({ studentId }: { studentId: string }) {
  const { data, isLoading, isError } = useStudentFourthSubject(studentId);

  const maxScore = 12;
  const mismatch = !!data?.hasChoice && !data?.isMatch;

  return (
    <Card variant="outlined">
      <CardHeader
        title="Четвертий предмет НМТ"
        subheader="Предмет за вибором учня проти рекомендації системи на основі оцінок"
      />
      <CardContent>
        {isLoading ? (
          <Box sx={{ display: "flex", justifyContent: "center", py: 4 }}>
            <CircularProgress />
          </Box>
        ) : isError || !data ? (
          <Alert severity="error">Не вдалося завантажити дані про четвертий предмет.</Alert>
        ) : (
          <Stack spacing={2}>
            <Stack direction={{ xs: "column", sm: "row" }} spacing={2} sx={{ alignItems: "stretch" }}>
              <ChoiceBox
                title="Вибір учня"
                subtitle={data.hasChoice ? "Обраний предмет" : "Ще не обрано"}
                subject={data.chosenSubjectName}
                accent={ORANGE}
                highlight={mismatch}
              />
              <ChoiceBox
                title="Рекомендація системи"
                subtitle="За результатами навчання"
                subject={data.recommendedSubjectName}
                accent={BLUE}
                highlight={mismatch}
              />
            </Stack>

            {/* status banner */}
            {data.recommendedSubject && (
              <Stack
                direction="row"
                spacing={1}
                sx={{
                  alignItems: "center",
                  p: 1.5,
                  borderRadius: 2,
                  border: 1,
                  bgcolor: !data.hasChoice
                    ? "transparent"
                    : mismatch
                      ? `${YELLOW}14`
                      : `${GREEN}14`,
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
                    ? "Учень ще не обрав четвертий предмет. Рекомендація системи може допомогти у виборі."
                    : mismatch
                      ? "Вибір учня не збігається з рекомендацією системи — варто обговорити."
                      : "Вибір учня збігається з рекомендацією системи."}
                </Typography>
              </Stack>
            )}

            {/* rationale */}
            {data.rationale && (
              <Paper variant="outlined" sx={{ p: 2, bgcolor: "#fafafa", borderStyle: "dashed" }}>
                <Typography variant="overline" color="text.secondary">
                  На основі оцінок
                </Typography>
                <Typography variant="body2" sx={{ mt: 0.5 }}>
                  {data.rationale}
                </Typography>
              </Paper>
            )}

            {/* ranked subjects */}
            {data.ranked.length > 0 && (
              <Box>
                <Divider sx={{ mb: 1.5 }}>
                  <Typography variant="overline" color="text.secondary">
                    Рейтинг предметів на вибір
                  </Typography>
                </Divider>
                <Stack spacing={1.25}>
                  {data.ranked.map((r) => {
                    const isRec = r.subject === data.recommendedSubject;
                    return (
                      <Box key={r.subject}>
                        <Stack
                          direction="row"
                          sx={{ justifyContent: "space-between", alignItems: "baseline" }}
                        >
                          <Typography variant="body2" sx={{ fontWeight: isRec ? 700 : 500 }}>
                            {r.subjectName}
                            {isRec && (
                              <Chip
                                size="small"
                                label="рекомендовано"
                                color="primary"
                                sx={{ ml: 1, height: 18 }}
                              />
                            )}
                          </Typography>
                          <Typography variant="body2" color="text.secondary">
                            {r.score.toFixed(1)} ·{" "}
                            <Typography component="span" variant="caption" color="text.secondary">
                              {r.evidenceCount} оцінок
                            </Typography>
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
          </Stack>
        )}
      </CardContent>
    </Card>
  );
}
