-- vw_WeeklyVehicleSummary view'ını doğru hesaplamalarla yeniden oluştur

USE VehicleRentalDB;
GO

-- Mevcut view'ı sil
IF OBJECT_ID('vw_WeeklyVehicleSummary', 'V') IS NOT NULL
    DROP VIEW vw_WeeklyVehicleSummary;
GO

-- Doğru hesaplamalarla view'ı oluştur
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
    -- Her kayıt bir gün (24 saat) temsil eder
    -- Yüzde = (Toplam Saat / (Kayıt Sayısı * 24)) * 100
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

PRINT 'vw_WeeklyVehicleSummary view başarıyla güncellendi.';
GO

-- Kontrol: View'dan örnek veri çek
SELECT * FROM vw_WeeklyVehicleSummary
ORDER BY VehicleName;
GO

