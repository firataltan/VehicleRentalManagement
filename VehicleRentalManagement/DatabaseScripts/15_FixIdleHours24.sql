-- IdleHours computed column'u 24 saat için düzelt
-- Sorun: IdleHours = 168 - (ActiveWorkingHours + MaintenanceHours) 
-- Çözüm: IdleHours = 24 - (ActiveWorkingHours + MaintenanceHours)

USE VehicleRentalDB;
GO

PRINT '=== IDLEHOURS 24 SAAT İÇİN DÜZELTME BAŞLADI ===';
GO

-- Mevcut IdleHours computed column'u kaldır
IF EXISTS (
    SELECT 1 FROM sys.computed_columns 
    WHERE object_id = OBJECT_ID('WorkingHours') 
    AND name = 'IdleHours'
)
BEGIN
    ALTER TABLE WorkingHours DROP COLUMN IdleHours;
    PRINT '✓ IdleHours computed column kaldırıldı (168 saat formülü ile).';
END
ELSE
BEGIN
    -- Eğer IdleHours normal sütun ise (computed değilse), onu da kaldır
    IF EXISTS (
        SELECT 1 FROM sys.columns 
        WHERE object_id = OBJECT_ID('WorkingHours') 
        AND name = 'IdleHours'
        AND is_computed = 0
    )
    BEGIN
        ALTER TABLE WorkingHours DROP COLUMN IdleHours;
        PRINT '✓ IdleHours normal column kaldırıldı.';
    END
    ELSE
    BEGIN
        PRINT '⚠ IdleHours sütunu bulunamadı.';
    END
END
GO

-- 24 saat formülü ile IdleHours computed column ekle
-- Formül: 24 - (ActiveWorkingHours + MaintenanceHours)
ALTER TABLE WorkingHours
ADD IdleHours AS (24 - (ActiveWorkingHours + MaintenanceHours)) PERSISTED;
GO

PRINT '✓ IdleHours computed column 24 saat formülü ile eklendi: 24 - (ActiveWorkingHours + MaintenanceHours)';
GO

-- Kontrol: Mevcut kayıtları görüntüle
PRINT '=== DÜZELTME SONRASI KONTROL ===';
SELECT TOP 10
    v.VehicleName,
    v.LicensePlate,
    wh.RecordDate,
    wh.ActiveWorkingHours,
    wh.MaintenanceHours,
    wh.IdleHours,
    (wh.ActiveWorkingHours + wh.MaintenanceHours + wh.IdleHours) AS ToplamSaat,
    CASE 
        WHEN (wh.ActiveWorkingHours + wh.MaintenanceHours + wh.IdleHours) = 24 THEN '✓ DOĞRU'
        ELSE '⚠ HATALI'
    END AS Durum
FROM WorkingHours wh
INNER JOIN Vehicles v ON wh.VehicleId = v.VehicleId
ORDER BY wh.RecordDate DESC;
GO

-- Negatif değerleri kontrol et
PRINT '=== NEGATİF DEĞER KONTROLÜ ===';
SELECT 
    COUNT(*) AS NegatifIdleHoursKayitSayisi
FROM WorkingHours 
WHERE IdleHours < 0;
GO

-- Örnek hesaplama kontrolü
PRINT '=== ÖRNEK HESAPLAMA KONTROLÜ ===';
DECLARE @Aktif DECIMAL(10,2) = 22.5;
DECLARE @Bakim DECIMAL(10,2) = 1.5;
DECLARE @Idle DECIMAL(10,2) = 24 - (@Aktif + @Bakim);

SELECT 
    @Aktif AS AktifCalisma,
    @Bakim AS Bakim,
    @Idle AS IdleHours,
    (@Aktif + @Bakim + @Idle) AS Toplam,
    CASE 
        WHEN (@Aktif + @Bakim + @Idle) = 24 THEN '✓ DOĞRU'
        ELSE '⚠ HATALI'
    END AS Sonuc;
GO

PRINT '=== DÜZELTME TAMAMLANDI ===';
PRINT 'Artık IdleHours = 24 - (Aktif Çalışma + Bakım) formülü ile hesaplanıyor.';
PRINT 'Örnek: 22.5 + 1.5 = 24 → 24 - 24 = 0 saat (günlük hesaplama!)';
GO
