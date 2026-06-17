# API reference

All services expose interactive **Swagger UI** at `/swagger` on their own port.
Through the gateway (`:8080`) the same endpoints are reachable under a service prefix,
e.g. `GET http://localhost:8080/education/api/students`.

| Service        | Direct port | Gateway prefix   | Swagger (direct)              |
| -------------- | ----------- | ---------------- | ----------------------------- |
| Education      | 5101        | `/education`     | http://localhost:5101/swagger |
| Social         | 5102        | `/social`        | http://localhost:5102/swagger |
| Medical        | 5103        | `/medical`       | http://localhost:5103/swagger |
| Juvenile Police| 5104        | `/juvenile`      | http://localhost:5104/swagger |
| Analysis       | 5105        | `/analysis`      | http://localhost:5105/swagger |
| Notifications  | 5106        | `/notifications` | http://localhost:5106/swagger |
| API Gateway    | 8080        | —                | —                             |

## Education (`5101`)

| Method | Route                               | Purpose                                       |
| ------ | ----------------------------------- | --------------------------------------------- |
| GET    | `/api/students`                     | List pupils (`?schoolId=`, `?classId=`).      |
| GET    | `/api/students/{id}`                | Pupil profile: attendance + subject averages. |
| POST   | `/api/students/{id}/attendance`     | Record attendance; evaluates the red-flag policy and publishes an event past threshold. |
| POST   | `/api/students/{id}/grades`         | Record a subject grade (1–12).                |
| GET    | `/api/schools/{id}/overview`        | School-level aggregated overview.             |

## Analysis (`5105`)

| Method | Route                                          | Purpose                                  |
| ------ | ---------------------------------------------- | ---------------------------------------- |
| GET    | `/api/analysis/models`                         | Available analysis models.               |
| POST   | `/api/analysis/students/{studentId}/run`       | Run analysis for a pupil (`?model=`).    |
| POST   | `/api/analysis/schools/{schoolId}/run`         | Run school-wide analysis + aggregation.  |
| GET    | `/api/analysis/runs`                           | Recent analysis runs (audit trail).      |
| GET    | `/api/red-flags`                               | List red flags (`?severity=&scope=&subjectId=&status=`). |
| POST   | `/api/red-flags/{id}/acknowledge`              | Acknowledge a red flag.                  |
| POST   | `/api/red-flags/{id}/resolve`                  | Resolve a red flag.                      |
| GET    | `/api/recommendations`                         | List recommendations (`?scope=&subjectId=`). |
| GET    | `/api/dashboard/summary`                       | Dashboard KPIs.                          |

## Medical (`5103`)

| Method | Route                    | Purpose                                                |
| ------ | ------------------------ | ------------------------------------------------------ |
| GET    | `/api/medical/visits`    | List visits (`?studentId=`).                           |
| POST   | `/api/medical/visits`    | Record a visit; raises a medical concern past threshold. |

## Juvenile Police (`5104`)

| Method | Route                            | Purpose                                          |
| ------ | -------------------------------- | ------------------------------------------------ |
| GET    | `/api/juvenile/bullying-reports` | List bullying reports (`?classId=`).             |
| POST   | `/api/juvenile/bullying-reports` | File a bullying report; publishes a class signal. |

## Social Services (`5102`)

| Method | Route                | Purpose                                              |
| ------ | -------------------- | ---------------------------------------------------- |
| GET    | `/api/social/cases`  | List cases (`?subjectId=`).                          |
| POST   | `/api/social/cases`  | Open a case manually. (Cases also open automatically from referrals.) |

## Notifications (`5106`)

| Method | Route                  | Purpose                                          |
| ------ | ---------------------- | ------------------------------------------------ |
| GET    | `/api/notifications`   | List dispatched notifications (`?subjectId=`).   |
| GET    | `/api/referrals`       | List inter-agency referrals (`?toAgency=`).      |
| POST   | `/api/referrals`       | Raise a referral manually and publish it.        |

## Integration events (contracts)

| Event                                    | Published by     | Consumed by               |
| ---------------------------------------- | ---------------- | ------------------------- |
| `AttendanceThresholdReachedIntegrationEvent` | Education    | Analysis                  |
| `GradesDeclinedIntegrationEvent`         | Education        | Analysis                  |
| `MedicalConcernReportedIntegrationEvent` | Medical          | Analysis                  |
| `BullyingReportFiledIntegrationEvent`    | Juvenile Police  | Analysis                  |
| `RedFlagRaisedIntegrationEvent`          | Analysis         | Notifications             |
| `RecommendationIssuedIntegrationEvent`   | Analysis         | Notifications             |
| `InterAgencyReferralRequestedIntegrationEvent` | Notifications | Social Services         |
