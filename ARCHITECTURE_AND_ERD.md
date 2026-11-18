# System Architecture, Request Flows and Entity-Relationship Diagram

**Project:** Centralised Nasha Mukti Database (INT402)  
**Author:** KrChiranjeevi  
**Framework:** ASP.NET Core 8.0 MVC & Web API  
**Database:** Microsoft SQL Server 2022 / Azure SQL  

---

## 1. System Architecture Overview

The Centralised Nasha Mukti Database is architected following an enterprise N-Tier / Clean Architecture design pattern with clear separation of concerns:

```
+-----------------------------------------------------------------------------------+
|                                Presentation Layer                                 |
|  +---------------------------------------------+  +----------------------------+  |
|  |           ASP.NET Core MVC (Razor)          |  |  RESTful Web API (v1 DTOs) |  |
|  |   Views, ViewModels, TagHelpers, Bootstrap  |  |    Swagger OpenAPI 3.0     |  |
|  +---------------------------------------------+  +----------------------------+  |
+------------------------------------------+----------------------------------------+
                                           |
                                           v
+-----------------------------------------------------------------------------------+
|                         Cross-Cutting Security & Middleware                       |
|   Security Headers | Anti-Forgery (CSRF) | Cookie Auth | JWT Bearer | Auditing    |
+------------------------------------------+----------------------------------------+
                                           |
                                           v
+-----------------------------------------------------------------------------------+
|                                Application Service Layer                          |
|  +-------------------------+  +-------------------------+  +-------------------+  |
|  | ICentreService          |  | IBeneficiaryService     |  | ICounsellingSvc   |  |
|  +-------------------------+  +-------------------------+  +-------------------+  |
|  | IInterventionService    |  | IFollowUpService        |  | IReportService    |  |
|  +-------------------------+  +-------------------------+  +-------------------+  |
|  | ICentreAuthService (Scoping & IDOR Isolation)        |  | ITokenService     |  |
|  +------------------------------------------------------+  +-------------------+  |
+------------------------------------------+----------------------------------------+
                                           |
                                           v
+-----------------------------------------------------------------------------------+
|                          Data Access Layer (ORM)                                  |
|                 Entity Framework Core 8.0 (ApplicationDbContext)                  |
|    Code-First Mappings, LINQ to Entities, AsNoTracking, Change Tracker, UnitOfWork|
+------------------------------------------+----------------------------------------+
                                           |
                                           v
+-----------------------------------------------------------------------------------+
|                           Relational Database Layer                               |
|                         Microsoft SQL Server 2022                                 |
|  +--------------------+  +--------------------+  +------------------------------+ |
|  | Relational Tables  |  | Foreign Key Checks |  | SQL Views & Stored Procedures| |
|  +--------------------+  +--------------------+  +------------------------------+ |
+-----------------------------------------------------------------------------------+
```

---

## 2. Request Processing Flows

### A. MVC Request Flow (Server-Side Razor Views)

```mermaid
sequenceDiagram
    autonumber
    actor User as Client Browser
    participant MW as Middleware Pipeline
    participant Auth as Identity & Cookie Auth
    participant Ctrl as MVC Controller
    participant Svc as Service Layer
    participant Ctx as ApplicationDbContext
    participant DB as SQL Server
    participant Razor as Razor View Engine

    User->>MW: HTTP GET/POST (e.g. /Beneficiaries/Create)
    MW->>MW: Inject Security Headers (CSP, X-Frame-Options)
    MW->>Auth: Validate Cookie & Session
    alt User is Inactive / Invalid Stamp
        Auth-->>User: 302 Redirect to /Account/Login
    else User is Authorized
        Auth->>Ctrl: Dispatch to BeneficiariesController
        Ctrl->>Ctrl: Validate [ValidateAntiForgeryToken] & ModelState
        Ctrl->>Svc: Call CreateBeneficiaryAsync(model, userScope)
        Svc->>Svc: Verify Facility IDOR Boundary
        Svc->>Ctx: Add Beneficiary Entity
        Ctx->>DB: INSERT INTO Beneficiaries ...
        DB-->>Ctx: Confirm Insert
        Svc-->>Ctrl: Return Generated BeneficiaryCode
        Ctrl->>Razor: Render View with ViewModel
        Razor-->>User: Return Compiled HTML/CSS/JS Response
    end
```

### B. Web API Request Flow (RESTful JSON Services)

