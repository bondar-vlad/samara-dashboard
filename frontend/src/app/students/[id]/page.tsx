"use client";

import { useParams } from "next/navigation";
import { Box, Container, Stack, Typography } from "@mui/material";
import AppHeader from "@/components/AppHeader";
import AccessGate from "@/components/access/AccessGate";
import DirectionAnalysis from "@/components/admission/DirectionAnalysis";
import AnalysisStatusBar from "@/components/AnalysisStatusBar";
import { useTranslation } from "@/i18n/I18nProvider";

export default function StudentDirectionPage() {
  const params = useParams<{ id: string }>();
  const id = params?.id;
  const { t } = useTranslation();

  return (
    <Box sx={{ minHeight: "100vh", bgcolor: "background.default" }}>
      <AppHeader showHome />
      <Container maxWidth="lg" sx={{ py: 4 }}>
        <AccessGate permission="view:childProfile">
          <Stack spacing={2}>
            <Typography variant="h5" sx={{ fontWeight: 800 }}>
              {t("header.directionAnalysis")}
            </Typography>
            {id && <AnalysisStatusBar studentId={id} />}
            {id && <DirectionAnalysis studentId={id} />}
          </Stack>
        </AccessGate>
      </Container>
    </Box>
  );
}
