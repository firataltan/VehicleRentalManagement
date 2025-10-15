using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using VehicleRentalManagement.DataAccess.Repositories;
using VehicleRentalManagement.Models.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using VehicleRentalManagement.DataAccess;


namespace VehicleRentalManagement.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserRepository _userRepo;

        public AccountController(DatabaseConnection db)
        {
            _userRepo = new UserRepository(db);
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _userRepo.ValidateUser(model.Username, model.Password);

                if (user != null)
                {
                    // Kullanıcı claim'leri oluştur
                    var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim("FullName", user.FullName),
                new Claim(ClaimTypes.Role, user.Role)
            };

                    var claimsIdentity = new ClaimsIdentity(
                        claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = model.RememberMe
                    };

                    // Cookie oluştur ve oturum başlat
                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);

                    // Yönlendirme - query string'den returnUrl'i al
                    var returnUrl = Request.Query["returnUrl"].FirstOrDefault();
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                        return Redirect(returnUrl);

                    return RedirectToAction("Index", "Home");
                }

                TempData["ErrorMessage"] = "Kullanıcı adı veya şifre hatalı!";
            }

            // Hata durumunda returnUrl'i ViewBag'e geri koy
            ViewBag.ReturnUrl = Request.Query["returnUrl"].FirstOrDefault();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            // Çerez tabanlı kimlik doğrulamasını sonlandır
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Gerekirse ekstra oturum temizliği yapılabilir
            HttpContext.Session.Clear();

            // Ana sayfaya yönlendir (returnUrl olmadan)
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
