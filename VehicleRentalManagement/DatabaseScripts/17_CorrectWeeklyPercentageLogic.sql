-- DOĞRU Haftalık Araç Performansı Yüzde Hesaplama
-- Proje Gereksinimleri: "7 gün ve 24 saat araçların çalıştığı bir sistemde"
-- TÜM ARAÇLAR 7 GÜN × 24 SAAT = 168 SAAT ÇALIŞIR
-- Yüzde = (Kategori Saat / 168) × 100

USE VehicleRentalDB;
GO

PRINT '=== DOĞRU HAFTALIK YÜZDE HESAPLAMA MANTIĞI ===';
GO

-- 1. Mevcut view'ı sil
IF OBJECT_ID('vw_WeeklyVehicleSummary', 'V') IS NOT NULL
BEGIN
    DROP VIEW vw_WeeklyVehicleSummary;
    PRINT '✅ Eski view silindi';
END
GO

-- 2. PROJE GEREKSİNİMLERİNE UYGUN view oluştur
-- MANTIK: 
-- - Tüm araçlar 7 gün × 24 saat = 168 saat çalışır
-- - Yüzde = (Kategori Saat / 168) × 100
-- - Son 7 günün verilerini kullan
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
    ISNULL(TotalIdleHours, 0) AS TotalIdleHours,
    
    -- PROJE GEREKSİNİMLERİNE UYGUN YÜZDE HESAPLAMALARI
    -- Yüzde = (Kategori Saat / 168) × 100
    -- 168 = 7 gün × 24 saat (proje gereksinimi)
    CAST((ISNULL(TotalActiveHours, 0) / 168.0) * 100 AS DECIMAL(5,2)) AS ActivePercentage,
    CAST((ISNULL(TotalMaintenanceHours, 0) / 168.0) * 100 AS DECIMAL(5,2)) AS MaintenancePercentage,
    CAST((ISNULL(TotalIdleHours, 0) / 168.0) * 100 AS DECIMAL(5,2)) AS IdlePercentage,
    
    ISNULL(RecordCount, 0) AS RecordCount
FROM Last7DaysData;
GO

PRINT '✅ vw_WeeklyVehicleSummary view PROJE GEREKSİNİMLERİNE UYGUN oluşturuldu';
PRINT 'Yüzde hesaplama: (Kategori Saat / 168) × 100';
GO

-- 3. Test: Yeni hesaplamaları kontrol et
PRINT '=== PROJE GEREKSİNİMLERİNE UYGUN YÜZDE HESAPLAMALARI ===';
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
    RecordCount,
    CASE 
        WHEN RecordCount = 7 THEN '✅ 7 GÜN ÇALIŞMIŞ'
        WHEN RecordCount > 0 THEN '⚠ ' + CAST(RecordCount AS VARCHAR) + ' GÜN ÇALIŞMIŞ'
        ELSE '❌ HİÇ ÇALIŞMAMIŞ'
    END AS Durum
FROM vw_WeeklyVehicleSummary
ORDER BY VehicleName;
GO

-- 4. Örnek hesaplama açıklaması
PRINT '=== PROJE GEREKSİNİMLERİNE UYGUN HESAPLAMA ÖRNEĞİ ===';
PRINT 'Örnek: Mercedes Sprinter (7 gün çalışan)';
PRINT '- Toplam Aktif: 140.5 saat';
PRINT '- Toplam Bakım: 17.5 saat';
PRINT '- Toplam Boşta: 10.0 saat';
PRINT '- Toplam Çalışma: 168.0 saat (7 gün × 24 saat)';
PRINT '- Aktif %: (140.5 / 168.0) × 100 = %83.6';
PRINT '- Bakım %: (17.5 / 168.0) × 100 = %10.4';
PRINT '- Boşta %: (10.0 / 168.0) × 100 = %6.0';
PRINT '- TOPLAM: %100.0';
PRINT '';
PRINT 'Örnek: Ford Transit (3 gün çalışan)';
PRINT '- Toplam Aktif: 60.0 saat';
PRINT '- Toplam Bakım: 8.0 saat';
PRINT '- Toplam Boşta: 4.0 saat';
PRINT '- Toplam Çalışma: 72.0 saat (3 gün × 24 saat)';
PRINT '- Aktif %: (60.0 / 168.0) × 100 = %35.7';
PRINT '- Bakım %: (8.0 / 168.0) × 100 = %4.8';
PRINT '- Boşta %: (4.0 / 168.0) × 100 = %2.4';
PRINT '- TOPLAM: %42.9 (168 saatin %42.9''u kullanılmış)';
GO

PRINT '=== DÜZELTME TAMAMLANDI ===';
PRINT 'Artık yüzde hesaplama proje gereksinimlerine uygun:';
PRINT '- Tüm araçlar 7 gün × 24 saat = 168 saat çalışır';
PRINT '- Yüzde = (Kategori Saat / 168) × 100';
PRINT '- 7 gün çalışan araç: %100''e yakın yüzde';
PRINT '- Az çalışan araç: Düşük yüzde (normal)';
GO
