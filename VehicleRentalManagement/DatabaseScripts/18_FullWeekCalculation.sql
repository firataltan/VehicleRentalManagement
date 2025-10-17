-- TAM HAFTA HESAPLAMA MANTIĞI
-- Proje Gereksinimleri: "7 gün ve 24 saat araçların çalıştığı bir sistemde"
-- TÜM ARAÇLAR 7 GÜN × 24 SAAT = 168 SAAT ÇALIŞIR
-- Çalışmayan günler de boşta kalma süresine eklenir

USE VehicleRentalDB;
GO

PRINT '=== TAM HAFTA HESAPLAMA MANTIĞI ===';
GO

-- 1. Mevcut view'ı sil
IF OBJECT_ID('vw_WeeklyVehicleSummary', 'V') IS NOT NULL
BEGIN
    DROP VIEW vw_WeeklyVehicleSummary;
    PRINT '✅ Eski view silindi';
END
GO

-- 2. TAM HAFTA HESAPLAMA mantığı ile view oluştur
-- MANTIK: 
-- - Tüm araçlar 7 gün × 24 saat = 168 saat çalışır
-- - Çalışan günler: Girilen veriler
-- - Çalışmayan günler: 24 saat boşta kalma
-- - Yüzde = (Kategori Saat / 168) × 100
CREATE VIEW vw_WeeklyVehicleSummary
AS
WITH Last7DaysData AS (
    -- Son 7 gündeki verileri filtrele
    SELECT 
        v.VehicleId,
        v.VehicleName,
        v.LicensePlate,
        SUM(wh.ActiveWorkingHours) AS TotalActiveHours,
        SUM(wh.MaintenanceHours) AS TotalMaintenanceHours,
        SUM(wh.IdleHours) AS TotalIdleHours,
        COUNT(DISTINCT wh.RecordDate) AS WorkingDays, -- Çalışan gün sayısı
        COUNT(wh.WorkingHourId) AS RecordCount
    FROM Vehicles v
    LEFT JOIN WorkingHours wh ON v.VehicleId = wh.VehicleId 
        AND wh.RecordDate >= DATEADD(DAY, -7, CAST(GETDATE() AS DATE))
    WHERE v.IsActive = 1
    GROUP BY v.VehicleId, v.VehicleName, v.LicensePlate
)
SELECT 
    VehicleId,
    VehicleName,
    LicensePlate,
    ISNULL(TotalActiveHours, 0) AS TotalActiveHours,
    ISNULL(TotalMaintenanceHours, 0) AS TotalMaintenanceHours,
    
    -- TAM HAFTA BOŞTA KALMA HESAPLAMA
    -- Çalışan günlerdeki boşta kalma + Çalışmayan günlerdeki tam boşta kalma
    ISNULL(TotalIdleHours, 0) + ((7 - ISNULL(WorkingDays, 0)) * 24) AS TotalIdleHours,
    
    -- PROJE GEREKSİNİMLERİNE UYGUN YÜZDE HESAPLAMALARI
    -- Yüzde = (Kategori Saat / 168) × 100
    -- 168 = 7 gün × 24 saat (proje gereksinimi)
    CAST((ISNULL(TotalActiveHours, 0) / 168.0) * 100 AS DECIMAL(5,2)) AS ActivePercentage,
    CAST((ISNULL(TotalMaintenanceHours, 0) / 168.0) * 100 AS DECIMAL(5,2)) AS MaintenancePercentage,
    CAST(((ISNULL(TotalIdleHours, 0) + ((7 - ISNULL(WorkingDays, 0)) * 24)) / 168.0) * 100 AS DECIMAL(5,2)) AS IdlePercentage,
    
    ISNULL(RecordCount, 0) AS RecordCount,
    ISNULL(WorkingDays, 0) AS WorkingDays,
    (7 - ISNULL(WorkingDays, 0)) AS NonWorkingDays
FROM Last7DaysData;
GO

PRINT '✅ vw_WeeklyVehicleSummary view TAM HAFTA HESAPLAMA mantığı ile oluşturuldu';
PRINT 'Çalışmayan günler de boşta kalma süresine eklenir';
GO

-- 3. Test: Yeni hesaplamaları kontrol et
PRINT '=== TAM HAFTA HESAPLAMA SONUÇLARI ===';
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
    NonWorkingDays,
    CASE 
        WHEN WorkingDays = 7 THEN '✅ 7 GÜN ÇALIŞMIŞ'
        WHEN WorkingDays > 0 THEN '⚠ ' + CAST(WorkingDays AS VARCHAR) + ' GÜN ÇALIŞMIŞ, ' + CAST(NonWorkingDays AS VARCHAR) + ' GÜN BOŞTA'
        ELSE '❌ HİÇ ÇALIŞMAMIŞ (7 GÜN BOŞTA)'
    END AS Durum
FROM vw_WeeklyVehicleSummary
ORDER BY VehicleName;
GO

-- 4. Örnek hesaplama açıklaması
PRINT '=== TAM HAFTA HESAPLAMA ÖRNEĞİ ===';
PRINT 'Örnek: Ford Transit (3 gün çalışan)';
PRINT '';
PRINT 'Çalışan Günler (3 gün):';
PRINT '- Aktif: 60.0 saat';
PRINT '- Bakım: 8.0 saat';
PRINT '- Boşta: 4.0 saat';
PRINT '- Toplam: 72.0 saat';
PRINT '';
PRINT 'Çalışmayan Günler (4 gün):';
PRINT '- Tamamen Boşta: 4 × 24 = 96.0 saat';
PRINT '';
PRINT 'HAFTALIK TOPLAM:';
PRINT '- Aktif: 60.0 saat';
PRINT '- Bakım: 8.0 saat';
PRINT '- Boşta: 4.0 + 96.0 = 100.0 saat';
PRINT '- TOPLAM: 168.0 saat (7 gün × 24 saat)';
PRINT '';
PRINT 'YÜZDE:';
PRINT '- Aktif: (60.0 / 168.0) × 100 = %35.7';
PRINT '- Bakım: (8.0 / 168.0) × 100 = %4.8';
PRINT '- Boşta: (100.0 / 168.0) × 100 = %59.5';
PRINT '- TOPLAM: %100.0 ✅';
GO

PRINT '=== DÜZELTME TAMAMLANDI ===';
PRINT 'Artık TAM HAFTA HESAPLAMA mantığı uygulanıyor:';
PRINT '- Çalışan günler: Girilen veriler';
PRINT '- Çalışmayan günler: 24 saat boşta kalma';
PRINT '- Tüm araçlar için toplam %100';
GO
