"use client";

import NextLink from "next/link";
import { AppBar, Toolbar, Typography, Container, Box, Button, Stack } from "@mui/material";
import InsightsIcon from "@mui/icons-material/Insights";
import DirectionDashboard from "@/components/admission/DirectionDashboard";
import FourthSubjectDashboard from "@/components/admission/FourthSubjectDashboard";

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
