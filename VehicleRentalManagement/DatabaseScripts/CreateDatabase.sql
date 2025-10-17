-- =============================================
-- Vehicle Rental Management System
-- Database Creation Script
-- =============================================
-- Bu script, projenin veritabanını sıfırdan oluşturur
-- Tabloları, view'ları ve örnek verileri içerir
-- =============================================

USE master;
GO

-- Veritabanı varsa sil ve yeniden oluştur
IF EXISTS (SELECT name FROM sys.databases WHERE name = 'VehicleRentalDB')
BEGIN
    ALTER DATABASE VehicleRentalDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE VehicleRentalDB;
    PRINT 'Mevcut veritabanı silindi.';
END
GO

-- Yeni veritabanı oluştur
CREATE DATABASE VehicleRentalDB;
GO

PRINT 'VehicleRentalDB veritabanı oluşturuldu.';
GO

USE VehicleRentalDB;
GO

-- =============================================
-- 1. USERS TABLOSU
-- =============================================
-- Kullanıcı bilgilerini ve kimlik doğrulama verilerini saklar

CREATE TABLE Users (
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(50) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(256) NOT NULL,
    FullName NVARCHAR(100) NOT NULL,
    Role NVARCHAR(20) NOT NULL CHECK (Role IN ('Admin', 'User')),
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT CK_Users_Username CHECK (LEN(Username) >= 3)
);
GO

PRINT 'Users tablosu oluşturuldu.';
GO

-- =============================================
-- 2. VEHICLES TABLOSU
-- =============================================
-- Araç bilgilerini saklar

CREATE TABLE Vehicles (
    VehicleId INT IDENTITY(1,1) PRIMARY KEY,
    VehicleName NVARCHAR(100) NOT NULL,
    LicensePlate NVARCHAR(20) NOT NULL UNIQUE,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedBy INT NOT NULL,
    CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
    ModifiedBy INT NULL,
    ModifiedDate DATETIME NULL,
    CONSTRAINT FK_Vehicles_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(UserId),
    CONSTRAINT FK_Vehicles_ModifiedBy FOREIGN KEY (ModifiedBy) REFERENCES Users(UserId),
    CONSTRAINT CK_Vehicles_Name CHECK (LEN(VehicleName) >= 2)
);
GO

PRINT 'Vehicles tablosu oluşturuldu.';
GO

-- =============================================
-- 3. WORKINGHOURS TABLOSU
-- =============================================
-- Günlük çalışma saati kayıtlarını saklar
-- IdleHours: Computed column olarak otomatik hesaplanır

CREATE TABLE WorkingHours (
    WorkingHourId INT IDENTITY(1,1) PRIMARY KEY,
    VehicleId INT NOT NULL,
    RecordDate DATE NOT NULL,
    ActiveWorkingHours DECIMAL(5,2) NOT NULL,
    MaintenanceHours DECIMAL(5,2) NOT NULL,
    IdleHours AS (24 - (ActiveWorkingHours + MaintenanceHours)) PERSISTED,
    Notes NVARCHAR(500) NULL,
    CreatedBy INT NOT NULL,
    CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
    ModifiedBy INT NULL,
    ModifiedDate DATETIME NULL,
    CONSTRAINT FK_WorkingHours_Vehicle FOREIGN KEY (VehicleId) REFERENCES Vehicles(VehicleId),
    CONSTRAINT FK_WorkingHours_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(UserId),
    CONSTRAINT FK_WorkingHours_ModifiedBy FOREIGN KEY (ModifiedBy) REFERENCES Users(UserId),
    CONSTRAINT CK_WorkingHours_Active CHECK (ActiveWorkingHours >= 0 AND ActiveWorkingHours <= 24),
    CONSTRAINT CK_WorkingHours_Maintenance CHECK (MaintenanceHours >= 0 AND MaintenanceHours <= 24),
    CONSTRAINT CK_WorkingHours_Total CHECK ((ActiveWorkingHours + MaintenanceHours) <= 24),
    CONSTRAINT UQ_WorkingHours_VehicleDate UNIQUE (VehicleId, RecordDate)
);
GO

PRINT 'WorkingHours tablosu oluşturuldu (IdleHours computed column ile).';
GO

-- =============================================
-- 4. AUDITLOGS TABLOSU
-- =============================================
-- Sistem işlemlerini kayıt altına alır

