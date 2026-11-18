# Database Architecture and Technical Schema Documentation

**Project:** Centralised Nasha Mukti Database (INT402)  
**Database Engine:** Microsoft SQL Server 2019 / 2022 / Azure SQL Database  
**Data Access Method:** Entity Framework Core 8.0 (Code-First with Migrations)  
**Encoding:** UTF-8 / Unicode (`nvarchar`)  

---

## 1. Database Overview

The relational database architecture is designed to support high-integrity clinical transaction recording, facility tracking, and national recovery reporting for the Ministry of Social Justice and Empowerment (MoSJE).

### Core Design Rules
1. **Primary Keys:** Synthetic Identity columns (`INT IDENTITY(1,1)` or `NVARCHAR(450)` GUID for Identity tables).
2. **Clinical History Retention:** All foreign keys referencing Centres or Beneficiaries enforce `ON DELETE NO ACTION / RESTRICT`. Hard cascading deletes are forbidden. Soft deactivation (`IsActive = 0` or `Status = 'Inactive'`) is implemented to maintain permanent clinical audit trails.
3. **No Cartesian Explosion:** Analytical queries and views isolate aggregations using Common Table Expressions (CTEs) before joining, preventing inflated counts across 1-to-many relationships.

---

## 2. Relational Table Specifications

### A. Core Directory & Clinical Tables

#### Table: `States`
Master table of Indian States and Union Territories.
| Column | Data Type | Nullable | Default | Description |
| :--- | :--- | :--- | :--- | :--- |
| `StateId` | `INT IDENTITY(1,1)` | **NO (PK)** | Identity | Primary key |
| `StateName` | `NVARCHAR(100)` | **NO** | - | Name of State / UT (e.g. Punjab, Bihar) |
| `IsActive` | `BIT` | **NO** | `1` | Operational flag |

#### Table: `Centres`
Authorized de-addiction facilities (*Nasha Mukti Kendras*).
| Column | Data Type | Nullable | Default | Description |
| :--- | :--- | :--- | :--- | :--- |
| `CentreId` | `INT IDENTITY(1,1)` | **NO (PK)** | Identity | Primary key |
| `CentreName` | `NVARCHAR(200)` | **NO** | - | Facility name |
| `StateId` | `INT` | **NO (FK)** | - | Foreign key -> `States(StateId)` |
| `District` | `NVARCHAR(100)` | **NO** | - | Administrative district |
| `Address` | `NVARCHAR(500)` | **NO** | - | Full street address |
| `ContactNumber`| `NVARCHAR(20)` | **NO** | - | Official contact telephone |
| `Email` | `NVARCHAR(150)` | **NO** | - | Facility email address |
| `IsActive` | `BIT` | **NO** | `1` | Facility operational status |
| `CreatedAt` | `DATETIME2` | **NO** | `GETUTCDATE()` | Registration timestamp |

#### Table: `Beneficiaries`
Individuals registered for de-addiction and counselling services.
| Column | Data Type | Nullable | Default | Description |
| :--- | :--- | :--- | :--- | :--- |
| `BeneficiaryId` | `INT IDENTITY(1,1)` | **NO (PK)** | Identity | Primary key |
| `BeneficiaryCode`| `NVARCHAR(50)` | **NO (UQ)** | - | Unique system code (`BEN-PB-2026-0001`) |
| `CentreId` | `INT` | **NO (FK)** | - | Foreign key -> `Centres(CentreId)` |
| `FullName` | `NVARCHAR(150)` | **NO** | - | Beneficiary's full name |
| `DateOfBirth` | `DATE` | YES | `NULL` | Optional birth date |
| `RegistrationDate`| `DATE` | **NO** | - | Intake date |
| `Gender` | `NVARCHAR(20)` | YES | `NULL` | Gender (Male, Female, Other) |
| `ContactNumber`| `NVARCHAR(20)` | YES | `NULL` | Phone number |
| `Status` | `NVARCHAR(50)` | **NO** | `'Active'` | Status (`Active`, `Inactive`, `Completed`) |
| `CreatedAt` | `DATETIME2` | **NO** | `GETUTCDATE()` | Intake record creation time |
| `UpdatedAt` | `DATETIME2` | YES | `NULL` | Last modification time |

