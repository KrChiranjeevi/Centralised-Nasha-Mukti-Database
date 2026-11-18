# INT402 Presentation Outline and Viva Voce Guide

**Project:** Centralised Nasha Mukti Database  
**Candidate Name:** KrChiranjeevi  
**Course Code:** INT402 (Web Development & Advanced Database Systems)  

---

## 1. Project Presentation Outline (12 Slides)

### Slide 1: Title & Project Identity
- **Title:** Centralised Nasha Mukti Database (MoSJE Initiative)
- **Subtitle:** Web-Based De-Addiction Clinical Intake, Rehabilitation Monitoring & Analytics Platform
- **Student Name:** KrChiranjeevi (INT402)
- **Tech Stack:** ASP.NET Core 8.0 MVC | EF Core 8 | SQL Server 2022 | Chart.js | REST Web API

### Slide 2: Problem Statement & National Context
- Decentralized, fragmented records across state de-addiction facilities (*Nasha Mukti Kendras*).
- Inability to track patient recovery outcomes, relapse incidents, and multi-facility visits.
- Risk of data leaks regarding confidential medical and psychological counselling records.
- Absence of real-time national analytics for government welfare allocation.

### Slide 3: Proposed Solution & Core Objectives
- Centralized, multi-facility relational database with strict facility scoping.
- Non-sensitive unique patient coding (`BEN-STATE-YYYY-XXXX`) protecting patient privacy.
- End-to-end clinical lifecycle management: Intake -> Counselling -> Medical Interventions -> Follow-ups.
- Real-time Chart.js analytics powered by SQL views and stored procedures.
- Dual-layer access: MVC portal for administrative staff + RESTful Web API for mobile/external integration.

### Slide 4: System Architecture & Technology Stack
- **Pattern:** N-Tier / Clean MVC Architecture with Dependency Injection.
- **Backend:** C# 12, .NET 8.0 LTS, ASP.NET Core MVC & Web API.
- **ORM:** Entity Framework Core 8.0.10 (Code-First with Migrations).
- **Database:** Microsoft SQL Server 2022.
- **Frontend:** Razor Views, Bootstrap 5.3, Bootstrap Icons, Chart.js.

### Slide 5: Database Schema & Relationships (ERD)
- Core Entities: `States`, `Centres`, `AspNetUsers`, `Beneficiaries`, `CounsellingSessions`, `Interventions`, `FollowUps`.
- 1-to-Many Relationships with `DeleteBehavior.Restrict` ensuring no accidental cascading data loss.
- High-performance indexes on `StateId`, `District`, `BeneficiaryCode`, and `RegistrationDate`.

### Slide 6: Centre Management Module
- Facility registration across states and districts.
- Real-time search, state filtering, and pagination.
- Controlled soft deactivation preserving historical clinical and patient links.

### Slide 7: Beneficiary Intake & Clinical Dossier Module
- Unique identifier auto-generation formula (`BEN-PB-2026-0001`).
- Facility isolation (IDOR protection) ensuring Centre Staff cannot access external patients.
- Unified patient dossier aggregating historical counselling sessions, medical treatments, and follow-ups.

### Slide 8: Clinical Services (Counselling, Interventions, Follow-Ups)
- **Counselling:** Categorized sessions (Individual, Group, Family) with confidential therapy notes protected at the DTO level.
- **Interventions:** Medical Detoxification, Pharmacotherapy, and Psychosocial Support tracking.
- **Follow-Ups:** Automated overdue detection (`ScheduledDate < Today` & `Status = Pending`) and recovery outcome recording.

### Slide 9: Analytics, SQL Views & Stored Procedures
- Solved Cartesian product row-multiplication errors using isolated Common Table Expressions (CTEs).
- `vw_CentreServiceSummary`: Per-facility service aggregations.
- `vw_StateBeneficiarySummary`: State-level rollup.
- `usp_GetCentreServiceSummary`: Parameterized stored procedure with zero dynamic SQL.
- Sanitized CSV exports with formula injection mitigation (CWE-1236).

### Slide 10: RESTful Web API & Dual Authentication
- Versioned endpoints (`/api/v1/...`) with standard `ApiResponse<T>` envelope.
- Dual Authentication: Seamless Cookie Authentication for MVC + JWT Bearer Tokens for Web API.
- Interactive Swagger OpenAPI documentation at `/swagger`.
- Postman test collection with automated token extraction scripts.

