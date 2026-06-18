"use client";

import { Box, Container } from "@mui/material";
import AppHeader from "@/components/AppHeader";
import AccessGate from "@/components/access/AccessGate";
import RiskMonitor from "@/components/monitor/RiskMonitor";

export default function RisksPage() {
  return (
    <Box sx={{ minHeight: "100vh", bgcolor: "background.default" }}>
      <AppHeader showHome />
      <Container maxWidth="lg" sx={{ py: 4 }}>
        <AccessGate permission="view:risks">
          <RiskMonitor />
        </AccessGate>
      </Container>
    </Box>
  );
}
