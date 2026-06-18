"use client";

import { Box, Container } from "@mui/material";
import AppHeader from "@/components/AppHeader";
import EducationDemo from "@/components/demo/EducationDemo";

const DemoPage = () => (
  <Box sx={{ minHeight: "100vh", bgcolor: "background.default" }}>
    <AppHeader showHome />
    <Container maxWidth="lg" sx={{ py: 4 }}>
      <EducationDemo />
    </Container>
  </Box>
);

export default DemoPage;
