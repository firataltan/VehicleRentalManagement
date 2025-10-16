-- View'ı tamamen yenile ve cache'i temizle
-- Bu script kesin çözüm

USE VehicleRentalManagementDB;
GO

-- 1. Tüm cache'leri temizle
DBCC FREEPROCCACHE;
DBCC DROPCLEANBUFFERS;
GO

-- 2. View'ı tamamen sil
IF OBJECT_ID('vw_WeeklyVehicleSummary', 'V') IS NOT NULL
BEGIN
    DROP VIEW vw_WeeklyVehicleSummary;
    PRINT '✅ Eski view silindi';
END
ELSE
BEGIN
    PRINT 'ℹ️ View zaten mevcut değil';
END
GO

-- 3. Yeni view'ı oluştur
CREATE VIEW vw_WeeklyVehicleSummary
AS
WITH Last7Days AS (
    -- Son 7 gündeki verileri filtrele
    SELECT 
        v.VehicleId,
        v.VehicleName,
        v.LicensePlate,
        SUM(wh.ActiveWorkingHours) AS TotalActiveHours,
        SUM(wh.MaintenanceHours) AS TotalMaintenanceHours,
        SUM(wh.IdleHours) AS TotalIdleHours,
        COUNT(wh.WorkingHourId) AS RecordCount
    FROM Vehicles v
    LEFT JOIN WorkingHours wh ON v.VehicleId = wh.VehicleId 
        AND wh.RecordDate >= DATEADD(DAY, -7, CAST(GETDATE() AS DATE))
    GROUP BY v.VehicleId, v.VehicleName, v.LicensePlate
)
SELECT 
    VehicleId,
    VehicleName,
    LicensePlate,
    ISNULL(TotalActiveHours, 0) AS TotalActiveHours,
    ISNULL(TotalMaintenanceHours, 0) AS TotalMaintenanceHours,
    ISNULL(TotalIdleHours, 0) AS TotalIdleHours,
    
    -- Yüzde hesaplamaları - Toplam saat = Aktif + Bakım + Boşta
    CASE 
        WHEN ISNULL(TotalActiveHours, 0) + ISNULL(TotalMaintenanceHours, 0) + ISNULL(TotalIdleHours, 0) > 0 
        THEN 
            CAST(
                (ISNULL(TotalActiveHours, 0) / 
                 (ISNULL(TotalActiveHours, 0) + ISNULL(TotalMaintenanceHours, 0) + ISNULL(TotalIdleHours, 0))) * 100 
                AS DECIMAL(5,2)
            )
        ELSE 0 
    END AS ActivePercentage,
    
    CASE 
        WHEN ISNULL(TotalActiveHours, 0) + ISNULL(TotalMaintenanceHours, 0) + ISNULL(TotalIdleHours, 0) > 0 
        THEN 
            CAST(
                (ISNULL(TotalMaintenanceHours, 0) / 
                 (ISNULL(TotalActiveHours, 0) + ISNULL(TotalMaintenanceHours, 0) + ISNULL(TotalIdleHours, 0))) * 100 
                AS DECIMAL(5,2)
            )
        ELSE 0 
    END AS MaintenancePercentage,
    
    CASE 
        WHEN ISNULL(TotalActiveHours, 0) + ISNULL(TotalMaintenanceHours, 0) + ISNULL(TotalIdleHours, 0) > 0 
        THEN 
            CAST(
                (ISNULL(TotalIdleHours, 0) / 
                 (ISNULL(TotalActiveHours, 0) + ISNULL(TotalMaintenanceHours, 0) + ISNULL(TotalIdleHours, 0))) * 100 
                AS DECIMAL(5,2)
            )
        ELSE 0 
    END AS IdlePercentage,
    
    ISNULL(RecordCount, 0) AS RecordCount
    
FROM Last7Days;
GO

PRINT '✅ Yeni view oluşturuldu';
GO

-- 4. Test et
PRINT '';
PRINT 'Test sonuçları:';
PRINT '===============';

SELECT 
    VehicleName,
    ActivePercentage,
    IdlePercentage,
    MaintenancePercentage,
    (ActivePercentage + IdlePercentage + MaintenancePercentage) AS Toplam,
    CASE 
        WHEN ABS((ActivePercentage + IdlePercentage + MaintenancePercentage) - 100.00) < 0.01 
        THEN '✅ DOĞRU' 
        ELSE '❌ YANLIŞ' 
    END AS Durum
FROM vw_WeeklyVehicleSummary
ORDER BY VehicleName;
GO

PRINT '';
PRINT '🎯 Şimdi uygulamayı yeniden başlatın!';
PRINT '   Ctrl+C ile durdur, sonra: dotnet run';
GO
