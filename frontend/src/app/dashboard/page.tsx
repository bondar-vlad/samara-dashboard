"use client";

import { Box, Container } from "@mui/material";
import AppHeader from "@/components/AppHeader";
import ManagementDashboard from "@/components/monitor/ManagementDashboard";

export default function DashboardPage() {
  return (
    <Box sx={{ minHeight: "100vh", bgcolor: "background.default" }}>
      <AppHeader showHome />
      <Container maxWidth="lg" sx={{ py: 4 }}>
        <ManagementDashboard />
      </Container>
    </Box>
  );
}
