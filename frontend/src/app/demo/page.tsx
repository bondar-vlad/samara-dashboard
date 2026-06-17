"use client";

import NextLink from "next/link";
import { AppBar, Toolbar, Typography, Container, Box, Button } from "@mui/material";
import ArrowBackIcon from "@mui/icons-material/ArrowBack";
import EducationDemo from "@/components/demo/EducationDemo";

const DemoPage = () => (
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
          Демо: освіта та ринок праці
        </Typography>
        <Typography variant="body2" sx={{ opacity: 0.8 }}>
          Дані з відкритих джерел
        </Typography>
      </Toolbar>
    </AppBar>

    <Container maxWidth="lg" sx={{ py: 4 }}>
      <EducationDemo />
    </Container>
  </Box>
);

export default DemoPage;
