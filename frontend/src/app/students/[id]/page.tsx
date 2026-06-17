"use client";

import { useParams } from "next/navigation";
import NextLink from "next/link";
import {
  AppBar,
  Toolbar,
  Typography,
  Container,
  Box,
  Button,
} from "@mui/material";
import ArrowBackIcon from "@mui/icons-material/ArrowBack";
import DirectionAnalysis from "@/components/admission/DirectionAnalysis";
import LanguageSwitcher from "@/components/LanguageSwitcher";
import { useTranslation } from "@/i18n/I18nProvider";

export default function StudentDirectionPage() {
  const params = useParams<{ id: string }>();
  const id = params?.id;
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
            {t("header.directionAnalysis")}
          </Typography>
          <LanguageSwitcher />
        </Toolbar>
      </AppBar>

      <Container maxWidth="lg" sx={{ py: 4 }}>
        {id && <DirectionAnalysis studentId={id} />}
      </Container>
    </Box>
  );
}
