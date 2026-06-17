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
import ProfileComparison from "@/components/profile/ProfileComparison";

export default function StudentAnalysisPage() {
  const params = useParams<{ id: string }>();
  const id = params?.id;

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
            На головну
          </Button>
          <Typography variant="h6" sx={{ flexGrow: 1, fontWeight: 700 }}>
            Аналіз профілю учня
          </Typography>
        </Toolbar>
      </AppBar>

      <Container maxWidth="lg" sx={{ py: 4 }}>
        <ProfileComparison initialStudentId={id} />
      </Container>
    </Box>
  );
}
