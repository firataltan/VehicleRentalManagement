using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Web.Mvc;

namespace VehicleRentalManagement.Controllers
{
    [Authorize]
    public class BaseController : Controller
    {
        protected int CurrentUserId
        {
            get { return Session["UserId"] != null ? (int)Session["UserId"] : 0; }
        }

        protected string CurrentUsername
        {
            get { return Session["Username"]?.ToString() ?? ""; }
        }

        protected string CurrentUserFullName
        {
            get { return Session["FullName"]?.ToString() ?? ""; }
        }

        protected string CurrentUserRole
        {
            get { return Session["UserRole"]?.ToString() ?? ""; }
        }

        protected bool IsAdmin
        {
            get { return CurrentUserRole == "Admin"; }
        }

        protected bool IsUser
        {
            get { return CurrentUserRole == "User"; }
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (CurrentUserId == 0)
            {
                filterContext.Result = new RedirectResult("~/Account/Login");
                return;
            }

            ViewBag.CurrentUserId = CurrentUserId;
            ViewBag.CurrentUsername = CurrentUsername;
            ViewBag.CurrentUserFullName = CurrentUserFullName;
            ViewBag.CurrentUserRole = CurrentUserRole;
            ViewBag.IsAdmin = IsAdmin;

            base.OnActionExecuting(filterContext);
        }
    }
}
