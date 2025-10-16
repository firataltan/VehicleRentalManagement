-- WorkingHours tablosundaki IdleHours computed column'u düzelt
-- Eğer IdleHours computed column ise, önce silinip yeniden oluşturulmalı

USE VehicleRentalDB;
GO

-- Mevcut IdleHours sütununu kaldır (eğer computed ise)
IF EXISTS (
    SELECT 1 FROM sys.computed_columns 
    WHERE object_id = OBJECT_ID('WorkingHours') 
    AND name = 'IdleHours'
)
BEGIN
    ALTER TABLE WorkingHours DROP COLUMN IdleHours;
    PRINT 'IdleHours computed column kaldırıldı.';
END
GO

-- Eğer IdleHours normal sütun ise (computed değilse), onu da kaldır
IF EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID('WorkingHours') 
    AND name = 'IdleHours'
    AND is_computed = 0
)
BEGIN
    ALTER TABLE WorkingHours DROP COLUMN IdleHours;
    PRINT 'IdleHours normal column kaldırıldı.';
END
GO

-- Doğru formülle IdleHours computed column ekle
-- Formül: 24 - (ActiveWorkingHours + MaintenanceHours)
ALTER TABLE WorkingHours
ADD IdleHours AS (24 - (ActiveWorkingHours + MaintenanceHours)) PERSISTED;
GO

PRINT 'IdleHours computed column doğru formülle eklendi: 24 - (ActiveWorkingHours + MaintenanceHours)';
GO

-- Kontrol: Örnek kayıtları görüntüle
SELECT TOP 10
    v.VehicleName,
    wh.RecordDate,
    wh.ActiveWorkingHours,
    wh.MaintenanceHours,
    wh.IdleHours,
    (wh.ActiveWorkingHours + wh.MaintenanceHours + wh.IdleHours) AS ToplamSaat
FROM WorkingHours wh
INNER JOIN Vehicles v ON wh.VehicleId = v.VehicleId
ORDER BY wh.RecordDate DESC;
GO

