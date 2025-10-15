using System.Collections.Generic;

namespace VehicleRentalManagement.Models.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalVehicles { get; set; }
        public int ActiveVehicles { get; set; }
        public decimal AverageActivePercentage { get; set; }
        public decimal AverageIdlePercentage { get; set; }
        public List<VehicleSummary> VehicleSummaries { get; set; }
        public List<WorkingHour> RecentRecords { get; set; }

        public DashboardViewModel()
        {
            VehicleSummaries = new List<VehicleSummary>();
            RecentRecords = new List<WorkingHour>();
        }
    }

    public class VehicleSummary
    {
        public int VehicleId { get; set; }
        public string VehicleName { get; set; }
        public string LicensePlate { get; set; }
        public decimal TotalActiveHours { get; set; }
        public decimal TotalMaintenanceHours { get; set; }
        public decimal TotalIdleHours { get; set; }
        public decimal ActivePercentage { get; set; }
        public decimal MaintenancePercentage { get; set; }
        public decimal IdlePercentage { get; set; }
        public int RecordCount { get; set; }
    }
}