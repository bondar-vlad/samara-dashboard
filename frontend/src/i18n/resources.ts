/**
 * i18n resources for the dashboard UI. Two languages: Ukrainian (default,
 * matches the original content) and English.
 *
 * Keys are nested by feature area. Values may contain `{{var}}` placeholders
 * that the `t()` function interpolates. Data-layer strings (API responses,
 * sourced citations in `ukraineData`) are intentionally NOT translated here.
 */

export type Language = "uk" | "en";

export const DEFAULT_LANGUAGE: Language = "uk";
export const STORAGE_KEY = "samara.lang";

export const LANGUAGES: { code: Language; label: string; short: string }[] = [
  { code: "uk", label: "Українська", short: "УКР" },
  { code: "en", label: "English", short: "ENG" },
];

type Dict = { [key: string]: string | Dict };

const uk: Dict = {
  brand: "Samara Dashboard",
  header: {
    demoLink: "Демо: освіта",
    backHome: "На головну",
    monitoring: "Моніторинг прав дитини",
    studentAnalysis: "Аналіз профілю учня",
    directionAnalysis: "Аналіз напряму (профілю)",
    fourthSubjectAnalysis: "Аналіз четвертого предмета НМТ",
    profileChoiceAnalysis: "Аналіз вибору профілю (10 клас)",
    demoTitle: "Демо: освіта та ринок праці",
    demoSource: "Дані з відкритих джерел",
    language: "Мова",
  },
  home: {
    section10Title: "10 клас: вибір профілю навчання",
    section10Sub:
      "Профільна школа — який профіль (кластер) обирає учень проти рекомендації системи за результатами навчання.",
    section11Title: "Випуск 11 класу: вступ до ВНЗ за НМТ",
    section11Sub:
      "НМТ складають після 11 класу — напрям вступу та четвертий предмет: вибір учня проти рекомендації системи.",
  },
  common: {
    seriesChosen: "Вибір учнів",
    seriesRecommended: "Рекомендація системи",
    recommendedTag: "рекомендовано",
    notChosenCount: "Не обрали: {{count}}",
    mismatchesCount: "Розбіжностей: {{count}}",
    noChartData: "Немає даних для графіка.",
  },
  table: {
    student: "Учень",
    class: "Клас",
    choice: "Вибір учня",
    recommendation: "Рекомендація системи",
    status: "Статус",
    notChosen: "Не обрано",
    mismatch: "Розбіжність",
    rowsPerPage: "Рядків на сторінці:",
    displayedRows: "{{from}}–{{to}} з {{count}}",
  },
  direction: {
    loadError: "Не вдалося завантажити дані про напрям (профіль).",
    chartTitle: "Вибір напряму вищої освіти",
    chartSub:
      "Кількість учнів за напрямом вступу — вибір учнів проти рекомендації системи",
    studentsTitle: "Учні за напрямом",
    clickHint: "Натисніть на учня, щоб відкрити детальний аналіз.",
    cardTitle: "Аналіз напряму (профілю) за НМТ",
    cardSub:
      "Рекомендований напрям вступу рахується за балами НМТ із коефіцієнтами напряму та профільними темами",
    analysisLoadError: "Не вдалося завантажити аналіз напряму.",
    studentChoice: "Вибір учня",
    chosenDirection: "Обраний напрям",
    notChosenYet: "Ще не обрано",
    systemRec: "Рекомендація системи",
    byNmtAndTopics: "За балами НМТ і темами",
    bannerNotChosen:
      "Учень ще не обрав напрям. Рекомендація за НМТ може допомогти у виборі.",
    bannerMismatch:
      "Вибір учня не збігається з рекомендацією за НМТ — варто обговорити.",
    bannerMatch: "Вибір учня збігається з рекомендацією за НМТ.",
    nmtScores: "Бали НМТ",
    howComputed: "Як рахувалось",
    profileLabel: "· профіль {{name}}",
    keySubjects: "Профільні предмети: ",
    rankingTitle: "Рейтинг напрямів",
    nmtShort: "НМТ",
    topicsShort: "теми",
  },
  fourthSubject: {
    loadError: "Не вдалося завантажити дані про четвертий предмет НМТ.",
    chartTitle: "Четвертий предмет НМТ: вибір учнів проти рекомендації",
    chartSub:
      "Кількість учнів за предметом на вибір — що обрали учні та що рекомендує система",
    studentsTitle: "Учні: четвертий предмет",
    clickHint:
      "Натисніть на учня, щоб відкрити детальний аналіз четвертого предмета.",
    cardTitle: "Четвертий предмет НМТ",
    cardSub:
      "Предмет за вибором учня проти рекомендації системи на основі оцінок",
    analysisLoadError: "Не вдалося завантажити дані про четвертий предмет.",
    studentChoice: "Вибір учня",
    chosenSubject: "Обраний предмет",
    notChosenYet: "Ще не обрано",
    systemRec: "Рекомендація системи",
    byResults: "За результатами навчання",
    bannerNotChosen:
      "Учень ще не обрав четвертий предмет. Рекомендація системи може допомогти у виборі.",
    bannerMismatch:
      "Вибір учня не збігається з рекомендацією системи — варто обговорити.",
    bannerMatch: "Вибір учня збігається з рекомендацією системи.",
    basedOnGrades: "На основі оцінок",
    rankingTitle: "Рейтинг предметів на вибір",
    evidenceCount: "{{count}} оцінок",
  },
  profileChoice: {
    loadError: "Не вдалося завантажити дані про вибір профілю.",
    chartTitle: "Вибір профілю навчання (10 клас)",
    chartSub:
      "Кількість учнів за кластером профілю — вибір учнів проти рекомендації системи",
    studentsTitle: "Учні за профілем",
    clickHint: "Натисніть на учня, щоб відкрити детальний аналіз профілю.",
    cardTitle: "Аналіз вибору профілю (10 клас)",
    cardSub: "Рекомендований кластер визначається за профільними темами та оцінками учня",
    analysisLoadError: "Не вдалося завантажити аналіз профілю.",
    studentChoice: "Вибір учня",
    chosenCluster: "Обраний кластер",
    notChosenYet: "Ще не обрано",
    systemRec: "Рекомендація системи",
    byGradesAndTopics: "За темами та оцінками",
    confidence: "Впевненість {{pct}}%",
    bannerNotChosen:
      "Учень ще не обрав профіль. Рекомендація системи може допомогти у виборі.",
    bannerMismatch:
      "Вибір учня не збігається з рекомендацією системи — варто обговорити.",
    bannerMatch: "Вибір учня збігається з рекомендацією системи.",
    howComputed: "Як рахувалось",
    rankingTitle: "Рейтинг кластерів",
  },
  dashboard: {
    desiredLegend: "Бажання учнів (анкетування)",
    recommendedLegend: "Рекомендація системи",
    cardTitle: "Профілі: рекомендація системи проти бажання учнів",
    cardSubheader:
      "Кількість дітей за кластерами профілів — що рекомендує система та що обрали учні в анкетуванні",
    totalStudents: "Усього учнів: {{count}}",
    mismatches: "Розбіжностей: {{count}}",
    noChartData: "Немає даних для графіка. Запустіть аналіз для учнів.",
    studentsByProfileTitle: "Учні за обраним профілем",
    studentsByProfileSubtitle:
      "Натисніть на учня, щоб відкрити детальний аналіз і маркери.",
    clusterStudents: "{{count}} учн. · {{mismatch}} з розбіжністю",
    loadStudentsError: "Не вдалося завантажити дані учнів.",
    tableStudent: "Учень",
    tableClass: "Клас",
    tableRecommendation: "Рекомендація системи",
    tableStatus: "Статус",
    mismatchChip: "Розбіжність",
  },
  comparison: {
    title: "Порівняння профілів: вибір учня та рекомендація системи",
    subheader:
      "Інструмент для адміністрації та батьків — зважити маркери й обговорити кращий профіль для учня",
    recalc: "Перерахувати",
    runAnalysis: "Запустити аналіз",
    studentLabel: "Учень",
    loadStudentsError: "Не вдалося завантажити список учнів.",
    selectPlaceholder: "Оберіть учня…",
    loadStudentError: "Не вдалося завантажити дані учня.",
    gradeChip: "{{grade}} клас",
    noAnalysisYet:
      "Для цього учня ще не виконано аналіз. Натисніть «Запустити аналіз», щоб система сформувала рекомендацію та маркери.",
    markersDivider: "Маркери, виявлені системою",
    noMarkers:
      "Маркерів не виявлено — вибір учня узгоджений із рекомендацією.",
  },
  profileColumns: {
    ownChoiceTitle: "Власний вибір учня",
    ownChoiceSubtitle: "Бажані профілі, обрані учнем",
    systemTitle: "Рекомендація системи",
    confidence: "Впевненість {{pct}}%",
    notAnalyzed: "Аналіз ще не виконано",
    cluster: "Кластер",
    profiles: "Профілі",
    noData: "Немає даних",
    mismatchNote:
      "Виявлено розбіжність між вибором учня та рекомендацією системи. Перегляньте маркери нижче, щоб обговорити це з учнем і батьками.",
    matchNote: "Вибір учня збігається з рекомендацією системи.",
  },
  fitAnalysis: {
    basedOn: "На основі оцінок і тем",
    title: "Аналіз відповідності спеціальностям",
    noData: "Дані відповідності спеціальностям недоступні.",
    fit: "Відповідність {{pct}}%",
    strengths: "Сильні сторони",
    gapsTitle: "Що потрібно підтягнути",
    adviceTitle: "Що каже система",
    subject: "предмет",
    topic: "тема",
    gapProgress: "Прогрес до цільового рівня: {{pct}}%",
  },
  warningCard: {
    recommendedActions: "Рекомендовані дії",
    forAudience: "Для:",
    collapse: "Згорнути аналіз",
    expand: "Детальний аналіз",
  },
  audiences: {
    ClassTeacher: "Класний керівник",
    Student: "Учень",
    Parent: "Батьки",
    SchoolAdmin: "Адміністрація",
    Psychologist: "Психолог",
    SocialWorker: "Соцпрацівник",
  },
  charts: {
    export: "Зберегти як зображення",
    exportNotFound: "Не вдалося знайти діаграму для експорту.",
    exportError: "Помилка експорту",
    formatPng: "PNG (растрове зображення)",
    formatJpeg: "JPEG (растрове зображення)",
    formatSvg: "SVG (векторне зображення)",
    forecast: "AI-прогноз",
  },
  demo: {
    heroTitle: "Випускники вишів: робота не за фахом і ціна для держави",
    heroSubtitle:
      "Демо на відкритих даних з інтернету: яка частка випускників працює не за спеціальністю, скільки з них навчались за бюджетні кошти, і яку суму держбюджету це орієнтовно «зʼїдає». Кожну діаграму можна зберегти як PNG / JPEG / SVG (кнопка завантаження у правому куті картки).",
    statOffSpecLabel: "Не за фахом",
    statOffSpecHint: "оцінка {{year}} р. (діапазон {{min}}–{{max}}%)",
    statBudgetLabel: "Навчались на бюджеті",
    statBudgetHint: "частка бюджету у наборі, {{year}} р.",
    statSpendLabel: "Держвидатки на ВО",
    statSpendHint: "держбюджет, {{year}} р. (грн)",
    bnUnit: "млрд",
    statWastedLabel: "«Втрачено» (прогноз)",
    statWastedHint: "орієнтовна верхня межа, {{year}} р.",
    infoAlert:
      "Єдиної офіційної держстатистики «працюють не за фахом» немає — оцінки коливаються від ~{{min}}% до ~{{max}}%. Лінії-прогнози (пунктир) побудовані ШІ-екстраполяцією тренду по знайдених точках і позначені як оцінка, а не факт.",
    chartOffSpecTitle: "Частка випускників, що працюють не за спеціальністю",
    chartOffSpecSub:
      "Виміряні точки (суцільна) + AI-прогноз із насиченням ~85% (пунктир)",
    chartDonutTitle: "Бюджет проти контракту ({{year}})",
    sliceBudget: "Бюджет",
    sliceContract: "Контракт",
    donutCenterSub: "на бюджеті",
    chartSpendTitle: "Держвидатки на вищу освіту, млрд грн",
    chartSpendSub: "Виміряні 2017–2021 + AI-екстраполяція тренду (пунктир)",
    chartPlacesTitle: "Бюджетні місця / зарахування, тис.",
    chartPlacesSub: "Виміряні значення за роками",
    spendVsWastedTitle: "Скільки держкоштів іде на тих, хто не працює за фахом",
    spendVsWastedSub:
      "Держвидатки на ВО × частка «не за фахом» (орієнтовна верхня межа; останні роки — прогноз)",
    spendSeriesName: "Держвидатки на ВО, млрд грн",
    wastedSeriesName: "З них «не за фахом», млрд грн",
    methodologyTitle: "Методологія та джерела",
    methodologySubheader: "Як зібрано дані й побудовано прогноз",
    jsonTitle: "Зібрані дані (JSON)",
    jsonSubheader: "Дані, на яких побудовано всі діаграми",
    jsonShow: "Показати",
    jsonHide: "Сховати",
    demoChip: "Демо · дані індикативні, не офіційна статистика",
  },
};

