"use client";

import { useMemo, useState, useEffect } from "react";
import {
  Box,
  Card,
  CardContent,
  CardHeader,
  Typography,
  Stack,
  Button,
  Chip,
  CircularProgress,
  Alert,
  Divider,
} from "@mui/material";
import PlayArrowIcon from "@mui/icons-material/PlayArrow";
import BlackSelect, { type Option } from "@/components/BlackSelect";
import type { RedFlag } from "@/lib/types";
import FitAnalysis from "./FitAnalysis";
import WarningCard from "./WarningCard";
import ProfileColumns from "./ProfileColumns";
import {
  useStudents,
  useStudent,
  useStudentRedFlags,
  useStudentRecommendations,
  useUniversityFit,
  useRunAnalysis,
} from "@/lib/hooks";

const PROFILE_MISMATCH = "EDU-PROFILE-MISMATCH";

export default function ProfileComparison({
  initialStudentId,
}: {
  initialStudentId?: string;
}) {
  const students = useStudents();
  const [studentId, setStudentId] = useState<string | null>(
    initialStudentId ?? null,
  );

  // If no student is selected yet, default to the one passed in (e.g. from the
  // dashboard) or the first student with a detected mismatch, else the first.
  useEffect(() => {
    if (studentId) return;
    if (initialStudentId) {
      setStudentId(initialStudentId);
    } else if (students.data?.length) {
      const mismatch = students.data.find((s) => s.hasProfileMismatch);
      setStudentId((mismatch ?? students.data[0]).id);
    }
  }, [students.data, studentId, initialStudentId]);

  const student = useStudent(studentId);
  const flags = useStudentRedFlags(studentId);
  const recs = useStudentRecommendations(studentId);
  const fit = useUniversityFit(studentId);
  const runAnalysis = useRunAnalysis();

  const options: Option[] = useMemo(
    () =>
      (students.data ?? []).map((s) => ({
        value: s.id,
        label: `${s.hasProfileMismatch ? "⚠ " : ""}${s.fullName} · ${s.className}`,
      })),
    [students.data],
  );

  const selectedOption = options.find((o) => o.value === studentId) ?? null;

  // Each analysis run appends new flags, so the same student can accumulate
  // duplicate flags with the same ruleCode. Keep only the most recent per code.
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

  const profileRec = recs.data?.find((r) => r.kind === "ProfileChange") ?? recs.data?.[0];
  const choice = student.data?.profileChoice;
  const hasRecommendation = !!choice?.recommendedCluster;

  return (
    <Card variant="outlined">
      <CardHeader
        title="Порівняння профілів: вибір учня та рекомендація системи"
        subheader="Інструмент для адміністрації та батьків — зважити маркери й обговорити кращий профіль для учня"
        action={
          studentId ? (
            <Button
              variant="contained"
              color="primary"
              startIcon={
                runAnalysis.isPending ? (
                  <CircularProgress size={16} color="inherit" />
                ) : (
                  <PlayArrowIcon />
                )
              }
              disabled={runAnalysis.isPending}
              onClick={() => runAnalysis.mutate(studentId)}
            >
              {hasRecommendation ? "Перерахувати" : "Запустити аналіз"}
            </Button>
          ) : null
        }
      />
      <CardContent>
        {/* Student picker */}
        <Box sx={{ maxWidth: 420, mb: 3 }}>
          <Typography variant="body2" sx={{ mb: 0.5 }}>
            Учень
          </Typography>
          {students.isLoading ? (
            <CircularProgress size={20} />
          ) : students.isError ? (
            <Alert severity="error">Не вдалося завантажити список учнів.</Alert>
          ) : (
            <BlackSelect
              options={options}
              value={selectedOption}
              onChange={(v) => setStudentId((v as Option | null)?.value ?? null)}
              placeholder="Оберіть учня…"
            />
          )}
        </Box>

        {/* Comparison + warnings */}
        {!studentId ? null : student.isLoading ? (
          <Box sx={{ display: "flex", justifyContent: "center", py: 6 }}>
            <CircularProgress />
          </Box>
        ) : student.isError || !student.data || !choice ? (
          <Alert severity="error">Не вдалося завантажити дані учня.</Alert>
        ) : (
          <Stack spacing={3}>
            <Stack direction="row" spacing={1} useFlexGap sx={{ alignItems: "center", flexWrap: "wrap" }}>
              <Typography variant="h6" sx={{ fontWeight: 700 }}>
                {student.data.fullName}
              </Typography>
              <Chip size="small" label={`${student.data.gradeLevel} клас`} />
              <Chip size="small" label={student.data.className} variant="outlined" />
            </Stack>

            <ProfileColumns choice={choice} />

            {!hasRecommendation && (
              <Alert severity="info">
                Для цього учня ще не виконано аналіз. Натисніть «Запустити аналіз»,
                щоб система сформувала рекомендацію та маркери.
              </Alert>
            )}

            {/* Warnings */}
            {hasRecommendation && (
              <Box>
                <Divider sx={{ mb: 2 }}>
                  <Typography variant="overline" color="text.secondary">
                    Маркери, виявлені системою
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
                        defaultExpanded={flag.ruleCode === PROFILE_MISMATCH}
                      >
                        {flag.ruleCode === PROFILE_MISMATCH ? (
                          <FitAnalysis
                            recommendation={profileRec}
                            matches={fit.data?.matches ?? []}
                          />
                        ) : undefined}
                      </WarningCard>
                    ))}
                  </Stack>
                ) : (
                  <Alert severity="success">
                    Маркерів не виявлено — вибір учня узгоджений із рекомендацією.
                  </Alert>
                )}
              </Box>
            )}
          </Stack>
        )}
      </CardContent>
    </Card>
  );
}