CREATE TABLE AuditLogs (
    AuditLogId INT IDENTITY(1,1) PRIMARY KEY,
    TableName NVARCHAR(50) NOT NULL,
    RecordId INT NOT NULL,
    Action NVARCHAR(50) NOT NULL CHECK (Action IN ('INSERT', 'UPDATE', 'DELETE')),
    OldValues NVARCHAR(MAX) NULL,
    NewValues NVARCHAR(MAX) NULL,
    ChangedBy INT NOT NULL,
    ChangedDate DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_AuditLogs_ChangedBy FOREIGN KEY (ChangedBy) REFERENCES Users(UserId)
);
GO

PRINT 'AuditLogs tablosu oluşturuldu.';
GO

-- =============================================
-- 5. İNDEXLER
-- =============================================
-- Performans iyileştirmeleri için indexler

CREATE INDEX IX_WorkingHours_VehicleId ON WorkingHours(VehicleId);
CREATE INDEX IX_WorkingHours_RecordDate ON WorkingHours(RecordDate);
CREATE INDEX IX_WorkingHours_CreatedBy ON WorkingHours(CreatedBy);
CREATE INDEX IX_Vehicles_IsActive ON Vehicles(IsActive);
CREATE INDEX IX_AuditLogs_TableName ON AuditLogs(TableName, RecordId);
GO

PRINT 'İndexler oluşturuldu.';
GO

-- =============================================
-- 6. VIEW: vw_WeeklyVehicleSummary
-- =============================================
-- Son 7 günün araç bazlı özet istatistiklerini sunar
-- Yüzdeler her zaman toplamda %100 olacak şekilde hesaplanır

CREATE VIEW vw_WeeklyVehicleSummary
AS
SELECT 
    v.VehicleId,
    v.VehicleName,
    v.LicensePlate,
    COUNT(wh.WorkingHourId) AS RecordCount,
    ISNULL(SUM(wh.ActiveWorkingHours), 0) AS TotalActiveHours,
    ISNULL(SUM(wh.MaintenanceHours), 0) AS TotalMaintenanceHours,
    ISNULL(SUM(wh.IdleHours), 0) AS TotalIdleHours,
    -- Yüzde hesaplamaları: (İlgili Saat / Toplam Saat) * 100
    CASE 
        WHEN (ISNULL(SUM(wh.ActiveWorkingHours), 0) + 
              ISNULL(SUM(wh.MaintenanceHours), 0) + 
              ISNULL(SUM(wh.IdleHours), 0)) > 0 
        THEN CAST((ISNULL(SUM(wh.ActiveWorkingHours), 0) / 
                  (ISNULL(SUM(wh.ActiveWorkingHours), 0) + 
                   ISNULL(SUM(wh.MaintenanceHours), 0) + 
                   ISNULL(SUM(wh.IdleHours), 0))) * 100 AS DECIMAL(5,2))
        ELSE 0 
    END AS ActivePercentage,
    CASE 
        WHEN (ISNULL(SUM(wh.ActiveWorkingHours), 0) + 
              ISNULL(SUM(wh.MaintenanceHours), 0) + 
              ISNULL(SUM(wh.IdleHours), 0)) > 0 
        THEN CAST((ISNULL(SUM(wh.MaintenanceHours), 0) / 
                  (ISNULL(SUM(wh.ActiveWorkingHours), 0) + 
                   ISNULL(SUM(wh.MaintenanceHours), 0) + 
                   ISNULL(SUM(wh.IdleHours), 0))) * 100 AS DECIMAL(5,2))
        ELSE 0 
    END AS MaintenancePercentage,
    CASE 
        WHEN (ISNULL(SUM(wh.ActiveWorkingHours), 0) + 
              ISNULL(SUM(wh.MaintenanceHours), 0) + 
              ISNULL(SUM(wh.IdleHours), 0)) > 0 
        THEN CAST((ISNULL(SUM(wh.IdleHours), 0) / 
                  (ISNULL(SUM(wh.ActiveWorkingHours), 0) + 
                   ISNULL(SUM(wh.MaintenanceHours), 0) + 
                   ISNULL(SUM(wh.IdleHours), 0))) * 100 AS DECIMAL(5,2))
        ELSE 0 
    END AS IdlePercentage
FROM 
    Vehicles v
