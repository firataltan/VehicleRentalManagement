-- Soft Delete Sorunu Düzeltme Scripti
-- Sorun: Silinen araçlar (IsActive = 0) grafiklerde ve diğer yerlerde görünmeye devam ediyor
-- Çözüm: Tüm sorgularda v.IsActive = 1 kontrolü ekle

USE VehicleRentalDB;
GO

PRINT '=== SOFT DELETE SORUNU DÜZELTME BAŞLADI ===';
GO

-- 1. Mevcut view'ı sil
IF OBJECT_ID('vw_WeeklyVehicleSummary', 'V') IS NOT NULL
BEGIN
    DROP VIEW vw_WeeklyVehicleSummary;
    PRINT '✅ Eski view silindi';
END
GO

-- 2. IsActive kontrolü ile yeni view oluştur
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
    
    -- Doğru yüzde hesaplamaları (168 saat üzerinden)
    CASE 
        WHEN COUNT(wh.WorkingHourId) > 0 THEN 
            CAST((SUM(wh.ActiveWorkingHours) / (COUNT(wh.WorkingHourId) * 168.0)) * 100 AS DECIMAL(5,2))
        ELSE 0 
    END AS ActivePercentage,
    
    CASE 
        WHEN COUNT(wh.WorkingHourId) > 0 THEN 
            CAST((SUM(wh.MaintenanceHours) / (COUNT(wh.WorkingHourId) * 168.0)) * 100 AS DECIMAL(5,2))
        ELSE 0 
    END AS MaintenancePercentage,
    
    CASE 
        WHEN COUNT(wh.WorkingHourId) > 0 THEN 
            CAST((SUM(wh.IdleHours) / (COUNT(wh.WorkingHourId) * 168.0)) * 100 AS DECIMAL(5,2))
        ELSE 0 
    END AS IdlePercentage,
    
    COUNT(wh.WorkingHourId) AS RecordCount
FROM Vehicles v
LEFT JOIN WorkingHours wh ON v.VehicleId = wh.VehicleId
WHERE v.IsActive = 1  -- ✅ SADECE AKTİF ARAÇLAR
GROUP BY v.VehicleId, v.VehicleName, v.LicensePlate;
GO

PRINT '✅ vw_WeeklyVehicleSummary view IsActive kontrolü ile yeniden oluşturuldu';
GO

-- 3. Test: Silinen araçları kontrol et
PRINT '=== SİLİNEN ARAÇLAR KONTROLÜ ===';
SELECT 
    v.VehicleId,
    v.VehicleName,
    v.LicensePlate,
    v.IsActive,
    CASE 
        WHEN v.IsActive = 1 THEN '✅ AKTİF'
        ELSE '❌ SİLİNMİŞ'
    END AS Durum
FROM Vehicles v
ORDER BY v.IsActive DESC, v.VehicleName;
GO

-- 4. Test: View'dan sadece aktif araçları kontrol et
PRINT '=== VIEW DAN AKTİF ARAÇLAR ===';
SELECT 
    VehicleName,
    LicensePlate,
    TotalActiveHours,
    TotalMaintenanceHours,
    TotalIdleHours,
    ActivePercentage,
    MaintenancePercentage,
    IdlePercentage,
    RecordCount
FROM vw_WeeklyVehicleSummary
ORDER BY VehicleName;
GO

-- 5. Test: Silinen araçların WorkingHours kayıtlarını kontrol et
PRINT '=== SİLİNEN ARAÇLARIN ÇALIŞMA SAATLERİ ===';
SELECT 
    v.VehicleName,
    v.LicensePlate,
    v.IsActive,
    COUNT(wh.WorkingHourId) AS WorkingHourKayitSayisi
FROM Vehicles v
LEFT JOIN WorkingHours wh ON v.VehicleId = wh.VehicleId
WHERE v.IsActive = 0  -- Sadece silinen araçlar
GROUP BY v.VehicleId, v.VehicleName, v.LicensePlate, v.IsActive
ORDER BY v.VehicleName;
GO

PRINT '=== DÜZELTME TAMAMLANDI ===';
PRINT 'Artık silinen araçlar (IsActive = 0) grafiklerde ve listelerde görünmeyecek.';
PRINT 'Sadece aktif araçlar (IsActive = 1) işlemlerde yer alacak.';
GO
