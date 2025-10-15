using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using VehicleRentalManagement.DataAccess;
using VehicleRentalManagement.DataAccess.Repositories;

namespace VehicleRentalManagement.Controllers
{
    public class ChartController : BaseController
    {
        private readonly WorkingHourRepository _workingHourRepo;

        public ChartController(DatabaseConnection db)
        {
            _workingHourRepo = new WorkingHourRepository(db);
        }

        public IActionResult ActiveHours()
        {
            return View();
        }

        public IActionResult IdleHours()
        {
            return View();
        }

        [HttpGet]
        public JsonResult GetActiveHoursData()
        {
            try
            {
                var summaries = _workingHourRepo.GetWeeklySummary();

                var data = summaries.Select(s => new
                {
                    vehicleName = s.VehicleName,
                    licensePlate = s.LicensePlate,
                    activePercentage = Math.Round(s.ActivePercentage, 2),
                    activeHours = Math.Round(s.TotalActiveHours, 2),
                    totalHours = 168
                }).ToList();

                return Json(new { success = true, data = data });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public JsonResult GetIdleHoursData()
        {
            try
            {
                var summaries = _workingHourRepo.GetWeeklySummary();

                var data = summaries.Select(s => new
                {
                    vehicleName = s.VehicleName,
                    licensePlate = s.LicensePlate,
                    idlePercentage = Math.Round(s.IdlePercentage, 2),
                    idleHours = Math.Round(s.TotalIdleHours, 2),
                    totalHours = 168
                }).ToList();

                return Json(new { success = true, data = data });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public JsonResult GetComparisonData()
        {
            try
            {
                var summaries = _workingHourRepo.GetWeeklySummary();

                var data = summaries.Select(s => new
                {
                    vehicleName = s.VehicleName,
                    licensePlate = s.LicensePlate,
                    activePercentage = Math.Round(s.ActivePercentage, 2),
                    maintenancePercentage = Math.Round(s.MaintenancePercentage, 2),
                    idlePercentage = Math.Round(s.IdlePercentage, 2)
                }).ToList();

                return Json(new { success = true, data = data });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
