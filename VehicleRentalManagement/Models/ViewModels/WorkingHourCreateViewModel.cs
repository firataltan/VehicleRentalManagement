using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace VehicleRentalManagement.Models.ViewModels
{
    public class WorkingHourCreateViewModel
    {
        [Required(ErrorMessage = "Araç seçimi gereklidir")]
        [Display(Name = "Araç")]
        public int VehicleId { get; set; }

        [Required(ErrorMessage = "Tarih gereklidir")]
        [DataType(DataType.Date)]
        [Display(Name = "Kayıt Tarihi")]
        public DateTime RecordDate { get; set; }

        [Required(ErrorMessage = "Aktif çalışma süresi gereklidir")]
        [Range(0, 24, ErrorMessage = "0-24 saat arasında olmalıdır")]
        [Display(Name = "Aktif Çalışma Süresi (saat)")]
        public decimal ActiveWorkingHours { get; set; }

        [Required(ErrorMessage = "Bakım süresi gereklidir")]
        [Range(0, 24, ErrorMessage = "0-24 saat arasında olmalıdır")]
        [Display(Name = "Bakım Süresi (saat)")]
        public decimal MaintenanceHours { get; set; }

        [StringLength(500)]
        [Display(Name = "Notlar")]
        [DataType(DataType.MultilineText)]
        public string? Notes { get; set; }

        // Dropdown için
        public IEnumerable<SelectListItem> VehicleList { get; set; }

        // Hesaplanan değerler (View'da gösterim için)
        public decimal CalculatedIdleHours
        {
            get { return 24 - (ActiveWorkingHours + MaintenanceHours); }
        }

        public WorkingHourCreateViewModel()
        {
            RecordDate = DateTime.Today;
            VehicleList = new List<SelectListItem>();
        }
    }
}