"use client";

import { Box, Container } from "@mui/material";
import AppHeader from "@/components/AppHeader";
import RoleDashboard from "@/components/monitor/RoleDashboard";

export default function DashboardPage() {
  return (
    <Box sx={{ minHeight: "100vh", bgcolor: "background.default" }}>
      <AppHeader showHome />
      <Container maxWidth="lg" sx={{ py: 4 }}>
        <RoleDashboard />
      </Container>
    </Box>
  );
}
