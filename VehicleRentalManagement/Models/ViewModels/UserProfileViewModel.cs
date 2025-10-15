using System;
using System.ComponentModel.DataAnnotations;

namespace VehicleRentalManagement.Models.ViewModels
{
    public class UserProfileViewModel
    {
        public int UserId { get; set; }

        [Display(Name = "Kullanıcı Adı")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Ad Soyad gereklidir")]
        [StringLength(100)]
        [Display(Name = "Ad Soyad")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "E-posta gereklidir")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta giriniz")]
        [Display(Name = "E-posta")]
        public string Email { get; set; }

        [Display(Name = "Rol")]
        public string Role { get; set; }

        [Display(Name = "Kayıt Tarihi")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Son Giriş")]
        public DateTime? LastLoginDate { get; set; }

        // Şifre değiştirme için (opsiyonel)
        [DataType(DataType.Password)]
        [Display(Name = "Mevcut Şifre")]
        public string CurrentPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Yeni Şifre")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Şifre en az 6 karakter olmalıdır")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Yeni Şifre (Tekrar)")]
        [Compare("NewPassword", ErrorMessage = "Şifreler eşleşmiyor")]
        public string ConfirmPassword { get; set; }
    }
}