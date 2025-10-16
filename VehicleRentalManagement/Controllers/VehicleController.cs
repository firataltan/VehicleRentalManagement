using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using VehicleRentalManagement.DataAccess;
using VehicleRentalManagement.DataAccess.Repositories;
using VehicleRentalManagement.Models;

namespace VehicleRentalManagement.Controllers
{
    [Authorize]
    public class VehicleController : BaseController
    {
        private readonly VehicleRepository _vehicleRepo;

        public VehicleController(DatabaseConnection db)
        {
            _vehicleRepo = new VehicleRepository(db);
        }

        // GET: Vehicle
        public ActionResult Index()
        {
            if (!IsAdmin)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var vehicles = _vehicleRepo.GetAll();
            return View(vehicles);
        }

        // GET: Vehicle/Create
        [HttpGet]
        public ActionResult Create()
        {
            if (!IsAdmin)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            return View();
        }

        // POST: Vehicle/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind("VehicleName,LicensePlate")] Vehicle vehicle)
        {
            if (!IsAdmin)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            // Server-managed fields: set before validation and remove from ModelState
            vehicle.CreatedBy = CurrentUserId;
            vehicle.IsActive = true;
            ModelState.Remove(nameof(Vehicle.CreatedBy));
            ModelState.Remove(nameof(Vehicle.IsActive));
            ModelState.Remove(nameof(Vehicle.CreatedDate));
            ModelState.Remove(nameof(Vehicle.ModifiedBy));
            ModelState.Remove(nameof(Vehicle.ModifiedDate));

            if (ModelState.IsValid)
            {
                try
                {
                    var id = _vehicleRepo.Add(vehicle);

                    if (id > 0)
                    {
                        TempData["SuccessMessage"] = "Araç başarıyla eklendi!";
                        return RedirectToAction("Index");
                    }

                    ModelState.AddModelError("", "Araç eklenirken bir hata oluştu!");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Hata: " + ex.Message);
                }
            }

            return View(vehicle);
        }

        // GET: Vehicle/Edit/5
        [HttpGet]
        public ActionResult Edit(int id)
        {
            if (!IsAdmin)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var vehicle = _vehicleRepo.GetById(id);

            if (vehicle == null)
            {
                return NotFound();
            }

            return View(vehicle);
        }

        // POST: Vehicle/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Vehicle vehicle)
        {
            if (!IsAdmin)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    vehicle.ModifiedBy = CurrentUserId;

                    if (_vehicleRepo.Update(vehicle))
                    {
                        TempData["SuccessMessage"] = "Araç başarıyla güncellendi!";
                        return RedirectToAction("Index");
                    }

                    ModelState.AddModelError("", "Araç güncellenirken bir hata oluştu!");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Hata: " + ex.Message);
                }
            }

            return View(vehicle);
        }

        // POST: Vehicle/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            if (!IsAdmin)
            {
                return Json(new { success = false, message = "Yetkiniz yok!" });
            }

            try
            {
                if (_vehicleRepo.Delete(id))
                {
                    return Json(new { success = true, message = "Araç başarıyla silindi!" });
                }

                return Json(new { success = false, message = "Araç silinirken bir hata oluştu!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Hata: " + ex.Message });
            }
        }

        // GET: Vehicle/Details/5
        public ActionResult Details(int id)
        {
            var vehicle = _vehicleRepo.GetById(id);

            if (vehicle == null)
            {
                return NotFound();
            }

            return View(vehicle);
        }
    }
}