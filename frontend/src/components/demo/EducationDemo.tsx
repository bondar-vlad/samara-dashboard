"use client";

import { useMemo, useState } from "react";
import {
  Alert,
  Box,
  Button,
  Card,
  CardContent,
  CardHeader,
  Chip,
  Collapse,
  Divider,
  Link,
  List,
  ListItem,
  ListItemText,
  Stack,
  Typography,
} from "@mui/material";
import CodeIcon from "@mui/icons-material/Code";
import ChartFrame from "@/components/charts/ChartFrame";
import DonutChart, { type DonutSlice } from "@/components/charts/DonutChart";
import LineChart from "@/components/charts/LineChart";
import GroupedBarChart, {
  type GroupedDatum,
  type SeriesDef,
} from "@/components/charts/GroupedBarChart";
import {
  BLUE,
  ORANGE,
  RED,
  GREEN,
  TEXT_SECONDARY,
} from "@/theme/colors";
import {
  NOT_BY_SPECIALTY,
  NOT_BY_SPECIALTY_RANGE,
  FUNDING_SPLIT,
  BUDGET_PLACES,
  HE_SPENDING_BN,
  SPEND_VS_WASTE,
  CONTEXT,
  METHODOLOGY,
  SOURCES,
} from "@/lib/ukraineData";
import { useTranslation } from "@/i18n/I18nProvider";

interface StatCardProps {
  label: string;
  value: string;
  hint: string;
  accent: string;
}

const StatCard = ({ label, value, hint, accent }: StatCardProps) => (
  <Card variant="outlined" sx={{ height: "100%" }}>
    <CardContent>
      <Typography variant="overline" color="text.secondary">
        {label}
      </Typography>
      <Typography variant="h3" sx={{ color: accent, fontWeight: 800, lineHeight: 1.1 }}>
        {value}
      </Typography>
      <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5 }}>
        {hint}
      </Typography>
    </CardContent>
  </Card>
);

