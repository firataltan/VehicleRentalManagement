using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VehicleRentalManagement.Models.ViewModels
{
    public class GanttChartViewModel
    {
        public List<Vehicle> Vehicles { get; set; }

        [Display(Name = "Seçili Araçlar")]
        public List<int> SelectedVehicleIds { get; set; }

        [Required(ErrorMessage = "Başlangıç tarihi gereklidir")]
        [DataType(DataType.Date)]
        [Display(Name = "Başlangıç Tarihi")]
        public DateTime? StartDate { get; set; }

        [Required(ErrorMessage = "Bitiş tarihi gereklidir")]
        [DataType(DataType.Date)]
        [Display(Name = "Bitiş Tarihi")]
        public DateTime? EndDate { get; set; }

        public List<GanttDataItem> GanttData { get; set; }

        public GanttChartViewModel()
        {
            Vehicles = new List<Vehicle>();
            SelectedVehicleIds = new List<int>();
            GanttData = new List<GanttDataItem>();
        }
    }

    public class GanttDataItem
    {
        public string VehicleName { get; set; }
        public string LicensePlate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal Hours { get; set; }
        public string RecordedBy { get; set; }
        public string Type { get; set; } // Active, Maintenance, Idle
        public string Color { get; set; } // Grafik rengi için
    }
}
