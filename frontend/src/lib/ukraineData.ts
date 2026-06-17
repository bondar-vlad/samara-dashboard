/**
 * Static, internet-sourced dataset on Ukrainian higher-education graduates:
 * how many work OUTSIDE their specialty, the state-funded ("budget") share,
 * the state spending on higher education, and an illustrative estimate of how
 * much of that state money goes to graduates who never work in their field.
 *
 * Every point is tagged `measured` (reported by a named source) or `estimated`
 * (interpolated, or projected by AI from the measured points). The projection
 * method is documented in `METHODOLOGY` below.
 *
 * IMPORTANT: there is no single official Ukrainian time series for "% working
 * off-specialty" — estimates range from ~40% to ~80% depending on methodology.
 * The series here use the recurring media/survey headline figures, so they are
 * indicative rather than authoritative. The demo surfaces that explicitly.
 *
 * Sources are listed in `SOURCES`.
 */

export type PointKind = "measured" | "estimated";

export interface YearPoint {
  year: number;
  value: number;
  kind: PointKind;
  note?: string;
}

/** % of HE graduates working NOT by their specialty (headline survey figures). */
export const NOT_BY_SPECIALTY: YearPoint[] = [
  { year: 2007, value: 47, kind: "measured", note: "Держ. інститут розвитку сім'ї та молоді (47%)" },
  { year: 2019, value: 60, kind: "measured", note: "hh.ua «Професія до душі» — 60% не за фахом" },
  { year: 2021, value: 64, kind: "measured", note: "опитування: лише ~36% працюють за фахом → 64% ні" },
  { year: 2023, value: 80, kind: "measured", note: "«понад 80%» — оцінки експертів (op.ua, OBOZ.UA)" },
  // AI projection: linear trend on the measured points + saturation cap ~85%.
  { year: 2024, value: 82, kind: "estimated", note: "AI-прогноз" },
  { year: 2025, value: 84, kind: "estimated", note: "AI-прогноз" },
  { year: 2026, value: 85, kind: "estimated", note: "AI-прогноз (насичення ~85%)" },
];

/**
 * Lower/upper range of published estimates for the off-specialty share. Used to
 * draw an uncertainty band — the single-line series above sits inside it.
 */
export const NOT_BY_SPECIALTY_RANGE = {
  min: 40,
  max: 80,
  note: "Оцінки коливаються від ~40% (мінімум) до ~80% (максимум) — єдиної держстатистики немає (Український тиждень).",
};

/** State / municipal budget-funded share of enrolled students (vs. contract). */
export const FUNDING_SPLIT = {
  year: 2025,
  budget: 98_182,
  contract: 207_831,
  get total() {
    return this.budget + this.contract;
  },
  get budgetPct() {
    return +((this.budget / this.total) * 100).toFixed(1);
  },
  get contractPct() {
    return +((this.contract / this.total) * 100).toFixed(1);
  },
  source: "МОН / ЄДЕБО, вступна кампанія 2025 (усі рівні)",
};

/** Budget-funded study places / enrollment (thousands). */
export const BUDGET_PLACES: YearPoint[] = [
  { year: 2015, value: 97, kind: "measured", note: "97 тис. бюджетних місць" },
  { year: 2021, value: 61, kind: "measured", note: "61 тис. бюджетних місць" },
  { year: 2025, value: 98.2, kind: "measured", note: "98,2 тис. зараховано на бюджет (усі рівні)" },
];

/**
 * State-budget spending on higher education (UAH, billions).
 * Measured: «Економіка, управління та адміністрування» (ЖДТУ), держбюджет.
 * Estimated: linear projection of the 2017–2021 trend (nominal UAH; actual
 * wartime spending is likely lower — treat as a pre-war-trend extrapolation).
 */
