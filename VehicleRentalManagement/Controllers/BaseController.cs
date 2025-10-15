using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Http;

namespace VehicleRentalManagement.Controllers
{
    [Authorize]
    public class BaseController : Controller
    {
        protected int CurrentUserId
        {
            get { return HttpContext.Session.GetInt32("UserId") ?? 0; }
        }

        protected string CurrentUsername
        {
            get { return HttpContext.Session.GetString("Username") ?? ""; }
        }

        protected string CurrentUserFullName
        {
            get { return HttpContext.Session.GetString("FullName") ?? ""; }
        }

        protected string CurrentUserRole
        {
            get { return HttpContext.Session.GetString("UserRole") ?? ""; }
        }

        protected bool IsAdmin
        {
            get { return CurrentUserRole == "Admin"; }
        }

        protected bool IsUser
        {
            get { return CurrentUserRole == "User"; }
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (CurrentUserId == 0)
            {
                context.Result = new RedirectResult("~/Account/Login");
                return;
            }

            ViewBag.CurrentUserId = CurrentUserId;
            ViewBag.CurrentUsername = CurrentUsername;
            ViewBag.CurrentUserFullName = CurrentUserFullName;
            ViewBag.CurrentUserRole = CurrentUserRole;
            ViewBag.IsAdmin = IsAdmin;

            base.OnActionExecuting(context);
        }
    }
}
