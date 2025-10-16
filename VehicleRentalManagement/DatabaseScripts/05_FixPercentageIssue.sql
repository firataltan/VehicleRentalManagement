-- Yüzde sorununun kaynağını bul ve düzelt
USE VehicleRentalDB;
GO

-- 1. Önce tüm araçların durumunu kontrol et
PRINT '=== TÜM ARAÇLARIN DURUM KONTROLÜ ==='
SELECT 
    VehicleName,
    RecordCount AS 'Kayıt Sayısı',
    TotalActiveHours AS 'Toplam Aktif',
    TotalMaintenanceHours AS 'Toplam Bakım',
    TotalIdleHours AS 'Toplam Boşta',
    (TotalActiveHours + TotalMaintenanceHours + TotalIdleHours) AS 'Toplam Saat',
    (RecordCount * 24) AS 'Beklenen Toplam',
    ActivePercentage AS 'Aktif %',
    MaintenancePercentage AS 'Bakım %',
    IdlePercentage AS 'Boşta %',
    (ActivePercentage + MaintenancePercentage + IdlePercentage) AS 'TOPLAM %'
FROM vw_WeeklyVehicleSummary
ORDER BY VehicleName;
GO

-- 2. Sorunlu kayıtları bul (günlük toplamı 24'ten farklı olanlar)
PRINT '=== SORUNLU KAYITLAR (Günlük toplam ≠ 24) ==='
SELECT 
    v.VehicleName,
    wh.RecordDate,
    wh.ActiveWorkingHours,
    wh.MaintenanceHours,
    wh.IdleHours,
    (wh.ActiveWorkingHours + wh.MaintenanceHours + wh.IdleHours) AS GünlükToplam,
    CASE 
        WHEN (wh.ActiveWorkingHours + wh.MaintenanceHours + wh.IdleHours) > 24 THEN '⚠️ 24 SAATI AŞIYOR!'
        WHEN (wh.ActiveWorkingHours + wh.MaintenanceHours + wh.IdleHours) < 24 THEN '⚠️ 24 SAATTEN AZ!'
        ELSE '✓ Doğru'
    END AS Durum
FROM WorkingHours wh
INNER JOIN Vehicles v ON wh.VehicleId = v.VehicleId
WHERE (wh.ActiveWorkingHours + wh.MaintenanceHours + wh.IdleHours) <> 24
ORDER BY wh.RecordDate DESC, v.VehicleName;
GO

-- 3. View tanımını kontrol et
PRINT '=== MEVCUT VIEW TANIMI ==='
SELECT OBJECT_DEFINITION(OBJECT_ID('vw_WeeklyVehicleSummary')) AS ViewDefinition;
GO

-- 4. Sorunlu kayıtları düzelt (IdleHours'ı yeniden hesapla)
PRINT '=== SORUNLU KAYITLARI DÜZELT ==='
PRINT 'Düzeltme başlıyor...'

-- Önce backup al (opsiyonel)
IF OBJECT_ID('WorkingHours_Backup', 'U') IS NOT NULL
    DROP TABLE WorkingHours_Backup;
GO

SELECT * INTO WorkingHours_Backup FROM WorkingHours;
PRINT 'Backup alındı: WorkingHours_Backup'
GO

-- IdleHours computed column'u kaldır
ALTER TABLE WorkingHours DROP COLUMN IF EXISTS IdleHours;
GO

-- Doğru formülle yeniden ekle
ALTER TABLE WorkingHours
ADD IdleHours AS (24 - (ActiveWorkingHours + MaintenanceHours)) PERSISTED;
GO

PRINT 'IdleHours computed column yeniden oluşturuldu.'
GO

-- 5. Düzeltme sonrası kontrol
PRINT '=== DÜZELTME SONRASI KONTROL ==='
SELECT 
    VehicleName,
    RecordCount AS 'Kayıt',
    CAST(TotalActiveHours AS DECIMAL(10,2)) AS 'Aktif',
    CAST(TotalMaintenanceHours AS DECIMAL(10,2)) AS 'Bakım',
    CAST(TotalIdleHours AS DECIMAL(10,2)) AS 'Boşta',
    CAST(ActivePercentage AS DECIMAL(5,2)) AS 'Aktif %',
    CAST(MaintenancePercentage AS DECIMAL(5,2)) AS 'Bakım %',
    CAST(IdlePercentage AS DECIMAL(5,2)) AS 'Boşta %',
    CAST((ActivePercentage + MaintenancePercentage + IdlePercentage) AS DECIMAL(5,2)) AS 'TOPLAM %'
FROM vw_WeeklyVehicleSummary
ORDER BY VehicleName;
GO

PRINT '✓ İşlem tamamlandı!'
GO

