"use client";

import NextLink from "next/link";
import { AppBar, Toolbar, Typography, Container, Box, Button, Stack } from "@mui/material";
import InsightsIcon from "@mui/icons-material/Insights";
import DirectionDashboard from "@/components/admission/DirectionDashboard";
import FourthSubjectDashboard from "@/components/admission/FourthSubjectDashboard";
import LanguageSwitcher from "@/components/LanguageSwitcher";
import { useTranslation } from "@/i18n/I18nProvider";

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
          {/* Widget 1: профіль (напрям) за НМТ */}
          <DirectionDashboard />
          {/* Widget 2: 4-й предмет НМТ */}
          <FourthSubjectDashboard />
        </Stack>
      </Container>
    </Box>
  );
}
