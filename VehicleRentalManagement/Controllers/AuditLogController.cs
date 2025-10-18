using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using VehicleRentalManagement.DataAccess;
using VehicleRentalManagement.DataAccess.Repositories;

namespace VehicleRentalManagement.Controllers
{
    [Authorize]
    public class AuditLogController : BaseController
    {
        private readonly AuditLogRepository _auditLogRepo;

        public AuditLogController(DatabaseConnection db)
        {
            _auditLogRepo = new AuditLogRepository(db.ConnectionString);
        }

        // GET: AuditLog
        public ActionResult Index(string tableName = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            // Sadece Admin'ler audit log'ları görüntüleyebilir
            if (!IsAdmin)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            try
            {
                // Varsayılan tarih aralığı: Son 30 gün
                if (!startDate.HasValue)
                {
                    startDate = DateTime.Today.AddDays(-30);
                }

                if (!endDate.HasValue)
                {
                    endDate = DateTime.Today;
                }

                var logs = _auditLogRepo.GetAll(1000, tableName, startDate, endDate);

                // Filtreleme parametrelerini View'a gönder
                ViewBag.TableName = tableName;
                ViewBag.StartDate = startDate.Value.ToString("yyyy-MM-dd");
                ViewBag.EndDate = endDate.Value.ToString("yyyy-MM-dd");

                // İstatistik bilgileri
                ViewBag.Statistics = _auditLogRepo.GetStatistics();

                return View(logs);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Audit log'ları yüklenirken hata oluştu: " + ex.Message;
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: AuditLog/Details/5 - Belirli bir kayıt için tüm audit log'ları
        public ActionResult Details(string tableName, int recordId)
        {
            // Sadece Admin'ler audit log'ları görüntüleyebilir
            if (!IsAdmin)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            try
            {
                var logs = _auditLogRepo.GetByRecord(tableName, recordId);

                ViewBag.TableName = tableName;
                ViewBag.RecordId = recordId;

                return View(logs);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Audit log detayları yüklenirken hata oluştu: " + ex.Message;
                return RedirectToAction("Index");
            }
        }
    }
}

