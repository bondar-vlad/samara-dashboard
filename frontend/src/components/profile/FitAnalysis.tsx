"use client";

import {
  Box,
  Typography,
  Stack,
  Chip,
  Paper,
  LinearProgress,
  Tooltip,
} from "@mui/material";
import type { Recommendation, ProgramMatch } from "@/lib/types";
import { ORANGE } from "@/theme/colors";

/** A single subject/topic gap rendered as current -> target with a bar. */
function GapRow({
  name,
  area,
  current,
  target,
}: {
  name: string;
  area: string;
  current: number;
  target: number;
}) {
  const pct = target > 0 ? Math.min(100, (current / target) * 100) : 0;
  return (
    <Box sx={{ mb: 1.25 }}>
      <Stack direction="row" sx={{ justifyContent: "space-between", alignItems: "baseline" }}>
        <Typography variant="body2" sx={{ fontWeight: 600 }}>
          {name}{" "}
          <Typography component="span" variant="caption" color="text.secondary">
            ({area === "subject" ? "предмет" : "тема"})
          </Typography>
        </Typography>
        <Typography variant="body2" color="text.secondary">
          {current.toFixed(1)} → <strong>{target.toFixed(1)}</strong>
        </Typography>
      </Stack>
      <Tooltip title={`Прогрес до цільового рівня: ${pct.toFixed(0)}%`}>
        <LinearProgress
          variant="determinate"
          value={pct}
          sx={{
            height: 8,
            borderRadius: 4,
            mt: 0.5,
            "& .MuiLinearProgress-bar": { bgcolor: ORANGE },
            bgcolor: "#eef0f6",
          }}
        />
      </Tooltip>
    </Box>
  );
}

function MatchCard({ match }: { match: ProgramMatch }) {
  return (
    <Paper variant="outlined" sx={{ p: 2 }}>
      <Stack
        direction="row"
        sx={{ justifyContent: "space-between", alignItems: "center", mb: 1 }}
      >
        <Box>
          <Typography variant="subtitle2" sx={{ fontWeight: 700 }}>
            {match.programName}
          </Typography>
          <Typography variant="caption" color="text.secondary">
            {match.universityName} · {match.clusterName}
          </Typography>
        </Box>
        <Chip
          size="small"
          label={`Відповідність ${(match.fitScore * 100).toFixed(0)}%`}
          color={match.meetsThreshold ? "success" : "warning"}
          variant={match.meetsThreshold ? "filled" : "outlined"}
        />
      </Stack>

      {match.strengths.length > 0 && (
        <Box sx={{ mb: match.gaps.length ? 1.5 : 0 }}>
          <Typography variant="overline" color="text.secondary">
            Сильні сторони
          </Typography>
          <Stack direction="row" spacing={1} useFlexGap sx={{ mt: 0.5, flexWrap: "wrap" }}>
            {match.strengths.map((s) => (
              <Chip key={s} label={s} size="small" color="success" variant="outlined" />
            ))}
          </Stack>
        </Box>
      )}

      {match.gaps.length > 0 && (
        <Box>
          <Typography variant="overline" color="text.secondary">
            Що потрібно підтягнути
          </Typography>
          <Box sx={{ mt: 0.5 }}>
            {match.gaps.map((g) => (
              <GapRow
                key={`${g.area}-${g.name}`}
                name={g.name}
                area={g.area}
                current={g.current}
                target={g.target}
              />
            ))}
          </Box>
        </Box>
      )}

      {match.advice.length > 0 && (
        <Box sx={{ mt: 1.5 }}>
          <Typography variant="overline" color="text.secondary">
            Що каже система
          </Typography>
          <Stack component="ul" sx={{ m: 0, mt: 0.5, pl: 2.5 }}>
            {match.advice.map((a) => (
              <Typography key={a} component="li" variant="body2" color="text.secondary">
                {a}
              </Typography>
            ))}
          </Stack>
        </Box>
      )}
    </Paper>
  );
}

export default function FitAnalysis({
  recommendation,
  matches,
}: {
  recommendation?: Recommendation;
  matches: ProgramMatch[];
}) {
  // Best-fitting program (top of the ranked list) + a contrasting weaker one.
  const top = matches.slice(0, 3);

  return (
    <Box>
      {recommendation && (
        <Paper
          variant="outlined"
          sx={{ p: 2, mb: 2, bgcolor: "#fafafa", borderStyle: "dashed" }}
        >
          <Typography variant="overline" color="text.secondary">
            На основі оцінок і тем
          </Typography>
          <Typography variant="body2" sx={{ mt: 0.5, lineHeight: 1.6 }}>
            {recommendation.rationale}
          </Typography>
        </Paper>
      )}

      {top.length > 0 ? (
        <Stack spacing={1.5}>
          <Typography variant="subtitle2" sx={{ fontWeight: 700 }}>
            Аналіз відповідності спеціальностям
          </Typography>
          {top.map((m) => (
            <MatchCard key={m.programId} match={m} />
          ))}
        </Stack>
      ) : (
        <Typography variant="body2" color="text.secondary">
          Дані відповідності спеціальностям недоступні.
        </Typography>
      )}
    </Box>
  );
}
