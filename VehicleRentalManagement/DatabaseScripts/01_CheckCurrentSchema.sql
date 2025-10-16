-- Mevcut WorkingHours tablosu yapısını kontrol et
EXEC sp_help 'WorkingHours';

-- IdleHours computed column tanımını göster
SELECT 
    c.name AS ColumnName,
    OBJECT_DEFINITION(c.default_object_id) AS DefaultValue,
    cc.definition AS ComputedColumnDefinition,
    c.is_computed
FROM sys.columns c
LEFT JOIN sys.computed_columns cc ON c.object_id = cc.object_id AND c.column_id = cc.column_id
WHERE c.object_id = OBJECT_ID('WorkingHours')
  AND c.name = 'IdleHours';

-- Mevcut view tanımını göster
SELECT OBJECT_DEFINITION(OBJECT_ID('vw_WeeklyVehicleSummary')) AS ViewDefinition;

-- Örnek veri kontrolü
SELECT TOP 5
    v.VehicleName,
    wh.RecordDate,
    wh.ActiveWorkingHours,
    wh.MaintenanceHours,
    wh.IdleHours,
    (wh.ActiveWorkingHours + wh.MaintenanceHours + wh.IdleHours) AS ToplamSaat
FROM WorkingHours wh
INNER JOIN Vehicles v ON wh.VehicleId = v.VehicleId
ORDER BY wh.RecordDate DESC;

