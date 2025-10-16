# Veritabanı Düzeltme Scriptleri

## Sorun #1: IdleHours Yanlış Hesaplanıyor
Boşta bekleme süresi (IdleHours) yanlış hesaplanıyordu. Örneğin: 745.3 saat / 168 = %443 gibi mantıksız değerler gösteriliyordu.

### Neden Yanlıştı?
1. **IdleHours Computed Column**: Yanlış formülle hesaplanıyor olabilirdi
2. **vw_WeeklyVehicleSummary View**: Yüzde hesaplaması yanlıştı
   - Yanlış: `(TotalIdleHours / 168) * 100`
   - Doğru: `(TotalIdleHours / (Kayıt Sayısı × 24)) * 100`

### Doğru Mantık
- **Her gün için**: `IdleHours = 24 - (ActiveWorkingHours + MaintenanceHours)`
- **Toplam IdleHours**: Her günün IdleHours değerlerinin toplamı
- **IdlePercentage**: `(Toplam IdleHours / (Kayıt Sayısı × 24)) × 100`

### Örnek:
- 7 günlük kayıt varsa ve toplam IdleHours = 84 saat ise:
- IdlePercentage = (84 / (7 × 24)) × 100 = (84 / 168) × 100 = %50 ✓

## Sorun #2: Haftalık Yüzde Hesaplaması Tüm Zamanı Kapsıyor
Dashboard'da Ford Transit %200 gibi yanlış yüzdeler gösteriyordu.

### Neden Yanlıştı?
- View **TÜM kayıtları** hesaplıyordu, sadece son 7 günü değil
- Yüzde hesaplaması yanlış mantıkla yapılıyordu

### Doğru Mantık
- **Son 7 günü filtrele**
- **Yüzde hesaplaması**: `(Aktif Saat / Toplam Çalışma Saati) × 100`
- **Toplam Çalışma Saati**: `Aktif + Boşta + Bakım` (Son 7 gündeki)
- Bu şekilde yüzdeler her zaman toplamda %100 olur

### Örnek:
Ford Transit için son 7 günde:
- Aktif: 120 saat
- Boşta: 30 saat
- Bakım: 18 saat
- **Toplam**: 168 saat
- **Aktif %**: (120/168) × 100 = %71.43 ✓
- **Boşta %**: (30/168) × 100 = %17.86 ✓
- **Bakım %**: (18/168) × 100 = %10.71 ✓
- **Toplam**: %100.00 ✓

## Nasıl Çalıştırılır?

### 1. SQL Server Management Studio (SSMS) veya Azure Data Studio Açın

### 2. Veritabanınıza Bağlanın
```
Server: localhost (veya SQL Server adresiniz)
Database: VehicleRentalDB
```

### 3. Scriptleri Sırayla Çalıştırın

#### Adım 1: Mevcut Durumu Kontrol Edin (Opsiyonel)
```sql
-- 01_CheckCurrentSchema.sql dosyasını açıp çalıştırın
-- Bu script mevcut yapıyı gösterir, hiçbir şeyi değiştirmez
```

#### Adım 2: IdleHours Computed Column'u Düzeltin
```sql
-- 02_FixIdleHoursComputed.sql dosyasını açıp çalıştırın
-- Bu script:
-- 1. Mevcut IdleHours sütununu kaldırır
-- 2. Doğru formülle yeniden oluşturur: 24 - (ActiveWorkingHours + MaintenanceHours)
```

#### Adım 3: View'ı Düzeltin (ESKİ - KULLANMAYIN)
```sql
-- 03_FixWeeklySummaryView.sql YANLIŞ - Tüm kayıtları hesaplıyor
-- Bu script artık kullanılmıyor, yerine Adım 4'ü kullanın
```

#### Adım 4: Haftalık Yüzde Hesaplamasını Düzeltin (YENİ - BUNU KULLANIN!)
```sql
-- 07_FixWeeklyPercentageCalculation.sql dosyasını açıp çalıştırın
-- Bu script:
-- 1. vw_WeeklyVehicleSummary view'ını siler
-- 2. SON 7 GÜN filtresini ekler
-- 3. Doğru yüzde hesaplamasını yapar: (Aktif / Toplam) * 100
-- 4. Yüzdelerin toplamı her zaman %100 olur
```

### 4. Uygulamayı Yeniden Başlatın
```powershell
# Eğer çalışıyorsa, uygulamayı durdurup yeniden başlatın
# Ctrl+C ile durdurun, sonra:
dotnet run --project VehicleRentalManagement
```

### 5. Test Edin
- Dashboard'a gidin: `https://localhost:7099/`
- Araç istatistiklerini kontrol edin
- Yüzdeler artık mantıklı olmalı (0-100 arasında)

## Kontrol Sorguları

### Tek bir aracın verilerini kontrol edin:
```sql
SELECT 
    v.VehicleName,
    COUNT(wh.WorkingHourId) AS KayitSayisi,
    SUM(wh.ActiveWorkingHours) AS ToplamAktif,
    SUM(wh.MaintenanceHours) AS ToplamBakim,
    SUM(wh.IdleHours) AS ToplamBosta,
    CAST((SUM(wh.IdleHours) / (COUNT(wh.WorkingHourId) * 24.0)) * 100 AS DECIMAL(5,2)) AS BostaYuzde
FROM Vehicles v
LEFT JOIN WorkingHours wh ON v.VehicleId = wh.VehicleId
WHERE v.VehicleName = 'Volkswagen Crafter'
GROUP BY v.VehicleName;
```

### Tüm view'ı kontrol edin:
```sql
SELECT * FROM vw_WeeklyVehicleSummary
ORDER BY VehicleName;
```

## Sorun Çözülmezse

Eğer hala sorun varsa:
1. `01_CheckCurrentSchema.sql` çalıştırarak mevcut yapıyı kontrol edin
2. Sonuçları paylaşın
3. Manuel düzeltme yapabiliriz

## Yedekleme Önerisi

Scriptleri çalıştırmadan önce veritabanını yedekleyin:
```sql
BACKUP DATABASE VehicleRentalDB 
TO DISK = 'C:\Backups\VehicleRentalDB_Before_Fix.bak'
WITH FORMAT;
```

## Hızlı Referans - Hangi Script Ne İçin?

| Script | Amaç | Kullan? |
|--------|------|---------|
| `01_CheckCurrentSchema.sql` | Mevcut veritabanı yapısını kontrol et | ✅ Test için |
| `02_FixIdleHoursComputed.sql` | IdleHours computed column'u düzelt | ✅ Evet |
| `03_FixWeeklySummaryView.sql` | ❌ ESKİ - Kullanma! | ❌ Hayır |
| `04_CheckFordTransitData.sql` | Ford Transit verilerini kontrol et | ✅ Test için |
| `05_FixPercentageIssue.sql` | Yüzde sorununu kontrol et | ✅ Test için |
| `06_ClearCacheAndTest.sql` | Cache'i temizle ve test et | ✅ Test için |
| `07_FixWeeklyPercentageCalculation.sql` | ✅ YENİ - Haftalık yüzdeleri düzelt | ✅✅ EVET! |

### Özetle Çalıştırmanız Gerekenler:
1. `02_FixIdleHoursComputed.sql` - IdleHours'u düzelt
2. `07_FixWeeklyPercentageCalculation.sql` - Haftalık yüzde hesaplamasını düzelt

Bu iki script sorunu tamamen çözer!

