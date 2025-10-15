using System;
using System.ComponentModel.DataAnnotations;

namespace VehicleRentalManagement.Models
{
    public class User
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "Kullanıcı adı gereklidir")]
        [StringLength(50, ErrorMessage = "Kullanıcı adı en fazla 50 karakter olabilir")]
        [Display(Name = "Kullanıcı Adı")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Şifre gereklidir")]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        public string PasswordHash { get; set; }

        [Required(ErrorMessage = "Ad Soyad gereklidir")]
        [StringLength(100, ErrorMessage = "Ad Soyad en fazla 100 karakter olabilir")]
        [Display(Name = "Ad Soyad")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "E-posta gereklidir")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
        [Display(Name = "E-posta")]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Rol")]
        public string Role { get; set; } // Admin, User

        [Display(Name = "Aktif")]
        public bool IsActive { get; set; }

        [Display(Name = "Oluşturulma Tarihi")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Son Giriş Tarihi")]
        public DateTime? LastLoginDate { get; set; }
    }
}