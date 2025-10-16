-- Cache sorununu çözmek için kontrol scripti
USE VehicleRentalDB;
GO

-- 1. View'ın güncel verilerini tekrar kontrol et
PRINT '=== GÜNCEL VIEW VERİLERİ ==='
SELECT 
    VehicleName,
    TotalActiveHours,
    TotalMaintenanceHours,
    TotalIdleHours,
    ActivePercentage,
    MaintenancePercentage,
    IdlePercentage,
    (ActivePercentage + MaintenancePercentage + IdlePercentage) AS ToplamYuzde
FROM vw_WeeklyVehicleSummary
ORDER BY VehicleName;
GO

-- 2. View'ı yeniden oluştur (cache'i temizle)
PRINT '=== VIEW YENİDEN OLUŞTURULUYOR ==='
DROP VIEW IF EXISTS vw_WeeklyVehicleSummary;
GO

CREATE VIEW vw_WeeklyVehicleSummary
AS
SELECT 
    v.VehicleId,
    v.VehicleName,
    v.LicensePlate,
    -- Toplam saatler
    ISNULL(SUM(wh.ActiveWorkingHours), 0) AS TotalActiveHours,
    ISNULL(SUM(wh.MaintenanceHours), 0) AS TotalMaintenanceHours,
    ISNULL(SUM(wh.IdleHours), 0) AS TotalIdleHours,
    
    -- Doğru yüzde hesaplamaları
    CASE 
        WHEN COUNT(wh.WorkingHourId) > 0 THEN 
            CAST((SUM(wh.ActiveWorkingHours) / (COUNT(wh.WorkingHourId) * 24.0)) * 100 AS DECIMAL(5,2))
        ELSE 0 
    END AS ActivePercentage,
    
    CASE 
        WHEN COUNT(wh.WorkingHourId) > 0 THEN 
            CAST((SUM(wh.MaintenanceHours) / (COUNT(wh.WorkingHourId) * 24.0)) * 100 AS DECIMAL(5,2))
        ELSE 0 
    END AS MaintenancePercentage,
    
    CASE 
        WHEN COUNT(wh.WorkingHourId) > 0 THEN 
            CAST((SUM(wh.IdleHours) / (COUNT(wh.WorkingHourId) * 24.0)) * 100 AS DECIMAL(5,2))
        ELSE 0 
    END AS IdlePercentage,
    
    COUNT(wh.WorkingHourId) AS RecordCount
FROM Vehicles v
LEFT JOIN WorkingHours wh ON v.VehicleId = wh.VehicleId
GROUP BY v.VehicleId, v.VehicleName, v.LicensePlate;
GO

PRINT 'View yeniden oluşturuldu.'
GO

-- 3. Final kontrol
PRINT '=== FİNAL KONTROL ==='
SELECT 
    VehicleName,
    CAST(ActivePercentage AS DECIMAL(5,2)) AS 'Aktif %',
    CAST(MaintenancePercentage AS DECIMAL(5,2)) AS 'Bakım %',
    CAST(IdlePercentage AS DECIMAL(5,2)) AS 'Boşta %',
    CAST((ActivePercentage + MaintenancePercentage + IdlePercentage) AS DECIMAL(5,2)) AS 'TOPLAM %'
FROM vw_WeeklyVehicleSummary
ORDER BY VehicleName;
GO

-- 4. Force refresh için
PRINT '=== FORCE REFRESH ==='
-- SQL Server cache'ini temizle
DBCC FREEPROCCACHE;
DBCC DROPCLEANBUFFERS;
PRINT 'SQL Server cache temizlendi.'
GO
