"use client";

import NextLink from "next/link";
import { AppBar, Toolbar, Typography, Button, Stack } from "@mui/material";
import SpaceDashboardIcon from "@mui/icons-material/SpaceDashboard";
import WarningAmberIcon from "@mui/icons-material/WarningAmber";
import InsightsIcon from "@mui/icons-material/Insights";
import HomeIcon from "@mui/icons-material/Home";
import LanguageSwitcher from "@/components/LanguageSwitcher";
import { useTranslation } from "@/i18n/I18nProvider";

/** Shared top navigation used across the dashboard surfaces. */
export default function AppHeader({ showHome = false }: { showHome?: boolean }) {
  const { t } = useTranslation();
  return (
    <AppBar position="static">
      <Toolbar sx={{ gap: 1, flexWrap: "wrap" }}>
        <Typography
          component={NextLink}
          href="/"
          variant="h6"
          sx={{ fontWeight: 700, color: "#fff", textDecoration: "none", mr: 2 }}
        >
          {t("brand")}
        </Typography>
        <Stack direction="row" spacing={0.5} sx={{ flexGrow: 1, flexWrap: "wrap" }} useFlexGap>
          {showHome && (
            <Button component={NextLink} href="/" startIcon={<HomeIcon />} sx={{ color: "#fff" }}>
              {t("nav.home")}
            </Button>
          )}
          <Button component={NextLink} href="/dashboard" startIcon={<SpaceDashboardIcon />} sx={{ color: "#fff" }}>
            {t("nav.dashboard")}
          </Button>
          <Button component={NextLink} href="/risks" startIcon={<WarningAmberIcon />} sx={{ color: "#fff" }}>
            {t("nav.risks")}
          </Button>
          <Button component={NextLink} href="/demo" startIcon={<InsightsIcon />} sx={{ color: "#fff" }}>
            {t("header.demoLink")}
          </Button>
        </Stack>
        <LanguageSwitcher />
      </Toolbar>
    </AppBar>
  );
}
