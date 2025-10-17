-- WorkingHours tablosundaki IdleHours computed column'u 168 saat için düzelt
-- Sorun: IdleHours = 24 - (ActiveWorkingHours + MaintenanceHours) 
-- Çözüm: IdleHours = 168 - (ActiveWorkingHours + MaintenanceHours)

USE VehicleRentalDB;
GO

PRINT '=== IDLEHOURS 168 SAAT İÇİN DÜZELTME BAŞLADI ===';
GO

-- Mevcut IdleHours computed column'u kaldır
IF EXISTS (
    SELECT 1 FROM sys.computed_columns 
    WHERE object_id = OBJECT_ID('WorkingHours') 
    AND name = 'IdleHours'
)
BEGIN
    ALTER TABLE WorkingHours DROP COLUMN IdleHours;
    PRINT '✓ IdleHours computed column kaldırıldı (24 saat formülü ile).';
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

-- 168 saat formülü ile IdleHours computed column ekle
-- Formül: 168 - (ActiveWorkingHours + MaintenanceHours)
ALTER TABLE WorkingHours
ADD IdleHours AS (168 - (ActiveWorkingHours + MaintenanceHours)) PERSISTED;
GO

PRINT '✓ IdleHours computed column 168 saat formülü ile eklendi: 168 - (ActiveWorkingHours + MaintenanceHours)';
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
        WHEN (wh.ActiveWorkingHours + wh.MaintenanceHours + wh.IdleHours) = 168 THEN '✓ DOĞRU'
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
DECLARE @Aktif DECIMAL(10,2) = 120.0;
DECLARE @Bakim DECIMAL(10,2) = 4.0;
DECLARE @Idle DECIMAL(10,2) = 168 - (@Aktif + @Bakim);

SELECT 
    @Aktif AS AktifCalisma,
    @Bakim AS Bakim,
    @Idle AS IdleHours,
    (@Aktif + @Bakim + @Idle) AS Toplam,
    CASE 
        WHEN (@Aktif + @Bakim + @Idle) = 168 THEN '✓ DOĞRU'
        ELSE '⚠ HATALI'
    END AS Sonuc;
GO

PRINT '=== DÜZELTME TAMAMLANDI ===';
PRINT 'Artık IdleHours = 168 - (Aktif Çalışma + Bakım) formülü ile hesaplanıyor.';
PRINT 'Örnek: 120 + 4 = 124 → 168 - 124 = 44 saat (negatif değil!)';
GO
