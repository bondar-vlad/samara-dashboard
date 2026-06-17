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

const SPEND_SERIES: SeriesDef[] = [
  { key: "spending", name: "Держвидатки на ВО, млрд грн", color: BLUE },
  { key: "wasted", name: "З них «не за фахом», млрд грн", color: RED },
];

const EducationDemo = () => {
  const [showJson, setShowJson] = useState(false);

  const fundingSlices: DonutSlice[] = useMemo(
    () => [
      { label: "Бюджет", value: FUNDING_SPLIT.budgetPct, color: BLUE },
      { label: "Контракт", value: FUNDING_SPLIT.contractPct, color: ORANGE },
    ],
    [],
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
          Випускники вишів: робота не за фахом і ціна для держави
        </Typography>
        <Typography variant="body1" color="text.secondary">
          Демо на відкритих даних з інтернету: яка частка випускників працює не за
          спеціальністю, скільки з них навчались за бюджетні кошти, і яку суму
          держбюджету це орієнтовно «зʼїдає». Кожну діаграму можна зберегти як
          PNG / JPEG / SVG (кнопка завантаження у правому куті картки).
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
          label="Не за фахом"
          value={`${lastMeasuredOffSpec.value}%`}
          hint={`оцінка ${lastMeasuredOffSpec.year} р. (діапазон ${NOT_BY_SPECIALTY_RANGE.min}–${NOT_BY_SPECIALTY_RANGE.max}%)`}
          accent={RED}
        />
        <StatCard
          label="Навчались на бюджеті"
          value={`${FUNDING_SPLIT.budgetPct}%`}
          hint={`частка бюджету у наборі, ${FUNDING_SPLIT.year} р.`}
          accent={BLUE}
        />
        <StatCard
          label="Держвидатки на ВО"
          value={`${lastMeasuredSpend.value} млрд`}
          hint={`держбюджет, ${lastMeasuredSpend.year} р. (грн)`}
          accent={GREEN}
        />
        <StatCard
          label="«Втрачено» (прогноз)"
          value={`~${projectedWasted.wasted} млрд`}
          hint={`орієнтовна верхня межа, ${projectedWasted.year} р.`}
          accent={ORANGE}
        />
      </Box>

      <Alert severity="info" variant="outlined">
        Єдиної офіційної держстатистики «працюють не за фахом» немає — оцінки
        коливаються від ~{NOT_BY_SPECIALTY_RANGE.min}% до ~{NOT_BY_SPECIALTY_RANGE.max}%.
        Лінії-прогнози (пунктир) побудовані ШІ-екстраполяцією тренду по знайдених
        точках і позначені як оцінка, а не факт.
      </Alert>

      <Box
        sx={{
          display: "grid",
          gap: 3,
          gridTemplateColumns: { xs: "1fr", md: "1fr 1fr" },
        }}
      >
        <ChartFrame
          title="Частка випускників, що працюють не за спеціальністю"
          subheader="Виміряні точки (суцільна) + AI-прогноз із насиченням ~85% (пунктир)"
          filename="ukraine-off-specialty-share"
        >
          <LineChart data={NOT_BY_SPECIALTY} color={RED} yMax={100} ySuffix="%" />
        </ChartFrame>

        <ChartFrame
          title={`Бюджет проти контракту (${FUNDING_SPLIT.year})`}
          subheader={FUNDING_SPLIT.source}
          filename="ukraine-budget-vs-contract"
        >
          <DonutChart
            data={fundingSlices}
            centerLabel={`${FUNDING_SPLIT.budgetPct}%`}
            centerSub="на бюджеті"
          />
        </ChartFrame>

        <ChartFrame
          title="Держвидатки на вищу освіту, млрд грн"
          subheader="Виміряні 2017–2021 + AI-екстраполяція тренду (пунктир)"
          filename="ukraine-he-spending"
        >
          <LineChart data={HE_SPENDING_BN} color={BLUE} ySuffix="" />
        </ChartFrame>

        <ChartFrame
          title="Бюджетні місця / зарахування, тис."
          subheader="Виміряні значення за роками"
          filename="ukraine-budget-places"
        >
          <LineChart data={budgetPlacesData} color={GREEN} ySuffix="" />
        </ChartFrame>
      </Box>

      <ChartFrame
        title="Скільки держкоштів іде на тих, хто не працює за фахом"
        subheader="Держвидатки на ВО × частка «не за фахом» (орієнтовна верхня межа; останні роки — прогноз)"
        filename="ukraine-spending-vs-wasted"
      >
        <Stack direction="row" spacing={2} sx={{ flexWrap: "wrap", mb: 1 }} useFlexGap>
          {SPEND_SERIES.map((s) => (
            <Stack key={s.key} direction="row" spacing={0.75} sx={{ alignItems: "center" }}>
              <Box sx={{ width: 12, height: 12, borderRadius: "3px", bgcolor: s.color }} />
              <Typography variant="body2" color="text.secondary">
                {s.name}
              </Typography>
            </Stack>
          ))}
        </Stack>
        <GroupedBarChart data={spendVsWasteData} series={SPEND_SERIES} />
      </ChartFrame>

      <Card variant="outlined">
        <CardHeader
          title="Методологія та джерела"
          subheader="Як зібрано дані й побудовано прогноз"
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
          title="Зібрані дані (JSON)"
          subheader="Дані, на яких побудовано всі діаграми"
          action={
            <Button
              startIcon={<CodeIcon />}
              onClick={() => setShowJson((v) => !v)}
              variant="outlined"
            >
              {showJson ? "Сховати" : "Показати"}
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
          label="Демо · дані індикативні, не офіційна статистика"
        />
      </Box>
    </Stack>
  );
};

export default EducationDemo;
