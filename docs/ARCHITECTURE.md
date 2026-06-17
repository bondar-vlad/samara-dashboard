# Architecture

The Child Rights Monitoring Dashboard is a microservices platform that ingests data
from several government agencies, analyses it (with rules and/or AI), raises tiered
**red flags**, issues **profiling recommendations**, and coordinates **cross-agency
actions** — all at multiple levels of the education hierarchy (pupil → class → school
→ community → region → country).

## 1. Solution layout

```
samara-dashboard/
├─ ChildRights.slnx                 # solution (new .slnx format, .NET 10)
├─ global.json                      # pins the .NET 10 SDK
├─ Directory.Build.props            # shared build settings (net10.0, nullable, …)
├─ Directory.Packages.props         # central NuGet package versions
├─ deploy/                          # Dockerfile + docker-compose for the demo
├─ scripts/                         # run-local.ps1, demo.ps1
├─ docs/                            # this folder
└─ src/
   ├─ BuildingBlocks/               # shared kernel + cross-cutting infrastructure
   │  ├─ ChildRights.BuildingBlocks.Domain
   │  ├─ ChildRights.BuildingBlocks.Application
   │  ├─ ChildRights.BuildingBlocks.Infrastructure
   │  └─ ChildRights.Contracts      # integration events (public service contracts)
   ├─ ApiGateway/
   │  └─ ChildRights.ApiGateway     # YARP reverse proxy (single entry point)
   └─ Services/
      ├─ Education/                 # 4-layer clean architecture (reference service)
      ├─ Analysis/                  # 4-layer clean architecture (AI "brain")
      ├─ Social/                    # agency service (single Web API project)
      ├─ Medical/                   # agency service
      ├─ JuvenilePolice/            # agency service
      └─ Notifications/             # fan-out + inter-agency referrals
```

## 2. Per-service clean architecture

The two information-rich services (**Education**, **Analysis**) use a four-project
clean/onion architecture. Dependencies always point **inwards**:

```
Api  ─►  Infrastructure  ─►  Application  ─►  Domain
                 └──────────────►──────────────┘
```

| Layer            | Responsibility                                                            | Depends on            |
| ---------------- | ------------------------------------------------------------------------- | --------------------- |
| `Domain`         | Entities, value objects, enums, **pure business rules/policies**          | BuildingBlocks.Domain |
| `Application`    | CQRS commands/queries + handlers, DTOs, ports (interfaces)                | Domain                |
| `Infrastructure` | EF Core, message bus, HTTP clients, AI providers, schedulers              | Application           |
| `Api`            | Thin controllers, request models, composition root                        | Infrastructure        |

The four agency/notification services are intentionally lighter — a single Web API
project with internal `Domain` / `Persistence` / `Controllers` / `Messaging` folders —
because their job is mostly to record events and publish/consume integration messages.

## 3. Building blocks (shared kernel)

- **Domain** — `Entity`, `AggregateRoot`, `ValueObject`, `Result`/`Error` (the Result
  pattern), and the cross-service enums: `Agency`, `FlagSeverity` (Green/Yellow/Orange/Red),
  `AnalysisScope`, `AudienceRole`, `AnalysisTrigger`.
- **Application** — a tiny in-house CQRS mediator (`IDispatcher`) with a FluentValidation
  pipeline, plus ports (`IEventPublisher`, `IUnitOfWork`, `IClock`, `IDataSeeder`).
  No third-party mediator dependency.
- **Infrastructure** — one-line host setup (`AddServiceDefaults`/`UseServiceDefaults`):
  Serilog, RFC 7807 ProblemDetails, health checks, Swagger; the message bus
  (`AddEventBus`, MassTransit over RabbitMQ with an in-memory fallback); EF Core
  PostgreSQL registration; and a shared `ApiControllerBase` that maps `Result` → HTTP.
- **Contracts** — the versioned integration events exchanged between services.

## 4. Red-flag model (tiered)

Severity is a **traffic-light tier**, not a binary, so escalation policy differs per level:

