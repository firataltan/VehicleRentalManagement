using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using VehicleRentalManagement.DataAccess.Repositories;
using VehicleRentalManagement.Models;
using VehicleRentalManagement.DataAccess;

namespace VehicleRentalManagement.Controllers
{
    [Authorize]
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
        public ActionResult Index(DateTime? startDate, DateTime? endDate, int? vehicleId)
        {
            // Debug için filtreleme parametrelerini logla
            System.Diagnostics.Debug.WriteLine($"WorkingHourController.Index - startDate: {startDate}, endDate: {endDate}, vehicleId: {vehicleId}");
            
            // Hem Admin hem User çalışma sürelerini görüntüleyebilir
            if (!IsUser && !IsAdmin)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            IEnumerable<WorkingHour> workingHours;
            
            if (IsAdmin)
            {
                // Admin tüm kayıtları görür
                workingHours = _workingHourRepo.GetAll();
            }
            else
            {
                // User sadece kendi kayıtlarını görür
                workingHours = _workingHourRepo.GetByUserId(CurrentUserId);
            }

            // Filtreleme uygula
            var originalCount = workingHours.Count();
            System.Diagnostics.Debug.WriteLine($"Filtreleme öncesi kayıt sayısı: {originalCount}");
            
            if (startDate.HasValue)
            {
                workingHours = workingHours.Where(w => w.RecordDate >= startDate.Value);
                System.Diagnostics.Debug.WriteLine($"startDate filtresi uygulandı: {startDate.Value:yyyy-MM-dd}");
            }

            if (endDate.HasValue)
            {
                workingHours = workingHours.Where(w => w.RecordDate <= endDate.Value);
                System.Diagnostics.Debug.WriteLine($"endDate filtresi uygulandı: {endDate.Value:yyyy-MM-dd}");
            }

            if (vehicleId.HasValue && vehicleId.Value > 0)
            {
                workingHours = workingHours.Where(w => w.VehicleId == vehicleId.Value);
                System.Diagnostics.Debug.WriteLine($"vehicleId filtresi uygulandı: {vehicleId.Value}");
            }
            
            var filteredCount = workingHours.Count();
            System.Diagnostics.Debug.WriteLine($"Filtreleme sonrası kayıt sayısı: {filteredCount}");

            // Araç listesini ViewBag'e ekle (filtreleme için)
            try
            {
                var vehicles = _vehicleRepo.GetAll().ToList();
                ViewBag.Vehicles = vehicles;
                
                // Debug için araç sayısını logla
                System.Diagnostics.Debug.WriteLine($"WorkingHourController - Araç sayısı: {vehicles.Count}");
                
                // Eğer araç yoksa uyarı mesajı ekle
                if (!vehicles.Any())
                {
                    TempData["WarningMessage"] = $"Hiç araç bulunamadı (Toplam: {vehicles.Count}). Önce araç eklemeniz gerekiyor. Admin olarak giriş yapıp Vehicle sayfasından araç ekleyebilirsiniz.";
                }
                else
                {
                    // Araç listesini debug için yazdır
                    foreach (var vehicle in vehicles)
                    {
                        System.Diagnostics.Debug.WriteLine($"Araç: {vehicle.VehicleName} - {vehicle.LicensePlate} (ID: {vehicle.VehicleId})");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"WorkingHourController - Araç listesi yüklenirken hata: {ex.Message}");
                ViewBag.Vehicles = new List<Vehicle>();
                TempData["ErrorMessage"] = "Araç listesi yüklenirken hata oluştu: " + ex.Message;
            }
            
            // Filtreleme değerlerini ViewBag'e ekle (View'da değerleri korumak için)
            ViewBag.FilterStartDate = startDate?.ToString("yyyy-MM-dd") ?? "";
            ViewBag.FilterEndDate = endDate?.ToString("yyyy-MM-dd") ?? "";
            ViewBag.FilterVehicleId = vehicleId ?? 0;
            
            // Admin durumunu ViewBag'e ekle
            ViewBag.IsAdmin = IsAdmin;

            return View(workingHours);
        }

        // GET: WorkingHour/Create
        [HttpGet]
        public ActionResult Create()
        {
            // Sadece User'lar çalışma süresi ekleyebilir
            if (!IsUser)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            try
            {
                var vehicles = _vehicleRepo.GetAll();
                if (vehicles == null || !vehicles.Any())
                {
                    TempData["ErrorMessage"] = "Kayıtlı araç bulunamadı. Önce araç eklemeniz gerekiyor.";
                    return RedirectToAction("Index", "Vehicle");
                }

                // ViewBag.Vehicles'ı direkt liste olarak gönder
                ViewBag.Vehicles = vehicles.ToList();

                var model = new WorkingHour
                {
                    RecordDate = DateTime.Today
                };

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Araç listesi yüklenirken hata oluştu: " + ex.Message;
                return RedirectToAction("Index", "Home");
            }
        }

        // POST: WorkingHour/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(WorkingHour workingHour)
        {
            // Sadece User'lar çalışma süresi ekleyebilir
            if (!IsUser)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            // Validation: Total hours should not exceed 24 (daily limit)
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

            ViewBag.Vehicles = _vehicleRepo.GetAll().ToList();
            return View(workingHour);
        }

        // GET: WorkingHour/Edit/5
        [HttpGet]
        public ActionResult Edit(int id)
        {
            // Sadece User'lar çalışma süresi düzenleyebilir
            if (!IsUser)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var workingHour = _workingHourRepo.GetById(id);

            if (workingHour == null)
            {
                return NotFound();
            }

            // User sadece kendi kayıtlarını düzenleyebilir
            if (workingHour.CreatedBy != CurrentUserId)
            {
                TempData["ErrorMessage"] = "Sadece kendi kayıtlarınızı düzenleyebilirsiniz!";
                return RedirectToAction("Index");
            }

            ViewBag.Vehicles = _vehicleRepo.GetAll().ToList();
            return View(workingHour);
        }

        // POST: WorkingHour/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(WorkingHour workingHour)
        {
            // Sadece User'lar çalışma süresi düzenleyebilir
            if (!IsUser)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            // User sadece kendi kayıtlarını düzenleyebilir
            var existingRecord = _workingHourRepo.GetById(workingHour.WorkingHourId);
            if (existingRecord != null && existingRecord.CreatedBy != CurrentUserId)
            {
                TempData["ErrorMessage"] = "Sadece kendi kayıtlarınızı düzenleyebilirsiniz!";
                return RedirectToAction("Index");
            }

            // Validation: Total hours should not exceed 24 (daily limit)
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

            ViewBag.Vehicles = _vehicleRepo.GetAll().ToList();
            return View(workingHour);
        }

        // POST: WorkingHour/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            // Sadece User'lar çalışma süresi silebilir
            if (!IsUser)
            {
                return Json(new { success = false, message = "Yetkiniz yok!" });
            }

            try
            {
                // User sadece kendi kayıtlarını silebilir
                var existingRecord = _workingHourRepo.GetById(id);
                if (existingRecord != null && existingRecord.CreatedBy != CurrentUserId)
                {
                    return Json(new { success = false, message = "Sadece kendi kayıtlarınızı silebilirsiniz!" });
                }

                if (_workingHourRepo.Delete(id, CurrentUserId))
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
            // Details görüntüleme için hem Admin hem User erişebilir
            if (!IsUser && !IsAdmin)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var workingHour = _workingHourRepo.GetById(id);

            if (workingHour == null)
            {
                return NotFound();
            }

            // User sadece kendi kayıtlarını görüntüleyebilir
            if (IsUser && workingHour.CreatedBy != CurrentUserId)
            {
                TempData["ErrorMessage"] = "Sadece kendi kayıtlarınızı görüntüleyebilirsiniz!";
                return RedirectToAction("Index");
            }

            return View(workingHour);
        }
    }
}