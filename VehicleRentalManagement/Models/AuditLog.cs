using System;
using System.ComponentModel.DataAnnotations;

namespace VehicleRentalManagement.Models
{
    public class AuditLog
    {
        public int LogId { get; set; }

        [Display(Name = "Kullanıcı ID")]
        public int UserId { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "İşlem")]
        public string Action { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Tablo")]
        public string TableName { get; set; }

        [Display(Name = "Kayıt ID")]
        public int RecordId { get; set; }

        [Display(Name = "Eski Değer")]
        public string OldValue { get; set; }

        [Display(Name = "Yeni Değer")]
        public string NewValue { get; set; }

        [StringLength(50)]
        [Display(Name = "IP Adresi")]
        public string IPAddress { get; set; }

        [Display(Name = "Log Tarihi")]
        public DateTime LogDate { get; set; }

        // Navigation property
        [Display(Name = "Kullanıcı")]
        public string UserName { get; set; }
    }
}