| Severity | Meaning            | Typical action                                          |
| -------- | ------------------ | ------------------------------------------------------- |
| Green    | no concern         | none                                                    |
| Yellow   | advisory           | handled inside the school (class teacher)               |
| Orange   | elevated           | notify parents + administration                         |
| Red      | urgent             | **cross-agency referral** (social services / police / medical) |

Example rules implemented in `Analysis.Domain` (`StudentRiskRules`):

- **Attendance** — 5 → Yellow, 10 → Orange (parents/admin), 20 → Red (escalate).
- **Academic risk** — any subject average below 4/12.
- **Profile mismatch** — declared specialisation underperforms a clearly stronger area.
- **Medical** (from the Medical service) — recurring condition category.
- **Bullying** (from Juvenile Police) — class-level signal, risk to the whole class.

## 5. Recommendations (education profiling)

`StudentRiskRules` + aggregation produce recommendations at several scopes:

- **Pupil** — which profile to choose in grade 10, or whether to change the current one.
- **School** — which profiles to open/close based on aggregated demand.
- **Community / region** — which courses to create for continuing-education academies.

## 6. AI strategy & best practices

The Analysis service treats "the model" as a swappable strategy
(`IAiAnalysisProvider` + `IAiAnalysisProviderFactory`):

- `rule-based-v1` — a deterministic engine wrapping the pure domain rules. Always
  available; the safe default and fallback.
- `gpt-4o-mini` (OpenAI-compatible) — registered **only** when an API key is configured.

Baked-in practices:

- **Pluggable models** — choose per request (`?model=`), no code change to swap.
- **Data minimisation** — no personal identifiers are sent to the LLM prompt.
- **Strict output contract** — the model must return JSON, parsed defensively.
- **Resilience** — outbound HTTP uses the standard retry/timeout/circuit-breaker handler.
- **Graceful degradation** — any LLM failure falls back to the deterministic engine.
- **Auditability** — every run is persisted (`AnalysisRun`) with its trigger and model.

### Three execution modes

| Trigger     | How                                                                  |
| ----------- | ------------------------------------------------------------------- |
| `Event`     | a consumer reacts to an integration event (e.g. attendance threshold) |
| `Scheduled` | `ScheduledAnalysisService` background sweep (`Ai:ScheduleMinutes`)   |
| `OnDemand`  | an explicit API call                                                 |

## 7. Cross-agency event flow

```mermaid
flowchart LR
    subgraph Agencies
        EDU[Education]
        MED[Medical]
        JUV[Juvenile Police]
    end

    EDU -- AttendanceThresholdReached --> AN[Analysis]
    MED -- MedicalConcernReported --> AN
    JUV -- BullyingReportFiled --> AN

    AN -- RedFlagRaised --> NO[Notifications]
    AN -- RecommendationIssued --> NO

    NO -- per-audience messages --> AUD[(Notifications store)]
    NO -- InterAgencyReferralRequested (Red) --> SOC[Social Services]
    SOC -- opens case --> CASES[(Social cases)]

    AN -- pulls profiles (HTTP) --> EDU
```

1. An agency records something and **publishes an integration event**.
2. **Analysis** consumes it, runs the selected model, persists flags/recommendations,
   and publishes `RedFlagRaised` / `RecommendationIssued`.
3. **Notifications** fans each red flag out to its target audiences and, for **Red**
   severity, raises `InterAgencyReferralRequested`.
4. **Social Services** consumes referrals addressed to it and opens a case — closing
   the loop end to end.

## 8. Data & messaging

- **Database**: PostgreSQL, **one logical database per service** (database-per-service
  pattern). Each service owns its schema (`education`, `analysis`, `social`, …) and never
  reaches into another service's tables — cross-service reads happen over HTTP, writes
  over events. For the demo the schema is created with `EnsureCreated` and seeded on
  startup; production should switch to EF Core migrations.
- **Messaging**: MassTransit. With `MessageBus:Host` set it uses RabbitMQ; otherwise it
  falls back to an in-memory transport so a single service still runs without a broker.

## 9. API gateway

`ChildRights.ApiGateway` (YARP) is the single entry point on port **8080**. It routes
`/education/**`, `/social/**`, `/medical/**`, `/juvenile/**`, `/analysis/**` and
`/notifications/**` to the corresponding service, stripping the path prefix.
