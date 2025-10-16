-- Chart verilerini debug et
-- Bu script ile JavaScript'e giden verileri kontrol et

USE VehicleRentalManagementDB;
GO

PRINT 'Chart için JavaScript verileri:';
PRINT '==============================';

-- 1. View'dan gelen veriler
SELECT 
    VehicleName,
    ActivePercentage,
    IdlePercentage,
    MaintenancePercentage,
    (ActivePercentage + IdlePercentage + MaintenancePercentage) AS Toplam
FROM vw_WeeklyVehicleSummary
ORDER BY VehicleName;
GO

PRINT '';
PRINT 'JavaScript array formatında:';
PRINT '============================';

-- 2. JavaScript array formatında
SELECT 
    'var vehicleNames = [' + 
    STRING_AGG('''' + VehicleName + '''', ',') + '];' AS JavaScript_VehicleNames
FROM vw_WeeklyVehicleSummary;
GO

SELECT 
    'var activeData = [' + 
    STRING_AGG(CAST(ActivePercentage AS VARCHAR(10)), ',') + '];' AS JavaScript_ActiveData
FROM vw_WeeklyVehicleSummary;
GO

SELECT 
    'var idleData = [' + 
    STRING_AGG(CAST(IdlePercentage AS VARCHAR(10)), ',') + '];' AS JavaScript_IdleData
FROM vw_WeeklyVehicleSummary;
GO

SELECT 
    'var maintenanceData = [' + 
    STRING_AGG(CAST(MaintenancePercentage AS VARCHAR(10)), ',') + '];' AS JavaScript_MaintenanceData
FROM vw_WeeklyVehicleSummary;
GO

-- 3. Manuel toplam kontrolü
PRINT '';
PRINT 'Manuel toplam kontrolü:';
PRINT '======================';
SELECT 
    VehicleName,
    ActivePercentage,
    IdlePercentage,
    MaintenancePercentage,
    (ActivePercentage + IdlePercentage + MaintenancePercentage) AS JavaScriptToplam,
    CASE 
        WHEN ABS((ActivePercentage + IdlePercentage + MaintenancePercentage) - 100.00) < 0.01 
        THEN '✅ DOĞRU' 
        ELSE '❌ YANLIŞ' 
    END AS Durum
FROM vw_WeeklyVehicleSummary
ORDER BY VehicleName;
GO
