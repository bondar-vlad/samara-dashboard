"use client";

import { Box, Stack, Typography } from "@mui/material";
import BlackSelect, { type Option } from "@/components/BlackSelect";
import { useRole } from "@/access/RoleProvider";
import { useScopeData } from "@/access/useScopeData";
import type { AgencyCode, SelectorKind } from "@/access/roles";
import { useTranslation } from "@/i18n/I18nProvider";

const ALL = "__all__";

function Field({ label, children }: { label: string; children: React.ReactNode }) {
  return (
    <Box sx={{ minWidth: 210, flex: "1 1 210px", maxWidth: 280 }}>
      <Typography
        variant="caption"
        sx={{ color: "rgba(255,255,255,0.85)", fontWeight: 600, display: "block", mb: 0.5 }}
      >
        {label}
      </Typography>
      {children}
    </Box>
  );
}

/**
 * Cascading territory pickers shown for the active role. A community specialist
 * (Ф-Г / Ф-М) only gets a school dropdown; an oblast analyst (Ф-О) gets
 * community → school; national roles get region → community → school; the
 * interagency role gets an agency vertical picker. Pinned levels are locked.
 */
export default function ScopeSelectors() {
  const { t } = useTranslation();
  const { def, scope, setRegion, setCommunity, setSchool, setAgency } = useRole();
  const data = useScopeData();

  if (def.selectors.length === 0) return null;

  const find = (opts: Option[], value: string | null): Option | null =>
    value ? opts.find((o) => o.value === value) ?? null : null;

  const withAll = (opts: Option[], allLabel: string): Option[] => [
    { value: ALL, label: allLabel },
    ...opts,
  ];

  const render = (kind: SelectorKind) => {
    switch (kind) {
      case "region": {
        const pinned = !!def.pin.region;
        const opts = pinned ? data.regions : withAll(data.regions, t("scope.allRegions"));
        return (
          <Field key={kind} label={t("scope.region")}>
            <BlackSelect
              isDisabled={pinned}
              options={opts}
              value={find(data.regions, scope.region) ?? (pinned ? data.regions[0] : opts[0])}
              onChange={(o) => setRegion(!o || o.value === ALL ? null : o.value)}
              isSearchable={false}
            />
          </Field>
        );
      }
      case "community": {
        const pinned = !!def.pin.community;
        const opts = pinned
          ? data.communities
          : withAll(data.communities, t("scope.allCommunities"));
        return (
          <Field key={kind} label={t("scope.community")}>
            <BlackSelect
              isDisabled={pinned}
              options={opts}
              value={find(data.communities, scope.community) ?? (pinned ? data.communities[0] : opts[0])}
              onChange={(o) => setCommunity(!o || o.value === ALL ? null : o.value)}
              placeholder={t("scope.allCommunities")}
            />
          </Field>
        );
      }
      case "school": {
        const opts = withAll(data.schools, t("scope.allSchools"));
        return (
          <Field key={kind} label={t("scope.school")}>
            <BlackSelect
              options={opts}
              value={find(data.schools, scope.schoolId) ?? opts[0]}
              onChange={(o) => setSchool(!o || o.value === ALL ? null : o.value)}
              placeholder={t("scope.allSchools")}
              isLoading={data.isLoading}
            />
          </Field>
        );
      }
      case "agency": {
        return (
          <Field key={kind} label={t("scope.agency")}>
            <BlackSelect
              options={data.agencies}
              value={data.agencies.find((o) => o.value === scope.agency) ?? data.agencies[0]}
              onChange={(o) => setAgency((o?.value as AgencyCode) ?? null)}
              isSearchable={false}
            />
          </Field>
        );
      }
      default:
        return null;
    }
  };

  return (
    <Stack direction="row" spacing={2} useFlexGap sx={{ flexWrap: "wrap", width: "100%" }}>
      {def.selectors.map(render)}
    </Stack>
  );
}
