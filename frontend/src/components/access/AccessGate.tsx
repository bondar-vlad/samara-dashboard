"use client";

import type { ReactNode } from "react";
import NextLink from "next/link";
import { Stack, Alert, AlertTitle, Button } from "@mui/material";
import LockIcon from "@mui/icons-material/Lock";
import { useRole } from "@/access/RoleProvider";
import type { Permission } from "@/access/roles";
import AccessBanner from "./AccessBanner";
import { useTranslation } from "@/i18n/I18nProvider";

interface AccessGateProps {
  /** Allowed when the active role has this permission… */
  permission?: Permission;
  /** …or ANY of these permissions. */
  anyOf?: Permission[];
  children: ReactNode;
}

/**
 * Gates personal-data / privileged content by the active role's permissions. When denied it
 * renders the access banner (so the user understands who they are signed in as) plus a clear
 * "not available for this role" notice with a route back to a permitted surface.
 */
export default function AccessGate({ permission, anyOf, children }: AccessGateProps) {
  const { t } = useTranslation();
  const { can } = useRole();

  const checks: boolean[] = [];
  if (permission) checks.push(can(permission));
  if (anyOf) checks.push(anyOf.some((p) => can(p)));
  const allowed = checks.length === 0 ? true : checks.some(Boolean);

  if (allowed) return <>{children}</>;

  return (
    <Stack spacing={3}>
      <AccessBanner />
      <Alert
        severity="warning"
        icon={<LockIcon />}
        action={
          <Button color="inherit" size="small" component={NextLink} href="/dashboard">
            {t("access.goToDashboard")}
          </Button>
        }
      >
        <AlertTitle>{t("access.restrictedTitle")}</AlertTitle>
        {t("access.restrictedNotice")}
      </Alert>
    </Stack>
  );
}
