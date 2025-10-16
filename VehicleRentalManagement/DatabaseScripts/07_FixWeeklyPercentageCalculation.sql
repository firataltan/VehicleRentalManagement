-- Haftalık yüzde hesaplamasını düzelt
-- Sorun: View tüm kayıtları hesaplıyor, son 7 günü değil
-- Çözüm: Son 7 günün verilerini kullanarak doğru yüzde hesapla

USE VehicleRentalManagementDB;
GO

-- Mevcut view'ı sil
IF OBJECT_ID('vw_WeeklyVehicleSummary', 'V') IS NOT NULL
    DROP VIEW vw_WeeklyVehicleSummary;
GO

-- Doğru hesaplamalarla view'ı oluştur
-- Yüzde hesaplama mantığı:
-- Aktif % = (Son 7 gündeki Aktif Saat / Son 7 gündeki Toplam Saat) * 100
-- Toplam Saat = Aktif + Boşta + Bakım
-- ÖNEMLİ: Sadece son 7 gündeki verileri kullan!

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

PRINT 'vw_WeeklyVehicleSummary view başarıyla güncellendi (Son 7 gün filtresi eklendi).';
GO

-- Kontrol: View'dan örnek veri çek
PRINT '';
PRINT 'Güncel view sonuçları:';
PRINT '=====================';
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
    RecordCount AS Son7GunKayitSayisi
FROM vw_WeeklyVehicleSummary
ORDER BY VehicleName;
GO

-- Her aracın son 7 gündeki ham verilerini de göster
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

