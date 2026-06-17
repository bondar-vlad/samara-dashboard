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
import FourthSubjectAnalysis from "@/components/admission/FourthSubjectAnalysis";

export default function StudentFourthSubjectPage() {
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
            Аналіз четвертого предмета НМТ
          </Typography>
        </Toolbar>
      </AppBar>

      <Container maxWidth="lg" sx={{ py: 4 }}>
        {id && <FourthSubjectAnalysis studentId={id} />}
      </Container>
    </Box>
  );
}
