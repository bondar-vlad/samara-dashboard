"use client";

import {
  Box,
  Paper,
  Typography,
  Stack,
  Chip,
  Divider,
} from "@mui/material";
import WarningAmberIcon from "@mui/icons-material/WarningAmber";
import CheckCircleOutlineIcon from "@mui/icons-material/CheckCircleOutlined";
import type { ProfileChoice } from "@/lib/types";
import { BLUE, ORANGE, YELLOW, GREEN } from "@/theme/colors";

function ProfileColumn({
  title,
  subtitle,
  clusterName,
  profiles,
  accent,
  highlight,
}: {
  title: string;
  subtitle: string;
  clusterName: string | null;
  profiles: { profileName: string }[];
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
      <Stack direction="row" spacing={1} sx={{ mb: 0.5, alignItems: "center" }}>
        <Box sx={{ width: 10, height: 10, borderRadius: "50%", bgcolor: accent }} />
        <Typography variant="subtitle1" sx={{ fontWeight: 700 }}>
          {title}
        </Typography>
      </Stack>
      <Typography variant="caption" color="text.secondary">
        {subtitle}
      </Typography>
      <Divider sx={{ my: 1.5 }} />
      <Typography variant="overline" color="text.secondary">
        Кластер
      </Typography>
      <Typography variant="body1" sx={{ fontWeight: 600, mb: 1.5 }}>
        {clusterName ?? "—"}
      </Typography>
      <Typography variant="overline" color="text.secondary">
        Профілі
      </Typography>
      <Stack direction="row" spacing={1} useFlexGap sx={{ mt: 0.5, flexWrap: "wrap" }}>
        {profiles.length ? (
          profiles.map((p) => (
            <Chip
              key={p.profileName}
              label={p.profileName}
              size="small"
              sx={{ borderColor: accent }}
              variant="outlined"
            />
          ))
        ) : (
          <Typography variant="body2" color="text.secondary">
            Немає даних
          </Typography>
        )}
      </Stack>
    </Paper>
  );
}

export default function ProfileColumns({ choice }: { choice: ProfileChoice }) {
  const mismatch = choice.hasMismatch;
  const hasRecommendation = !!choice.recommendedCluster;

  return (
    <Box>
      <Stack
        direction={{ xs: "column", md: "row" }}
        spacing={2}
        sx={{ alignItems: "stretch" }}
      >
        <ProfileColumn
          title="Власний вибір учня"
          subtitle="Бажані профілі, обрані учнем"
          clusterName={choice.desiredClusterName}
          profiles={choice.desiredProfiles}
          accent={ORANGE}
          highlight={mismatch}
        />
        <ProfileColumn
          title="Рекомендація системи"
          subtitle={
            choice.recommendationConfidence != null
              ? `Впевненість ${(choice.recommendationConfidence * 100).toFixed(0)}%`
              : "Аналіз ще не виконано"
          }
          clusterName={choice.recommendedClusterName}
          profiles={choice.recommendedProfiles}
          accent={BLUE}
          highlight={mismatch}
        />
      </Stack>

      {hasRecommendation && (
        <Stack
          direction="row"
          spacing={1}
          sx={{
            alignItems: "center",
            mt: 2,
            p: 1.5,
            borderRadius: 2,
            bgcolor: mismatch ? `${YELLOW}14` : `${GREEN}14`,
            border: 1,
            borderColor: mismatch ? YELLOW : GREEN,
          }}
        >
          {mismatch ? (
            <WarningAmberIcon sx={{ color: YELLOW }} />
          ) : (
            <CheckCircleOutlineIcon sx={{ color: GREEN }} />
          )}
          <Typography variant="body2">
            {mismatch
              ? "Виявлено розбіжність між вибором учня та рекомендацією системи. Перегляньте маркери нижче, щоб обговорити це з учнем і батьками."
              : "Вибір учня збігається з рекомендацією системи."}
          </Typography>
        </Stack>
      )}
    </Box>
  );
}
