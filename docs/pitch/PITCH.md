---
marp: true
size: 16:9
paginate: true
html: true
title: Моніторинг ризиків прав дитини — Challenge 1
author: Команда Samara
backgroundColor: "#C6D8F7"
style: |
  @import url('https://fonts.googleapis.com/css2?family=Archivo+Black&family=Space+Grotesk:wght@400;500;700&family=Space+Mono:wght@400;700&display=swap');

  :root {
    --ink: #0B0B0B;
    --blue: #C6D8F7;
    --blue-deep: #2A4A86;
    --yellow: #ECF23C;
    --card: rgba(255,255,255,0.55);
    --line: rgba(11,11,11,0.18);
    --red: #E5484D;
    --orange: #F2922A;
    --green: #2E9E5B;
  }

  section {
    background: var(--blue);
    color: var(--ink);
    font-family: 'Space Grotesk', system-ui, sans-serif;
    font-size: 22px;
    padding: 52px 64px;
    letter-spacing: 0.1px;
  }
  h1 { font-family: 'Archivo Black', sans-serif; font-size: 56px; line-height: 1.02; margin: 0 0 8px; }
  h2 { font-family: 'Archivo Black', sans-serif; font-size: 34px; line-height: 1.05; margin: 0 0 18px; }
  h2::after { content: ""; display: block; width: 84px; height: 10px; background: var(--yellow); margin-top: 10px; border-radius: 2px; }
  h3 { font-family: 'Space Grotesk', sans-serif; font-weight: 700; font-size: 24px; margin: 6px 0; }
  strong { font-weight: 700; }
  a { color: var(--blue-deep); }
  ul { margin: 6px 0; } li { margin: 7px 0; }
  code { font-family: 'Space Mono', monospace; background: rgba(11,11,11,0.07); padding: 1px 6px; border-radius: 4px; font-size: 0.92em; }

  .mono { font-family: 'Space Mono', monospace; }
  .muted { color: rgba(11,11,11,0.62); font-size: 15px; line-height: 1.4; }
  .tag { background: var(--yellow); padding: 1px 8px; font-weight: 700; border-radius: 3px; }
  .kicker { font-family: 'Space Mono', monospace; font-weight: 700; letter-spacing: 3px; text-transform: uppercase; font-size: 15px; color: var(--blue-deep); }

  .cols { display: flex; gap: 26px; align-items: flex-start; }
  .col { flex: 1; }

  .cards { display: grid; grid-template-columns: 1fr 1fr; gap: 18px; margin-top: 8px; }
  .card { background: var(--card); border: 2px solid var(--ink); border-radius: 10px; padding: 16px 18px; }
  .card .num { font-family: 'Space Mono', monospace; font-weight: 700; font-size: 30px; letter-spacing: 4px; }
  .card h3 { margin: 2px 0 6px; font-size: 22px; }
  .card p { margin: 0; font-size: 16px; line-height: 1.35; }

  .kpis { display: flex; gap: 18px; margin: 10px 0; }
  .kpi { flex: 1; background: var(--card); border: 2px solid var(--ink); border-radius: 10px; padding: 16px; text-align: center; }
  .kpi .big { font-family: 'Archivo Black', sans-serif; font-size: 40px; line-height: 1; }
  .kpi .lbl { font-size: 14px; color: rgba(11,11,11,0.7); margin-top: 8px; }

  .flow { display: flex; align-items: stretch; gap: 10px; flex-wrap: wrap; }
  .box { background: var(--card); border: 2px solid var(--ink); border-radius: 8px; padding: 10px 12px; font-size: 15px; font-weight: 700; display: flex; align-items: center; }
  .box small { display:block; font-weight: 400; color: rgba(11,11,11,0.65); }
  .arrow { display:flex; align-items:center; font-weight:700; font-size: 20px; }

  table { border-collapse: collapse; width: 100%; font-size: 18px; }
  th { background: var(--ink); color: var(--blue); text-align: left; padding: 8px 12px; }
  td { border-bottom: 1px solid var(--line); padding: 8px 12px; vertical-align: top; }
  tr:nth-child(even) td { background: rgba(255,255,255,0.28); }

  .pill { display:inline-block; font-family:'Space Mono',monospace; font-weight:700; font-size:13px; padding:1px 8px; border-radius:20px; color:#fff; }
  .p-red{background:var(--red)} .p-org{background:var(--orange)} .p-yel{background:#C9A400} .p-grn{background:var(--green)}

  footer { font-family: 'Space Mono', monospace; font-size: 12px; color: rgba(11,11,11,0.55); }
  section::after { font-family: 'Space Mono', monospace; color: rgba(11,11,11,0.55); }

  section.lead { display: flex; flex-direction: column; justify-content: center; }
  section.lead h1 { font-size: 72px; }
  section.lead .partners { margin-top: 26px; font-family:'Space Mono',monospace; font-size:14px; text-transform:uppercase; letter-spacing:1px; }
  section.dark { background: var(--ink); color: var(--blue); }
  section.dark h1, section.dark h2 { color: #fff; }
  section.dark h2::after { background: var(--yellow); }
  section.dark .muted { color: rgba(255,255,255,0.6); }
  section.dark .card, section.dark .kpi, section.dark .box { background: rgba(255,255,255,0.06); border-color: rgba(255,255,255,0.5); color: #fff; }
---

<!-- _class: lead -->
<!-- _paginate: false -->

<span class="kicker">Challenge 1 · Child Rights Risk Monitoring</span>

# Моніторинг ризиків прав дитини

### Від реактивного захисту — до проактивного. Єдина міжвідомча екосистема, що бачить ризик у траєкторії дитини **до** того, як він стане кризою.

<div class="partners">
Партнери челенджу: Координаційний центр з розвитку сімейного виховання та догляду за дітьми · Державна служба України у справах дітей
</div>

<!-- Спікер: Привіт. Челендж — моніторинг ризиків прав дитини. Сьогодні держава реагує постфактум, коли право вже порушено. Ми побудували платформу, яка з'єднує реєстри й бачить ризик заздалегідь. І покажемо це на гострому, грошовому прикладі — реформі профільної освіти 2027. -->

---

## Чому зараз: реформа профільної освіти 2027

<div class="cols">
<div class="col">

З 2027 року старша школа стає **профільною**. У 9 класі дитина обирає напрям, від якого залежить її подальша освіта, вступ і професія.

- **Direction → Cluster → Profile** — три рівні вибору
- 12 профілів: 2 академічні кластери + 10 професійних напрямів
- Заклад пропонує лише дозволені йому профілі (ліцей / фаховий коледж / гімназія)

</div>
<div class="col">

<div class="box" style="display:block">
Держава вкладає <strong>бюджетні кошти</strong> у профілі, місця й заклади під цей вибір.
<small>А вибір сьогодні робиться майже наосліп — без даних про сильні сторони дитини.</small>
</div>

<p class="muted" style="margin-top:14px">Реформа — це вікно можливостей: правильна інфраструктура даних має зʼявитися <strong>разом</strong> із реформою, а не через 5 років після неї.</p>

</div>
</div>

<!-- Спікер: Реформа 2027 — це момент, коли вибір профілю стає визначальним і коштовним. Якщо вибір помилковий, держава фінансує траєкторію, яка «не спрацює». Тому інструмент моніторингу потрібен саме зараз. -->

---

## Проблема

<div class="cards">
<div class="card">
<div class="num">01</div>
<h3>Фрагментовані дані</h3>
<p>Інформація про дитину розпорошена між реєстрами: цивільний стан, здоровʼя, освіта, правоохоронці — без наскрізної координації.</p>
</div>
<div class="card">
<div class="num">02</div>
<h3>Реактивна модель</h3>
<p>Держава втручається переважно <strong>після</strong> кризи — коли права дитини вже серйозно порушені.</p>
</div>
<div class="card">
<div class="num">03</div>
<h3>Немає раннього сигналу</h3>
<p>Відсутня єдина цифрова система, що виявляє ризики рано й запускає адресну підтримку до загострення.</p>
</div>
<div class="card" style="background:var(--yellow); border-color:var(--ink)">
<div class="num">04</div>
<h3>Прихована ціна</h3>
<p>Помилкові траєкторії = змарновані бюджетні кошти + втрачений потенціал дитини.</p>
</div>
</div>

<!-- Спікер: Перші три пункти — дослівно з челенджу. Четвертий — наш акцент: у фрагментованої, реактивної системи є цілком конкретна грошова ціна. Її ми зараз покажемо. -->

---

## Прихована ціна: бюджет, що «не працює за профілем»

<div class="kpis">
<div class="kpi"><div class="big">64–80%</div><div class="lbl">випускників працюють <strong>не за фахом</strong> (2021→2023, відкриті оцінки)</div></div>
<div class="kpi"><div class="big">~70 млрд</div><div class="lbl">грн/рік держвидатки на вищу освіту (оцінка тренду, 2025)</div></div>
<div class="kpi"><div class="big">до ~59 млрд</div><div class="lbl">грн/рік — верхня оцінка коштів на освіту, що не конвертується в роботу за фахом</div></div>
</div>

Держава **платить за профіль/спеціальність**, але результат «по профілю не спрацьовує»: дитина роками вчиться не туди, а кошти не дають віддачі ані для неї, ані для держави.

<p class="muted">Джерела — відкриті дані (МОН/ЄДЕБО, опитування, медіа). Єдиної держстатистики «не за фахом» немає: оцінки 40–80%. «Змарновані кошти» — орієнтовна верхня межа для дискусії (робота не за фахом ≠ повністю втрачені кошти). Методологія й джерела вшиті в систему.</p>

<!-- Спікер: Ось і грошовий гачок. Десятки мільярдів гривень щороку йдуть на освіту, яка не конвертується в роботу за фахом. Профільна реформа без даних ризикує відтворити цю ж проблему — лише раніше, на рівні школи. Ми навмисно показуємо діапазони й джерела: перед державним журі чесність цифр важливіша за драму. -->

---

## Інсайт: мисмеч профілю — це ризик прав дитини

<div class="cols">
<div class="col">

**Право дитини на розвиток і самореалізацію** (Конвенція ООН, ст. 28–29) порушується тихо — без скандалу, але системно.

Якщо **бажаний профіль ≠ профіль за даними** — це ранній сигнал:
- дитина в чужій траєкторії,
- мотивація й успішність падають,
- бюджетне місце витрачається даремно.

</div>
<div class="col">

<div class="box" style="display:block; background:var(--yellow)">
<strong>Ключова теза</strong>
<small style="color:#0B0B0B">Той самий сигнал = одночасно ризик прав дитини <em>і</em> ризик бюджету. Виявивши його в 9 класі, ми рятуємо і дитину, і кошти.</small>
</div>

<p class="muted" style="margin-top:14px">Ми перетворюємо «втрату за фахом» з невидимої статистики на <strong>конкретний червоний прапорець</strong> по конкретній дитині — вчасно.</p>

</div>
</div>

<!-- Спікер: Головна думка пітчу. Мисмеч профілю — не «профорієнтаційна дрібниця», а порушення права дитини на розвиток, яке ще й коштує державі грошей. Один сигнал закриває обидва ризики. -->

---

## Рішення: одна платформа замість шести розрізнених систем

<span class="kicker">Що це</span>

**Міжвідомча екосистема даних про дитину** з AI-двигуном аналізу: збирає сигнали з відомств, виявляє ризики тиражованими алгоритмами, запускає адресну відповідь і дає управлінський дашборд.

<div class="flow" style="margin-top:18px">
<div class="box">Освіта<small>відвідування, оцінки за темами</small></div>
<div class="arrow">→</div>
<div class="box">Медицина<small>повторні звернення</small></div>
<div class="arrow">→</div>
<div class="box">Ювенальна поліція<small>булінг, безпека</small></div>
<div class="arrow">→</div>
<div class="box" style="background:var(--yellow)">Analysis · AI<small>прапорці + рекомендації</small></div>
<div class="arrow">→</div>
<div class="box">Соцслужби<small>кейс + втручання</small></div>
</div>

<p class="muted" style="margin-top:16px">Бекенд-референс уже працює: 6 мікросервісів + API-шлюз, асинхронні події між відомствами, Swagger на кожному сервісі, демо в один docker-команду.</p>

<!-- Спікер: Це не концепт-слайд — це працюючий бекенд. Кожне відомство — окремий сервіс, вони обмінюються подіями, а Analysis — мозок, що зводить усе в прапорці та дії. -->

---

## Як ми закриваємо всі 4 напрями челенджу

| Напрям челенджу | Що ми побудували |
|---|---|
| **01 · Data integration** | Мікросервіс на відомство (Освіта, Медицина, Соц, Ювенальна) + інтеграційні події через RabbitMQ → єдиний електронний профіль дитини |
| **02 · Risk detection** | Детермінований двигун правил + AI-ready: багаторівневі прапорці 🟡🟠🔴, мисмеч профілю, ризик вступу, булінг, здоровʼя |
| **03 · Targeted response** | Червоні прапорці **автоматично** генерують міжвідомчі направлення → кейси в соцслужбах + нотифікації відповідальним |
| **04 · Management dashboard** | KPI та аналітика по рівнях: учень → клас → школа → громада → область → країна, з ролями й територіальним скоупом |

<!-- Спікер: Прямий маппінг. Жоден напрям челенджу не залишився на папері — під кожен є реальний код. Зліва — їхні слова, справа — наші артефакти. -->

---

## Архітектура

<div class="flow">
<div class="box" style="background:var(--yellow)">API Gateway · YARP<small>єдина точка входу :8080</small></div>
</div>

<div class="flow" style="margin-top:12px">
<div class="box">Education :5101<small>профіль, теми, оцінки</small></div>
<div class="box">Social :5102<small>кейси</small></div>
<div class="box">Medical :5103<small>звернення</small></div>
<div class="box">Juvenile :5104<small>булінг</small></div>
<div class="box" style="background:var(--yellow)">Analysis · AI :5105<small>прапорці, профілі, вступ</small></div>
<div class="box">Notifications :5106<small>направлення</small></div>
</div>

<div class="flow" style="margin-top:12px">
<div class="box" style="display:block">RabbitMQ — шина подій<small>StudentProfileRecommended, MedicalConcern, BullyingReport, Referral…</small></div>
<div class="box" style="display:block">PostgreSQL на сервіс · Clean Architecture · .NET 10</div>
</div>

<p class="muted" style="margin-top:14px">Кожен сервіс автономний і замінний. Нове відомство (напр. цивільний реєстр / РАЦС, ПМСД) підключається як ще один продюсер подій — без переписування ядра.</p>

<!-- Спікер: Мікросервіси на чисту архітектуру. Важливо для журі: масштабованість закладена. Додати реєстр народжень чи медреєстр — це новий сервіс-продюсер, а не злам системи. -->

---

## Єдина траєкторія дитини — від народження до дорослості

<div class="flow">
<div class="box">Народження<small>цивільний реєстр*</small></div>
<div class="arrow">→</div>
<div class="box">Здоровʼя<small>ПМСД, щеплення*</small></div>
<div class="arrow">→</div>
<div class="box" style="background:var(--yellow)">Школа 1–9<small>відвідування, оцінки</small></div>
<div class="arrow">→</div>
<div class="box" style="background:var(--yellow)">Профіль 9–11<small>реформа 2027</small></div>
<div class="arrow">→</div>
<div class="box" style="background:var(--yellow)">Вступ / Університет<small>fit + попит</small></div>
<div class="arrow">→</div>
<div class="box">Дорослість</div>
</div>

<div class="cols" style="margin-top:18px">
<div class="col">
<h3>Де ми вже глибокі</h3>
<p class="muted">Шкільний вік → профіль → вступ. Тут реформа 2027 і найбільший грошовий ризик — тому ми зробили цей сегмент по-справжньому розумним.</p>
</div>
<div class="col">
<h3>Як розширюється</h3>
<p class="muted">* Реєстр народжень, медичні реєстри, опіка — підключаються тією ж подієвою моделлю. Архітектура вже готова приймати ці сигнали.</p>
</div>
</div>

<!-- Спікер: Челендж хоче траєкторію «від народження». Ми чесні: найглибше зробили шкільно-профільний сегмент, бо там реформа і там гроші. Але архітектура — це конвеєр подій, у який інші реєстри вмикаються природно. -->

---

## Двигун ризиків: світлофор, а не «так/ні»

| Сигнал | Рівень | Кого інформуємо / дія |
|---|---|---|
| 10 пропусків без причини | <span class="pill p-org">ORANGE</span> | батьки + адміністрація |
| 20 пропусків без причини | <span class="pill p-red">RED</span> | ескалація: соцслужби + ювенальна поліція |
| Середній бал < 4/12 | <span class="pill p-yel">YELLOW</span> | класний керівник + батьки |
| Бажаний профіль ≠ рекомендований | <span class="pill p-yel">YELLOW</span> | профорієнтаційна консультація |
| Повторні звернення (медкатегорія) | <span class="pill p-org">ORANGE</span> | направлення до спеціаліста |
| Булінг у класі | <span class="pill p-red">RED</span> | ризик усьому класу → поліція + офіцер безпеки |

<p class="muted">Кожен рівень має свою політику ескалації. Правила — чистий, повністю тестований домен (працює навіть без AI); AI лише підсилює формулювання й поради.</p>

<!-- Спікер: Прапорці — це світлофор із чіткою політикою на кожен колір. Ядро детерміноване й покрите тестами, тож воно надійне й пояснюване — критично для державної системи, де рішення треба обґрунтовувати. -->

---

## Профіль: рекомендація за даними, а не наосліп

<div class="cols">
<div class="col">

**Topic-aware scoring.** Оцінка несе **тему** (напр. «Фінансове право» всередині Правознавства). Теми важать більше за середній бал предмета.

- Кластери рахуються з **баєсівським згладжуванням** — один тонкий сигнал не переважує добре підтверджений
- Двигун рекомендує **кластер + найсильніші профілі** в ньому
- Порівнює з **бажаним** → прапорець мисмечу
- Результат пишеться назад у профіль учня (`StudentProfileRecommended`)

</div>
<div class="col">

<div class="box" style="display:block; background:var(--yellow)">
<strong>Приклад: Марія</strong>
<small style="color:#0B0B0B">Обрала математичний профіль, але дані показують силу у філології → система рекомендує суспільно-гуманітарний кластер і піднімає 🟡 «зміна профілю» з консультацією.</small>
</div>

<p class="muted" style="margin-top:14px">Так дитина потрапляє у свою траєкторію <strong>до</strong> того, як держава профінансує чужу.</p>

</div>
</div>

<!-- Спікер: Це серце «по профілю не спрацьовує». Ми не вгадуємо — рахуємо по темах із згладжуванням, рекомендуємо кластер і ловимо розбіжність із бажанням дитини. Приклад Марії — у демо. -->

---

## Адресна відповідь: університетський fit + план покращення

<div class="cols">
<div class="col">

**UniversityFitCalculator** зіставляє оцінки з ключовими напрямами спеціальностей:

- **Fit** — оцінка 0–1 і чи проходить конкурентний поріг
- **Gaps** — конкретно: «підняти Фінансове право з 7 до 10»
- **Demand** — знеособлений попит у виші та громади (жодного учня поіменно)

</div>
<div class="col">

**ImprovementPlanBuilder** перетворює розрив на план:
- ціль — 10/12, топ-6 зон зростання за величиною розриву
- AI-коуч робить із розриву зрозумілу пораду

<div class="box" style="display:block; margin-top:10px">
Дитина бачить не «ти не туди», а <strong>покроковий маршрут «куди і як дотягнути»</strong>.
</div>

</div>
</div>

<!-- Спікер: Виявити ризик мало — челендж хоче адресну відповідь. Ми даємо дитині конкретний план, а вишам і громадам — знеособлений попит для планування місць. Це і є проактивне втручання. -->

---

## Міжвідомча координація = case management

<div class="flow">
<div class="box" style="background:var(--red); color:#fff">🔴 Red flag<small style="color:#fff">напр. 20 пропусків / булінг</small></div>
<div class="arrow">→</div>
<div class="box">Referral<small>автоматичне направлення</small></div>
<div class="arrow">→</div>
<div class="box">Notification<small>відповідальному спеціалісту</small></div>
<div class="arrow">→</div>
<div class="box" style="background:var(--yellow)">Social case<small>відкритий кейс + ведення</small></div>
</div>

<p style="margin-top:18px">Червоні прапорці **самі** запускають ланцюг: направлення → нотифікація → кейс у соцслужбах. Жоден сигнал не «загубиться» в листуванні між відомствами.</p>

<p class="muted">Це і є «equip child protection services with a data-based tool» з челенджу: рішення приймається на даних, а не на телефонному дзвінку.</p>

<!-- Спікер: Тут закриваємо case management. Red автоматично перетворюється на направлення й кейс. Прибираємо «людський провал координації» між відомствами — головну причину реактивності. -->

---

## Управлінський дашборд + ролі + приватність

<div class="cols">
<div class="col">

**Дашборд** (напрям 04):
- KPI: усього/відкритих прапорців, розподіл за рівнями
- Зріз по школах і громадах: де найбільше критичних
- Розбивка за типами ризику (пропуски, мисмеч, вступ)
- Поради керівнику з кількістю кейсів

</div>
<div class="col">

**Приватність дитини — за дизайном:**
- **RBAC + територіальний скоуп**: область/громада/школа бачать лише своє
- **Знеособлений попит** для вишів — жодного учня поіменно
- Мінімізація даних, події версіоновані, аудит

</div>
</div>

<p class="muted" style="margin-top:8px">Для системи про дітей приватність — не опція. Ми показуємо агрегати керівнику й персональні дані — лише тому, хто має право й територію.</p>

<!-- Спікер: Дашборд для управлінців — і одразу знімаємо головне заперечення журі: дані дітей. Рольовий доступ, територіальний скоуп, знеособлення попиту. Приватність вшита, а не пришита. -->

---

<!-- _class: dark -->

## Демо-сценарій: один день системи

<div class="flow">
<div class="box">1 · Олег<small>12 пропусків</small></div>
<div class="arrow">→</div>
<div class="box">Analysis<small>🟠 прапорець відвідування</small></div>
</div>
<div class="flow" style="margin-top:10px">
<div class="box">2 · Марія<small>матем. профіль</small></div>
<div class="arrow">→</div>
<div class="box" style="background:var(--yellow); color:#0B0B0B">🟡 мисмеч → гуманітарний<small style="color:#0B0B0B">+ university fit + план</small></div>
</div>
<div class="flow" style="margin-top:10px">
<div class="box">3 · Медицина<small>3-тє звернення</small></div>
<div class="box">4 · Булінг у 9-А<small>🔴 ризик класу</small></div>
<div class="arrow">→</div>
<div class="box">Referrals → Notifications → Social cases</div>
</div>

<p class="muted" style="margin-top:16px"><code>scripts/demo.ps1</code> проганяє це end-to-end через шлюз :8080 — від сирих подій до відкритих кейсів і дашборду.</p>

<!-- Спікер: Це наш живий сценарій одним скриптом. Олег — відвідування. Марія — профільний мисмеч і повний маршрут виправлення. Медицина й булінг — міжвідомча ескалація в реальні кейси. За дві хвилини видно всю екосистему в дії. -->

---

## Вплив: вперше тренд розвертається вниз

<div class="cols">
<div class="col">

Без системи частка «не за фахом» **зростає** до насичення ~85%.

З раннім наведенням навіть **консервативний сценарій** (20% дослухались) **розвертає тренд донизу** — вперше частка падає, а не росте.

<p class="muted">Модель — прозора оцінка (не валідований прогноз): sₜ = floor + (sₜ₋₁ − floor)·(1 − cohortRenewal·adoption). Формула й припущення вшиті в систему.</p>

</div>
<div class="col">

<div class="kpi"><div class="big">−13…23 в.п.</div><div class="lbl">«не за фахом» до 2030 (20–50% adoption)</div></div>
<div class="kpi" style="margin-top:12px"><div class="big">×0 ризиків</div><div class="lbl">втрачених у міжвідомчому листуванні — все стає кейсом</div></div>

</div>
</div>

<p class="muted">Кожен відсотковий пункт — це і сотні мільйонів гривень, і тисячі дітей у власній траєкторії. Економіка й права тут — одне й те саме число.</p>

<!-- Спікер: Не обіцяємо чудес. Показуємо: будь-яке раннє наведення вперше розвертає криву вниз. Кожен пункт — це і гроші, і діти. Свідомо консервативно — щоб цифрам можна було вірити. -->

---

## Технології та чому ми

<div class="cards">
<div class="card"><h3>Working backend</h3><p>.NET 10, 6 мікросервісів + YARP-шлюз. Не слайди — запускається в одну docker-команду.</p></div>
<div class="card"><h3>AI-ready, не AI-залежне</h3><p>Детерміноване ядро правил (тестоване) + AI-провайдери поверх. Працює й офлайн.</p></div>
<div class="card"><h3>Подієва інтеграція</h3><p>RabbitMQ, версіоновані контракти. Нове відомство = новий продюсер.</p></div>
<div class="card" style="background:var(--yellow); border-color:var(--ink)"><h3>Прив'язано до реформи 2027</h3><p>Direction→Cluster→Profile змодельовано в ядрі. Готові саме до того, що відбувається.</p></div>
</div>

<!-- Спікер: Чому нам можна довірити пілот. Це працюючий, тестований, масштабований бекенд, змодельований під реформу 2027 — а не презентація майбутнього. -->

---

## Дорожня карта та заклик

<div class="cols">
<div class="col">

**Next:**
1. Пілот в одній громаді (реальні школи)
2. Підключити цивільний реєстр і медреєстр як продюсери подій
3. Валідувати моделі ризику на реальних когортах
4. Фронтенд-дашборд для управлінців (API вже готові)

</div>
<div class="col">

<div class="box" style="display:block; background:var(--yellow)">
<strong>Заклик</strong>
<small style="color:#0B0B0B">Дайте нам пілотну громаду й доступ до тестових даних реєстрів — і ми перетворимо реактивний захист на ранній сигнал по кожній дитині.</small>
</div>

</div>
</div>

<h2 style="margin-top:24px">Кожна дитина — у своїй траєкторії. Вчасно.</h2>

<!-- Спікер: Підсумок. Просимо пілот і тестові дані. Обіцяємо: ранній сигнал по кожній дитині замість реакції на кризу. Дякую — готові до запитань. -->

---

<!-- _class: lead -->
<!-- _paginate: false -->

<span class="kicker">Дякуємо · Q&A</span>

# Готові показати в дії

### `docker compose -f deploy/docker-compose.yml up --build` → `scripts/demo.ps1`

<p class="muted" style="margin-top:18px">Бекенд-референс · .NET 10 · 6 мікросервісів · AI-двигун аналізу · модель реформи профільної освіти 2027</p>

<!-- Спікер: Залишаємо контакти й пропонуємо запустити демо просто зараз. -->