### Slide 11: Security Hardening & Audit Results
- HTTP Security Headers (CSP, X-Frame-Options, X-Content-Type-Options, Referrer-Policy).
- Instant session invalidation via `OnValidatePrincipal` and `OnTokenValidated` when accounts are deactivated.
- Anti-CSRF verification on all POST actions.
- 25 automated unit/integration tests with a 100% pass rate.

### Slide 12: Limitations, Future Scope & Conclusion
- **Current Limitations:** In-memory session store suitable for single-node hosting; SMS/WhatsApp reminder integrations require external gateways.
- **Future Enhancements:** Biometric de-duplication, Redis distributed cache for multi-region clustering, AI relapse risk prediction models.
- **Conclusion:** A robust, secure, and academically verified demonstration platform satisfying all syllabus criteria of INT402.

---

## 2. Comprehensive Viva Voce Questions & Accurate Answers

### Q1: Why did you choose ASP.NET Core MVC for this project?
**Answer:** ASP.NET Core MVC is a cross-platform, high-performance, open-source framework that implements the Model-View-Controller architectural pattern. It provides built-in dependency injection, a robust middleware pipeline, integrated ASP.NET Core Identity for authentication and authorization, and the Razor view engine for dynamic server-side HTML rendering. It also enables dual deployment of server-rendered MVC views alongside RESTful Web API controllers within a single unified host.

### Q2: What is the Razor View Engine and how does it execute?
**Answer:** Razor is ASP.NET Core's server-side templating engine that allows embedding C# code within HTML using the `@` symbol syntax. Razor files (`.cshtml`) are parsed and compiled into C# classes at build/publish time by the Roslyn compiler. At runtime, the compiled Razor view executes on the server, combines HTML markup with the supplied ViewModel data, and streams the resulting pure HTML/CSS/JavaScript response to the client browser.

### Q3: How does Model Binding work in ASP.NET Core?
**Answer:** Model binding is the automated process by which ASP.NET Core extracts data from various parts of an HTTP request (form fields, route values, query string parameters, or JSON request bodies) and maps them into strongly typed C# action method parameters or ViewModel properties. It performs data type conversion, executes DataAnnotation validation rules (e.g. `[Required]`, `[StringLength]`), and populates `ModelState.IsValid` to allow controllers to verify data integrity before executing business logic.

### Q4: What is Dependency Injection (DI) and how is it used here?
**Answer:** Dependency Injection is a design pattern that implements Inversion of Control (IoC), allowing classes to receive their dependencies from an external container rather than instantiating them directly. In ASP.NET Core, services are registered in `Program.cs` with specific lifetimes:
- **Scoped (`AddScoped`):** Created once per client request (e.g., `ApplicationDbContext`, `ICentreService`, `IBeneficiaryService`).
- **Singleton (`AddSingleton`):** Created once and shared across all requests.
- **Transient (`AddTransient`):** Created every time they are requested.
Our application uses Scoped injection for database context and domain services to ensure transaction consistency per HTTP request.

### Q5: What is Middleware and what is the pipeline order?
**Answer:** Middleware is software assembled into an application pipeline to handle HTTP requests and responses. Each middleware component can either pass execution to the next component using `next()` or short-circuit the pipeline. The sequence is critical:
1. Exception handling (`UseExceptionHandler`)
2. Security headers (`Use`)
3. Static files (`UseStaticFiles`)
4. Routing (`UseRouting`)
5. Authentication (`UseAuthentication`) — *Must precede Authorization*
6. Authorization (`UseAuthorization`)
7. Endpoint mapping (`MapControllerRoute`)

### Q6: What is EF Core Code-First approach?
**Answer:** In EF Core Code-First, the C# domain model classes (`State`, `Centre`, `Beneficiary`, `CounsellingSession`) and the `DbContext` configuration are authored first in C#. Entity Framework Core then generates relational database schema, tables, constraints, and indexes using migrations, keeping the database schema synchronized with C# code without manual DDL scripting.

### Q7: What are Migrations in Entity Framework Core?
**Answer:** Migrations are a mechanism in EF Core to incrementally update the database schema over time to keep it in sync with the application's data model, while preserving existing data. Each migration consists of an `Up()` method (applying changes) and a `Down()` method (reverting changes). Applied migrations are tracked in the database inside the `__EFMigrationsHistory` table.