#### Table: `CounsellingSessions`
Psychological and therapy sessions conducted with beneficiaries.
| Column | Data Type | Nullable | Default | Description |
| :--- | :--- | :--- | :--- | :--- |
| `SessionId` | `INT IDENTITY(1,1)` | **NO (PK)** | Identity | Primary key |
| `BeneficiaryId`| `INT` | **NO (FK)** | - | Foreign key -> `Beneficiaries(BeneficiaryId)` |
| `SessionDate` | `DATE` | **NO** | - | Date session occurred |
| `SessionType` | `NVARCHAR(100)` | **NO** | `'Individual'`| Individual, Group, Family, Relapse Prevention |
| `CounsellorName`| `NVARCHAR(150)` | YES | `NULL` | Counsellor / Therapist name |
| `SessionStatus`| `NVARCHAR(50)` | **NO** | `'Completed'` | Completed, Scheduled, Cancelled, No-Show |
| `Notes` | `NVARCHAR(2000)`| YES | `NULL` | **Confidential** clinical session notes |
| `CreatedAt` | `DATETIME2` | **NO** | `GETUTCDATE()` | Record creation time |
| `UpdatedAt` | `DATETIME2` | YES | `NULL` | Last update time |

#### Table: `Interventions`
Medical and psychosocial rehabilitation treatments.
| Column | Data Type | Nullable | Default | Description |
| :--- | :--- | :--- | :--- | :--- |
| `InterventionId`| `INT IDENTITY(1,1)` | **NO (PK)** | Identity | Primary key |
| `BeneficiaryId`| `INT` | **NO (FK)** | - | Foreign key -> `Beneficiaries(BeneficiaryId)` |
| `InterventionType`|`NVARCHAR(100)` | **NO** | - | Detoxification, Pharmacotherapy, etc. |
| `ServiceDate` | `DATE` | **NO** | - | Treatment delivery date |
| `Status` | `NVARCHAR(50)` | **NO** | `'In Progress'`| In Progress, Completed, Discontinued |
| `ResponsibleStaffName`|`NVARCHAR(150)`| YES | `NULL` | Doctor / Medical Officer / Worker |
| `Notes` | `NVARCHAR(2000)`| YES | `NULL` | Clinical treatment remarks |
| `CreatedAt` | `DATETIME2` | **NO** | `GETUTCDATE()` | Record timestamp |
| `UpdatedAt` | `DATETIME2` | YES | `NULL` | Last update timestamp |

#### Table: `FollowUps`
Post-treatment rehabilitation recovery visits.
| Column | Data Type | Nullable | Default | Description |
| :--- | :--- | :--- | :--- | :--- |
| `FollowUpId` | `INT IDENTITY(1,1)` | **NO (PK)** | Identity | Primary key |
| `BeneficiaryId`| `INT` | **NO (FK)** | - | Foreign key -> `Beneficiaries(BeneficiaryId)` |
| `ScheduledDate`| `DATE` | **NO** | - | Scheduled appointment date |
| `Status` | `NVARCHAR(50)` | **NO** | `'Pending'` | Pending, Completed, Missed, Cancelled |
| `CompletedDate`| `DATE` | YES | `NULL` | Actual date attended |
| `Notes` | `NVARCHAR(1000)`| YES | `NULL` | Recovery evaluation notes |
| `CreatedAt` | `DATETIME2` | **NO** | `GETUTCDATE()` | Appointment creation time |
| `UpdatedAt` | `DATETIME2` | YES | `NULL` | Modification time |

---

### B. ASP.NET Core Identity Tables
- `AspNetUsers`: Extends `IdentityUser` with `FullName`, `CentreId` (nullable FK to `Centres`), `IsActive`, `CreatedAt`.
- `AspNetRoles`: Identity roles (`Admin`, `CentreStaff`).
- `AspNetUserRoles`: Mapping table (`UserId`, `RoleId`).
- `AspNetUserClaims`, `AspNetUserLogins`, `AspNetUserTokens`, `AspNetRoleClaims`.

