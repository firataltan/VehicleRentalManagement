-- Mevcut view durumunu kontrol et
USE VehicleRentalManagementDB;
GO

-- 1. Mevcut view'ın tanımını göster
PRINT 'Mevcut vw_WeeklyVehicleSummary view tanımı:';
PRINT '==========================================';
SELECT 
    OBJECT_DEFINITION(OBJECT_ID('vw_WeeklyVehicleSummary')) AS ViewDefinition;
GO

-- 2. View'dan veri çek
PRINT '';
PRINT 'View sonuçları:';
PRINT '==============';
SELECT 
    VehicleName,
    LicensePlate,
    TotalActiveHours,
    TotalMaintenanceHours,
    TotalIdleHours,
    (TotalActiveHours + TotalMaintenanceHours + TotalIdleHours) AS ToplamSaat,
    ActivePercentage,
    MaintenancePercentage,
    IdlePercentage,
    (ActivePercentage + MaintenancePercentage + IdlePercentage) AS YüzdeToplam,
    RecordCount
FROM vw_WeeklyVehicleSummary
ORDER BY VehicleName;
GO

-- 3. Ham verileri kontrol et (son 7 gün)
PRINT '';
PRINT 'Son 7 gündeki ham veriler:';
PRINT '==========================';
SELECT 
    v.VehicleName,
    v.LicensePlate,
    wh.RecordDate,
    wh.ActiveWorkingHours,
    wh.MaintenanceHours,
    wh.IdleHours,
    (wh.ActiveWorkingHours + wh.MaintenanceHours + wh.IdleHours) AS GünlükToplam
FROM Vehicles v
LEFT JOIN WorkingHours wh ON v.VehicleId = wh.VehicleId
WHERE wh.RecordDate >= DATEADD(DAY, -7, CAST(GETDATE() AS DATE))
   OR wh.RecordDate IS NULL
ORDER BY v.VehicleName, wh.RecordDate DESC;
GO

-- 4. Manuel hesaplama yap (son 7 gün)
PRINT '';
PRINT 'Manuel hesaplama (son 7 gün):';
PRINT '=============================';
SELECT 
    v.VehicleName,
    v.LicensePlate,
    COUNT(CASE WHEN wh.RecordDate >= DATEADD(DAY, -7, CAST(GETDATE() AS DATE)) 
               THEN wh.WorkingHourId ELSE NULL END) AS Son7GunKayitSayisi,
    
    SUM(CASE WHEN wh.RecordDate >= DATEADD(DAY, -7, CAST(GETDATE() AS DATE)) 
             THEN wh.ActiveWorkingHours ELSE 0 END) AS Son7GunAktif,
    
    SUM(CASE WHEN wh.RecordDate >= DATEADD(DAY, -7, CAST(GETDATE() AS DATE)) 
             THEN wh.MaintenanceHours ELSE 0 END) AS Son7GunBakim,
    
    SUM(CASE WHEN wh.RecordDate >= DATEADD(DAY, -7, CAST(GETDATE() AS DATE)) 
             THEN wh.IdleHours ELSE 0 END) AS Son7GunBosta,
    
    SUM(CASE WHEN wh.RecordDate >= DATEADD(DAY, -7, CAST(GETDATE() AS DATE)) 
             THEN wh.ActiveWorkingHours + wh.MaintenanceHours + wh.IdleHours 
             ELSE 0 END) AS Son7GunToplamSaat,
    
    -- Manuel yüzde hesaplaması
    CASE 
        WHEN SUM(CASE WHEN wh.RecordDate >= DATEADD(DAY, -7, CAST(GETDATE() AS DATE)) 
                      THEN wh.ActiveWorkingHours + wh.MaintenanceHours + wh.IdleHours 
                      ELSE 0 END) > 0 
        THEN 
            CAST(
                (SUM(CASE WHEN wh.RecordDate >= DATEADD(DAY, -7, CAST(GETDATE() AS DATE)) 
                          THEN wh.ActiveWorkingHours ELSE 0 END) / 
                 SUM(CASE WHEN wh.RecordDate >= DATEADD(DAY, -7, CAST(GETDATE() AS DATE)) 
                          THEN wh.ActiveWorkingHours + wh.MaintenanceHours + wh.IdleHours 
                          ELSE 0 END)) * 100 
                AS DECIMAL(5,2)
            )
        ELSE 0 
    END AS AktifYuzde,
    
    CASE 
        WHEN SUM(CASE WHEN wh.RecordDate >= DATEADD(DAY, -7, CAST(GETDATE() AS DATE)) 
                      THEN wh.ActiveWorkingHours + wh.MaintenanceHours + wh.IdleHours 
                      ELSE 0 END) > 0 
        THEN 
            CAST(
                (SUM(CASE WHEN wh.RecordDate >= DATEADD(DAY, -7, CAST(GETDATE() AS DATE)) 
                          THEN wh.MaintenanceHours ELSE 0 END) / 
                 SUM(CASE WHEN wh.RecordDate >= DATEADD(DAY, -7, CAST(GETDATE() AS DATE)) 
                          THEN wh.ActiveWorkingHours + wh.MaintenanceHours + wh.IdleHours 
                          ELSE 0 END)) * 100 
                AS DECIMAL(5,2)
            )
        ELSE 0 
    END AS BakimYuzde,
    
    CASE 
        WHEN SUM(CASE WHEN wh.RecordDate >= DATEADD(DAY, -7, CAST(GETDATE() AS DATE)) 
                      THEN wh.ActiveWorkingHours + wh.MaintenanceHours + wh.IdleHours 
                      ELSE 0 END) > 0 
        THEN 
            CAST(
                (SUM(CASE WHEN wh.RecordDate >= DATEADD(DAY, -7, CAST(GETDATE() AS DATE)) 
                          THEN wh.IdleHours ELSE 0 END) / 
                 SUM(CASE WHEN wh.RecordDate >= DATEADD(DAY, -7, CAST(GETDATE() AS DATE)) 
                          THEN wh.ActiveWorkingHours + wh.MaintenanceHours + wh.IdleHours 
                          ELSE 0 END)) * 100 
                AS DECIMAL(5,2)
            )
        ELSE 0 
    END AS BostaYuzde
    
FROM Vehicles v
LEFT JOIN WorkingHours wh ON v.VehicleId = wh.VehicleId
GROUP BY v.VehicleId, v.VehicleName, v.LicensePlate
ORDER BY v.VehicleName;
GO
