# Live Verification and Screenshot Capture Checklist

**Project:** Centralised Nasha Mukti Database (INT402)  
**Purpose:** Academic viva demonstration script and screenshot capture guide for INT402 project evaluation.

> [!IMPORTANT]
> All steps below are designed to be demonstrated on the live running application (`dotnet run`) connected to Microsoft SQL Server. Do not use fabricated or mock screenshots.

---

## 1. Application Startup & Verification

1. Start SQL Server (or verify LocalDB is running).
2. Open PowerShell in `C:\INT402` and launch the application:
   ```powershell
   dotnet run
   ```
3. Open Google Chrome or Microsoft Edge and navigate to `http://localhost:5000`.

---

## 2. Screenshot Capture Checklist (11 Critical Screens)

| # | Screen / View | URL / Action | Verification Checklist |
| :-: | :--- | :--- | :--- |
| **1** | **Secure Login Screen** | `/Account/Login` | - Clean Ministry header with Ashoka emblem / de-addiction theme.<br>- Email and password inputs with client-side validation.<br>- Remember Me checkbox.<br>- Sign in as Administrator (`admin@nashamukti.gov.in` / `Admin@12345`). |
| **2** | **Executive Analytics Dashboard** | `/Dashboard` | - 9 KPI summary metric cards (Registered Beneficiaries, Active Centres, Counselling Sessions, etc.).<br>- Interactive Chart.js graphs showing Facility Distribution & Intake Trends.<br>- Active period date filter.<br>- Top-bar displaying current logged-in administrator. |
| **3** | **Centres Directory & Search** | `/Centres` | - Paginated tabular directory of nationwide de-addiction facilities.<br>- Filter by State (Punjab, Bihar, etc.) and District.<br>- Operational status badges (`Active` / `Inactive`).<br>- Action buttons (View Details, Edit, Deactivate). |
| **4** | **Register New Centre Form** | `/Centres/Create` | - Restricted to `Admin` role.<br>- State dropdown populated from `States` table.<br>- District, Complete Address, Telephone, and Email validation.<br>- Submit and confirm redirect to index with success alert. |
| **5** | **Beneficiary Intake Directory** | `/Beneficiaries` | - Master patient intake directory.<br>- Search by unique code (`BEN-...`) or patient name.<br>- Centre affiliation and registration date.<br>- Facility isolation indicator for CentreStaff. |
| **6** | **Beneficiary Intake Registration** | `/Beneficiaries/Create` | - Full Name, Registration Date (validated <= Today), Date of Birth (validated <= Today), Gender, Contact Number.<br>- Target facility selection.<br>- Submit and verify generated unique code (e.g. `BEN-PB-2026-0001`). |
| **7** | **Integrated Patient Clinical Dossier** | `/Beneficiaries/Details/{id}` | - Patient demographic overview card.<br>- Computed age based on Date of Birth.<br>- Tabbed/Sectioned clinical history displaying Counselling Sessions, Medical Treatments, and Follow-Up Visits.<br>- Quick action buttons to record new sessions directly. |
| **8** | **Counselling Sessions Register** | `/Counselling` | - List of psychological sessions across beneficiaries.<br>- Session types: Individual, Group, Family, Relapse Prevention.<br>- Session statuses: Completed, Scheduled, Cancelled.<br>- Verify confidential clinical notes are omitted from table list for privacy. |
| **9** | **Treatment Interventions Module** | `/Interventions` | - Record of medical detoxification and pharmacotherapy services.<br>- Service date and responsible physician/staff name.<br>- Status transitions (`In Progress`, `Completed`, `Discontinued`). |
| **10**| **Recovery Follow-Ups Management**| `/FollowUps` | - Scheduled follow-up appointments.<br>- Overdue badge calculation (`ScheduledDate < Today` & `Status = Pending`).<br>- Outcome recording modal/view with rehabilitation evaluation notes. |
| **11**| **Reports, SQL Views & CSV Export** | `/Reports` | - Aggregated metrics powered by `vw_CentreServiceSummary` and `vw_StateBeneficiarySummary`.<br>- State and centre filter selection.<br>- Click **Export Summary CSV** and **Export Beneficiaries CSV**; inspect downloaded file to confirm UTF-8 BOM and formula sanitization. |

---

## 3. Web API & Postman Demonstration

1. Open browser to **Swagger UI**: `http://localhost:5000/swagger`.
2. Execute `POST /api/v1/auth/login` to retrieve JWT token.
3. Open **Postman**:
   - Open `NashaMukti_API_Postman_Collection.json`.
   - Run `1. Authenticate (Login)` — Verify response is `200 OK` and token is automatically stored.
   - Run `2. Centres - List All` — Verify `200 OK` and paginated JSON response.
   - Run `4. Beneficiaries - List Scoped` — Verify scoped beneficiary response.
   - Run `9. Reports - Summary KPIs` — Verify 9 KPIs match the dashboard figures.
4. Take screenshot of Postman executing the API with HTTP status `200 OK`.

---

## 4. SQL Server Management Studio (SSMS) Verification

1. Open SSMS and connect to your SQL Server instance (`localhost`).
2. Expand `Databases` -> `NashaMuktiDB`.
3. Expand **Views**: Verify `vw_CentreServiceSummary` and `vw_StateBeneficiarySummary` are listed.
4. Right-click `vw_CentreServiceSummary` -> **Select Top 1000 Rows**.
5. Expand **Programmability** -> **Stored Procedures**: Verify `usp_GetCentreServiceSummary`.
6. Execute stored procedure:
   ```sql
   EXEC usp_GetCentreServiceSummary @StateId = 1;
   ```
7. Take screenshot of SSMS displaying the query result grid.

---

## 5. Automated Test Suite Execution

In PowerShell terminal:
```powershell
dotnet test NashaMuktiDB.Tests/NashaMuktiDB.Tests.csproj
```
Verify the output:
```
Passed! - Failed: 0, Passed: 25, Skipped: 0, Total: 25
```
Take screenshot of the terminal displaying `25 passed` tests.

