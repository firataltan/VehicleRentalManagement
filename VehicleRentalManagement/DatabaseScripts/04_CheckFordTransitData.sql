-- Ford Transit için detaylı kontrol
USE VehicleRentalDB;
GO

-- 1. Ham kayıtları göster
SELECT 
    v.VehicleName,
    wh.RecordDate,
    wh.ActiveWorkingHours,
    wh.MaintenanceHours,
    wh.IdleHours,
    (wh.ActiveWorkingHours + wh.MaintenanceHours + wh.IdleHours) AS GünlükToplam
FROM WorkingHours wh
INNER JOIN Vehicles v ON wh.VehicleId = v.VehicleId
WHERE v.VehicleName LIKE '%Ford Transit%'
ORDER BY wh.RecordDate DESC;
GO

-- 2. View'dan özet verileri çek
SELECT 
    VehicleName,
    TotalActiveHours,
    TotalMaintenanceHours,
    TotalIdleHours,
    (TotalActiveHours + TotalMaintenanceHours + TotalIdleHours) AS ToplamSaat,
    RecordCount,
    (RecordCount * 24) AS BeklenenToplamSaat,
    ActivePercentage,
    MaintenancePercentage,
    IdlePercentage,
    (ActivePercentage + MaintenancePercentage + IdlePercentage) AS YüzdeToplam
FROM vw_WeeklyVehicleSummary
WHERE VehicleName LIKE '%Ford Transit%';
GO

-- 3. Manuel hesaplama yap ve karşılaştır
SELECT 
    v.VehicleName,
    COUNT(wh.WorkingHourId) AS KayitSayisi,
    SUM(wh.ActiveWorkingHours) AS ToplamAktif,
    SUM(wh.MaintenanceHours) AS ToplamBakim,
    SUM(wh.IdleHours) AS ToplamBosta,
    (SUM(wh.ActiveWorkingHours) + SUM(wh.MaintenanceHours) + SUM(wh.IdleHours)) AS ToplamSaat,
    (COUNT(wh.WorkingHourId) * 24) AS BeklenenToplam,
    -- Manuel yüzde hesaplama
    CAST((SUM(wh.ActiveWorkingHours) / (COUNT(wh.WorkingHourId) * 24.0)) * 100 AS DECIMAL(5,2)) AS AktifYuzde,
    CAST((SUM(wh.MaintenanceHours) / (COUNT(wh.WorkingHourId) * 24.0)) * 100 AS DECIMAL(5,2)) AS BakimYuzde,
    CAST((SUM(wh.IdleHours) / (COUNT(wh.WorkingHourId) * 24.0)) * 100 AS DECIMAL(5,2)) AS BostaYuzde,
    CAST((SUM(wh.ActiveWorkingHours) / (COUNT(wh.WorkingHourId) * 24.0)) * 100 AS DECIMAL(5,2)) +
    CAST((SUM(wh.MaintenanceHours) / (COUNT(wh.WorkingHourId) * 24.0)) * 100 AS DECIMAL(5,2)) +
    CAST((SUM(wh.IdleHours) / (COUNT(wh.WorkingHourId) * 24.0)) * 100 AS DECIMAL(5,2)) AS ToplamYuzde
FROM Vehicles v
LEFT JOIN WorkingHours wh ON v.VehicleId = wh.VehicleId
WHERE v.VehicleName LIKE '%Ford Transit%'
GROUP BY v.VehicleName;
GO

-- 4. IdleHours computed column tanımını kontrol et
SELECT 
    c.name AS ColumnName,
    cc.definition AS ComputedFormula
FROM sys.columns c
INNER JOIN sys.computed_columns cc ON c.object_id = cc.object_id AND c.column_id = cc.column_id
WHERE c.object_id = OBJECT_ID('WorkingHours')
  AND c.name = 'IdleHours';
GO

