using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Web.Mvc;
using VehicleRentalManagement.DataAccess.Repositories;
using VehicleRentalManagement.Models.ViewModels;

namespace VehicleRentalManagement.Controllers
{
    public class HomeController : BaseController
    {
        private readonly VehicleRepository _vehicleRepo;
        private readonly WorkingHourRepository _workingHourRepo;

        public HomeController()
        {
            _vehicleRepo = new VehicleRepository();
            _workingHourRepo = new WorkingHourRepository();
        }

        public ActionResult Index()
        {
            var summaries = _workingHourRepo.GetWeeklySummary();
            var allRecords = _workingHourRepo.GetAll().Take(10).ToList();

            var viewModel = new DashboardViewModel
            {
                TotalVehicles = summaries.Count,
                ActiveVehicles = summaries.Count(s => s.RecordCount > 0),
                AverageActivePercentage = summaries.Any() ? summaries.Average(s => s.ActivePercentage) : 0,
                AverageIdlePercentage = summaries.Any() ? summaries.Average(s => s.IdlePercentage) : 0,
                VehicleSummaries = summaries,
                RecentRecords = allRecords
            };

            return View(viewModel);
        }

        public ActionResult About()
        {
            return View();
        }
    }
}