"use client";

import NextLink from "next/link";
import { AppBar, Toolbar, Typography, Container, Box, Button, Stack, Divider } from "@mui/material";
import InsightsIcon from "@mui/icons-material/Insights";
import ProfileChoiceDashboard from "@/components/admission/ProfileChoiceDashboard";
import DirectionDashboard from "@/components/admission/DirectionDashboard";
import FourthSubjectDashboard from "@/components/admission/FourthSubjectDashboard";
import LanguageSwitcher from "@/components/LanguageSwitcher";
import { useTranslation } from "@/i18n/I18nProvider";

function SectionHeader({ title, subtitle }: { title: string; subtitle: string }) {
  return (
    <Box>
      <Typography variant="h5" sx={{ fontWeight: 800 }}>
        {title}
      </Typography>
      <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5 }}>
        {subtitle}
      </Typography>
    </Box>
  );
}

export default function HomePage() {
  const { t } = useTranslation();
  return (
    <Box sx={{ minHeight: "100vh", bgcolor: "background.default" }}>
      <AppBar position="static">
        <Toolbar>
          <Typography variant="h6" sx={{ flexGrow: 1, fontWeight: 700 }}>
            {t("brand")}
          </Typography>
          <Button
            component={NextLink}
            href="/demo"
            startIcon={<InsightsIcon />}
            sx={{ color: "#fff", mr: 2 }}
          >
            {t("header.demoLink")}
          </Button>
          <LanguageSwitcher />
          <Typography variant="body2" sx={{ opacity: 0.8 }}>
            {t("header.monitoring")}
          </Typography>
        </Toolbar>
      </AppBar>

      <Container maxWidth="lg" sx={{ py: 4 }}>
        <Stack spacing={5}>
          {/* Рішення 1 — 10 клас: вибір профілю навчання (профільна школа) */}
          <SectionHeader title={t("home.section10Title")} subtitle={t("home.section10Sub")} />
          <ProfileChoiceDashboard />

          <Divider />

          {/* Рішення 2 — випуск 11 класу: вступ до ВНЗ за НМТ */}
          <SectionHeader title={t("home.section11Title")} subtitle={t("home.section11Sub")} />
          <DirectionDashboard />
          <FourthSubjectDashboard />
        </Stack>
      </Container>
    </Box>
  );
}