```mermaid
sequenceDiagram
    autonumber
    actor Client as Postman / Mobile / External Client
    participant MW as Middleware Pipeline
    participant JWT as JWT Bearer Handler
    participant Api as API Controller (e.g. /api/v1/centres)
    participant Svc as Application Services / EF Core
    participant DB as SQL Server

    Client->>MW: HTTP Request + Header [Authorization: Bearer <token>]
    MW->>MW: Apply Security Headers
    MW->>JWT: Validate Token Signature, Issuer, Audience, Expiry
    JWT->>JWT: Check User.IsActive via OnTokenValidated event
    alt Token Missing / Deactivated
        JWT-->>Client: HTTP 401 Unauthorized (ApiResponse.Fail)
    else Token Valid
        JWT->>Api: Invoke Action with ClaimsPrincipal
        Api->>Api: Model Validation ([FromBody] DTO)
        alt Invalid DTO
            Api-->>Client: HTTP 400 Bad Request (Validation Errors)
        else Valid DTO
            Api->>Svc: Query or Mutate Data via Scoped Service
            Svc->>DB: Execute Parameterized SQL / SP
            DB-->>Svc: Tabular Result Set
            Svc-->>Api: Map Entities to Response DTO
            Api-->>Client: HTTP 200/201 ApiResponse<T> JSON Envelope
        end
    end
```

---

## 3. Entity-Relationship (ER) Diagram

The ER Diagram accurately reflects the physical database schema, foreign keys, and cardinalities implemented in `ApplicationDbContext.cs`:

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
        nvarchar(max) PasswordHash
        nvarchar(max) SecurityStamp
    }

    ASPNETROLES {
        nvarchar(450) Id PK
        nvarchar(256) Name
        nvarchar(256) NormalizedName
    }

    ASPNETUSERROLES {
        nvarchar(450) UserId PK,FK
        nvarchar(450) RoleId PK,FK
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
        datetime2 UpdatedAt "nullable"
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
        datetime2 UpdatedAt "nullable"
    }

    FOLLOW_UPS {
        int FollowUpId PK
        int BeneficiaryId FK
        date ScheduledDate
        nvarchar(50) Status
        date CompletedDate "nullable"
        nvarchar(1000) Notes "confidential"
        datetime2 CreatedAt
        datetime2 UpdatedAt "nullable"
    }
```

---

## 4. Entity Framework Core Data Access Design

### A. Fluent API Configuration
All relationships and schema rules are explicitly configured using Fluent API in `Data/ApplicationDbContext.cs`:
1. **Cascade Delete Restriction:** All clinical foreign keys specify `.OnDelete(DeleteBehavior.Restrict)`. This guarantees that if a centre or patient record is deactivated, child clinical records (counselling sessions, interventions, and follow-ups) can never be accidentally cascade-deleted from the database.
2. **Unique Constraints & Performance Indexes:**
   - `UQ_Beneficiaries_BeneficiaryCode`: Unique index guaranteeing code uniqueness nationwide.
   - Compound indexes: `IX_Centres_StateId`, `IX_Centres_District`, `IX_Beneficiaries_CentreId`, `IX_Beneficiaries_Status`, `IX_CounsellingSessions_BeneficiaryId`, `IX_Interventions_BeneficiaryId`, `IX_FollowUps_BeneficiaryId`.
3. **Database Defaults:** `CreatedAt` columns utilize `GETUTCDATE()`.

### B. Read Query Optimization (`AsNoTracking`)
Read-only queries throughout MVC services and Web API controllers utilize `.AsNoTracking()` to eliminate the EF Core change tracker overhead, drastically decreasing memory allocation and boosting query throughput.

---

## 5. Centre-Level Access Control & IDOR Prevention

### Problem Definition (Insecure Direct Object Reference - CWE-639)
In a multi-facility healthcare system, a standard vulnerability arises when a counsellor logged into Centre A attempts to view or modify records belonging to Centre B by tampering with the route parameter (e.g. `GET /Beneficiaries/Details/501` or `PUT /api/v1/beneficiaries/501`).

### Architectural Solution (`ICentreAuthorizationService`)
The application implements an authorization gateway service injected into all controllers:

```csharp
public interface ICentreAuthorizationService
{
    Task<bool> IsAdminAsync(ClaimsPrincipal principal);
    Task<int?> GetUserCentreIdAsync(ClaimsPrincipal principal);
    Task<bool> CanAccessCentreAsync(ClaimsPrincipal principal, int centreId);
    Task<bool> CanAccessBeneficiaryAsync(ClaimsPrincipal principal, int beneficiaryId);
    Task<ApplicationUser?> GetCurrentUserAsync(ClaimsPrincipal principal);
}
```

### Authorization Rules:
1. **National Administrator (`Admin`):** Has universal read and write access across all states, facilities, and patients nationwide.
2. **Centre Staff (`CentreStaff`):**
   - Bound strictly to `user.CentreId`.
   - In intake forms (`Create`), the `model.CentreId` submitted from the browser is unconditionally overwritten on the server with `user.CentreId`.
   - In detail, edit, and API actions, `CanAccessBeneficiaryAsync` verifies that `beneficiary.CentreId == user.CentreId`.
   - If unauthorized, the request immediately terminates with HTTP `403 Forbidden` or redirects to `AccessDenied`.

