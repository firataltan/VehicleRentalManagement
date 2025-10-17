using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using VehicleRentalManagement.DataAccess;
using VehicleRentalManagement.DataAccess.Repositories;
using VehicleRentalManagement.Models;
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
            List<VehicleSummary> summaries;
            if (IsAdmin)
            {
                // Admin tüm araçların özetini görür
                summaries = _workingHourRepo.GetWeeklySummary();
            }
            else
            {
                // User sadece kendi kayıtlarına göre araç özetini görür
                summaries = _workingHourRepo.GetWeeklySummaryByUserId(CurrentUserId);
            }
            
            IEnumerable<WorkingHour> allRecords;
            if (IsAdmin)
            {
                // Admin tüm kayıtları görür
                allRecords = _workingHourRepo.GetAll().Take(10).ToList();
            }
            else
            {
                // User sadece kendi kayıtlarını görür
                allRecords = _workingHourRepo.GetByUserId(CurrentUserId).Take(10).ToList();
            }

            // DEBUG: Console'da verileri kontrol et
            System.Diagnostics.Debug.WriteLine("=== CONTROLLER DEBUG ===");
            foreach (var summary in summaries)
            {
                var total = summary.ActivePercentage + summary.IdlePercentage + summary.MaintenancePercentage;
                System.Diagnostics.Debug.WriteLine($"{summary.VehicleName}: {summary.ActivePercentage}% + {summary.IdlePercentage}% + {summary.MaintenancePercentage}% = {total}%");
            }

            var viewModel = new DashboardViewModel
            {
                TotalVehicles = summaries.Count,
                ActiveVehicles = summaries.Count(s => s.RecordCount > 0),
                AverageActivePercentage = summaries.Any() ? summaries.Average(s => s.ActivePercentage) : 0,
                AverageIdlePercentage = summaries.Any() ? summaries.Average(s => s.IdlePercentage) : 0,
                VehicleSummaries = summaries,
                RecentRecords = allRecords.ToList()
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