"use client";

import { Box, Container } from "@mui/material";
import AppHeader from "@/components/AppHeader";
import RiskMonitor from "@/components/monitor/RiskMonitor";

export default function RisksPage() {
  return (
    <Box sx={{ minHeight: "100vh", bgcolor: "background.default" }}>
      <AppHeader showHome />
      <Container maxWidth="lg" sx={{ py: 4 }}>
        <RiskMonitor />
      </Container>
    </Box>
  );
}
