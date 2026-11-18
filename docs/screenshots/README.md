# Application Screenshots Guide

This directory holds genuine screenshots captured from the live running application (`http://localhost:5000` or `http://localhost:5101`).

### Screenshot Checklist & Naming Convention

| File Name | View / Component | URL / Action | Description |
| :--- | :--- | :--- | :--- |
| `01-login.png` | Sign-in Screen | `/Account/Login` | Login screen with Ashoka emblem / de-addiction portal header. |
| `02-dashboard.png` | Executive Analytics | `/Dashboard` | 9 KPI metric cards and Chart.js graphs. |
| `03-centres-directory.png` | Centres List | `/Centres` | Tabular list of facilities with state/district filters. |
| `04-centre-create.png` | Register Centre | `/Centres/Create` | Facility registration form with state dropdown. |
| `05-beneficiaries-list.png` | Beneficiary Directory | `/Beneficiaries` | Paginated patient register with search. |
| `06-beneficiary-registration.png` | Beneficiary Intake | `/Beneficiaries/Create` | Patient registration with deterministic code generation. |
| `07-beneficiary-dossier.png` | Clinical Dossier | `/Beneficiaries/Details/{id}` | Patient history aggregating counselling, treatments, and follow-ups. |
| `08-counselling-sessions.png` | Counselling Register | `/Counselling` | Psychological sessions list (confidential notes omitted). |
| `09-interventions-register.png` | Medical Interventions | `/Interventions` | Medical detox and pharmacotherapy records. |
| `10-followups-tracking.png` | Recovery Follow-ups | `/FollowUps` | Scheduled visits with overdue badge tracking. |
| `11-reports-analytics.png` | SQL Reports & CSV Export | `/Reports` | State/centre summaries and CSV export buttons. |
| `12-swagger-ui.png` | REST API OpenAPI | `/swagger` | Interactive Swagger UI displaying `/api/v1` routes. |
| `13-postman-api-execution.png`| Postman Test Run | Collection Runner | Postman executing requests with `200 OK` status assertions. |
| `14-test-suite-results.png` | Automated Test Suite | Terminal Console | PowerShell running `dotnet test` (25/25 passed). |

> [!NOTE]
> Save captured `.png` images directly in this folder (`docs/screenshots/`). The root `README.md` references these relative paths.

