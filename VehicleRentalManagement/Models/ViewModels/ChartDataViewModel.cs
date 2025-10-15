using System.Collections.Generic;

namespace VehicleRentalManagement.Models.ViewModels
{
    public class ChartDataViewModel
    {
        public List<string> Labels { get; set; }
        public List<decimal> ActiveData { get; set; }
        public List<decimal> MaintenanceData { get; set; }
        public List<decimal> IdleData { get; set; }

        public ChartDataViewModel()
        {
            Labels = new List<string>();
            ActiveData = new List<decimal>();
            MaintenanceData = new List<decimal>();
            IdleData = new List<decimal>();
        }
    }

    public class PieChartDataViewModel
    {
        public List<string> Labels { get; set; }
        public List<decimal> Data { get; set; }
        public List<string> BackgroundColors { get; set; }

        public PieChartDataViewModel()
        {
            Labels = new List<string>();
            Data = new List<decimal>();
            BackgroundColors = new List<string>();
        }
    }
}