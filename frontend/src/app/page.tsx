"use client";

import NextLink from "next/link";
import { Typography, Container, Box, Stack, Divider, Alert, Button } from "@mui/material";
import AppHeader from "@/components/AppHeader";
import AccessBanner from "@/components/access/AccessBanner";
import ProfileChoiceDashboard from "@/components/admission/ProfileChoiceDashboard";
import DirectionDashboard from "@/components/admission/DirectionDashboard";
import FourthSubjectDashboard from "@/components/admission/FourthSubjectDashboard";
import { useRole } from "@/access/RoleProvider";
import { useTranslation } from "@/i18n/I18nProvider";

function StepHeader({ step, title, subtitle }: { step: number; title: string; subtitle: string }) {
  return (
    <Stack direction="row" spacing={2} sx={{ alignItems: "flex-start" }}>
      <Box
        sx={{
          flexShrink: 0,
          width: 40,
          height: 40,
          borderRadius: "50%",
          bgcolor: "primary.main",
          color: "#fff",
          display: "flex",
          alignItems: "center",
          justifyContent: "center",
          fontWeight: 800,
          fontSize: 18,
          mt: 0.25,
        }}
      >
        {step}
      </Box>
      <Box>
        <Typography variant="h5" sx={{ fontWeight: 800 }}>
          {title}
        </Typography>
        <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5 }}>
          {subtitle}
        </Typography>
      </Box>
    </Stack>
  );
}

/** For non-specialist roles the home page never lists pupils — it explains the role and routes
 *  the user to the surface they are entitled to. */
function RoleHomeNotice() {
  const { t } = useTranslation();
  const { role, def } = useRole();

  const notice =
    def.view === "public"
      ? t("access.publicNotice")
      : def.view === "parent"
        ? role === "CHILD"
          ? t("access.childNotice")
          : t("access.parentNotice")
        : def.view === "interagency"
          ? t("access.interagencyNotice")
          : def.view === "admin"
            ? t("access.adminSubtitle")
            : t("access.restrictedNotice");

  const adminCta = def.view === "admin";

  return (
    <Alert
      severity="info"
      action={
        <Button
          color="inherit"
          size="small"
          component={NextLink}
          href={adminCta ? "/access" : "/dashboard"}
        >
          {t(adminCta ? "access.goToAccess" : "access.goToDashboard")}
        </Button>
      }
    >
      {notice}
    </Alert>
  );
}

export default function HomePage() {
  const { t } = useTranslation();
  const { def } = useRole();

  // The cross-pupil education journey lists pupils by name, so it is shown only to roles that may
  // see personal data across pupils (territorial specialists). Everyone else gets a role notice.
  const showJourney = def.view === "operational" || def.view === "analytics";

  return (
    <Box sx={{ minHeight: "100vh", bgcolor: "background.default" }}>
      <AppHeader />

      <Container maxWidth="lg" sx={{ py: 4 }}>
        <Stack spacing={5}>
          {/* Intro: the pupil's journey, in chronological order */}
          <Box>
            <Typography variant="h4" sx={{ fontWeight: 800 }}>
              {t("home.heroTitle")}
            </Typography>
            <Typography variant="body1" color="text.secondary" sx={{ mt: 1, maxWidth: 760 }}>
              {t("home.heroSubtitle")}
            </Typography>
          </Box>

          {/* Always show who is signed in and the active scope, so the view is unambiguous. */}
          <AccessBanner />

          {showJourney ? (
            <>
              {/* Step 1 — profile high school (10th grade) */}
              <StepHeader step={1} title={t("home.step1Title")} subtitle={t("home.step1Sub")} />
              <ProfileChoiceDashboard />

              <Divider />

              {/* Step 2 — NMT 4th subject (taken before admission) */}
              <StepHeader step={2} title={t("home.step2Title")} subtitle={t("home.step2Sub")} />
              <FourthSubjectDashboard />

              <Divider />

              {/* Step 3 — university direction (chosen with NMT results) */}
              <StepHeader step={3} title={t("home.step3Title")} subtitle={t("home.step3Sub")} />
              <DirectionDashboard />
            </>
          ) : (
            <RoleHomeNotice />
          )}
        </Stack>
      </Container>
    </Box>
  );
}