const EducationDemo = () => {
  const { t } = useTranslation();
  const [showJson, setShowJson] = useState(false);

  const spendSeries: SeriesDef[] = useMemo(
    () => [
      { key: "spending", name: t("demo.spendSeriesName"), color: BLUE },
      { key: "wasted", name: t("demo.wastedSeriesName"), color: RED },
    ],
    [t],
  );

  const fundingSlices: DonutSlice[] = useMemo(
    () => [
      { label: t("demo.sliceBudget"), value: FUNDING_SPLIT.budgetPct, color: BLUE },
      { label: t("demo.sliceContract"), value: FUNDING_SPLIT.contractPct, color: ORANGE },
    ],
    [t],
  );

  const spendVsWasteData: GroupedDatum[] = useMemo(
    () =>
      SPEND_VS_WASTE.map((d) => ({
        label: String(d.year),
        values: { spending: d.spending, wasted: d.wasted },
      })),
    [],
  );

  const budgetPlacesData = useMemo(
    () => BUDGET_PLACES.map((d) => ({ year: d.year, value: d.value, kind: d.kind })),
    [],
  );

  const lastMeasuredOffSpec = useMemo(
    () => [...NOT_BY_SPECIALTY].filter((d) => d.kind === "measured").at(-1)!,
    [],
  );
  const lastMeasuredSpend = useMemo(
    () => [...HE_SPENDING_BN].filter((d) => d.kind === "measured").at(-1)!,
    [],
  );
  const projectedWasted = useMemo(
    () => [...SPEND_VS_WASTE].at(-1)!,
    [],
  );

  const jsonDump = useMemo(
    () =>
      JSON.stringify(
        {
          notBySpecialty: NOT_BY_SPECIALTY,
          notBySpecialtyRange: NOT_BY_SPECIALTY_RANGE,
          fundingSplit: {
            year: FUNDING_SPLIT.year,
            budget: FUNDING_SPLIT.budget,
            contract: FUNDING_SPLIT.contract,
            budgetPct: FUNDING_SPLIT.budgetPct,
            contractPct: FUNDING_SPLIT.contractPct,
            source: FUNDING_SPLIT.source,
          },
          budgetPlaces: BUDGET_PLACES,
          stateSpendingBn: HE_SPENDING_BN,
          spendVsWasted: SPEND_VS_WASTE,
          context: CONTEXT,
        },
        null,
        2,
      ),
    [],
  );

  return (
    <Stack spacing={3}>
      <Box>
        <Typography variant="h4" sx={{ fontWeight: 800, mb: 0.5 }}>
          {t("demo.heroTitle")}
        </Typography>
        <Typography variant="body1" color="text.secondary">
          {t("demo.heroSubtitle")}
        </Typography>
      </Box>

      <Box
        sx={{
          display: "grid",
          gap: 2,
          gridTemplateColumns: { xs: "1fr", sm: "1fr 1fr", lg: "repeat(4, 1fr)" },
        }}
      >
        <StatCard
          label={t("demo.statOffSpecLabel")}
          value={`${lastMeasuredOffSpec.value}%`}
          hint={t("demo.statOffSpecHint", {
            year: lastMeasuredOffSpec.year,
            min: NOT_BY_SPECIALTY_RANGE.min,
            max: NOT_BY_SPECIALTY_RANGE.max,
          })}
          accent={RED}
        />
        <StatCard
          label={t("demo.statBudgetLabel")}
          value={`${FUNDING_SPLIT.budgetPct}%`}
          hint={t("demo.statBudgetHint", { year: FUNDING_SPLIT.year })}
          accent={BLUE}
        />
        <StatCard
          label={t("demo.statSpendLabel")}
          value={`${lastMeasuredSpend.value} ${t("demo.bnUnit")}`}
          hint={t("demo.statSpendHint", { year: lastMeasuredSpend.year })}
          accent={GREEN}
        />
        <StatCard
          label={t("demo.statWastedLabel")}
          value={`~${projectedWasted.wasted} ${t("demo.bnUnit")}`}
          hint={t("demo.statWastedHint", { year: projectedWasted.year })}
          accent={ORANGE}
        />
      </Box>

      <Alert severity="info" variant="outlined">
        {t("demo.infoAlert", {
          min: NOT_BY_SPECIALTY_RANGE.min,
          max: NOT_BY_SPECIALTY_RANGE.max,
        })}
      </Alert>

      <Box
        sx={{
          display: "grid",
          gap: 3,
          gridTemplateColumns: { xs: "1fr", md: "1fr 1fr" },
        }}
      >
        <ChartFrame
          title={t("demo.chartOffSpecTitle")}
          subheader={t("demo.chartOffSpecSub")}
          filename="ukraine-off-specialty-share"
        >
          <LineChart
            data={NOT_BY_SPECIALTY}
            color={RED}
            yMax={100}
            ySuffix="%"
            forecastLabel={t("charts.forecast")}
          />
        </ChartFrame>

        <ChartFrame
          title={t("demo.chartDonutTitle", { year: FUNDING_SPLIT.year })}
          subheader={FUNDING_SPLIT.source}
          filename="ukraine-budget-vs-contract"
        >
          <DonutChart
            data={fundingSlices}
            centerLabel={`${FUNDING_SPLIT.budgetPct}%`}
            centerSub={t("demo.donutCenterSub")}
          />
        </ChartFrame>

        <ChartFrame
          title={t("demo.chartSpendTitle")}
          subheader={t("demo.chartSpendSub")}
          filename="ukraine-he-spending"
        >
          <LineChart
            data={HE_SPENDING_BN}
            color={BLUE}
            ySuffix=""
            forecastLabel={t("charts.forecast")}
          />
        </ChartFrame>

        <ChartFrame
          title={t("demo.chartPlacesTitle")}
          subheader={t("demo.chartPlacesSub")}
          filename="ukraine-budget-places"
        >
          <LineChart
            data={budgetPlacesData}
            color={GREEN}
            ySuffix=""
            forecastLabel={t("charts.forecast")}
          />
        </ChartFrame>
      </Box>

      <ChartFrame
        title={t("demo.spendVsWastedTitle")}
        subheader={t("demo.spendVsWastedSub")}
        filename="ukraine-spending-vs-wasted"
      >
        <Stack direction="row" spacing={2} sx={{ flexWrap: "wrap", mb: 1 }} useFlexGap>
          {spendSeries.map((s) => (
            <Stack key={s.key} direction="row" spacing={0.75} sx={{ alignItems: "center" }}>
              <Box sx={{ width: 12, height: 12, borderRadius: "3px", bgcolor: s.color }} />
              <Typography variant="body2" color="text.secondary">
                {s.name}
              </Typography>
            </Stack>
          ))}
        </Stack>
        <GroupedBarChart data={spendVsWasteData} series={spendSeries} />
      </ChartFrame>

      <Card variant="outlined">
        <CardHeader
          title={t("demo.methodologyTitle")}
          subheader={t("demo.methodologySubheader")}
        />
        <CardContent>
          <Typography variant="body2" sx={{ mb: 2 }}>
            {METHODOLOGY}
          </Typography>
          <Divider sx={{ mb: 1 }} />
          <List dense disablePadding>
            {SOURCES.map((s) => (
              <ListItem key={s.url} disableGutters>
                <ListItemText
                  primary={
                    <Link href={s.url} target="_blank" rel="noopener noreferrer">
                      {s.title}
                    </Link>
                  }
                  secondary={s.url}
                  slotProps={{
                    secondary: {
                      sx: { color: TEXT_SECONDARY, wordBreak: "break-all" },
                    },
                  }}
                />
              </ListItem>
            ))}
          </List>
        </CardContent>
      </Card>

      <Card variant="outlined">
        <CardHeader
          title={t("demo.jsonTitle")}
          subheader={t("demo.jsonSubheader")}
          action={
            <Button
              startIcon={<CodeIcon />}
              onClick={() => setShowJson((v) => !v)}
              variant="outlined"
            >
              {showJson ? t("demo.jsonHide") : t("demo.jsonShow")}
            </Button>
          }
        />
        <Collapse in={showJson} unmountOnExit>
          <CardContent>
            <Box
              component="pre"
              sx={{
                m: 0,
                p: 2,
                borderRadius: 2,
                bgcolor: "#0f172a",
                color: "#e2e8f0",
                fontSize: 12,
                lineHeight: 1.5,
                overflow: "auto",
                maxHeight: 480,
                fontFamily: "var(--font-geist-mono), monospace",
              }}
            >
              {jsonDump}
            </Box>
          </CardContent>
        </Collapse>
      </Card>

      <Box sx={{ pb: 2 }}>
        <Chip
          size="small"
          variant="outlined"
          label={t("demo.demoChip")}
        />
      </Box>
    </Stack>
  );
};

export default EducationDemo;
