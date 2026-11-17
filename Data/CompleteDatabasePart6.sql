IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117215736_InitialDatabase'
)
BEGIN
    CREATE TABLE [States] (
        [StateId] int NOT NULL IDENTITY,
        [StateName] nvarchar(100) NOT NULL,
        [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
        CONSTRAINT [PK_States] PRIMARY KEY ([StateId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117215736_InitialDatabase'
)
BEGIN
    CREATE TABLE [Centres] (
        [CentreId] int NOT NULL IDENTITY,
        [CentreName] nvarchar(200) NOT NULL,
        [StateId] int NOT NULL,
        [District] nvarchar(100) NOT NULL,
        [Address] nvarchar(500) NOT NULL,
        [ContactNumber] nvarchar(20) NOT NULL,
        [Email] nvarchar(150) NOT NULL,
        [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
        [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        CONSTRAINT [PK_Centres] PRIMARY KEY ([CentreId]),
        CONSTRAINT [FK_Centres_States] FOREIGN KEY ([StateId]) REFERENCES [States] ([StateId]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117215736_InitialDatabase'
)
BEGIN
    CREATE TABLE [Beneficiaries] (
        [BeneficiaryId] int NOT NULL IDENTITY,
        [BeneficiaryCode] nvarchar(50) NOT NULL,
        [CentreId] int NOT NULL,
        [FullName] nvarchar(150) NOT NULL,
        [DateOfBirth] date NULL,
        [RegistrationDate] date NOT NULL,
        [Gender] nvarchar(20) NULL,
        [ContactNumber] nvarchar(20) NULL,
        [Status] nvarchar(50) NOT NULL DEFAULT N'Active',
        [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_Beneficiaries] PRIMARY KEY ([BeneficiaryId]),
        CONSTRAINT [FK_Beneficiaries_Centres] FOREIGN KEY ([CentreId]) REFERENCES [Centres] ([CentreId]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117215736_InitialDatabase'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'StateId', N'IsActive', N'StateName') AND [object_id] = OBJECT_ID(N'[States]'))
        SET IDENTITY_INSERT [States] ON;
    EXEC(N'INSERT INTO [States] ([StateId], [IsActive], [StateName])
    VALUES (1, CAST(1 AS bit), N''Punjab''),
    (2, CAST(1 AS bit), N''Bihar''),
    (3, CAST(1 AS bit), N''Maharashtra''),
    (4, CAST(1 AS bit), N''Rajasthan''),
    (5, CAST(1 AS bit), N''Delhi''),
    (6, CAST(1 AS bit), N''Madhya Pradesh''),
    (7, CAST(1 AS bit), N''Uttar Pradesh''),
    (8, CAST(1 AS bit), N''Haryana'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'StateId', N'IsActive', N'StateName') AND [object_id] = OBJECT_ID(N'[States]'))
        SET IDENTITY_INSERT [States] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117215736_InitialDatabase'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'CentreId', N'Address', N'CentreName', N'ContactNumber', N'CreatedAt', N'District', N'Email', N'IsActive', N'StateId') AND [object_id] = OBJECT_ID(N'[Centres]'))
        SET IDENTITY_INSERT [Centres] ON;
    EXEC(N'INSERT INTO [Centres] ([CentreId], [Address], [CentreName], [ContactNumber], [CreatedAt], [District], [Email], [IsActive], [StateId])
    VALUES (1, N''Civil Lines, Near Red Cross Bhawan, Amritsar, Punjab - 143001'', N''Navchetan De-Addiction & Rehabilitation Centre'', N''0183-2550101'', ''2026-01-15T00:00:00.0000000Z'', N''Amritsar'', N''navchetan.amritsar@example.gov.in'', CAST(1 AS bit), 1),
    (2, N''Model Town, Phase II, Ludhiana, Punjab - 141002'', N''Asha Kiran Counselling and Recovery Centre'', N''0161-2440202'', ''2026-01-20T00:00:00.0000000Z'', N''Ludhiana'', N''ashakiran.ludhiana@example.gov.in'', CAST(1 AS bit), 1),
    (3, N''Bailey Road, Near High Court, Patna, Bihar - 800001'', N''Naya Savera Integrated Rehabilitation Facility'', N''0612-2230303'', ''2026-02-01T00:00:00.0000000Z'', N''Patna'', N''nayasavera.patna@example.gov.in'', CAST(1 AS bit), 2),
    (4, N''Shivajinagar, J.M. Road, Pune, Maharashtra - 411005'', N''Prerna De-Addiction and Care Kendra'', N''020-25510404'', ''2026-02-10T00:00:00.0000000Z'', N''Pune'', N''prerna.pune@example.gov.in'', CAST(1 AS bit), 3),
    (5, N''Tonk Road, Gandhi Nagar, Jaipur, Rajasthan - 302015'', N''Sanjeevani Wellness and Rehabilitation Kendra'', N''0141-2700505'', ''2026-02-15T00:00:00.0000000Z'', N''Jaipur'', N''sanjeevani.jaipur@example.gov.in'', CAST(1 AS bit), 4)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'CentreId', N'Address', N'CentreName', N'ContactNumber', N'CreatedAt', N'District', N'Email', N'IsActive', N'StateId') AND [object_id] = OBJECT_ID(N'[Centres]'))
        SET IDENTITY_INSERT [Centres] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117215736_InitialDatabase'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'BeneficiaryId', N'BeneficiaryCode', N'CentreId', N'ContactNumber', N'CreatedAt', N'DateOfBirth', N'FullName', N'Gender', N'RegistrationDate', N'Status', N'UpdatedAt') AND [object_id] = OBJECT_ID(N'[Beneficiaries]'))
        SET IDENTITY_INSERT [Beneficiaries] ON;
    EXEC(N'INSERT INTO [Beneficiaries] ([BeneficiaryId], [BeneficiaryCode], [CentreId], [ContactNumber], [CreatedAt], [DateOfBirth], [FullName], [Gender], [RegistrationDate], [Status], [UpdatedAt])
    VALUES (1, N''BEN-PB-2026-0001'', 1, N''9876543210'', ''2026-02-01T00:00:00.0000000Z'', ''1992-05-14'', N''Gurpreet Singh (Demo)'', N''Male'', ''2026-02-01'', N''Active'', NULL),
    (2, N''BEN-PB-2026-0002'', 1, N''9876543211'', ''2026-02-03T00:00:00.0000000Z'', ''1988-11-20'', N''Jaswinder Kaur (Demo)'', N''Female'', ''2026-02-03'', N''Active'', NULL),
    (3, N''BEN-PB-2026-0003'', 2, N''9876543212'', ''2026-02-10T00:00:00.0000000Z'', ''1995-08-09'', N''Harmanpreet Sharma (Demo)'', N''Male'', ''2026-02-10'', N''Active'', NULL),
    (4, N''BEN-BR-2026-0001'', 3, N''9876543213'', ''2026-02-12T00:00:00.0000000Z'', ''1985-03-17'', N''Ramesh Kumar Verma (Demo)'', N''Male'', ''2026-02-12'', N''Active'', NULL),
    (5, N''BEN-MH-2026-0001'', 4, N''9876543214'', ''2026-02-14T00:00:00.0000000Z'', ''1990-07-25'', N''Sachin Raghunath Patil (Demo)'', N''Male'', ''2026-02-14'', N''Active'', NULL),
    (6, N''BEN-RJ-2026-0001'', 5, N''9876543215'', ''2026-02-18T00:00:00.0000000Z'', ''1993-12-04'', N''Vikram Singh Rathore (Demo)'', N''Male'', ''2026-02-18'', N''Active'', NULL)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'BeneficiaryId', N'BeneficiaryCode', N'CentreId', N'ContactNumber', N'CreatedAt', N'DateOfBirth', N'FullName', N'Gender', N'RegistrationDate', N'Status', N'UpdatedAt') AND [object_id] = OBJECT_ID(N'[Beneficiaries]'))
        SET IDENTITY_INSERT [Beneficiaries] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117215736_InitialDatabase'
)
BEGIN
    CREATE INDEX [IX_Beneficiaries_CentreId] ON [Beneficiaries] ([CentreId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117215736_InitialDatabase'
)
BEGIN
    CREATE INDEX [IX_Beneficiaries_FullName] ON [Beneficiaries] ([FullName]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117215736_InitialDatabase'
)
BEGIN
    CREATE INDEX [IX_Beneficiaries_RegistrationDate] ON [Beneficiaries] ([RegistrationDate]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117215736_InitialDatabase'
)
BEGIN
    CREATE INDEX [IX_Beneficiaries_Status] ON [Beneficiaries] ([Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117215736_InitialDatabase'
)
BEGIN
    CREATE UNIQUE INDEX [UQ_Beneficiaries_BeneficiaryCode] ON [Beneficiaries] ([BeneficiaryCode]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117215736_InitialDatabase'
)
BEGIN
    CREATE INDEX [IX_Centres_CentreName] ON [Centres] ([CentreName]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117215736_InitialDatabase'
)
BEGIN
    CREATE INDEX [IX_Centres_District] ON [Centres] ([District]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117215736_InitialDatabase'
)
BEGIN
    CREATE INDEX [IX_Centres_IsActive] ON [Centres] ([IsActive]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117215736_InitialDatabase'
)
BEGIN
    CREATE INDEX [IX_Centres_StateId] ON [Centres] ([StateId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117215736_InitialDatabase'
)
BEGIN
    CREATE INDEX [IX_States_StateName] ON [States] ([StateName]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117215736_InitialDatabase'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261117215736_InitialDatabase', N'8.0.10');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117220450_AddIdentitySupport'
)
BEGIN
    CREATE TABLE [AspNetRoles] (
        [Id] nvarchar(450) NOT NULL,
        [Name] nvarchar(256) NULL,
        [NormalizedName] nvarchar(256) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117220450_AddIdentitySupport'
)
BEGIN
    CREATE TABLE [AspNetUsers] (
        [Id] nvarchar(450) NOT NULL,
        [FullName] nvarchar(150) NOT NULL,
        [CentreId] int NULL,
        [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
        [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        [UserName] nvarchar(256) NULL,
        [NormalizedUserName] nvarchar(256) NULL,
        [Email] nvarchar(256) NULL,
        [NormalizedEmail] nvarchar(256) NULL,
        [EmailConfirmed] bit NOT NULL,
        [PasswordHash] nvarchar(max) NULL,
        [SecurityStamp] nvarchar(max) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        [PhoneNumber] nvarchar(max) NULL,
        [PhoneNumberConfirmed] bit NOT NULL,
        [TwoFactorEnabled] bit NOT NULL,
        [LockoutEnd] datetimeoffset NULL,
        [LockoutEnabled] bit NOT NULL,
        [AccessFailedCount] int NOT NULL,
        CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetUsers_Centres] FOREIGN KEY ([CentreId]) REFERENCES [Centres] ([CentreId]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117220450_AddIdentitySupport'
)
BEGIN
    CREATE TABLE [AspNetRoleClaims] (
        [Id] int NOT NULL IDENTITY,
        [RoleId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117220450_AddIdentitySupport'
)
BEGIN
    CREATE TABLE [AspNetUserClaims] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117220450_AddIdentitySupport'
)
BEGIN
    CREATE TABLE [AspNetUserLogins] (
        [LoginProvider] nvarchar(450) NOT NULL,
        [ProviderKey] nvarchar(450) NOT NULL,
        [ProviderDisplayName] nvarchar(max) NULL,
        [UserId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
        CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117220450_AddIdentitySupport'
)
BEGIN
    CREATE TABLE [AspNetUserRoles] (
        [UserId] nvarchar(450) NOT NULL,
        [RoleId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117220450_AddIdentitySupport'
)
BEGIN
    CREATE TABLE [AspNetUserTokens] (
        [UserId] nvarchar(450) NOT NULL,
        [LoginProvider] nvarchar(450) NOT NULL,
        [Name] nvarchar(450) NOT NULL,
        [Value] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
        CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117220450_AddIdentitySupport'
)
BEGIN
    CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117220450_AddIdentitySupport'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117220450_AddIdentitySupport'
)
BEGIN
    CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117220450_AddIdentitySupport'
)
BEGIN
    CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117220450_AddIdentitySupport'
)
BEGIN
    CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117220450_AddIdentitySupport'
)
BEGIN
    CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117220450_AddIdentitySupport'
)
BEGIN
    CREATE INDEX [IX_AspNetUsers_CentreId] ON [AspNetUsers] ([CentreId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117220450_AddIdentitySupport'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117220450_AddIdentitySupport'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261117220450_AddIdentitySupport', N'8.0.10');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117222831_AddClinicalModules'
)
BEGIN
    CREATE TABLE [CounsellingSessions] (
        [SessionId] int NOT NULL IDENTITY,
        [BeneficiaryId] int NOT NULL,
        [SessionDate] date NOT NULL,
        [SessionType] nvarchar(100) NOT NULL,
        [CounsellorName] nvarchar(150) NULL,
        [SessionStatus] nvarchar(50) NOT NULL DEFAULT N'Completed',
        [Notes] nvarchar(2000) NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_CounsellingSessions] PRIMARY KEY ([SessionId]),
        CONSTRAINT [FK_CounsellingSessions_Beneficiaries] FOREIGN KEY ([BeneficiaryId]) REFERENCES [Beneficiaries] ([BeneficiaryId]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117222831_AddClinicalModules'
)
BEGIN
    CREATE TABLE [FollowUps] (
        [FollowUpId] int NOT NULL IDENTITY,
        [BeneficiaryId] int NOT NULL,
        [ScheduledDate] date NOT NULL,
        [Status] nvarchar(50) NOT NULL DEFAULT N'Pending',
        [CompletedDate] date NULL,
        [Notes] nvarchar(1000) NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_FollowUps] PRIMARY KEY ([FollowUpId]),
        CONSTRAINT [FK_FollowUps_Beneficiaries] FOREIGN KEY ([BeneficiaryId]) REFERENCES [Beneficiaries] ([BeneficiaryId]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117222831_AddClinicalModules'
)
BEGIN
    CREATE TABLE [Interventions] (
        [InterventionId] int NOT NULL IDENTITY,
        [BeneficiaryId] int NOT NULL,
        [InterventionType] nvarchar(100) NOT NULL,
        [ServiceDate] date NOT NULL,
        [Status] nvarchar(50) NOT NULL DEFAULT N'In Progress',
        [ResponsibleStaffName] nvarchar(150) NULL,
        [Notes] nvarchar(2000) NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_Interventions] PRIMARY KEY ([InterventionId]),
        CONSTRAINT [FK_Interventions_Beneficiaries] FOREIGN KEY ([BeneficiaryId]) REFERENCES [Beneficiaries] ([BeneficiaryId]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117222831_AddClinicalModules'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'SessionId', N'BeneficiaryId', N'CounsellorName', N'CreatedAt', N'Notes', N'SessionDate', N'SessionStatus', N'SessionType', N'UpdatedAt') AND [object_id] = OBJECT_ID(N'[CounsellingSessions]'))
        SET IDENTITY_INSERT [CounsellingSessions] ON;
    EXEC(N'INSERT INTO [CounsellingSessions] ([SessionId], [BeneficiaryId], [CounsellorName], [CreatedAt], [Notes], [SessionDate], [SessionStatus], [SessionType], [UpdatedAt])
    VALUES (1, 1, N''Dr. Harvinder Singh (Psychologist)'', ''2026-02-05T00:00:00.0000000Z'', N''Initial motivational interviewing completed. Beneficiary expressed readiness for rehabilitation.'', ''2026-02-05'', N''Completed'', N''Individual'', NULL),
    (2, 1, N''Dr. Harvinder Singh (Psychologist)'', ''2026-02-12T00:00:00.0000000Z'', N''Family session conducted with spouse. Established positive household recovery support framework.'', ''2026-02-12'', N''Completed'', N''Family'', NULL),
    (3, 2, N''Dr. Manpreet Kaur (Counsellor)'', ''2026-02-08T00:00:00.0000000Z'', N''Cognitive behavioral therapy intro. Identified primary emotional triggers and coping strategies.'', ''2026-02-08'', N''Completed'', N''Individual'', NULL),
    (4, 4, N''Sanjay Jha (Social Worker)'', ''2026-02-15T00:00:00.0000000Z'', N''Participated in peer support group. Shared challenges and received positive group feedback.'', ''2026-02-15'', N''Completed'', N''Group'', NULL)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'SessionId', N'BeneficiaryId', N'CounsellorName', N'CreatedAt', N'Notes', N'SessionDate', N'SessionStatus', N'SessionType', N'UpdatedAt') AND [object_id] = OBJECT_ID(N'[CounsellingSessions]'))
        SET IDENTITY_INSERT [CounsellingSessions] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117222831_AddClinicalModules'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'FollowUpId', N'BeneficiaryId', N'CompletedDate', N'CreatedAt', N'Notes', N'ScheduledDate', N'Status', N'UpdatedAt') AND [object_id] = OBJECT_ID(N'[FollowUps]'))
        SET IDENTITY_INSERT [FollowUps] ON;
    EXEC(N'INSERT INTO [FollowUps] ([FollowUpId], [BeneficiaryId], [CompletedDate], [CreatedAt], [Notes], [ScheduledDate], [Status], [UpdatedAt])
    VALUES (1, 1, NULL, ''2026-02-12T00:00:00.0000000Z'', N''1-month post-intake recovery evaluation scheduled.'', ''2026-03-01'', N''Pending'', NULL),
    (2, 2, ''2026-02-20'', ''2026-02-10T00:00:00.0000000Z'', N''Routine bi-weekly check-in completed. Beneficiary reports good adherence to program.'', ''2026-02-20'', N''Completed'', NULL),
    (3, 4, NULL, ''2026-02-12T00:00:00.0000000Z'', N''Beneficiary was unable to attend due to travel. Re-contact initiated by field team.'', ''2026-02-15'', N''Missed'', NULL)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'FollowUpId', N'BeneficiaryId', N'CompletedDate', N'CreatedAt', N'Notes', N'ScheduledDate', N'Status', N'UpdatedAt') AND [object_id] = OBJECT_ID(N'[FollowUps]'))
        SET IDENTITY_INSERT [FollowUps] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117222831_AddClinicalModules'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'InterventionId', N'BeneficiaryId', N'CreatedAt', N'InterventionType', N'Notes', N'ResponsibleStaffName', N'ServiceDate', N'Status', N'UpdatedAt') AND [object_id] = OBJECT_ID(N'[Interventions]'))
        SET IDENTITY_INSERT [Interventions] ON;
    EXEC(N'INSERT INTO [Interventions] ([InterventionId], [BeneficiaryId], [CreatedAt], [InterventionType], [Notes], [ResponsibleStaffName], [ServiceDate], [Status], [UpdatedAt])
    VALUES (1, 1, ''2026-02-02T00:00:00.0000000Z'', N''Medical Detoxification'', N''5-day inpatient clinical detox successfully administered without acute complications.'', N''Dr. Rajesh Sharma (Medical Officer)'', ''2026-02-02'', N''Completed'', NULL),
    (2, 1, ''2026-02-10T00:00:00.0000000Z'', N''Psychosocial Rehabilitation'', N''Enrolled in 60-day residential psychosocial recovery module.'', N''Navchetan Rehabilitation Team'', ''2026-02-10'', N''In Progress'', NULL),
    (3, 4, ''2026-02-13T00:00:00.0000000Z'', N''Medical Detoxification'', N''Supervised pharmacotherapy detox provided at Patna facility.'', N''Dr. P.K. Sinha (Physician)'', ''2026-02-13'', N''Completed'', NULL)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'InterventionId', N'BeneficiaryId', N'CreatedAt', N'InterventionType', N'Notes', N'ResponsibleStaffName', N'ServiceDate', N'Status', N'UpdatedAt') AND [object_id] = OBJECT_ID(N'[Interventions]'))
        SET IDENTITY_INSERT [Interventions] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117222831_AddClinicalModules'
)
BEGIN
    CREATE INDEX [IX_CounsellingSessions_BeneficiaryId] ON [CounsellingSessions] ([BeneficiaryId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117222831_AddClinicalModules'
)
BEGIN
    CREATE INDEX [IX_CounsellingSessions_SessionDate] ON [CounsellingSessions] ([SessionDate]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117222831_AddClinicalModules'
)
BEGIN
    CREATE INDEX [IX_CounsellingSessions_SessionStatus] ON [CounsellingSessions] ([SessionStatus]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117222831_AddClinicalModules'
)
BEGIN
    CREATE INDEX [IX_CounsellingSessions_SessionType] ON [CounsellingSessions] ([SessionType]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117222831_AddClinicalModules'
)
BEGIN
    CREATE INDEX [IX_FollowUps_BeneficiaryId] ON [FollowUps] ([BeneficiaryId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117222831_AddClinicalModules'
)
BEGIN
    CREATE INDEX [IX_FollowUps_ScheduledDate] ON [FollowUps] ([ScheduledDate]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117222831_AddClinicalModules'
)
BEGIN
    CREATE INDEX [IX_FollowUps_Status] ON [FollowUps] ([Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117222831_AddClinicalModules'
)
BEGIN
    CREATE INDEX [IX_Interventions_BeneficiaryId] ON [Interventions] ([BeneficiaryId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117222831_AddClinicalModules'
)
BEGIN
    CREATE INDEX [IX_Interventions_InterventionType] ON [Interventions] ([InterventionType]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117222831_AddClinicalModules'
)
BEGIN
    CREATE INDEX [IX_Interventions_ServiceDate] ON [Interventions] ([ServiceDate]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117222831_AddClinicalModules'
)
BEGIN
    CREATE INDEX [IX_Interventions_Status] ON [Interventions] ([Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117222831_AddClinicalModules'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261117222831_AddClinicalModules', N'8.0.10');
END;
GO

COMMIT;
GO

