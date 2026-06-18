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
    font-size: 23px;
    padding: 52px 64px;
    letter-spacing: 0.1px;
  }
  h1 { font-family: 'Archivo Black', sans-serif; font-size: 56px; line-height: 1.02; margin: 0 0 8px; }
  h2 { font-family: 'Archivo Black', sans-serif; font-size: 36px; line-height: 1.05; margin: 0 0 18px; }
  h2::after { content: ""; display: block; width: 84px; height: 10px; background: var(--yellow); margin-top: 10px; border-radius: 2px; }
  h3 { font-family: 'Space Grotesk', sans-serif; font-weight: 700; font-size: 25px; margin: 6px 0; }
  strong { font-weight: 700; }
  a { color: var(--blue-deep); }
  ul { margin: 6px 0; } li { margin: 9px 0; }
  code { font-family: 'Space Mono', monospace; background: rgba(11,11,11,0.07); padding: 1px 6px; border-radius: 4px; font-size: 0.92em; }

  .mono { font-family: 'Space Mono', monospace; }
  .muted { color: rgba(11,11,11,0.62); font-size: 15px; line-height: 1.4; }
  .tag { background: var(--yellow); padding: 1px 8px; font-weight: 700; border-radius: 3px; }
  .kicker { font-family: 'Space Mono', monospace; font-weight: 700; letter-spacing: 3px; text-transform: uppercase; font-size: 16px; color: var(--blue-deep); }

  .cols { display: flex; gap: 26px; align-items: flex-start; }
  .col { flex: 1; }

  .cards { display: grid; grid-template-columns: 1fr 1fr; gap: 18px; margin-top: 8px; }
  .card { background: var(--card); border: 2px solid var(--ink); border-radius: 10px; padding: 16px 18px; }
  .card .num { font-family: 'Space Mono', monospace; font-weight: 700; font-size: 28px; letter-spacing: 4px; }
  .card h3 { margin: 2px 0 6px; font-size: 22px; }
  .card p { margin: 0; font-size: 17px; line-height: 1.35; }

  .kpis { display: flex; gap: 18px; margin: 14px 0; }
  .kpi { flex: 1; background: var(--card); border: 2px solid var(--ink); border-radius: 10px; padding: 18px; text-align: center; }
  .kpi .big { font-family: 'Archivo Black', sans-serif; font-size: 44px; line-height: 1; }
  .kpi .lbl { font-size: 15px; color: rgba(11,11,11,0.72); margin-top: 8px; }

  .flow { display: flex; align-items: stretch; gap: 10px; flex-wrap: wrap; }
  .box { background: var(--card); border: 2px solid var(--ink); border-radius: 8px; padding: 11px 14px; font-size: 16px; font-weight: 700; display: flex; flex-direction: column; align-items: flex-start; justify-content: center; }
  .box small { display:block; margin-top: 3px; font-weight: 400; line-height: 1.25; color: rgba(11,11,11,0.65); }
  .arrow { display:flex; align-items:center; font-weight:700; font-size: 22px; }

  .hero { background: var(--yellow); border: 3px solid var(--ink); border-radius: 12px; padding: 20px 24px; font-size: 26px; font-weight: 700; line-height: 1.25; }

  table { border-collapse: collapse; width: 100%; font-size: 19px; }
  th { background: var(--ink); color: var(--blue); text-align: left; padding: 9px 12px; }
  td { border-bottom: 1px solid var(--line); padding: 9px 12px; vertical-align: top; }
  tr:nth-child(even) td { background: rgba(255,255,255,0.28); }

  .pill { display:inline-block; font-family:'Space Mono',monospace; font-weight:700; font-size:13px; padding:1px 8px; border-radius:20px; color:#fff; }
  .p-red{background:var(--red)} .p-org{background:var(--orange)} .p-yel{background:#C9A400} .p-grn{background:var(--green)}

  footer { font-family: 'Space Mono', monospace; font-size: 12px; color: rgba(11,11,11,0.55); }
  section::after { font-family: 'Space Mono', monospace; color: rgba(11,11,11,0.55); }

  section.lead { display: flex; flex-direction: column; justify-content: center; }
  section.lead h1 { font-size: 70px; }
  section.dark { background: var(--ink); color: var(--blue); }
  section.dark h1, section.dark h2 { color: #fff; }
  section.dark h2::after { background: var(--yellow); }
  section.dark .muted { color: rgba(255,255,255,0.6); }
  section.dark .box { background: rgba(255,255,255,0.06); border-color: rgba(255,255,255,0.5); color: #fff; }
---

<!-- _class: lead -->
<!-- _paginate: false -->

<span class="kicker">Challenge 1 · Child Rights Risk Monitoring</span>

# Моніторинг ризиків прав дитини

### Єдина міжвідомча система, що бачить ризик у житті дитини — і діє **до** кризи, а не після.

<div class="kpis">
<div class="kpi"><div class="big">до ~59 млрд ₴</div><div class="lbl">щороку повз ціль — освіта, що «не спрацьовує за профілем»</div></div>
<div class="kpi"><div class="big">64–80%</div><div class="lbl">випускників працюють не за фахом</div></div>
<div class="kpi"><div class="big">2027</div><div class="lbl">реформа профілів — інфраструктуру треба зараз</div></div>
</div>

<!-- Спікер (0:00–0:25): Щороку держава вкладає десятки мільярдів у освіту, яка не конвертується в роботу за фахом — до 80% випускників. А права дитини на розвиток порушуються тихо, без сигналу. У 2027 реформа профілів зробить цей вибір ще дорожчим. Ми побудували систему, що ловить ризик заздалегідь. -->

---

## Проблема

<div class="cards">
<div class="card"><div class="num">01</div><h3>Фрагментовані дані</h3><p>Дитина розпорошена між реєстрами: освіта, здоровʼя, поліція, цивільний стан — без наскрізної координації.</p></div>
<div class="card"><div class="num">02</div><h3>Реакція постфактум</h3><p>Держава втручається <strong>після</strong> кризи, коли право вже порушене.</p></div>
<div class="card"><div class="num">03</div><h3>Немає раннього сигналу</h3><p>Жодна система не виявляє ризик рано й не запускає адресну підтримку.</p></div>
<div class="card" style="background:var(--yellow)"><div class="num">04</div><h3>Прихована ціна</h3><p>Помилкові траєкторії = змарновані бюджетні кошти + втрачений потенціал.</p></div>
</div>

<!-- Спікер (0:25–0:50): Челендж називає три болі: дані розірвані, реакція запізніла, раннього сигналу немає. Ми додаємо четвертий — у цього є грошова ціна. Саме її покажемо. -->

---

## Інсайт: один сигнал — два врятовані ризики

Коли **бажаний профіль ≠ профіль за даними** — це ранній сигнал: дитина в чужій траєкторії, а бюджетне місце витрачається даремно.

<div class="hero" style="margin-top:18px">
Той самий сигнал = ризик прав дитини (ст. 28–29 Конвенції ООН) <strong>І</strong> ризик бюджету.<br>
Ловимо його ще в 9 класі — рятуємо і дитину, і кошти.
</div>

<!-- Спікер (0:50–1:15): Головна думка. Мисмеч профілю — не профорієнтаційна дрібниця. Це порушення права дитини на розвиток, яке ще й коштує державі грошей. Виявивши його рано, ми закриваємо обидва ризики одним рухом. -->

---

## Рішення: одна екосистема замість шести систем

<div class="flow">
<div class="box">Освіта<small>відвідування, оцінки за темами</small></div>
<div class="arrow">→</div>
<div class="box">Медицина<small>повторні звернення</small></div>
<div class="arrow">→</div>
<div class="box">Ювенальна поліція<small>булінг</small></div>
<div class="arrow">→</div>
<div class="box" style="background:var(--yellow)">Analysis · AI<small>прапорці + рекомендації</small></div>
<div class="arrow">→</div>
<div class="box">Соцслужби<small>кейс + втручання</small></div>
</div>

<p style="margin-top:16px">Закриває <strong>усі 4 напрями челенджу</strong>: <span class="tag">01</span> інтеграція даних · <span class="tag">02</span> виявлення ризиків · <span class="tag">03</span> адресна відповідь · <span class="tag">04</span> управлінський дашборд.</p>

<p class="muted">Не концепт — працюючий бекенд: .NET 10, 6 мікросервісів + API-шлюз, події RabbitMQ, демо в одну docker-команду.</p>

<!-- Спікер (1:15–1:45): Кожне відомство — окремий сервіс, вони обмінюються подіями, Analysis зводить усе в прапорці й дії. Це одразу закриває всі чотири напрями челенджу. І це не слайди — це працюючий бекенд, що стартує однією командою. -->

---

## Ранній сигнал, що сам запускає дію

<table>
<tr><th>Сигнал</th><th>Рівень</th><th>Автоматична дія</th></tr>
<tr><td>20 пропусків без причини</td><td><span class="pill p-red">RED</span></td><td>ескалація → соцслужби + ювенальна поліція</td></tr>
<tr><td>Булінг у класі</td><td><span class="pill p-red">RED</span></td><td>ризик усьому класу → поліція + офіцер безпеки</td></tr>
<tr><td>Бажаний профіль ≠ рекомендований</td><td><span class="pill p-yel">YELLOW</span></td><td>профорієнтаційна консультація</td></tr>
<tr><td>Повторні звернення (медкатегорія)</td><td><span class="pill p-org">ORANGE</span></td><td>направлення до спеціаліста</td></tr>
</table>

<p style="margin-top:14px"><strong>Червоний прапорець сам створює направлення й кейс</strong> — жоден сигнал не губиться між відомствами.</p>

<!-- Спікер (1:45–2:10): Прапорці — це світлофор із чіткою дією на кожен колір. Найважливіше: червоний автоматично перетворюється на міжвідомче направлення й кейс у соцслужбах. Прибираємо головну причину реактивності — провал координації між відомствами. -->

---

<!-- _class: dark -->

## Демо за 30 секунд

<div class="flow">
<div class="box">Марія<small>матем. профіль</small></div>
<div class="arrow">→</div>
<div class="box" style="background:var(--yellow); color:#0B0B0B">🟡 мисмеч → правильний кластер<small style="color:#0B0B0B">+ план «куди і як дотягнути»</small></div>
</div>
<div class="flow" style="margin-top:12px">
<div class="box">Олег<small>20 пропусків</small></div>
<div class="arrow">→</div>
<div class="box" style="background:var(--red); color:#fff">🔴 авто-кейс у соцслужбах<small style="color:#fff">+ медицина й булінг ескалюються самі</small></div>
</div>

<p class="muted" style="margin-top:18px"><code>scripts/demo.ps1</code> — повний наскрізний прогін через шлюз :8080: від сирих подій до кейсів і дашборду.</p>

<!-- Спікер (2:10–2:35): Наживо. Марія обрала математику, але дані показують філологію — система рекомендує правильний кластер і дає план. Олег з 20 пропусками — система сама відкриває кейс у соцслужбах. Усе наскрізно, одним скриптом. -->

---

## Вплив

<div class="cols">
<div class="col">
<div class="kpi"><div class="big">тренд ↓</div><div class="lbl">вперше частка «не за фахом» падає, а не росте</div></div>
</div>
<div class="col">
<div class="kpi"><div class="big">= ₴ + діти</div><div class="lbl">кожен в.п. — сотні млн грн і тисячі дітей у своїй траєкторії</div></div>
</div>
<div class="col">
<div class="kpi"><div class="big">0</div><div class="lbl">сигналів, втрачених у міжвідомчому листуванні</div></div>
</div>
</div>

<p class="muted">Економічний ефект — прозора оцінка (формула вшита в систему), не валідований прогноз. Цифри ризику — відкриті дані з діапазонами.</p>

<!-- Спікер (2:35–2:50): Навіть консервативний сценарій уперше розвертає криву «не за фахом» донизу. Кожен відсотковий пункт — це і гроші, і діти. Економіка й права тут — одне й те саме число. -->

---

<!-- _class: lead -->

## Кожна дитина — у своїй траєкторії. Вчасно.

### Дайте пілотну громаду й тестові дані реєстрів — і ми перетворимо реакцію на ранній сигнал по кожній дитині.

<p class="mono" style="margin-top:20px; font-size:18px">docker compose up --build → scripts/demo.ps1 · .NET 10 · 6 мікросервісів · AI-аналіз · модель реформи 2027</p>

<!-- Спікер (2:50–3:00): Просимо пілот і тестові дані. Обіцяємо: ранній сигнал по кожній дитині замість реакції на кризу. Дякую — готові показати в дії. -->