export const HE_SPENDING_BN: YearPoint[] = [
  { year: 2017, value: 34.6, kind: "measured", note: "держбюджет, видатки на ВО" },
  { year: 2018, value: 36.7, kind: "measured", note: "держбюджет, видатки на ВО" },
  { year: 2019, value: 43.0, kind: "measured", note: "держбюджет, видатки на ВО" },
  { year: 2020, value: 44.5, kind: "measured", note: "держбюджет, видатки на ВО" },
  { year: 2021, value: 53.8, kind: "measured", note: "держбюджет, видатки на ВО" },
  { year: 2023, value: 61.0, kind: "estimated", note: "AI-прогноз (тренд 2017–2021)" },
  { year: 2025, value: 70.2, kind: "estimated", note: "AI-прогноз (тренд 2017–2021)" },
];

/**
 * Illustrative "wasted" state spend (UAH bn) = state HE spending × share of
 * graduates NOT working by specialty. This is an UPPER BOUND for discussion —
 * working off-specialty is not the same as money fully wasted.
 */
export interface SpendVsWaste {
  year: number;
  spending: number;
  wasted: number;
  kind: PointKind;
}

const shareForYear = (year: number): number => {
  const p = NOT_BY_SPECIALTY.find((d) => d.year === year);
  if (p) return p.value / 100;
  // nearest preceding measured/estimated point
  const sorted = [...NOT_BY_SPECIALTY].sort((a, b) => a.year - b.year);
  const prev = sorted.filter((d) => d.year <= year).at(-1) ?? sorted[0];
  return prev.value / 100;
};

export const SPEND_VS_WASTE: SpendVsWaste[] = HE_SPENDING_BN.map((d) => ({
  year: d.year,
  spending: d.value,
  wasted: +(d.value * shareForYear(d.year)).toFixed(1),
  kind: d.kind,
}));

export const CONTEXT = {
  graduatesPerYear: 300_000, // ~300 тис. зараховано на 1 курс (усі рівні), 2025
  budgetSharePct: FUNDING_SPLIT.budgetPct,
  educationGdpTargetPct: 7, // законодавча ціль ≥7% ВВП на освіту
};

export const METHODOLOGY =
  "Виміряні точки взято з відкритих джерел (різні методики/роки). Єдиної держстатистики частки «не за фахом» немає — оцінки коливаються 40–80%. AI-прогноз % «не за фахом» — лінійний тренд за точками 2007–2023 із насиченням ~85%. AI-прогноз держвидатків — лінійна екстраполяція тренду 2017–2021 (номінальні грн; фактичні воєнні видатки можуть бути нижчими). «Втрачені кошти» — орієнтовна верхня межа: держвидатки на ВО × частка випускників, що працюють не за фахом (робота не за фахом ≠ повністю змарновані кошти).";

export interface Source {
  title: string;
  url: string;
}

export const SOURCES: Source[] = [
  {
    title: "Український тиждень — щонайменше 40% (діапазон оцінок 40–80%)",
    url: "https://tyzhden.ua/shchonajmenshe-40-vypusknykiv-ukrainskykh-vyshiv-pratsiuiut-ne-za-fakhom/",
  },
  {
    title: "Освіторія — більшість українців працюють не за спеціальністю (hh.ua, 2019)",
    url: "https://osvitoria.media/news/bilshist-ukrayintsiv-ne-pratsyuyut-za-spetsialnistyu/",
  },
  {
    title: "op.ua — понад 80% випускників вишів не працюють за спеціальністю",
    url: "https://op.ua/news/osvita-v-ukraini/v-ukrayini-ponad-80-vipusknikiv-vishiv-ne-pracyuyut-za-specialnistyu-yak-vipraviti-situaciyu",
  },
  {
    title: "УНІАН / МОН — частка бюджету у наборі 32,1% (вступ 2025)",
    url: "https://www.unian.ua/society/vstupna-kampaniya-u-minosviti-rozpovili-skilki-studentiv-v-ukrajini-cogo-roku-13172487.html",
  },
  {
    title: "ЖДТУ «Економіка, управління та адміністрування» — видатки бюджету на вищу освіту",
    url: "https://ema.ztu.edu.ua/article/view/288286",
  },
];
