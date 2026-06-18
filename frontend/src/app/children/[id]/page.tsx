"use client";

import { useParams } from "next/navigation";
import { Box, Container } from "@mui/material";
import AppHeader from "@/components/AppHeader";
import ChildProfile from "@/components/monitor/ChildProfile";

export default function ChildProfilePage() {
  const params = useParams<{ id: string }>();
  const id = params?.id;

  return (
    <Box sx={{ minHeight: "100vh", bgcolor: "background.default" }}>
      <AppHeader showHome />
      <Container maxWidth="lg" sx={{ py: 4 }}>
        {id && <ChildProfile studentId={id} />}
      </Container>
    </Box>
  );
}