---

## 3. Database Indexes and Performance Optimizations

```sql
-- Unique Indexes
CREATE UNIQUE INDEX [UQ_Beneficiaries_BeneficiaryCode] ON [Beneficiaries]([BeneficiaryCode]);

-- Scoping & Foreign Key Indexes
CREATE INDEX [IX_Centres_StateId] ON [Centres]([StateId]);
CREATE INDEX [IX_Centres_District] ON [Centres]([District]);
CREATE INDEX [IX_Centres_IsActive] ON [Centres]([IsActive]);
CREATE INDEX [IX_Beneficiaries_CentreId] ON [Beneficiaries]([CentreId]);
CREATE INDEX [IX_Beneficiaries_Status] ON [Beneficiaries]([Status]);
CREATE INDEX [IX_Beneficiaries_RegistrationDate] ON [Beneficiaries]([RegistrationDate]);
CREATE INDEX [IX_AspNetUsers_CentreId] ON [AspNetUsers]([CentreId]);
CREATE INDEX [IX_CounsellingSessions_BeneficiaryId] ON [CounsellingSessions]([BeneficiaryId]);
CREATE INDEX [IX_CounsellingSessions_SessionDate] ON [CounsellingSessions]([SessionDate]);
CREATE INDEX [IX_Interventions_BeneficiaryId] ON [Interventions]([BeneficiaryId]);
CREATE INDEX [IX_Interventions_ServiceDate] ON [Interventions]([ServiceDate]);
CREATE INDEX [IX_FollowUps_BeneficiaryId] ON [FollowUps]([BeneficiaryId]);
CREATE INDEX [IX_FollowUps_ScheduledDate] ON [FollowUps]([ScheduledDate]);
```

---

## 4. Entity Framework Core Migrations

| Migration Name | Applied Objects | Purpose |
| :--- | :--- | :--- |
| `20261117215736_InitialDatabase` | `States`, `Centres`, `Beneficiaries` | Baseline database creation & initial seed data. |
| `20261117220450_AddIdentitySupport` | `AspNetUsers`, `AspNetRoles`, `AspNetUserRoles`, etc. | ASP.NET Core Identity authentication integration. |
| `20261117222831_AddClinicalModules` | `CounsellingSessions`, `Interventions`, `FollowUps` | Comprehensive clinical management tables. |
| `20261117223615_AddReportingViewsAndProcedures` | `vw_CentreServiceSummary`, `vw_StateBeneficiarySummary`, `usp_GetCentreServiceSummary` | SQL Server views & analytical stored procedures. |

---

## 5. SQL Views and Stored Procedures

### A. SQL View: `vw_CentreServiceSummary`
Aggregates beneficiaries, counselling sessions, interventions, and follow-ups per centre using isolated CTEs to prevent row inflation.

