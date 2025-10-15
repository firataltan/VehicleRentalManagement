using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using VehicleRentalManagement.DataAccess.Repositories;
using VehicleRentalManagement.Models;
using VehicleRentalManagement.DataAccess;

namespace VehicleRentalManagement.Controllers
{
    public class WorkingHourController : BaseController
    {
        private readonly WorkingHourRepository _workingHourRepo;
        private readonly VehicleRepository _vehicleRepo;

        public WorkingHourController(DatabaseConnection db)
        {
            _workingHourRepo = new WorkingHourRepository(db);
            _vehicleRepo = new VehicleRepository(db);
        }

        // GET: WorkingHour
        public ActionResult Index()
        {
            var workingHours = _workingHourRepo.GetAll();
            return View(workingHours);
        }

        // GET: WorkingHour/Create
        [HttpGet]
        public ActionResult Create()
        {
            if (!IsUser && !IsAdmin)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            ViewBag.Vehicles = new SelectList(_vehicleRepo.GetAll(), "VehicleId", "VehicleName");

            var model = new WorkingHour
            {
                RecordDate = DateTime.Today
            };

            return View(model);
        }

        // POST: WorkingHour/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(WorkingHour workingHour)
        {
            if (!IsUser && !IsAdmin)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            // Validation: Total hours should not exceed 24
            if (workingHour.ActiveWorkingHours + workingHour.MaintenanceHours > 24)
            {
                ModelState.AddModelError("", "Aktif çalışma ve bakım sürelerinin toplamı 24 saati geçemez!");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    workingHour.CreatedBy = CurrentUserId;

                    var id = _workingHourRepo.Add(workingHour);

                    if (id > 0)
                    {
                        TempData["SuccessMessage"] = "Çalışma süresi başarıyla eklendi!";
                        return RedirectToAction("Index");
                    }

                    ModelState.AddModelError("", "Çalışma süresi eklenirken bir hata oluştu!");
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("UNIQUE"))
                    {
                        ModelState.AddModelError("", "Bu araç için bu tarihte zaten bir kayıt mevcut!");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Hata: " + ex.Message);
                    }
                }
            }

            ViewBag.Vehicles = new SelectList(_vehicleRepo.GetAll(), "VehicleId", "VehicleName", workingHour.VehicleId);
            return View(workingHour);
        }

        // GET: WorkingHour/Edit/5
        [HttpGet]
        public ActionResult Edit(int id)
        {
            if (!IsUser && !IsAdmin)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var workingHour = _workingHourRepo.GetById(id);

            if (workingHour == null)
            {
                return NotFound();
            }

            ViewBag.Vehicles = new SelectList(_vehicleRepo.GetAll(), "VehicleId", "VehicleName", workingHour.VehicleId);
            return View(workingHour);
        }

        // POST: WorkingHour/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(WorkingHour workingHour)
        {
            if (!IsUser && !IsAdmin)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            // Validation: Total hours should not exceed 24
            if (workingHour.ActiveWorkingHours + workingHour.MaintenanceHours > 24)
            {
                ModelState.AddModelError("", "Aktif çalışma ve bakım sürelerinin toplamı 24 saati geçemez!");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    workingHour.ModifiedBy = CurrentUserId;

                    if (_workingHourRepo.Update(workingHour))
                    {
                        TempData["SuccessMessage"] = "Çalışma süresi başarıyla güncellendi!";
                        return RedirectToAction("Index");
                    }

                    ModelState.AddModelError("", "Çalışma süresi güncellenirken bir hata oluştu!");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Hata: " + ex.Message);
                }
            }

            ViewBag.Vehicles = new SelectList(_vehicleRepo.GetAll(), "VehicleId", "VehicleName", workingHour.VehicleId);
            return View(workingHour);
        }

        // POST: WorkingHour/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            if (!IsUser && !IsAdmin)
            {
                return Json(new { success = false, message = "Yetkiniz yok!" });
            }

            try
            {
                if (_workingHourRepo.Delete(id))
                {
                    return Json(new { success = true, message = "Çalışma süresi başarıyla silindi!" });
                }

                return Json(new { success = false, message = "Çalışma süresi silinirken bir hata oluştu!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Hata: " + ex.Message });
            }
        }

        // GET: WorkingHour/Details/5
        public ActionResult Details(int id)
        {
            var workingHour = _workingHourRepo.GetById(id);

            if (workingHour == null)
            {
                return NotFound();
            }

            return View(workingHour);
        }
    }
}