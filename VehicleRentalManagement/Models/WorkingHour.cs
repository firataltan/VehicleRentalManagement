using System;
using System.ComponentModel.DataAnnotations;

namespace VehicleRentalManagement.Models
{
    public class WorkingHour
    {
        public int WorkingHourId { get; set; }

        [Required(ErrorMessage = "Araç seçimi gereklidir")]
        [Display(Name = "Araç")]
        public int VehicleId { get; set; }

        [Required(ErrorMessage = "Tarih gereklidir")]
        [DataType(DataType.Date)]
        [Display(Name = "Kayıt Tarihi")]
        public DateTime RecordDate { get; set; }

        [Required(ErrorMessage = "Aktif çalışma süresi gereklidir")]
        [Range(0, 24, ErrorMessage = "Aktif çalışma süresi 0-24 saat arasında olmalıdır")]
        [Display(Name = "Aktif Çalışma Süresi (saat)")]
        [RegularExpression(@"^\d+(\.\d{1,2})?$", ErrorMessage = "Geçerli bir saat değeri giriniz (örn: 23.5)")]
        public decimal ActiveWorkingHours { get; set; }

        [Required(ErrorMessage = "Bakım süresi gereklidir")]
        [Range(0, 24, ErrorMessage = "Bakım süresi 0-24 saat arasında olmalıdır")]
        [Display(Name = "Bakım Süresi (saat)")]
        [RegularExpression(@"^\d+(\.\d{1,2})?$", ErrorMessage = "Geçerli bir saat değeri giriniz (örn: 2.5)")]
        public decimal MaintenanceHours { get; set; }

        // Computed column - Database'de otomatik hesaplanıyor
        [Display(Name = "Boşta Bekleme Süresi (saat)")]
        public decimal IdleHours { get; set; }

        [StringLength(500, ErrorMessage = "Notlar en fazla 500 karakter olabilir")]
        [Display(Name = "Notlar")]
        [DataType(DataType.MultilineText)]
        public string Notes { get; set; }

        public int CreatedBy { get; set; }

        [Display(Name = "Oluşturulma Tarihi")]
        public DateTime CreatedDate { get; set; }

        public int? ModifiedBy { get; set; }

        [Display(Name = "Güncellenme Tarihi")]
        public DateTime? ModifiedDate { get; set; }

        // Navigation properties - Database'den JOIN ile gelecek
        public string? VehicleName { get; set; }

        public string? LicensePlate { get; set; }

        public string? CreatedByName { get; set; }

        public string? ModifiedByName { get; set; }
    }
}
