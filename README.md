# Centralised Nasha Mukti Database (INT402)

[![.NET 8.0](https://img.shields.io/badge/.NET-8.0%20LTS-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Entity Framework Core](https://img.shields.io/badge/EF%20Core-8.0.10-512BD4)](https://learn.microsoft.com/ef/)
[![Microsoft SQL Server](https://img.shields.io/badge/Database-SQL%20Server%202022-CC292B?logo=microsoftsqlserver)](https://www.microsoft.com/sql-server)
[![Tests Passing](https://img.shields.io/badge/Tests-25%2F25%20Passing-success)](https://github.com/KrChiranjeevi/Centralised-Nasha-Mukti-Database)
[![License](https://img.shields.io/badge/License-Academic%20Evaluation-blue)]()

A comprehensive, enterprise-grade Centralised De-Addiction Management and Rehabilitation Information System developed for the **Ministry of Social Justice and Empowerment (MoSJE)** Nasha Mukti Abhiyaan initiative as part of the **INT402** academic project.

Built strictly using **C# 12, ASP.NET Core 8.0 MVC, Razor Views, Entity Framework Core 8, Microsoft SQL Server, ASP.NET Core Identity, RESTful Web API, and Chart.js**.

---

## 1. Project Overview & Features

The system connects nationwide de-addiction facilities (*Nasha Mukti Kendras*) to a unified central database, providing dual-layer role-based access for National Administrators and facility-level Centre Staff.

### Key Functional Modules
1. **Facility / Centre Directory (`/Centres`):**
   - National register of authorized rehabilitation centres across states and districts.
   - Real-time search by facility name and district, with state and operational status filters.
   - Administrator-only facility creation, profile editing, and controlled soft deactivation.
2. **Beneficiary Intake & Patient Dossier (`/Beneficiaries`):**
   - Central registration generating synthetic, non-sensitive unique codes (`BEN-<STATE>-YYYY-XXXX`).
   - Facility-level data isolation (IDOR protection) ensuring Centre Staff can only access their facility's patients.
   - Comprehensive patient clinical dossier linking counselling sessions, medical interventions, and follow-ups.
3. **Counselling Management (`/Counselling`):**
   - Structured tracking of Individual, Group, Family, Relapse Prevention, and Crisis Intervention sessions.
   - High-sensitivity clinical psychotherapy notes protected from list views, exported reports, and general logs.
4. **De-Addiction Interventions (`/Interventions`):**
   - Treatment intervention records covering Medical Detoxification, Pharmacotherapy, Psychosocial Support, and Vocational Rehabilitation.
   - Clinical status lifecycle tracking (`In Progress`, `Completed`, `Discontinued`, `Referred`).
5. **Recovery Follow-Up Tracking (`/FollowUps`):**
   - Proactive appointment scheduling and automated detection of pending vs. overdue visits.
   - Clinical recovery outcome logging with confidential rehabilitation remarks.
6. **Analytics, SQL Views & Reports (`/Reports`):**
   - Real-time Chart.js visualisations for facility distribution, intake trends, and clinical completion rates.
   - All 9 core KPIs calculated via independent subqueries to prevent Cartesian product errors.
   - Database-level analytics powered by SQL Views (`vw_CentreServiceSummary`, `vw_StateBeneficiarySummary`) and Stored Procedure (`usp_GetCentreServiceSummary`).
   - Sanitized CSV exports with UTF-8 BOM, strict clinical note privacy, and formula injection mitigation (CWE-1236).
7. **RESTful Web API (`/api/v1/...`):**
   - Full REST API with DTO request/response contracts, standardized `ApiResponse<T>` envelopes, and JWT Bearer token authentication.
   - Interactive OpenAPI documentation available at `/swagger`.
   - Comprehensive Postman collection included (`NashaMukti_API_Postman_Collection.json`).
8. **Role-Based Security & Governance (`/Admin`):**
   - Strict segregation between National Administrators (`Admin`) and facility counsellors (`CentreStaff`).
   - Anti-CSRF protection on all mutating actions, modern HTTP security headers, and instant session revocation upon account deactivation.

---

## 2. Technology Stack

- **Target Framework:** .NET 8.0 LTS (`net8.0`)
- **Web Application Architecture:** ASP.NET Core MVC (Model-View-Controller) + Razor View Engine
- **Web API Architecture:** ASP.NET Core Web API + Swashbuckle OpenAPI (Swagger)
- **Object-Relational Mapper (ORM):** Entity Framework Core 8.0.10 (Code-First with Migrations)
- **Database Engine:** Microsoft SQL Server 2019 / 2022 / Azure SQL / LocalDB
- **Authentication & Authorization:** ASP.NET Core Identity + Dual Cookie/JWT Bearer Tokens
- **Client-Side Libraries:** Bootstrap 5.3, Bootstrap Icons, Chart.js 4.4, Native ES6 Vanilla JavaScript
- **Unit & Integration Testing:** xUnit, Microsoft.EntityFrameworkCore.InMemory (25/25 Passing Tests)

---

## 3. Prerequisites

Before installing and running the project, verify that the following tools are installed:

1. **.NET 8.0 SDK (LTS):**
   - Verify by running:
     ```powershell
     dotnet --version
     # Output must be 8.0.xxx
     ```
   - Download from: [https://dotnet.microsoft.com/download/dotnet/8.0](https://dotnet.microsoft.com/download/dotnet/8.0)
2. **Microsoft SQL Server:**
   - Any of: SQL Server 2019/2022 Express, Developer Edition, or LocalDB.
   - Alternatively, Docker:
     ```bash
     docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=YourStrong@Pass123" -p 1433:1433 -d mcr.microsoft.com/mssql/server:2022-latest
     ```
3. **EF Core Global CLI Tool (Optional but recommended):**
   ```powershell
   dotnet tool install --global dotnet-ef
   ```
4. **Git:** To clone and manage source code.
5. **Postman (Optional):** To test REST API endpoints using the bundled collection.

---

## 4. Local Installation & Setup

### Step 1: Clone Repository
```powershell
git clone https://github.com/KrChiranjeevi/Centralised-Nasha-Mukti-Database.git
cd Centralised-Nasha-Mukti-Database
```

### Step 2: Configure Database Connection String
Open `appsettings.json` (or `appsettings.Development.json`) and configure the `DefaultConnection` string for your local SQL Server instance:

- **LocalDB (Default for Visual Studio):**
  ```json
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=NashaMuktiDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  }
  ```
- **SQL Server Express:**
  ```json
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=NashaMuktiDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
  ```
- **SQL Server Developer / Default Instance:**
  ```json
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=NashaMuktiDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
  ```

### Step 3: Apply Database Migrations & Seed Data
You can initialize the database using either of two methods:

#### Method A: Automatic Application Initialization (Recommended)
Simply running the application will automatically apply any pending EF Core migrations and seed initial roles and default admin credentials:
```powershell
dotnet run
```

#### Method B: EF Core CLI
```powershell
dotnet ef database update
```

#### Method C: SQL Script (Offline / Direct Execution in SSMS)
If you prefer direct SQL execution, execute the bundled idempotent script in SQL Server Management Studio:
```powershell
sqlcmd -S localhost -d master -i Data/CompleteDatabasePart7.sql
```

---

## 5. Seeded Credentials & Roles

| Email Address | Password | Role | Assigned Centre | Scope |
| :--- | :--- | :--- | :--- | :--- |
| `admin@nashamukti.gov.in` | `Admin@12345` | **Admin** | *None (Global)* | All facilities, user management, reports |
| `admin@nmd.gov.in` | `Admin@NMD2026!` | **Admin** | *None (Global)* | System administrative account |
| `staff.amritsar@nmd.gov.in` | `Staff@NMD2026!` | **CentreStaff** | Amritsar Centre (ID: `1`) | Scoped to Centre 1 patients only |
| `staff.patna@nmd.gov.in` | `Staff@NMD2026!` | **CentreStaff** | Patna Centre (ID: `3`) | Scoped to Centre 3 patients only |

> [!NOTE]
> In production environments (`ASPNETCORE_ENVIRONMENT=Production`), development dummy staff accounts are automatically omitted from database seeding.

---

## 6. Running the Application

### Start Development Server
```powershell
dotnet run
```
The application will start listening on:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`

Navigate to `http://localhost:5000` in your web browser. You will be redirected to the secure sign-in screen.

---

## 7. Interactive API Documentation (Swagger)

Swagger UI is enabled across all environments for academic viva demonstration:
- **Swagger URL:** [http://localhost:5000/swagger](http://localhost:5000/swagger)

To authenticate in Swagger:
1. Call `POST /api/v1/auth/login` with your credentials (`admin@nashamukti.gov.in` / `Admin@12345`).
2. Copy the returned JWT token.
3. Click the **Authorize** button in Swagger and enter `Bearer <your_jwt_token>`.
4. Test any protected endpoint directly from the browser!

---

## 8. Postman Collection

The repository includes a ready-to-use Postman collection:
- **File:** `NashaMukti_API_Postman_Collection.json`

### Importing into Postman:
1. Open Postman.
2. Click **Import** (top left).
3. Select `NashaMukti_API_Postman_Collection.json`.
4. The collection will import with pre-configured requests and test scripts:
   - `1. Authenticate (Login)` — *Automatically extracts and sets `{{jwt_token}}` variable!*
   - `2. Centres - List All`
   - `3. Centres - Get By ID`
   - `4. Beneficiaries - List Scoped`
   - `5. Beneficiaries - Create Intake`
   - `6. Counselling - List Sessions`
   - `7. Interventions - List Treatments`
   - `8. Follow-Ups - List Appointments`
   - `9. Reports - Summary KPIs`

---

## 9. Running Automated Tests

The solution contains a dedicated test project `NashaMuktiDB.Tests` containing 25 unit and integration tests covering business logic, security headers, IDOR facility scoping, inactive user invalidation, and data calculations:

```powershell
dotnet test NashaMuktiDB.Tests/NashaMuktiDB.Tests.csproj
```

**Test Output:**
```
Test run for NashaMuktiDB.Tests.dll (.NETCoreApp,Version=v8.0)
Passed!  - Failed: 0, Passed: 25, Skipped: 0, Total: 25, Duration: 1.8 s
```

---

## 10. Production Deployment Procedure

### Step 1: Publish Release Binaries
```powershell
dotnet publish -c Release -o ./publish
```

### Step 2: Provision Database on Production SQL Server
1. Provision an Azure SQL Database or self-hosted SQL Server 2022 instance.
2. Run the idempotent migration script `Data/deploy.sql` against the production database:
   ```powershell
   sqlcmd -S <prod_server>.database.windows.net -U <admin_user> -P <password> -d NashaMuktiProdDB -i Data/deploy.sql
   ```

### Step 3: Configure Environment Variables
Set the following environment variables in your hosting provider (e.g., Azure App Service, IIS, or Linux systemd service):
- `ASPNETCORE_ENVIRONMENT`: `Production`
- `ConnectionStrings__DefaultConnection`: `<Your_Production_SQL_Server_Connection_String>`
- `Jwt__Key`: `<64_Character_Cryptographically_Secure_Random_String>`
- `Security__DefaultAdminPassword`: `<Strong_Production_Admin_Password>`

### Step 4: Verify Deployment
1. Verify HTTPS is enabled and security headers are returned.
2. Access the health/login page.
3. Sign in with the designated production administrator credentials.
4. Verify database operations by creating a facility and registering a test beneficiary.

---

## 11. Troubleshooting & Common Setup Errors

| Issue / Error | Likely Cause | Solution |
| :--- | :--- | :--- |
| `Cannot open database "NashaMuktiDB"` | Database does not exist yet. | Run `dotnet run` (automatic migration) or execute `Data/CompleteDatabasePart7.sql` in SSMS. |
| `Certificate chain was issued by an untrusted authority` | SQL Server SSL encryption requirement. | Add `TrustServerCertificate=True;` to your connection string in `appsettings.json`. |
| `Login failed for user` | Invalid SQL credentials or Windows auth mismatch. | Use `Trusted_Connection=True;` for local Windows Auth, or supply valid `User ID=...;Password=...;`. |
| `Failed to bind to address http://localhost:5000: address already in use` | Another process is using port 5000. | Change the port in `Properties/launchSettings.json` or run `dotnet run --urls "http://localhost:5005"`. |
| `Access Denied (403)` when creating a Centre | Logged in as CentreStaff. | Log in with the `Admin` role (`admin@nashamukti.gov.in`). Centre creation is restricted to National Administrators. |
| `Foreign Beneficiary Access Denied` | Scoping rule enforced. | Centre staff are intentionally forbidden from viewing patients outside their assigned facility. |

---

## 12. University Submission Details

- **Course:** INT402 — Web Development / Advanced Database Management Systems
- **Candidate Name:** KrChiranjeevi
- **Repository:** [https://github.com/KrChiranjeevi/Centralised-Nasha-Mukti-Database.git](https://github.com/KrChiranjeevi/Centralised-Nasha-Mukti-Database.git)
- **Framework & Runtime:** ASP.NET Core 8.0 LTS / C# 12 / Entity Framework Core 8 / SQL Server
- **Current Branch:** `main` (Audited, Hardened, 25/25 Tests Passing)
