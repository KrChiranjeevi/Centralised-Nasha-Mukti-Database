-- ============================================================================
-- SQL SERVER SAFE BACKUP AND RESTORATION VERIFICATION SCRIPT
-- Project: Centralised Nasha Mukti Database (INT402 Capstone)
-- Purpose: Demonstrates full database backup and isolated test restoration
-- ============================================================================

USE master;
GO

-- ── 1. Create a Full Database Backup of NashaMuktiDB ────────────────────────
PRINT '>>> 1. Creating Full Backup of NashaMuktiDB...';

DECLARE @BackupPath NVARCHAR(500) = N'C:\INT402\Data\NashaMuktiDB_Backup.bak';

BACKUP DATABASE [NashaMuktiDB]
TO DISK = @BackupPath
WITH FORMAT,
     INIT,
     NAME = N'NashaMuktiDB-Full Database Backup',
     SKIP,
     NOREWIND,
     NOUNLOAD,
     COMPRESSION,
     STATS = 10;
GO

-- ── 2. Verify Backup Header and File Contents ────────────────────────────────
PRINT '>>> 2. Verifying Backup File Integrity...';

DECLARE @BackupPath NVARCHAR(500) = N'C:\INT402\Data\NashaMuktiDB_Backup.bak';

RESTORE VERIFYONLY
FROM DISK = @BackupPath;
GO

-- Inspect logical file names inside the backup
DECLARE @BackupPath NVARCHAR(500) = N'C:\INT402\Data\NashaMuktiDB_Backup.bak';

RESTORE FILELISTONLY
FROM DISK = @BackupPath;
GO

-- ── 3. Restore Onto a Safe, Isolated Test Database (NashaMuktiDB_TestCopy) ───
PRINT '>>> 3. Restoring to Isolated Test Database: NashaMuktiDB_TestCopy...';

-- Drop test database if already exists from prior test runs
IF EXISTS (SELECT 1 FROM sys.databases WHERE name = N'NashaMuktiDB_TestCopy')
BEGIN
    ALTER DATABASE [NashaMuktiDB_TestCopy] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [NashaMuktiDB_TestCopy];
END
GO

DECLARE @BackupPath NVARCHAR(500) = N'C:\INT402\Data\NashaMuktiDB_Backup.bak';

-- Restore to isolated copy with file relocation (MOVE)
RESTORE DATABASE [NashaMuktiDB_TestCopy]
FROM DISK = @BackupPath
WITH FILE = 1,
     MOVE N'NashaMuktiDB' TO N'C:\INT402\Data\NashaMuktiDB_TestCopy.mdf',
     MOVE N'NashaMuktiDB_log' TO N'C:\INT402\Data\NashaMuktiDB_TestCopy_log.ldf',
     NOUNLOAD,
     REPLACE,
     STATS = 10;
GO

-- ── 4. Verify Integrity and Record Counts in the Restored Database ──────────
PRINT '>>> 4. Verifying Restored Database Schema & Data Integrity...';

USE [NashaMuktiDB_TestCopy];
GO

-- Consistency check
DBCC CHECKDB (N'NashaMuktiDB_TestCopy') WITH NO_INFOMSGS;
GO

-- Verify table record counts match original
SELECT 'Centres' AS TableName, COUNT(1) AS RecordCount FROM Centres
UNION ALL
SELECT 'Beneficiaries', COUNT(1) FROM Beneficiaries
UNION ALL
SELECT 'CounsellingSessions', COUNT(1) FROM CounsellingSessions
UNION ALL
SELECT 'Interventions', COUNT(1) FROM Interventions
UNION ALL
SELECT 'FollowUps', COUNT(1) FROM FollowUps
UNION ALL
SELECT 'AspNetUsers', COUNT(1) FROM AspNetUsers;
GO

-- ── 5. Safe Cleanup of Temporary Test Database ──────────────────────────────
PRINT '>>> 5. Cleaning up Isolated Test Database...';

USE master;
GO

IF EXISTS (SELECT 1 FROM sys.databases WHERE name = N'NashaMuktiDB_TestCopy')
BEGIN
    ALTER DATABASE [NashaMuktiDB_TestCopy] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [NashaMuktiDB_TestCopy];
    PRINT '>>> Isolated Test Database NashaMuktiDB_TestCopy safely dropped.';
END
GO
