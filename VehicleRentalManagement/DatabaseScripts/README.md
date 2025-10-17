# Veritabanı Kurulum Scripti

Bu klasör, **Vehicle Rental Management System** projesinin veritabanını oluşturmak için gerekli SQL scriptini içerir.

## 📋 İçerik

- **CreateDatabase.sql** - Ana veritabanı kurulum scripti

## 🚀 Kurulum Adımları

### 1. SQL Server Management Studio (SSMS) Açın

SQL Server Management Studio veya Azure Data Studio'yu açın ve SQL Server'ınıza bağlanın.

### 2. Script'i Açın

`CreateDatabase.sql` dosyasını SSMS'de açın:
- File → Open → File
- `CreateDatabase.sql` dosyasını seçin

### 3. Script'i Çalıştırın

- Tüm script'i seçin (Ctrl+A) veya hiçbir şey seçmeden
- Execute (F5) butonuna basın

### 4. Sonuçları Kontrol Edin

Script aşağıdakileri oluşturacaktır:
- ✅ VehicleRentalDB veritabanı
- ✅ 4 tablo (Users, Vehicles, WorkingHours, AuditLogs)
- ✅ 1 view (vw_WeeklyVehicleSummary)
- ✅ Indexler (performans için)
- ✅ Örnek kullanıcılar (admin, user1)
- ✅ Örnek araçlar (3 adet)
- ✅ Örnek çalışma saati kayıtları (son 7 gün)

## 👤 Varsayılan Kullanıcılar

Script çalıştırıldıktan sonra aşağıdaki kullanıcılarla giriş yapabilirsiniz:

| Kullanıcı Adı | Şifre | Rol |
|---------------|-------|-----|
| **admin** | admin123 | Admin |
| **user1** | user123 | User |

## 🔧 Connection String

`appsettings.json` dosyanızdaki connection string'i aşağıdaki gibi ayarlayın:

```json
{
  "ConnectionStrings": {
    "VehicleRentalDB": "Server=localhost;Database=VehicleRentalDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

**Not:** SQL Server Authentication kullanıyorsanız:

```json
{
  "ConnectionStrings": {
    "VehicleRentalDB": "Server=localhost;Database=VehicleRentalDB;User Id=sa;Password=YourPassword;TrustServerCertificate=True;"
  }
}
```

## 📊 Veritabanı Yapısı

### Tablolar

1. **Users** - Kullanıcı bilgileri ve kimlik doğrulama
2. **Vehicles** - Araç bilgileri
3. **WorkingHours** - Günlük çalışma saati kayıtları
4. **AuditLogs** - Sistem işlem logları

### View

- **vw_WeeklyVehicleSummary** - Son 7 günün araç bazlı özet istatistikleri

### Computed Column

**WorkingHours** tablosundaki `IdleHours` sütunu otomatik hesaplanır:
```sql
IdleHours = 24 - (ActiveWorkingHours + MaintenanceHours)
```

## ⚠️ Önemli Notlar

1. **Veritabanı Silinir:** Script çalıştırıldığında mevcut `VehicleRentalDB` veritabanı silinir ve yeniden oluşturulur. Eğer önemli verileriniz varsa önce yedek alın!

2. **Yedekleme:** Production ortamında çalıştırmadan önce mutlaka yedek alın:
   ```sql
   BACKUP DATABASE VehicleRentalDB 
   TO DISK = 'C:\Backups\VehicleRentalDB_Backup.bak'
   WITH FORMAT;
   ```

3. **Şifre Güvenliği:** Production ortamında varsayılan şifreleri mutlaka değiştirin!

## ✅ Kontrol

Script başarıyla çalıştıktan sonra aşağıdaki sorguyu çalıştırarak kontrolü yapabilirsiniz:

```sql
-- Tabloları kontrol et
SELECT name FROM sys.tables ORDER BY name;

-- View'ları kontrol et
SELECT name FROM sys.views ORDER BY name;

-- Kullanıcıları kontrol et
SELECT * FROM Users;

-- Araçları kontrol et
SELECT * FROM Vehicles;

-- Haftalık özeti kontrol et
SELECT * FROM vw_WeeklyVehicleSummary;
```

## 🆘 Sorun Giderme

### Hata: "Database already exists"

Script otomatik olarak mevcut veritabanını silmelidir. Eğer hata alırsanız:

```sql
USE master;
ALTER DATABASE VehicleRentalDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
DROP DATABASE VehicleRentalDB;
```

### Hata: "Permission denied"

SQL Server'da veritabanı oluşturma yetkisine sahip bir kullanıcı ile giriş yapmalısınız (örn: sa veya sysadmin rolü).

## 📞 Destek

Herhangi bir sorunla karşılaşırsanız lütfen proje ekibiyle iletişime geçin.