```sql
CREATE OR ALTER VIEW vw_CentreServiceSummary AS
WITH BenAgg AS (
    SELECT 
        CentreId,
        COUNT(1) AS TotalBeneficiaries,
        COUNT(CASE WHEN Status = 'Active' THEN 1 END) AS ActiveBeneficiaries
    FROM Beneficiaries
    GROUP BY CentreId
),
CounsellingAgg AS (
    SELECT 
        b.CentreId,
        COUNT(1) AS TotalCounsellingSessions,
        COUNT(CASE WHEN cs.SessionStatus = 'Completed' THEN 1 END) AS CompletedCounsellingSessions
    FROM CounsellingSessions cs
    INNER JOIN Beneficiaries b ON cs.BeneficiaryId = b.BeneficiaryId
    GROUP BY b.CentreId
),
InterventionAgg AS (
    SELECT 
        b.CentreId,
        COUNT(1) AS TotalInterventions
    FROM Interventions i
    INNER JOIN Beneficiaries b ON i.BeneficiaryId = b.BeneficiaryId
    GROUP BY b.CentreId
),
FollowUpAgg AS (
    SELECT 
        b.CentreId,
        COUNT(1) AS TotalFollowUps,
        COUNT(CASE WHEN fu.Status = 'Pending' THEN 1 END) AS PendingFollowUps,
        COUNT(CASE WHEN fu.Status = 'Pending' AND fu.ScheduledDate < CAST(GETUTCDATE() AS DATE) THEN 1 END) AS OverdueFollowUps
    FROM FollowUps fu
    INNER JOIN Beneficiaries b ON fu.BeneficiaryId = b.BeneficiaryId
    GROUP BY b.CentreId
)
SELECT 
    c.CentreId,
    c.CentreName,
    c.StateId,
    s.StateName,
    c.District,
    c.IsActive,
    ISNULL(ba.TotalBeneficiaries, 0) AS TotalBeneficiaries,
    ISNULL(ba.ActiveBeneficiaries, 0) AS ActiveBeneficiaries,
    ISNULL(ca.TotalCounsellingSessions, 0) AS TotalCounsellingSessions,
    ISNULL(ca.CompletedCounsellingSessions, 0) AS CompletedCounsellingSessions,
    ISNULL(ia.TotalInterventions, 0) AS TotalInterventions,
    ISNULL(fa.TotalFollowUps, 0) AS TotalFollowUps,
    ISNULL(fa.PendingFollowUps, 0) AS PendingFollowUps,
    ISNULL(fa.OverdueFollowUps, 0) AS OverdueFollowUps
FROM Centres c
INNER JOIN States s ON c.StateId = s.StateId
LEFT JOIN BenAgg ba ON c.CentreId = ba.CentreId
LEFT JOIN CounsellingAgg ca ON c.CentreId = ca.CentreId
LEFT JOIN InterventionAgg ia ON c.CentreId = ia.CentreId
LEFT JOIN FollowUpAgg fa ON c.CentreId = fa.CentreId;
```

### B. SQL View: `vw_StateBeneficiarySummary`
Rolls up facility counts, active patients, and clinical interventions to the State / UT level.

```sql
CREATE OR ALTER VIEW vw_StateBeneficiarySummary AS
WITH CentreAgg AS (
    SELECT 
        StateId,
        COUNT(1) AS TotalCentres,
        COUNT(CASE WHEN IsActive = 1 THEN 1 END) AS ActiveCentres
    FROM Centres
    GROUP BY StateId
),
BenAgg AS (
    SELECT 
        c.StateId,
        COUNT(1) AS TotalBeneficiaries,
        COUNT(CASE WHEN b.Status = 'Active' THEN 1 END) AS ActiveBeneficiaries
    FROM Beneficiaries b
    INNER JOIN Centres c ON b.CentreId = c.CentreId
    GROUP BY c.StateId
),
CounsellingAgg AS (
    SELECT 
        c.StateId,
        COUNT(1) AS TotalCounsellingSessions
    FROM CounsellingSessions cs
    INNER JOIN Beneficiaries b ON cs.BeneficiaryId = b.BeneficiaryId
    INNER JOIN Centres c ON b.CentreId = c.CentreId
    GROUP BY c.StateId
),
InterventionAgg AS (
    SELECT 
        c.StateId,
        COUNT(1) AS TotalInterventions
    FROM Interventions i
    INNER JOIN Beneficiaries b ON i.BeneficiaryId = b.BeneficiaryId
    INNER JOIN Centres c ON b.CentreId = c.CentreId
    GROUP BY c.StateId
)
SELECT 
    s.StateId,
    s.StateName,
    s.IsActive,
    ISNULL(ca.TotalCentres, 0) AS TotalCentres,
    ISNULL(ca.ActiveCentres, 0) AS ActiveCentres,
    ISNULL(ba.TotalBeneficiaries, 0) AS TotalBeneficiaries,
    ISNULL(ba.ActiveBeneficiaries, 0) AS ActiveBeneficiaries,
    ISNULL(csa.TotalCounsellingSessions, 0) AS TotalCounsellingSessions,
    ISNULL(ia.TotalInterventions, 0) AS TotalInterventions
FROM States s
LEFT JOIN CentreAgg ca ON s.StateId = ca.StateId
LEFT JOIN BenAgg ba ON s.StateId = ba.StateId
LEFT JOIN CounsellingAgg csa ON s.StateId = csa.StateId
LEFT JOIN InterventionAgg ia ON s.StateId = ia.StateId;
```