LEFT JOIN 
    WorkingHours wh ON v.VehicleId = wh.VehicleId 
    AND wh.RecordDate >= DATEADD(DAY, -7, CAST(GETDATE() AS DATE))
WHERE 
    v.IsActive = 1
GROUP BY 
    v.VehicleId, v.VehicleName, v.LicensePlate;
GO

PRINT 'vw_WeeklyVehicleSummary view oluşturuldu.';
GO

-- =============================================
-- 7. ÖRNEK VERİLER
-- =============================================

-- Admin kullanıcı oluştur
-- Username: admin
-- Password: admin123 (SHA256 hash: 240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a9)
INSERT INTO Users (Username, PasswordHash, FullName, Role, IsActive)
VALUES ('admin', '240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a9', 'Sistem Yöneticisi', 'Admin', 1);

-- Test kullanıcısı oluştur
-- Username: user1
-- Password: user123 (SHA256 hash: 0a041b9462caa4a31bac3567e0b6e6fd9100787db2ab433d96f6d178cabfce90)
INSERT INTO Users (Username, PasswordHash, FullName, Role, IsActive)
VALUES ('user1', '0a041b9462caa4a31bac3567e0b6e6fd9100787db2ab433d96f6d178cabfce90', 'Test Kullanıcısı', 'User', 1);

PRINT 'Kullanıcılar oluşturuldu (admin / admin123, user1 / user123).';
GO

-- Örnek araçlar ekle
INSERT INTO Vehicles (VehicleName, LicensePlate, IsActive, CreatedBy)
VALUES 
    ('Ford Transit', '34ABC123', 1, 1),
    ('Mercedes Sprinter', '06XYZ456', 1, 1),
    ('Volkswagen Crafter', '35DEF789', 1, 1);

PRINT 'Örnek araçlar eklendi.';
GO

-- Örnek çalışma saati kayıtları ekle (Son 7 gün için)
DECLARE @i INT = 0;
DECLARE @VehicleId INT;
DECLARE @RecordDate DATE;

WHILE @i < 7
BEGIN
    SET @RecordDate = CAST(DATEADD(DAY, -@i, GETDATE()) AS DATE);
    
    -- Ford Transit için kayıt
    INSERT INTO WorkingHours (VehicleId, RecordDate, ActiveWorkingHours, MaintenanceHours, CreatedBy)
    VALUES (1, @RecordDate, 18.5, 1.5, 1);
    
    -- Mercedes Sprinter için kayıt
    INSERT INTO WorkingHours (VehicleId, RecordDate, ActiveWorkingHours, MaintenanceHours, CreatedBy)
    VALUES (2, @RecordDate, 20.0, 2.0, 1);
    
    -- Volkswagen Crafter için kayıt
    INSERT INTO WorkingHours (VehicleId, RecordDate, ActiveWorkingHours, MaintenanceHours, CreatedBy)
    VALUES (3, @RecordDate, 16.0, 1.0, 1);
    
    SET @i = @i + 1;
END

PRINT 'Örnek çalışma saati kayıtları eklendi (Son 7 gün).';
GO

-- =============================================
-- 8. VERİTABANI DURUM KONTROLÜ
-- =============================================

PRINT '';
PRINT '=== VERİTABANI OLUŞTURMA TAMAMLANDI ===';
PRINT '';
PRINT 'Tablolar:';
SELECT name AS TableName FROM sys.tables ORDER BY name;
PRINT '';
PRINT 'View''lar:';
SELECT name AS ViewName FROM sys.views ORDER BY name;
PRINT '';
PRINT 'Kullanıcılar:';
SELECT UserId, Username, FullName, Role, IsActive FROM Users;
PRINT '';
PRINT 'Araçlar:';
SELECT VehicleId, VehicleName, LicensePlate, IsActive FROM Vehicles;
PRINT '';
PRINT 'Toplam Çalışma Saati Kayıtları:';
SELECT COUNT(*) AS TotalRecords FROM WorkingHours;
PRINT '';
PRINT 'Haftalık Özet (View):';
SELECT * FROM vw_WeeklyVehicleSummary;
PRINT '';
PRINT '=== KURULUM BAŞARIYLA TAMAMLANDI ===';
PRINT 'Login Bilgileri:';
PRINT '  Admin: admin / admin123';
PRINT '  User:  user1 / user123';
PRINT '';
GO

