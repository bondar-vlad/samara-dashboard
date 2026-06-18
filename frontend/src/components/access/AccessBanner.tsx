"use client";

import { Card, CardContent, Stack, Box, Typography, Chip } from "@mui/material";
import VisibilityIcon from "@mui/icons-material/Visibility";
import LockIcon from "@mui/icons-material/Lock";
import PlaceIcon from "@mui/icons-material/Place";
import { useRole } from "@/access/RoleProvider";
import { useScopeData } from "@/access/useScopeData";
import { BLUE } from "@/theme/colors";
import { useTranslation } from "@/i18n/I18nProvider";

/** Header card summarising who is signed in, what they may see and the scope. */
export default function AccessBanner() {
  const { t } = useTranslation();
  const { role, def, scope } = useRole();
  const data = useScopeData();

  const scopeSummary = (): string => {
    if (def.view === "public") return t("access.scopePublic");
    if (def.view === "parent") return t("access.scopeChild");
    if (def.view === "interagency") {
      return `${t("scope.agency")}: ${scope.agency ?? "—"}`;
    }
    const region = data.effective.region ?? t("scope.allRegions");
    const community = data.effective.community ?? t("scope.allCommunities");
    const schoolName = data.effective.schoolId
      ? data.schoolById.get(data.effective.schoolId)?.name ?? "—"
      : t("scope.allSchools");
    return [region, community, schoolName].join(" › ");
  };

  return (
    <Card variant="outlined" sx={{ borderLeft: `4px solid ${BLUE}` }}>
      <CardContent>
        <Stack
          direction={{ xs: "column", sm: "row" }}
          spacing={1.5}
          sx={{ alignItems: { sm: "center" }, flexWrap: "wrap" }}
          useFlexGap
        >
          <Chip label={def.badge} color="primary" sx={{ fontWeight: 800 }} />
          <Box sx={{ flexGrow: 1, minWidth: 200 }}>
            <Typography variant="subtitle1" sx={{ fontWeight: 800, lineHeight: 1.2 }}>
              {t(`roles.list.${role}.name`)}
            </Typography>
            <Typography variant="caption" color="text.secondary">
              {t(`roles.categories.${def.category}`)}
            </Typography>
          </Box>
          <Chip
            size="small"
            variant="outlined"
            color="primary"
            label={t(`roles.list.${role}.level`)}
          />
          <Chip
            size="small"
            icon={<PlaceIcon />}
            variant="outlined"
            label={scopeSummary()}
            sx={{ maxWidth: "100%" }}
          />
        </Stack>

        <Stack spacing={0.75} sx={{ mt: 1.5 }}>
          <Stack direction="row" spacing={1} sx={{ alignItems: "flex-start" }}>
            <VisibilityIcon fontSize="small" color="action" sx={{ mt: 0.25 }} />
            <Typography variant="body2" color="text.secondary">
              {t(`roles.list.${role}.sees`)}
            </Typography>
          </Stack>
          <Stack direction="row" spacing={1} sx={{ alignItems: "flex-start" }}>
            <LockIcon fontSize="small" color="action" sx={{ mt: 0.25 }} />
            <Typography variant="body2" color="text.secondary">
              {t(`roles.list.${role}.restriction`)}
            </Typography>
          </Stack>
        </Stack>
      </CardContent>
    </Card>
  );
}
