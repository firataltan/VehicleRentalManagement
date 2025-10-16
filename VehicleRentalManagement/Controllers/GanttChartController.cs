using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using VehicleRentalManagement.DataAccess;
using VehicleRentalManagement.DataAccess.Repositories;
using VehicleRentalManagement.Models.ViewModels;

namespace VehicleRentalManagement.Controllers
{
    public class GanttChartController : BaseController
    {
        private readonly VehicleRepository _vehicleRepo;
        private readonly WorkingHourRepository _workingHourRepo;

        public GanttChartController(DatabaseConnection db)
        {
            _vehicleRepo = new VehicleRepository(db);
            _workingHourRepo = new WorkingHourRepository(db);
        }

        // GET: GanttChart
        [HttpGet]
        public ActionResult Index()
        {
            if (!IsAdmin)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var vehicles = _vehicleRepo.GetAll();

            var viewModel = new GanttChartViewModel
            {
                Vehicles = vehicles.ToList(),
                SelectedVehicleIds = new List<int>(),
                StartDate = DateTime.Today.AddDays(-7),
                EndDate = DateTime.Today,
                GanttData = new List<GanttDataItem>()
            };

            return View(viewModel);
        }

        // POST: GanttChart/GetData
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult GetData(List<int> vehicleIds, DateTime startDate, DateTime endDate)
        {
            try
            {
                if (vehicleIds == null || !vehicleIds.Any())
                {
                    return Json(new { success = false, message = "Lütfen en az bir araç seçiniz!" });
                }

                var data = _workingHourRepo.GetGanttData(vehicleIds, startDate, endDate);

                var formattedData = data.Select(d => new
                {
                    vehicleName = d.VehicleName,
                    licensePlate = d.LicensePlate,
                    startDate = d.StartDate.ToString("yyyy-MM-dd'T'HH:mm:ss"),
                    endDate = d.EndDate.ToString("yyyy-MM-dd'T'HH:mm:ss"),
                    hours = Math.Round(d.Hours, 2),
                    recordedBy = d.RecordedBy,
                    type = d.Type
                }).ToList();

                return Json(new { success = true, data = formattedData });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}