"use client";

import { useParams } from "next/navigation";
import { Box, Container } from "@mui/material";
import AppHeader from "@/components/AppHeader";
import AccessGate from "@/components/access/AccessGate";
import ChildProfile from "@/components/monitor/ChildProfile";

export default function ChildProfilePage() {
  const params = useParams<{ id: string }>();
  const id = params?.id;

  return (
    <Box sx={{ minHeight: "100vh", bgcolor: "background.default" }}>
      <AppHeader showHome />
      <Container maxWidth="lg" sx={{ py: 4 }}>
        <AccessGate permission="view:childProfile">
          {id && <ChildProfile studentId={id} />}
        </AccessGate>
      </Container>
    </Box>
  );
}