const en: Dict = {
  brand: "Samara Dashboard",
  header: {
    demoLink: "Demo: education",
    backHome: "Back to home",
    monitoring: "Child rights monitoring",
    studentAnalysis: "Student profile analysis",
    directionAnalysis: "Direction (profile) analysis",
    fourthSubjectAnalysis: "NMT fourth-subject analysis",
    profileChoiceAnalysis: "Profile choice analysis (10th grade)",
    demoTitle: "Demo: education & labor market",
    demoSource: "Open-source data",
    language: "Language",
  },
  home: {
    section10Title: "10th grade: choosing a study profile",
    section10Sub:
      "Profile high school — which profile (cluster) a pupil picks versus the system's recommendation from their results.",
    section11Title: "11th-grade graduation: university admission by NMT",
    section11Sub:
      "NMT is taken after 11th grade — admission direction and the fourth subject: the pupil's choice versus the recommendation.",
  },
  common: {
    seriesChosen: "Students' choice",
    seriesRecommended: "System recommendation",
    recommendedTag: "recommended",
    notChosenCount: "Not chosen: {{count}}",
    mismatchesCount: "Mismatches: {{count}}",
    noChartData: "No data for the chart.",
  },
  table: {
    student: "Student",
    class: "Class",
    choice: "Student's choice",
    recommendation: "System recommendation",
    status: "Status",
    notChosen: "Not chosen",
    mismatch: "Mismatch",
    rowsPerPage: "Rows per page:",
    displayedRows: "{{from}}–{{to}} of {{count}}",
  },
  direction: {
    loadError: "Failed to load direction (profile) data.",
    chartTitle: "Choice of higher-education direction",
    chartSub:
      "Number of students by admission direction — students' choice vs system recommendation",
    studentsTitle: "Students by direction",
    clickHint: "Click a student to open the detailed analysis.",
    cardTitle: "Direction (profile) analysis by NMT",
    cardSub:
      "The recommended admission direction is computed from NMT scores with direction coefficients and core topics",
    analysisLoadError: "Failed to load the direction analysis.",
    studentChoice: "Student's choice",
    chosenDirection: "Chosen direction",
    notChosenYet: "Not chosen yet",
    systemRec: "System recommendation",
    byNmtAndTopics: "By NMT scores and topics",
    bannerNotChosen:
      "The student hasn't chosen a direction yet. The NMT-based recommendation can help.",
    bannerMismatch:
      "The student's choice differs from the NMT recommendation — worth discussing.",
    bannerMatch: "The student's choice matches the NMT recommendation.",
    nmtScores: "NMT scores",
    howComputed: "How it was computed",
    profileLabel: "· profile {{name}}",
    keySubjects: "Core subjects: ",
    rankingTitle: "Direction ranking",
    nmtShort: "NMT",
    topicsShort: "topics",
  },
  fourthSubject: {
    loadError: "Failed to load NMT fourth-subject data.",
    chartTitle: "NMT fourth subject: students' choice vs recommendation",
    chartSub:
      "Number of students by elective subject — what students chose and what the system recommends",
    studentsTitle: "Students: fourth subject",
    clickHint:
      "Click a student to open the detailed fourth-subject analysis.",
    cardTitle: "NMT fourth subject",
    cardSub:
      "The student's elective subject vs the system recommendation based on grades",
    analysisLoadError: "Failed to load fourth-subject data.",
    studentChoice: "Student's choice",
    chosenSubject: "Chosen subject",
    notChosenYet: "Not chosen yet",
    systemRec: "System recommendation",
    byResults: "By academic results",
    bannerNotChosen:
      "The student hasn't chosen a fourth subject yet. The system recommendation can help.",
    bannerMismatch:
      "The student's choice differs from the system recommendation — worth discussing.",
    bannerMatch: "The student's choice matches the system recommendation.",
    basedOnGrades: "Based on grades",
    rankingTitle: "Elective subjects ranking",
    evidenceCount: "{{count}} grades",
  },
  profileChoice: {
    loadError: "Failed to load profile-choice data.",
    chartTitle: "Study profile choice (10th grade)",
    chartSub:
      "Number of students by profile cluster — students' choice vs system recommendation",
    studentsTitle: "Students by profile",
    clickHint: "Click a student to open the detailed profile analysis.",
    cardTitle: "Profile choice analysis (10th grade)",
    cardSub: "The recommended cluster is computed from the pupil's core topics and grades",
    analysisLoadError: "Failed to load the profile analysis.",
    studentChoice: "Student's choice",
    chosenCluster: "Chosen cluster",
    notChosenYet: "Not chosen yet",
    systemRec: "System recommendation",
    byGradesAndTopics: "By topics and grades",
    confidence: "Confidence {{pct}}%",
    bannerNotChosen:
      "The student hasn't chosen a profile yet. The system recommendation can help.",
    bannerMismatch:
      "The student's choice differs from the recommendation — worth discussing.",
    bannerMatch: "The student's choice matches the system recommendation.",
    howComputed: "How it was computed",
    rankingTitle: "Cluster ranking",
  },
  dashboard: {
    desiredLegend: "Students' wishes (survey)",
    recommendedLegend: "System recommendation",
    cardTitle: "Profiles: system recommendation vs students' wishes",
    cardSubheader:
      "Number of children by profile clusters — what the system recommends and what students chose in the survey",
    totalStudents: "Total students: {{count}}",
    mismatches: "Mismatches: {{count}}",
    noChartData: "No data for the chart. Run analysis for students.",
    studentsByProfileTitle: "Students by chosen profile",
    studentsByProfileSubtitle:
      "Click a student to open the detailed analysis and markers.",
    clusterStudents: "{{count}} students · {{mismatch}} with mismatch",
    loadStudentsError: "Failed to load student data.",
    tableStudent: "Student",
    tableClass: "Class",
    tableRecommendation: "System recommendation",
    tableStatus: "Status",
    mismatchChip: "Mismatch",
  },
  comparison: {
    title: "Profile comparison: student's choice vs system recommendation",
    subheader:
      "A tool for administrators and parents — weigh the markers and discuss the best profile for the student",
    recalc: "Recalculate",
    runAnalysis: "Run analysis",
    studentLabel: "Student",
    loadStudentsError: "Failed to load the student list.",
    selectPlaceholder: "Select a student…",
    loadStudentError: "Failed to load student data.",
    gradeChip: "Grade {{grade}}",
    noAnalysisYet:
      "Analysis has not been run for this student yet. Click \u201cRun analysis\u201d so the system generates a recommendation and markers.",
    markersDivider: "Markers detected by the system",
    noMarkers:
      "No markers detected — the student's choice aligns with the recommendation.",
  },
  profileColumns: {
    ownChoiceTitle: "Student's own choice",
    ownChoiceSubtitle: "Desired profiles chosen by the student",
    systemTitle: "System recommendation",
    confidence: "Confidence {{pct}}%",
    notAnalyzed: "Analysis not run yet",
    cluster: "Cluster",
    profiles: "Profiles",
    noData: "No data",
    mismatchNote:
      "A mismatch was detected between the student's choice and the system's recommendation. Review the markers below to discuss it with the student and parents.",
    matchNote: "The student's choice matches the system's recommendation.",
  },
  fitAnalysis: {
    basedOn: "Based on grades and topics",
    title: "Specialty fit analysis",
    noData: "Specialty fit data is unavailable.",
    fit: "Fit {{pct}}%",
    strengths: "Strengths",
    gapsTitle: "What to improve",
    adviceTitle: "What the system says",
    subject: "subject",
    topic: "topic",
    gapProgress: "Progress to target level: {{pct}}%",
  },
  warningCard: {
    recommendedActions: "Recommended actions",
    forAudience: "For:",
    collapse: "Collapse analysis",
    expand: "Detailed analysis",
  },
  audiences: {
    ClassTeacher: "Class teacher",
    Student: "Student",
    Parent: "Parents",
    SchoolAdmin: "Administration",
    Psychologist: "Psychologist",
    SocialWorker: "Social worker",
  },
  charts: {
    export: "Save as image",
    exportNotFound: "Could not find a chart to export.",
    exportError: "Export error",
    formatPng: "PNG (raster image)",
    formatJpeg: "JPEG (raster image)",
    formatSvg: "SVG (vector image)",
    forecast: "AI forecast",
  },
  demo: {
    heroTitle: "University graduates: working off-specialty and the cost to the state",
    heroSubtitle:
      "A demo on open data from the internet: what share of graduates work outside their specialty, how many of them studied on state-funded places, and roughly how much of the state budget this \u201ceats up\u201d. Each chart can be saved as PNG / JPEG / SVG (the download button in the top corner of the card).",
    statOffSpecLabel: "Off-specialty",
    statOffSpecHint: "estimate for {{year}} (range {{min}}–{{max}}%)",
    statBudgetLabel: "Studied on state funding",
    statBudgetHint: "state-funded share of enrollment, {{year}}",
    statSpendLabel: "State spending on HE",
    statSpendHint: "state budget, {{year}} (UAH)",
    bnUnit: "bn",
    statWastedLabel: "\u201cWasted\u201d (forecast)",
    statWastedHint: "indicative upper bound, {{year}}",
    infoAlert:
      "There is no single official state statistic for \u201cworking off-specialty\u201d — estimates range from ~{{min}}% to ~{{max}}%. The forecast lines (dashed) are built by AI extrapolation of the trend over the found data points and are marked as estimates, not facts.",
    chartOffSpecTitle: "Share of graduates working outside their specialty",
    chartOffSpecSub:
      "Measured points (solid) + AI forecast with ~85% saturation (dashed)",
    chartDonutTitle: "State-funded vs contract ({{year}})",
    sliceBudget: "State-funded",
    sliceContract: "Contract",
    donutCenterSub: "state-funded",
    chartSpendTitle: "State spending on higher education, UAH bn",
    chartSpendSub: "Measured 2017–2021 + AI trend extrapolation (dashed)",
    chartPlacesTitle: "State-funded places / enrollment, thousands",
    chartPlacesSub: "Measured values by year",
    spendVsWastedTitle:
      "How much state money goes to those who don't work in their field",
    spendVsWastedSub:
      "State HE spending × off-specialty share (indicative upper bound; recent years are a forecast)",
    spendSeriesName: "State spending on HE, UAH bn",
    wastedSeriesName: "Of which \u201coff-specialty\u201d, UAH bn",
    methodologyTitle: "Methodology and sources",
    methodologySubheader: "How the data was collected and the forecast built",
    jsonTitle: "Collected data (JSON)",
    jsonSubheader: "The data behind all the charts",
    jsonShow: "Show",
    jsonHide: "Hide",
    demoChip: "Demo · data is indicative, not official statistics",
  },
};

export const resources: Record<Language, Dict> = { uk, en };
