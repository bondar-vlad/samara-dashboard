"use client";

import NextLink from "next/link";
import { AppBar, Toolbar, Typography, Container, Box, Button } from "@mui/material";
import InsightsIcon from "@mui/icons-material/Insights";
import ProfileDashboard from "@/components/dashboard/ProfileDashboard";

export default function HomePage() {
  return (
    <Box sx={{ minHeight: "100vh", bgcolor: "background.default" }}>
      <AppBar position="static">
        <Toolbar>
          <Typography variant="h6" sx={{ flexGrow: 1, fontWeight: 700 }}>
            Samara Dashboard
          </Typography>
          <Button
            component={NextLink}
            href="/demo"
            startIcon={<InsightsIcon />}
            sx={{ color: "#fff", mr: 2 }}
          >
            Демо: освіта
          </Button>
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
