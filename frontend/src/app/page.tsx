"use client";

import { AppBar, Toolbar, Typography, Container, Box } from "@mui/material";
import ProfileDashboard from "@/components/dashboard/ProfileDashboard";

export default function HomePage() {
  return (
    <Box sx={{ minHeight: "100vh", bgcolor: "background.default" }}>
      <AppBar position="static">
        <Toolbar>
          <Typography variant="h6" sx={{ flexGrow: 1, fontWeight: 700 }}>
            Samara Dashboard
          </Typography>
          <Typography variant="body2" sx={{ opacity: 0.8 }}>
            Моніторинг прав дитини
          </Typography>
        </Toolbar>
      </AppBar>

      <Container maxWidth="lg" sx={{ py: 4 }}>
        <ProfileDashboard />
      </Container>
    </Box>
  );
}