### C. Stored Procedure: `usp_GetCentreServiceSummary`
Accepts optional filtering parameters (`@CentreId`, `@StateId`, `@StartDate`, `@EndDate`) and returns real-time aggregated metrics with zero dynamic SQL.

```sql
CREATE OR ALTER PROCEDURE usp_GetCentreServiceSummary
    @CentreId INT = NULL,
    @StateId INT = NULL,
    @StartDate DATE = NULL,
    @EndDate DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @CurrentDate DATE = CAST(GETUTCDATE() AS DATE);

    SELECT 
        c.CentreId,
        c.CentreName,
        s.StateName,
        c.District,
        c.IsActive,
        ISNULL(b.BeneficiaryCount, 0) AS BeneficiaryCount,
        ISNULL(b.ActiveBeneficiaryCount, 0) AS ActiveBeneficiaryCount,
        ISNULL(cs.CounsellingCount, 0) AS CounsellingCount,
        ISNULL(i.InterventionCount, 0) AS InterventionCount,
        ISNULL(fu.PendingFollowUpCount, 0) AS PendingFollowUpCount,
        ISNULL(fu.OverdueFollowUpCount, 0) AS OverdueFollowUpCount
    FROM Centres c
    INNER JOIN States s ON c.StateId = s.StateId
    LEFT JOIN (
        SELECT 
            CentreId,
            COUNT(1) AS BeneficiaryCount,
            COUNT(CASE WHEN Status = 'Active' THEN 1 END) AS ActiveBeneficiaryCount
        FROM Beneficiaries
        WHERE (@StartDate IS NULL OR RegistrationDate >= @StartDate)
          AND (@EndDate IS NULL OR RegistrationDate <= @EndDate)
        GROUP BY CentreId
    ) b ON c.CentreId = b.CentreId
    LEFT JOIN (
        SELECT 
            ben.CentreId,
            COUNT(1) AS CounsellingCount
        FROM CounsellingSessions sess
        INNER JOIN Beneficiaries ben ON sess.BeneficiaryId = ben.BeneficiaryId
        WHERE (@StartDate IS NULL OR sess.SessionDate >= @StartDate)
          AND (@EndDate IS NULL OR sess.SessionDate <= @EndDate)
        GROUP BY ben.CentreId
    ) cs ON c.CentreId = cs.CentreId
    LEFT JOIN (
        SELECT 
            ben.CentreId,
            COUNT(1) AS InterventionCount
        FROM Interventions itv
        INNER JOIN Beneficiaries ben ON itv.BeneficiaryId = ben.BeneficiaryId
        WHERE (@StartDate IS NULL OR itv.ServiceDate >= @StartDate)
          AND (@EndDate IS NULL OR itv.ServiceDate <= @EndDate)
        GROUP BY ben.CentreId
    ) i ON c.CentreId = i.CentreId
    LEFT JOIN (
        SELECT 
            ben.CentreId,
            COUNT(CASE WHEN Status = 'Pending' THEN 1 END) AS PendingFollowUpCount,
            COUNT(CASE WHEN Status = 'Pending' AND ScheduledDate < @CurrentDate THEN 1 END) AS OverdueFollowUpCount
        FROM FollowUps flw
        INNER JOIN Beneficiaries ben ON flw.BeneficiaryId = ben.BeneficiaryId
        WHERE (@StartDate IS NULL OR flw.ScheduledDate >= @StartDate)
          AND (@EndDate IS NULL OR flw.ScheduledDate <= @EndDate)
        GROUP BY ben.CentreId
    ) fu ON c.CentreId = fu.CentreId
    WHERE (@CentreId IS NULL OR c.CentreId = @CentreId)
      AND (@StateId IS NULL OR c.StateId = @StateId)
    ORDER BY c.CentreName;
END;
```

