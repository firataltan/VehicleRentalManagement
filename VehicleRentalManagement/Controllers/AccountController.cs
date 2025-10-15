using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using VehicleRentalManagement.DataAccess.Repositories;
using VehicleRentalManagement.Models.ViewModels;

namespace VehicleRentalManagement.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserRepository _userRepo;

        public AccountController()
        {
            _userRepo = new UserRepository();
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
        public ActionResult Login(LoginViewModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                var user = _userRepo.ValidateUser(model.Username, model.Password);

                if (user != null)
                {
                    FormsAuthentication.SetAuthCookie(user.Username, model.RememberMe);

                    Session["UserId"] = user.UserId;
                    Session["Username"] = user.Username;
                    Session["FullName"] = user.FullName;
                    Session["UserRole"] = user.Role;

                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }

                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError("", "Kullanıcı adı veya şifre hatalı!");
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Clear();
            Session.Abandon();

            return RedirectToAction("Login", "Account");
        }

        public ActionResult AccessDenied()
        {
            return View();
        }
    }
}
