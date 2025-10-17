-- Haftalık Araç Performansı Yüzde Hesaplama Mantığını Düzelt
-- Sorun: Yüzdeler %100'e ulaşmıyor çünkü yanlış hesaplama mantığı kullanılıyor
-- Çözüm: Doğru mantık - Araç sadece kayıtlı günler için hesaplanmalı

USE VehicleRentalDB;
GO

PRINT '=== HAFTALIK YÜZDE HESAPLAMA MANTIĞI DÜZELTME BAŞLADI ===';
GO

-- 1. Mevcut view'ı sil
IF OBJECT_ID('vw_WeeklyVehicleSummary', 'V') IS NOT NULL
BEGIN
    DROP VIEW vw_WeeklyVehicleSummary;
    PRINT '✅ Eski view silindi';
END
GO

-- 2. DOĞRU hesaplama mantığı ile yeni view oluştur
-- MANTIK: 
-- - Eğer araç 7 gün çalıştıysa: Toplam = 168 saat (7x24)
-- - Eğer araç 3 gün çalıştıysa: Toplam = 72 saat (3x24)
-- - Yüzde = (Aktif Saat / Toplam Çalışma Saati) × 100
CREATE VIEW vw_WeeklyVehicleSummary
AS
WITH VehicleWorkingDays AS (
    -- Her aracın çalıştığı günleri hesapla
    SELECT 
        v.VehicleId,
        v.VehicleName,
        v.LicensePlate,
        COUNT(DISTINCT wh.RecordDate) as WorkingDays,
        SUM(wh.ActiveWorkingHours) AS TotalActiveHours,
        SUM(wh.MaintenanceHours) AS TotalMaintenanceHours,
        SUM(wh.IdleHours) AS TotalIdleHours,
        COUNT(wh.WorkingHourId) AS RecordCount
    FROM Vehicles v
    LEFT JOIN WorkingHours wh ON v.VehicleId = wh.VehicleId
    WHERE v.IsActive = 1
    GROUP BY v.VehicleId, v.VehicleName, v.LicensePlate
)
SELECT 
    VehicleId,
    VehicleName,
    LicensePlate,
    ISNULL(TotalActiveHours, 0) AS TotalActiveHours,
    ISNULL(TotalMaintenanceHours, 0) AS TotalMaintenanceHours,
    ISNULL(TotalIdleHours, 0) AS TotalIdleHours,
    
    -- DOĞRU YÜZDE HESAPLAMALARI
    -- Yüzde = (Kategori Saat / Toplam Çalışma Saati) × 100
    -- Toplam Çalışma Saati = Aktif + Bakım + Boşta
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
    
    ISNULL(RecordCount, 0) AS RecordCount,
    ISNULL(WorkingDays, 0) AS WorkingDays
FROM VehicleWorkingDays;
GO

PRINT '✅ vw_WeeklyVehicleSummary view DOĞRU yüzde hesaplama mantığı ile oluşturuldu';
GO

-- 3. Test: Yeni hesaplamaları kontrol et
PRINT '=== YENİ YÜZDE HESAPLAMALARI ===';
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
    (ActivePercentage + MaintenancePercentage + IdlePercentage) AS ToplamYuzde,
    WorkingDays,
    RecordCount
FROM vw_WeeklyVehicleSummary
ORDER BY VehicleName;
GO

-- 4. Örnek hesaplama açıklaması
PRINT '=== HESAPLAMA MANTIĞI AÇIKLAMASI ===';
PRINT 'Örnek: Mercedes Sprinter';
PRINT '- Toplam Aktif: 22.5 saat';
PRINT '- Toplam Bakım: 1.5 saat';
PRINT '- Toplam Boşta: 0.0 saat';
PRINT '- Toplam Çalışma: 24.0 saat';
PRINT '- Aktif %: (22.5 / 24.0) × 100 = %93.75';
PRINT '- Bakım %: (1.5 / 24.0) × 100 = %6.25';
PRINT '- Boşta %: (0.0 / 24.0) × 100 = %0.00';
PRINT '- TOPLAM: %100.00';
GO

PRINT '=== DÜZELTME TAMAMLANDI ===';
PRINT 'Artık yüzdeler doğru hesaplanıyor ve toplam %100 oluyor.';
PRINT 'Yüzde hesaplama: (Kategori Saat / Toplam Çalışma Saati) × 100';
GO
