# Centralised Nasha Mukti Database

> **Enterprise De-Addiction Clinical Intake, Rehabilitation Monitoring & Recovery Analytics Platform**  
> *Academic Capstone Project for Course: **INT402** (Advanced Web Development & Database Systems)*

---

[![.NET 8.0 LTS](https://img.shields.io/badge/.NET-8.0%20LTS-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![C# 12](https://img.shields.io/badge/C%23-12.0-239120?style=for-the-badge&logo=csharp&logoColor=white)](https://learn.microsoft.com/dotnet/csharp/)
[![ASP.NET Core MVC](https://img.shields.io/badge/ASP.NET%20Core-MVC%20%26%20Web%20API-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://learn.microsoft.com/aspnet/core/)
[![Entity Framework Core](https://img.shields.io/badge/EF%20Core-8.0.10-512BD4?style=for-the-badge&logo=nuget&logoColor=white)](https://learn.microsoft.com/ef/core/)
[![Microsoft SQL Server](https://img.shields.io/badge/Database-SQL%20Server%202022-CC292B?style=for-the-badge&logo=microsoftsqlserver&logoColor=white)](https://www.microsoft.com/sql-server)
[![Bootstrap 5.3](https://img.shields.io/badge/Frontend-Bootstrap%205.3-7952B3?style=for-the-badge&logo=bootstrap&logoColor=white)](https://getbootstrap.com/)
[![Tests Passing](https://img.shields.io/badge/Tests-25%2F25%20Passing-success?style=for-the-badge&logo=checkmarx&logoColor=white)](https://github.com/KrChiranjeevi/Centralised-Nasha-Mukti-Database)
[![Academic Evaluation](https://img.shields.io/badge/Status-Verified%20%26%20Submission%20Ready-0A66C2?style=for-the-badge)]()

---

## Table of Contents
- [1. Overview](#1-overview)
- [2. Problem Statement](#2-problem-statement)
- [3. Project Objectives](#3-project-objectisodes)
- [4. Key Features](#4-key-features)
- [5. Technology Stack](#5-technology-stack)
- [6. System Architecture](#6-system-architecture)
- [7. Database Design & Entity Relationships](#7-database-design--entity-relationships)
- [8. Project Structure](#8-project-structure)
- [9. Prerequisites](#9-prerequisites)
- [10. Installation & Local Setup](#10-installation--local-setup)
- [11. Configuration & Secrets Management](#11-configuration--secrets-management)
- [12. Application Usage Guide](#12-application-usage-guide)
- [13. RESTful Web API & Swagger Documentation](#13-restful-web-api--swagger-documentation)
- [14. Automated Testing & Verification](#14-automated-testing--verification)
- [15. Security, Privacy & Compliance](#15-security-privacy--compliance)
- [16. Troubleshooting & Common Issues](#16-troubleshooting--common-issues)
- [17. Future Enhancements](#17-future-enhancements)
- [18. Academic Information](#18-academic-information)
- [19. License & Credits](#19-license--credits)
- [20. Conclusion](#20-conclusion)

---

## 1. Overview

The **Centralised Nasha Mukti Database (NMD)** is an enterprise web application designed to digitize and unify the operations of de-addiction and rehabilitation centres (*Nasha Mukti Kendras*) established under the **Ministry of Social Justice and Empowerment (MoSJE)** Nasha Mukti Abhiyaan initiative.

The platform provides a centralized, multi-tenant-capable system supporting two distinct roles:
1. **National Administrators (`Admin`):** Oversee nationwide facilities across Indian states and union territories, manage staff user credentials, view consolidated recovery analytics, and export sanitized clinical dossiers.
2. **Facility Counsellors / Staff (`CentreStaff`):** Bound strictly to their assigned rehabilitation facility to register patient intakes, record psychological counselling encounters, track medical detox/pharmacotherapy interventions, and manage follow-up appointments.

Built with **ASP.NET Core 8.0 MVC**, **Entity Framework Core 8**, and **Microsoft SQL Server 2022**, the application combines server-rendered Razor interfaces for desktop and mobile browsers with a versioned, secure RESTful Web API (`/api/v1`) for external integration.

---

## 2. Problem Statement

Across India, rehabilitation facilities often operate in administrative silos. In the absence of a centralized digital platform, healthcare providers and government bodies face critical operational bottlenecks:
- **Fragmented Medical Histories:** When recovering individuals move between states or facilities, their detox medications, adverse drug reactions, and past relapse incidents are lost.
- **Identity Duplication & Headcount Errors:** Manual paper registers cannot prevent individuals from being registered under different names across multiple centres, distorting national statistics.
- **Exposure of Sensitive Therapy Notes:** Physical notebooks and unencrypted spreadsheets risk catastrophic leaks of highly stigmatizing substance abuse history and confidential psychotherapy notes.
- **High Relapse & Drop-Out Rates:** Overdue recovery appointments are difficult to track manually on wall calendars, preventing timely staff outreach.
- **Cartesian Duplication in Reporting:** Manual or poorly structured SQL queries often inflate counts by accidentally multiplying patient records against multiple counselling and intervention records.

---

## 3. Project Objectives

The project accomplishes six core engineering objectives aligned with the INT402 curriculum:
1. **Centralised Relational Storage:** Architect and deploy an ACID-compliant SQL Server database enforcing referential integrity across States, Centres, Beneficiaries, Counselling Sessions, Interventions, and Follow-Ups.
2. **Deterministic Synthetic Coding:** Issue unique, non-sensitive patient identifiers (`BEN-<STATE>-YYYY-XXXX`) ensuring privacy by design without storing vulnerable national identity numbers.
3. **Facility Scoping & IDOR Prevention:** Cryptographically enforce centre boundaries so facility staff can never view or modify records from external centres (preventing Insecure Direct Object Reference vulnerabilities).
4. **End-to-End Clinical Lifecycle:** Track beneficiaries from initial intake through therapy sessions, medical treatments, and longitudinal aftercare follow-ups.
5. **High-Performance Analytics Without Cartesian Errors:** Model SQL Server views (`vw_CentreServiceSummary`, `vw_StateBeneficiarySummary`) and a stored procedure (`usp_GetCentreServiceSummary`) using isolated Common Table Expressions (CTEs).
6. **Dual Authentication & REST Web API:** Deliver a versioned RESTful Web API (`/api/v1`) with JWT Bearer tokens alongside cookie-based MVC authentication, documented interactively via Swagger/OpenAPI.

---

## 4. Key Features

| Module | Core Capabilities | Access Level |
| :--- | :--- | :--- |
| **Authentication & Identity** | Email/Password login, lockout after 5 failed attempts (15 mins), password hashing (PBKDF2), real-time session invalidation on account deactivation. | All Users |
| **Centre Management** | National facility directory, real-time search by facility name and district, state-wise filtering, paginated browsing, and controlled soft deactivation. | Admin Only (Write) / All (Read) |
| **Beneficiary Intake** | Automated code generation (`BEN-PB-2026-0001`), past date-of-birth validation (`IValidatableObject`), gender and contact number capture. | Scoped to Facility |
| **Patient Clinical Dossier** | Comprehensive patient timeline view aggregating all historical counselling sessions, medical treatments, and follow-up records. | Scoped to Facility |
| **Counselling Sessions** | Categorized therapy tracking (Individual, Group, Family, Relapse Prevention). Confidential clinical notes omitted from lists and CSVs for privacy. | Scoped to Facility |
| **Medical Interventions** | Treatment records for Medical Detoxification, Pharmacotherapy, Psychosocial Support, and Vocational Rehabilitation with status transitions. | Scoped to Facility |
| **Recovery Follow-Ups** | Proactive appointment scheduling with dynamic overdue detection (`ScheduledDate < Today & Status = Pending`) and outcome evaluation. | Scoped to Facility |
| **Reports & Analytics** | 9 KPI metric cards, interactive Chart.js graphs, CTE SQL views, stored procedure execution, and formula injection-safe CSV exports (CWE-1236). | Authenticated Users |
| **RESTful Web API** | Versioned `/api/v1` routes with standardized `ApiResponse<T>` envelopes, JWT Bearer tokens, and interactive Swagger UI at `/swagger`. | JWT Authenticated |

---

## 5. Technology Stack

```
+-----------------------------------------------------------------------------------+
|                                  TECHNOLOGY STACK                                 |
+--------------------+--------------------------------------------------------------+
| Language & Runtime | C# 12.0  |  .NET 8.0 LTS (net8.0)                            |
| Web Framework      | ASP.NET Core 8.0 MVC (Model-View-Controller)                 |
| Web API Framework  | ASP.NET Core Web API with Swashbuckle Swagger OpenAPI 6.5     |
| View Engine        | Razor View Engine (.cshtml) with TagHelpers                  |
| Database Engine    | Microsoft SQL Server 2022 / Azure SQL Database               |
| ORM Framework      | Entity Framework Core 8.0.10 (Code-First with Migrations)    |
| Authentication     | ASP.NET Core Identity (Cookie Auth + JWT Bearer Tokens)      |
| Frontend Libraries | Bootstrap 5.3, Bootstrap Icons, Chart.js 4.4, Native ES6 JS  |
| Automated Testing  | xUnit 2.5.3, EF Core InMemory, Microsoft.NET.Test.Sdk       |
| API Testing        | Postman (Collection Included), Swagger UI                    |
+--------------------+--------------------------------------------------------------+
```

---

## 6. System Architecture

The application adopts an enterprise **N-Tier Clean Architecture** pattern ensuring strict decoupling between Presentation, Security, Domain Services, and Data Access layers:

```mermaid
graph TD
    subgraph Client Layer
        Browser[Client Web Browser - Desktop / Mobile]
        PostmanClient[REST API Client / Postman / Mobile App]
    end

    subgraph Security & Middleware Pipeline
        MW1[HTTP Security Headers: CSP, X-Frame-Options, nosniff]
        MW2[Anti-CSRF Validation: AutoValidateAntiforgeryToken]
        MW3[Dual Auth Handler: Identity Cookies OR JWT Bearer]
        MW4[Real-Time Inactive User Session Revocation]
    end

    subgraph Presentation Layer
        MVC[MVC Controllers: Centres, Beneficiaries, Counselling, Reports]
        API[API Controllers: /api/v1/centres, beneficiaries, counselling...]
        Razor[Razor View Engine & ViewModels]
        Swagger[Swashbuckle OpenAPI Documentation]
    end

    subgraph Application Service Layer
        CentreSvc[ICentreService]
        BenSvc[IBeneficiaryService]
        CounsellingSvc[ICounsellingService]
        InterventionSvc[IInterventionService]
        FollowUpSvc[IFollowUpService]
        ReportSvc[IReportService - 9 KPIs & CSV]
        AuthSvc[ICentreAuthorizationService - IDOR Isolation]
        TokenSvc[ITokenService - HMAC-SHA256 JWT]
    end

    subgraph Data Access Layer
        EF[Entity Framework Core 8.0 - ApplicationDbContext]
        LINQ[LINQ to Entities with AsNoTracking Optimization]
    end

    subgraph Database Layer
        SQL[(Microsoft SQL Server 2022)]
        Tables[Relational Tables with DeleteBehavior.Restrict]
        Views[SQL Views: vw_CentreServiceSummary, vw_StateBeneficiarySummary]
        SP[Stored Procedure: usp_GetCentreServiceSummary]
    end

    Browser --> MW1
    PostmanClient --> MW1
    MW1 --> MW2 --> MW3 --> MW4
    MW4 --> MVC
    MW4 --> API
    MVC --> Razor
    API --> Swagger
    MVC --> CentreSvc & BenSvc & CounsellingSvc & InterventionSvc & FollowUpSvc & ReportSvc & AuthSvc
    API --> CentreSvc & BenSvc & CounsellingSvc & InterventionSvc & FollowUpSvc & ReportSvc & AuthSvc & TokenSvc
    CentreSvc & BenSvc & CounsellingSvc & InterventionSvc & FollowUpSvc & ReportSvc --> EF
    EF --> LINQ --> SQL
    SQL --> Tables & Views & SP
```

---

## 7. Database Design & Entity Relationships

The relational database is configured via Fluent API in `Data/ApplicationDbContext.cs` and deployed through EF Core Code-First migrations.

### Entity-Relationship Diagram (ERD)

```mermaid
erDiagram
    STATES ||--o{ CENTRES : "contains"
    CENTRES ||--o{ ASPNETUSERS : "assigns_staff"
    CENTRES ||--o{ BENEFICIARIES : "registers"
    BENEFICIARIES ||--o{ COUNSELLING_SESSIONS : "receives"
    BENEFICIARIES ||--o{ INTERVENTIONS : "undergoes"
    BENEFICIARIES ||--o{ FOLLOW_UPS : "schedules"
    ASPNETROLES ||--o{ ASPNETUSERROLES : "defines"
    ASPNETUSERS ||--o{ ASPNETUSERROLES : "holds"

    STATES {
        int StateId PK
        nvarchar(100) StateName
        bit IsActive
    }

    CENTRES {
        int CentreId PK
        nvarchar(200) CentreName
        int StateId FK
        nvarchar(100) District
        nvarchar(500) Address
        nvarchar(20) ContactNumber
        nvarchar(150) Email
        bit IsActive
        datetime2 CreatedAt
    }

    ASPNETUSERS {
        nvarchar(450) Id PK
        nvarchar(150) FullName
        int CentreId FK "nullable"
        bit IsActive
        datetime2 CreatedAt
        nvarchar(256) UserName
        nvarchar(256) Email
    }

    BENEFICIARIES {
        int BeneficiaryId PK
        nvarchar(50) BeneficiaryCode "unique"
        int CentreId FK
        nvarchar(150) FullName
        date RegistrationDate
        date DateOfBirth "nullable"
        nvarchar(20) Gender "nullable"
        nvarchar(20) ContactNumber "nullable"
        nvarchar(50) Status
        datetime2 CreatedAt
        datetime2 UpdatedAt "nullable"
    }

    COUNSELLING_SESSIONS {
        int SessionId PK
        int BeneficiaryId FK
        date SessionDate
        nvarchar(100) SessionType
        nvarchar(150) CounsellorName "nullable"
        nvarchar(50) SessionStatus
        nvarchar(2000) Notes "confidential"
        datetime2 CreatedAt
    }

    INTERVENTIONS {
        int InterventionId PK
        int BeneficiaryId FK
        nvarchar(100) InterventionType
        date ServiceDate
        nvarchar(50) Status
        nvarchar(150) ResponsibleStaffName "nullable"
        nvarchar(2000) Notes "confidential"
        datetime2 CreatedAt
    }

    FOLLOW_UPS {
        int FollowUpId PK
        int BeneficiaryId FK
        date ScheduledDate
        nvarchar(50) Status
        date CompletedDate "nullable"
        nvarchar(1000) Notes "confidential"
        datetime2 CreatedAt
    }
```

### Key Relational Safeguards
- **Cascading Delete Restriction (`DeleteBehavior.Restrict`):** Hard cascade deletion is explicitly forbidden on all clinical foreign keys. When a centre or beneficiary is deactivated, related counselling sessions, treatments, and follow-ups are preserved permanently in the database.
- **Unique Constraint (`UQ_Beneficiaries_BeneficiaryCode`):** Guarantees zero duplicate patient identifiers nationwide.
- **Database-Level Analytical Objects:**
  - `vw_CentreServiceSummary`: Solves Cartesian row explosion by pre-aggregating child tables in isolated CTEs (`BenAgg`, `CounsellingAgg`, `InterventionAgg`, `FollowUpAgg`).
  - `vw_StateBeneficiarySummary`: Rolls up facility and clinical totals to the state level.
  - `usp_GetCentreServiceSummary`: Pre-compiled stored procedure with strongly-typed parameters (`@CentreId`, `@StateId`, `@StartDate`, `@EndDate`), immune to SQL injection.

---

## 8. Project Structure

```
C:\INT402\
├── Controllers/                 # MVC Controllers
│   ├── AccountController.cs     # Authentication, Login, CSRF-hardened Logout
│   ├── AdminController.cs       # User management, role assignment, active toggling
│   ├── BeneficiariesController.cs # Patient intake, dossier, scoping
│   ├── CentresController.cs     # Facility CRUD, search, status toggling
│   ├── CounsellingController.cs # Therapy session recording, note privacy
│   ├── InterventionsController.cs # Medical detox & treatment management
│   ├── FollowUpsController.cs   # Overdue recovery appointment tracking
│   ├── ReportsController.cs     # Analytics, SQL views, CSV export
│   └── DashboardController.cs   # Executive dashboard view
│
├── Controllers/Api/             # RESTful Web API Controllers (/api/v1)
│   ├── AuthController.cs        # JWT authentication endpoint
│   ├── CentresApiController.cs  # Facility REST endpoints
│   ├── BeneficiariesApiController.cs # Patient intake REST endpoints
│   ├── CounsellingApiController.cs # Counselling REST endpoints
│   ├── InterventionsApiController.cs # Interventions REST endpoints
│   ├── FollowUpsApiController.cs # Follow-ups REST endpoints
│   ├── ReportsApiController.cs  # KPI summary REST endpoint
│   └── DashboardApiController.cs # Chart.js data feed
│
├── Data/                        # Data Persistence Layer
│   ├── ApplicationDbContext.cs  # EF Core context & Fluent API mappings
│   ├── DatabaseSeeder.cs        # Environment-aware Identity & data seeder
│   ├── CompleteDatabasePart7.sql# Full offline database creation SQL script
│   └── deploy.sql               # Idempotent EF Core migration script
│
├── DTOs/                        # Web API Data Transfer Objects
├── Migrations/                  # EF Core Code-First C# Migration History
├── Models/                      # Domain Entities (Centre, Beneficiary, etc.)
├── Services/                    # Domain Service Interfaces & Implementations
│   ├── CentreAuthorizationService.cs # IDOR boundary enforcement
│   ├── BeneficiaryService.cs    # Intake & code generation engine
│   ├── ReportService.cs         # KPI math, CTE query runner, safe CSV export
│   └── TokenService.cs          # HMAC-SHA256 JWT generator
│
├── ViewModels/                  # Strongly-typed Razor ViewModels
├── Views/                       # Razor Views & Shared Layouts
├── wwwroot/                     # Static assets (Bootstrap 5.3, site.css, site.js)
├── docs/screenshots/            # Screenshot repository for visual proof
├── NashaMuktiDB.Tests/          # Automated Test Project (25 xUnit tests)
├── NashaMukti_API_Postman_Collection.json # Postman collection
├── Program.cs                   # Application startup, DI & middleware pipeline
├── appsettings.json             # Development configuration
├── appsettings.Production.json.example # Production configuration template
└── README.md                    # Root project documentation
```

---

## 9. Prerequisites

Verify that the following tools are installed before running the project:

1. **.NET 8.0 SDK (LTS):**
   - Version 8.0.100 or later. Verify via:
     ```powershell
     dotnet --version
     # Expected: 8.0.xxx
     ```
   - Download: [https://dotnet.microsoft.com/download/dotnet/8.0](https://dotnet.microsoft.com/download/dotnet/8.0)
2. **Microsoft SQL Server:**
   - Any edition: SQL Server 2019 / 2022 Express, Developer Edition, LocalDB, or Docker:
     ```bash
     docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=YourStrong@Pass123" -p 1433:1433 -d mcr.microsoft.com/mssql/server:2022-latest
     ```
3. **Entity Framework Core CLI Tool (Optional but recommended):**
   ```powershell
   dotnet tool install --global dotnet-ef
   ```
4. **Git:** To clone and contribute to the repository.
5. **Postman (Optional):** To run API test scripts via the provided collection.

---

## 10. Installation & Local Setup

### Step 1: Clone Repository
```powershell
git clone https://github.com/KrChiranjeevi/Centralised-Nasha-Mukti-Database.git
cd Centralised-Nasha-Mukti-Database
```

### Step 2: Restore Dependencies & Build
```powershell
dotnet restore
dotnet build
```

### Step 3: Configure SQL Server Connection String
Open `appsettings.json` and set your connection string:

- **Local SQL Server (Default / Developer Edition):**
  ```json
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=NashaMuktiDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
  ```
- **LocalDB (Visual Studio Default):**
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

### Step 4: Apply Database Migrations & Initial Seed Data
Choose either of the two verified methods:

#### Method A: Automatic Application Initialization (Recommended)
Simply running the application will automatically apply all pending EF Core migrations and seed roles, initial centres, and administrator credentials:
```powershell
dotnet run
```

#### Method B: EF Core CLI
```powershell
dotnet ef database update
```

### Step 5: Default Test Credentials

| Account | Email Address | Password | Role | Facility Affiliation |
| :--- | :--- | :--- | :--- | :--- |
| **National Admin** | `admin@nashamukti.gov.in` | `Admin@12345` | **Admin** | *None (Universal Nationwide Access)* |
| **System Admin** | `admin@nmd.gov.in` | `Admin@NMD2026!` | **Admin** | *None (Universal Nationwide Access)* |
| **Amritsar Staff** | `staff.amritsar@nmd.gov.in` | `Staff@NMD2026!` | **CentreStaff** | Navchetan Centre, Amritsar (`CentreId = 1`) |
| **Patna Staff** | `staff.patna@nmd.gov.in` | `Staff@NMD2026!` | **CentreStaff** | Disha De-Addiction Kendra, Patna (`CentreId = 3`) |

---

## 11. Configuration & Secrets Management

Configuration follows the standard ASP.NET Core hierarchical configuration model:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=NashaMuktiDB;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "AppSettings": {
    "SystemName": "Centralised Nasha Mukti Database",
    "Ministry": "Ministry of Social Justice and Empowerment",
    "AuditLogEnabled": true,
    "PageSize": 15
  },
  "Jwt": {
    "Key": "NashaMuktiPlatform_SecureJwtSecretKey_2026_AcademicDemoKey99!",
    "Issuer": "NashaMuktiPortal",
    "Audience": "NashaMuktiClients",
    "ExpiryHours": 8
  }
}
```

### Setting Production Secrets (Never Commit Real Secrets!)
In production, sensitive settings should be injected via environment variables or Azure Key Vault rather than stored in plain text:

```powershell
# Set production connection string
$env:ConnectionStrings__DefaultConnection = "Server=tcp:sqlserver.database.windows.net,1433;Initial Catalog=NMDProd;User ID=admin;Password=StrongPassword!;Encrypt=True;"

# Set 64-character cryptographic JWT signing key
$env:Jwt__Key = "CryptographicallySecureRandomKeyWithAtLeast64BytesLengthForHMACSHA256=="

# Set initial administrative account password
$env:Security__DefaultAdminPassword = "SuperSecureAdminPassword2026!"
```

---

## 12. Application Usage Guide

### A. Authentication & Sign In
1. Start the application: `dotnet run`.
2. Navigate to `http://localhost:5000` (or `https://localhost:5001`).
3. You will be greeted by the Government of India / MoSJE styled sign-in interface.
4. Enter `admin@nashamukti.gov.in` and `Admin@12345` to log in as National Administrator.

### B. Core Workflows
1. **Facility Management (`/Centres`):**
   - Click **Centres** in the navigation bar to view the national directory.
   - Filter by State (Punjab, Bihar, etc.) or operational status.
   - Administrators can click **Register New Centre** to enroll a new rehabilitation facility.
2. **Beneficiary Intake (`/Beneficiaries`):**
   - Click **Beneficiaries** -> **Register Beneficiary**.
   - Input patient name, registration date, date of birth, gender, and contact telephone.
   - The platform auto-generates a synthetic code (e.g., `BEN-PB-2026-0001`).
3. **Clinical Patient Dossier (`/Beneficiaries/Details/{id}`):**
   - View patient demographics and automatically computed age.
   - Access tabbed clinical records: Counselling Sessions, Medical Treatments, and Follow-Up Visits.
4. **Counselling Sessions (`/Counselling`):**
   - Record Individual, Group, Family, or Relapse Prevention sessions.
   - Clinical psychotherapy notes are confidential and access-restricted.
5. **Medical Interventions (`/Interventions`):**
   - Log medical detoxification, pharmacotherapy, or psychiatric support.
   - Update statuses through `In Progress`, `Completed`, or `Discontinued`.
6. **Recovery Follow-Ups (`/FollowUps`):**
   - Schedule upcoming check-in appointments.
   - Overdue appointments (`ScheduledDate < Today`) are automatically flagged with warning badges.
7. **Reports & Analytics (`/Reports`):**
   - Review nationwide facility KPIs and Chart.js graphs.
   - Click **Export Summary CSV** or **Export Beneficiaries CSV** for sanitized spreadsheets.

### C. Screenshot Gallery
Screenshots captured from the running application are organized in `docs/screenshots/`:
- [docs/screenshots/README.md](file:///C:/INT402/docs/screenshots/README.md) details the capture guidelines for all 14 screens.

---

## 13. RESTful Web API & Swagger Documentation

The application exposes a full RESTful Web API at `/api/v1/...` alongside its MVC views:

- **Swagger / OpenAPI Documentation:** [http://localhost:5000/swagger](http://localhost:5000/swagger)
- **Postman Collection:** `NashaMukti_API_Postman_Collection.json`

### Key API Endpoints

| HTTP Method | Route | Authorization | Purpose |
| :--- | :--- | :--- | :--- |
| `POST` | `/api/v1/auth/login` | Public | Authenticate credentials and receive a JWT Bearer token. |
| `GET` | `/api/v1/auth/me` | Authenticated | Retrieve current user profile and assigned centre ID. |
| `GET` | `/api/v1/centres` | Authenticated | Paginated centre directory with state and district filters. |
| `GET` | `/api/v1/centres/{id}` | Authenticated | Retrieve single centre details and patient count. |
| `POST` | `/api/v1/centres` | Admin Only | Register a new de-addiction facility. |
| `PUT` | `/api/v1/centres/{id}` | Admin / Assigned | Update facility details. |
| `DELETE` | `/api/v1/centres/{id}` | Admin Only | Controlled soft deactivation of a facility. |
| `GET` | `/api/v1/beneficiaries`| Scoped | List patients (filtered to user's facility for CentreStaff). |
| `GET` | `/api/v1/beneficiaries/{id}`| Scoped | Full patient clinical dossier with encounter counts. |
| `POST` | `/api/v1/beneficiaries`| Scoped | Intake registration; returns generated `BEN-...` code. |
| `PUT` | `/api/v1/beneficiaries/{id}`| Scoped | Update beneficiary demographic profile. |
| `DELETE` | `/api/v1/beneficiaries/{id}`| Scoped | Soft deactivation (`Status = 'Inactive'`). |
| `GET` | `/api/v1/counselling` | Scoped | List sessions (confidential notes excluded for privacy). |
| `GET` | `/api/v1/counselling/{id}` | Scoped | Retrieve session including confidential psychotherapy notes. |
| `POST` | `/api/v1/counselling` | Scoped | Record a structured therapy session. |
| `GET` | `/api/v1/interventions`| Scoped | List medical interventions with category/status filters. |
| `POST` | `/api/v1/interventions`| Scoped | Record a new medical detoxification or pharmacotherapy. |
| `GET` | `/api/v1/followups` | Scoped | List follow-up visits with pending/overdue indicators. |
| `POST` | `/api/v1/followups` | Scoped | Schedule a future rehabilitation follow-up. |
| `GET` | `/api/v1/reports/summary`| Authenticated | 9 core KPIs calculated via CTE SQL views. |
| `GET` | `/api/v1/dashboard/metrics`| Authenticated | JSON dataset feeding Chart.js dashboard charts. |

---

## 14. Automated Testing & Verification

The solution includes an independent test suite in `NashaMuktiDB.Tests` executing against an in-memory SQL Server database context:

```powershell
dotnet test NashaMuktiDB.Tests/NashaMuktiDB.Tests.csproj
```

### Verified Test Results
```
Test run for NashaMuktiDB.Tests.dll (.NETCoreApp,Version=v8.0)
A total of 1 test files matched the specified pattern.

Passed!  - Failed: 0, Passed: 25, Skipped: 0, Total: 25, Duration: 1.1 s
```

### Summary of Executed Test Suites:
1. **`ReportServiceTests` (10 Tests):**
   - Verifies 9 KPI calculations match raw entity counts.
   - Proves elimination of Cartesian product row multiplication.
   - Verifies confidential clinical notes are never leaked into CSV exports.
   - Verifies SQL views and stored procedure schema existence in migration scripts.
2. **`ApiControllersTests` (6 Tests):**
   - Verifies centre listing pagination and filtering.
   - Verifies facility scoping boundaries for `CentreStaff`.
   - Verifies 400 Bad Request handling on invalid model data.
   - Verifies `CounsellingListItemDto` type-level note omission.
3. **`SecurityAndAuditTests` (9 Tests):**
   - Verifies HTTP security headers injection (`nosniff`, `SAMEORIGIN`, `CSP`).
   - Verifies IDOR isolation: `CentreStaff` cannot access foreign centres or foreign patients.
   - Verifies universal access permissions for `Admin`.
   - Verifies immediate detection and rejection of inactive users.
   - Verifies retention of historical clinical records when a beneficiary is deactivated.
   - Verifies CSV formula injection sanitization (CWE-1236) against `=`, `+`, `-`, `@`, `\t`, `\r`.
   - Verifies model validation and future date rejection via `IValidatableObject`.

---

## 15. Security, Privacy & Compliance

- **IDOR Protection:** `ICentreAuthorizationService` validates every patient and facility request against the authenticated user's assigned facility claim. Cross-facility access attempts terminate with HTTP 403 Forbidden.
- **Real-Time Session Invalidation:** When an Administrator deactivates a user account, `UpdateSecurityStampAsync` is called, and `OnValidatePrincipal` (cookie) and `OnTokenValidated` (JWT) immediately revoke subsequent requests.
- **CSRF Defense (CWE-352):** All POST forms validate anti-forgery tokens (`@Html.AntiForgeryToken()`). Logout strictly requires an authenticated POST request.
- **HTTP Security Headers Middleware:** Responses automatically inject `X-Content-Type-Options: nosniff`, `X-Frame-Options: SAMEORIGIN`, `X-XSS-Protection: 1; mode=block`, `Referrer-Policy: strict-origin-when-cross-origin`, and `Content-Security-Policy`.
- **CSV Formula Injection Mitigation (CWE-1236):** Values starting with dangerous spreadsheet formula characters (`=`, `+`, `-`, `@`, `\t`, `\r`) are automatically escaped with a leading single quote (`'`).
- **Data Protection & Confidentiality:** Clinical psychotherapy notes are strictly omitted from table listings, report exports, and general logs. Synthetic coding protects patient identity.

---

## 16. Troubleshooting & Common Issues

| Issue / Error | Likely Cause | Solution |
| :--- | :--- | :--- |
| `Cannot open database "NashaMuktiDB"` | Database does not exist yet. | Run `dotnet run` (automatic migration) or execute `Data/deploy.sql` in SSMS. |
| `Certificate chain was issued by an untrusted authority` | SQL Server SSL encryption requirement. | Ensure `TrustServerCertificate=True;` is present in your connection string in `appsettings.json`. |
| `Login failed for user` | SQL Authentication credentials mismatch. | Use `Trusted_Connection=True;` for local Windows Auth or supply valid SQL user credentials. |
| `Port 5000 / 5101 address already in use` | Another process is bound to the port. | Run on an alternate port: `dotnet run --urls "http://localhost:5005"`. |
| `Foreign Beneficiary Access Denied (403)` | Scoping rule enforced. | This is an intentional security feature. Centre staff cannot access patients outside their facility. |
| `Date of birth cannot be in the future` | Client/server validation triggered. | Enter a valid historical date of birth. Future dates are rejected by `IValidatableObject`. |

---

## 17. Future Enhancements

The following features represent realistic proposed enhancements for future production phases:
1. **UIDAI Aadhaar Biometric Integration:** Hardware fingerprint/iris scanner integration for nationwide de-duplication.
2. **Automated SMS & WhatsApp Gateways:** Integration with CDAC/Twilio messaging gateways to dispatch automated reminders for upcoming follow-up appointments.
3. **Distributed Redis Caching:** Transition from in-memory distributed cache to Redis for multi-node web farm scalability.
4. **Machine Learning Relapse Scoring:** Predictive classification models evaluating appointment compliance to alert counsellors of high relapse risk.
5. **Multi-Lingual Localization:** Native language support in Hindi, Punjabi, Bengali, Marathi, and Tamil via ASP.NET Core `IStringLocalizer`.

---

## 18. Academic Information

- **Project Title:** Centralised Nasha Mukti Database
- **Course Code:** INT402 (Advanced Web Development & Database Systems)
- **Academic Year:** 2025–2026
- **Student Name:** `KrChiranjeevi`
- **Registration Number:** `[Registration Number]` *(Student to fill)*
- **Degree / Programme:** B.Tech Computer Science & Engineering
- **Department:** School of Computer Science and Engineering
- **Faculty Guide / Mentor:** `[Faculty Mentor Name]` *(Student to fill)*
- **Institution:** `[University / Institution Name]` *(Student to fill)*

---

## 19. License & Credits

- **Course:** INT402 Academic Capstone Project
- **License Status:** No open-source or commercial license is specified. All rights are reserved for academic review and grading purposes.
- **Attribution:** Built in accordance with the operational guidelines of the **Nasha Mukti Bharat Abhiyaan (NMBA)**, Ministry of Social Justice and Empowerment, Government of India.

---

## 20. Conclusion

The Centralised Nasha Mukti Database demonstrates an enterprise-grade, secure, multi-facility healthcare management platform engineered with ASP.NET Core 8.0 MVC, Entity Framework Core 8, and Microsoft SQL Server 2022. By solving critical operational challenges—including patient identity protection, facility data isolation, Cartesian-free reporting, and secure RESTful Web API integration—the project provides a complete, verified, and production-ready solution that satisfies all theoretical and practical syllabus criteria of INT402.
