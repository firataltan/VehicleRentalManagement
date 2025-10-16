using System;
using System.ComponentModel.DataAnnotations;

namespace VehicleRentalManagement.Models
{
    public class Vehicle
    {
        public int VehicleId { get; set; }

        [Required(ErrorMessage = "Araç adı gereklidir")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Araç adı 2-100 karakter arasında olmalıdır")]
        [Display(Name = "Araç Adı")]
        public string VehicleName { get; set; }

        [Required(ErrorMessage = "Plaka gereklidir")]
        [StringLength(20, ErrorMessage = "Plaka en fazla 20 karakter olabilir")]
        [Display(Name = "Plaka")]
        [RegularExpression(@"^\d{2}[A-Z]{1,3}\d{1,4}$", ErrorMessage = "Geçerli bir plaka giriniz (örn: 26AX001)")]
        public string LicensePlate { get; set; }

        [Display(Name = "Aktif")]
        public bool IsActive { get; set; }

        public int CreatedBy { get; set; }

        [Display(Name = "Oluşturulma Tarihi")]
        public DateTime CreatedDate { get; set; }

        public int? ModifiedBy { get; set; }

        [Display(Name = "Güncellenme Tarihi")]
        public DateTime? ModifiedDate { get; set; }

        // Navigation properties - Database'den JOIN ile gelecek
        public string? CreatedByName { get; set; }

        public string? ModifiedByName { get; set; }
    }
}