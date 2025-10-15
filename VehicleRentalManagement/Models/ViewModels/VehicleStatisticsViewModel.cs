using System;
using System.Collections.Generic;

namespace VehicleRentalManagement.Models.ViewModels
{
    public class VehicleStatisticsViewModel
    {
        public Vehicle Vehicle { get; set; }
        public List<WorkingHour> WorkingHours { get; set; }

        // İstatistikler
        public decimal TotalActiveHours { get; set; }
        public decimal TotalMaintenanceHours { get; set; }
        public decimal TotalIdleHours { get; set; }
        public decimal AverageActiveHoursPerDay { get; set; }
        public decimal EfficiencyRate { get; set; }

        // Tarih filtreleri
        public DateTime? FilterStartDate { get; set; }
        public DateTime? FilterEndDate { get; set; }

        public VehicleStatisticsViewModel()
        {
            WorkingHours = new List<WorkingHour>();
        }
    }
}