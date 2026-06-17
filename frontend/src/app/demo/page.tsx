"use client";

import NextLink from "next/link";
import { AppBar, Toolbar, Typography, Container, Box, Button } from "@mui/material";
import ArrowBackIcon from "@mui/icons-material/ArrowBack";
import EducationDemo from "@/components/demo/EducationDemo";
import LanguageSwitcher from "@/components/LanguageSwitcher";
import { useTranslation } from "@/i18n/I18nProvider";

const DemoPage = () => {
  const { t } = useTranslation();
  return (
    <Box sx={{ minHeight: "100vh", bgcolor: "background.default" }}>
      <AppBar position="static">
        <Toolbar>
          <Button
            component={NextLink}
            href="/"
            startIcon={<ArrowBackIcon />}
            sx={{ color: "#fff", mr: 2 }}
          >
            {t("header.backHome")}
          </Button>
          <Typography variant="h6" sx={{ flexGrow: 1, fontWeight: 700 }}>
            {t("header.demoTitle")}
          </Typography>
          <LanguageSwitcher />
          <Typography variant="body2" sx={{ opacity: 0.8 }}>
            {t("header.demoSource")}
          </Typography>
        </Toolbar>
      </AppBar>

      <Container maxWidth="lg" sx={{ py: 4 }}>
        <EducationDemo />
      </Container>
    </Box>
  );
};

export default DemoPage;
