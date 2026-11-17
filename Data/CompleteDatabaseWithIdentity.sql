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

