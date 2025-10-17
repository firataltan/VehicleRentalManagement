-- Kullanıcı Bilgilerini Güncelleme Scripti
-- John Doe -> Fırat Altan
-- john@vehiclerental.com -> altan@gmail.com
-- Jane Smith -> Ali Demir
-- jane@vehiclerental.com -> Ali@gmail.com

USE VehicleRentalDB;
GO

PRINT '=== KULLANICI BİLGİLERİ GÜNCELLEME BAŞLADI ===';
GO

-- 1. Mevcut bilgileri kontrol et
PRINT '=== MEVCUT KULLANICI BİLGİLERİ ===';
SELECT 
    UserId,
    Username,
    FullName,
    Email,
    Role,
    IsActive,
    CreatedDate,
    LastLoginDate
FROM Users
WHERE UserId IN (2, 3);  -- John Doe (2) ve Jane Smith (3) ID'leri
GO

-- 2. Kullanıcı bilgilerini güncelle
-- John Doe -> Fırat Altan
UPDATE Users 
SET 
    FullName = 'Fırat Altan',
    Email = 'altan@gmail.com'
WHERE UserId = 2;
GO

-- Jane Smith -> Ali Demir
UPDATE Users 
SET 
    FullName = 'Ali Demir',
    Email = 'Ali@gmail.com'
WHERE UserId = 3;
GO

-- 3. Güncellenmiş bilgileri kontrol et
PRINT '=== GÜNCELLENMİŞ KULLANICI BİLGİLERİ ===';
SELECT 
    UserId,
    Username,
    FullName,
    Email,
    Role,
    IsActive,
    CreatedDate,
    LastLoginDate
FROM Users
WHERE UserId IN (2, 3);
GO

-- 4. Tüm kullanıcıları listele (güncelleme sonrası)
PRINT '=== TÜM KULLANICILAR ===';
SELECT 
    UserId,
    Username,
    FullName,
    Email,
    Role,
    CASE 
        WHEN IsActive = 1 THEN '✅ AKTİF'
        ELSE '❌ PASİF'
    END AS Durum,
    CreatedDate,
    LastLoginDate
FROM Users
ORDER BY UserId;
GO

PRINT '=== GÜNCELLEME TAMAMLANDI ===';
PRINT '✅ John Doe -> Fırat Altan';
PRINT '✅ john@vehiclerental.com -> altan@gmail.com';
PRINT '✅ Jane Smith -> Ali Demir';
PRINT '✅ jane@vehiclerental.com -> Ali@gmail.com';
PRINT 'Kullanıcı bilgileri başarıyla güncellendi. Sistem çalışması etkilenmeyecek.';
GO