### Q8: What are Primary Keys and Foreign Keys?
**Answer:**
- **Primary Key (PK):** A column or combination of columns that uniquely identifies every row in a relational database table (e.g., `CentreId` in `Centres`, `BeneficiaryId` in `Beneficiaries`). It enforces entity integrity and cannot contain null values.
- **Foreign Key (FK):** A column in one table that references the Primary Key of another table (e.g., `StateId` in `Centres` references `States(StateId)`). It enforces referential integrity. In our system, all clinical FKs use `ON DELETE RESTRICT` to prevent accidental cascading data loss.

### Q9: What is the difference between a SQL View and a Stored Procedure?
**Answer:**
- **SQL View (`vw_CentreServiceSummary`):** A saved virtual table defined by a SQL query. It does not accept input parameters, cannot perform data modifications (INSERT/UPDATE/DELETE), and can be queried directly in the `FROM` clause of other SQL statements just like a table.
- **Stored Procedure (`usp_GetCentreServiceSummary`):** A precompiled batch of SQL statements saved in the database. It can accept input and output parameters (`@CentreId`, `@StartDate`), can perform complex control-flow operations (IF/ELSE, loops, transactions), and is executed using `EXEC procedure_name`.

### Q10: What is REST and what makes an API RESTful?
**Answer:** REST (Representational State Transfer) is an architectural style for networked systems. An API is RESTful when it adheres to core principles:
1. **Statelessness:** Every request contains all information needed to process it; the server stores no client session context.
2. **Client-Server Separation:** Client and server evolve independently.
3. **Uniform Interface:** Resources are identified by standard URI paths (`/api/v1/beneficiaries`), manipulated via standard HTTP verbs (`GET` for retrieve, `POST` for create, `PUT` for replace/update, `DELETE` for remove/deactivate), and communicate via standard MIME representations (JSON).
4. **Standard Status Codes:** Returns HTTP 200/201 on success, 400 for validation errors, 401 for unauthenticated, 403 for unauthorized, and 404 for missing resources.

### Q11: What is the difference between Authentication and Authorization?
**Answer:**
- **Authentication (AuthN):** The process of verifying *who* a user is (identity verification). In our application, this is handled by validating credentials via ASP.NET Core Identity (cookies for MVC or JWT Bearer tokens for API).
- **Authorization (AuthZ):** The process of verifying *what permissions* the authenticated user has. In our project, this is implemented via Role-based access (`[Authorize(Roles = "Admin")]`) and Facility-scoping checks via `ICentreAuthorizationService`.

### Q12: How does Centre-Level Access Control (IDOR Prevention) work in your code?
**Answer:** Insecure Direct Object Reference (IDOR) is prevented using the `ICentreAuthorizationService`. When a `CentreStaff` user attempts to view, edit, or create records:
1. The server reads the user's assigned `CentreId` directly from the database claims (never trusting form inputs or URL route parameters).
2. For intake creation, `model.CentreId = userCentreId.Value` is strictly forced on the server.
3. For patient lookup (`/Beneficiaries/Details/5`), the service queries the database to verify `beneficiary.CentreId == userCentreId.Value`. If they do not match, HTTP 403 Forbidden is returned.

### Q13: How are SQL Injection and CSRF attacks mitigated?
**Answer:**
- **SQL Injection:** Entity Framework Core uses parameterized LINQ queries, automatically generating parameterized SQL (`@p0, @p1`) which ensures user input is treated strictly as literal data, never executable code. Stored procedures also use strongly typed parameters.
- **Cross-Site Request Forgery (CSRF):** All MVC state-changing POST forms include `@Html.AntiForgeryToken()`, and controller actions are decorated with `[ValidateAntiForgeryToken]`. The server validates that the cryptographic request token matches the user's anti-forgery session cookie. Logout requires a POST request with CSRF token validation.

### Q14: How are Dashboard and Report counts calculated without Cartesian product errors?
**Answer:** A common error in reporting queries with multiple 1-to-many relationships (e.g. Beneficiaries -> Counselling Sessions, Beneficiaries -> Interventions) is performing simultaneous `LEFT JOIN`s on detail tables, which multiplies rows (Cartesian product) and inflates totals.
In our application, this is prevented in both SQL Views and C# services by **aggregating detail tables independently first** using Common Table Expressions (`WITH CounsellingAgg AS (...)`, `WITH InterventionAgg AS (...)`) or separate LINQ queries grouped by `CentreId`, and then joining the single-row aggregated metrics to the parent `Centres` table.

