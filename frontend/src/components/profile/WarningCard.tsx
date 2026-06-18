"use client";

import { useState, type ReactNode } from "react";
import {
  Paper,
  Box,
  Stack,
  Typography,
  Chip,
  Button,
  Collapse,
  Divider,
} from "@mui/material";
import ExpandMoreIcon from "@mui/icons-material/ExpandMore";
import WarningAmberIcon from "@mui/icons-material/WarningAmber";
import type { RedFlag } from "@/lib/types";
import { useFlagAction } from "@/lib/hooks";
import { useTranslation } from "@/i18n/I18nProvider";
import { severityColor, severityHex, audienceLabel } from "./severity";

export default function WarningCard({
  flag,
  children,
  defaultExpanded = false,
}: {
  flag: RedFlag;
  /** Detailed analysis shown when expanded. */
  children?: ReactNode;
  defaultExpanded?: boolean;
}) {
  const [open, setOpen] = useState(defaultExpanded);
  const { t } = useTranslation();
  const flagAction = useFlagAction();
  const hex = severityHex(flag.severity);

  const statusLabel =
    flag.status === "Resolved"
      ? t("warningCard.statusResolved")
      : flag.status === "Acknowledged"
        ? t("warningCard.statusAcknowledged")
        : t("warningCard.statusOpen");
  const statusColor: "success" | "info" | "default" =
    flag.status === "Resolved" ? "success" : flag.status === "Acknowledged" ? "info" : "default";

  return (
    <Paper
      variant="outlined"
      sx={{ borderLeft: `4px solid ${hex}`, overflow: "hidden" }}
    >
      <Box sx={{ p: 2 }}>
        <Stack direction="row" spacing={1.5} sx={{ alignItems: "flex-start" }}>
          <WarningAmberIcon sx={{ color: hex, mt: 0.25 }} />
          <Box sx={{ flex: 1 }}>
            <Stack
              direction="row"
              spacing={1}
              useFlexGap
              sx={{ alignItems: "center", flexWrap: "wrap" }}
            >
              <Typography variant="subtitle1" sx={{ fontWeight: 700 }}>
                {flag.title}
              </Typography>
              <Chip
                size="small"
                label={flag.ruleCode}
                color={severityColor(flag.severity)}
                variant="outlined"
              />
              <Chip size="small" variant="outlined" color={statusColor} label={statusLabel} />
            </Stack>
            <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5 }}>
              {flag.description}
            </Typography>

            {flag.recommendedActions.length > 0 && (
              <Box sx={{ mt: 1.5 }}>
                <Typography variant="overline" color="text.secondary">
                  Рекомендовані дії
                </Typography>
                <Stack component="ul" sx={{ m: 0, pl: 2.5 }}>
                  {flag.recommendedActions.map((a) => (
                    <Typography key={a} component="li" variant="body2">
                      {a}
                    </Typography>
                  ))}
                </Stack>
              </Box>
            )}

            <Stack
              direction="row"
              spacing={1}
              useFlexGap
              sx={{ flexWrap: "wrap", mt: 1.5 }}
            >
              <Typography variant="caption" color="text.secondary" sx={{ mr: 0.5 }}>
                Для:
              </Typography>
              {flag.targetAudiences.map((a) => (
                <Chip key={a} size="small" label={audienceLabel(a)} />
              ))}
            </Stack>

            {flag.status !== "Resolved" && (
              <Stack direction="row" spacing={1} useFlexGap sx={{ flexWrap: "wrap", mt: 1.5 }}>
                {flag.status === "Open" && (
                  <Button
                    size="small"
                    variant="outlined"
                    disabled={flagAction.isPending}
                    onClick={() => flagAction.mutate({ id: flag.id, action: "acknowledge" })}
                  >
                    {t("warningCard.acknowledge")}
                  </Button>
                )}
                <Button
                  size="small"
                  variant="contained"
                  color="success"
                  disabled={flagAction.isPending}
                  onClick={() => flagAction.mutate({ id: flag.id, action: "resolve" })}
                >
                  {t("warningCard.resolve")}
                </Button>
              </Stack>
            )}
          </Box>
        </Stack>

        {children && (
          <Box sx={{ mt: 1.5 }}>
            <Button
              size="small"
              onClick={() => setOpen((v) => !v)}
              endIcon={
                <ExpandMoreIcon
                  sx={{
                    transform: open ? "rotate(180deg)" : "none",
                    transition: "transform 0.2s",
                  }}
                />
              }
            >
              {open ? "Згорнути аналіз" : "Детальний аналіз"}
            </Button>
          </Box>
        )}
      </Box>

      {children && (
        <Collapse in={open} unmountOnExit>
          <Divider />
          <Box sx={{ p: 2, bgcolor: "#00000003" }}>{children}</Box>
        </Collapse>
      )}
    </Paper>
  );
}
