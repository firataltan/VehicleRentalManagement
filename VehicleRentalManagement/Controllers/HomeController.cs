using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using VehicleRentalManagement.DataAccess;
using VehicleRentalManagement.DataAccess.Repositories;
using VehicleRentalManagement.Models.ViewModels;

namespace VehicleRentalManagement.Controllers
{
    public class HomeController : BaseController
    {
        private readonly VehicleRepository _vehicleRepo;
        private readonly WorkingHourRepository _workingHourRepo;

        public HomeController(DatabaseConnection db)
        {
            _vehicleRepo = new VehicleRepository(db);
            _workingHourRepo = new WorkingHourRepository(db);
        }

        public ActionResult Index()
        {
            // Login olmamış kullanıcılar için basit bir hoş geldin sayfası
            if (!User.Identity.IsAuthenticated)
            {
                return View("Welcome");
            }

            // Login olmuş kullanıcılar için dashboard
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

        [AllowAnonymous]
        public ActionResult About()
        {
            return View();
        }
    }
}