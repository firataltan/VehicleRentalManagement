using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace VehicleRentalManagement.Controllers
{
    [Authorize]
    public class BaseController : Controller
    {
        protected int CurrentUserId
        {
            get 
            { 
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                return userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId) ? userId : 0;
            }
        }

        protected string CurrentUsername
        {
            get { return User.FindFirst(ClaimTypes.Name)?.Value ?? ""; }
        }

        protected string CurrentUserFullName
        {
            get { return User.FindFirst("FullName")?.Value ?? ""; }
        }

        protected string CurrentUserRole
        {
            get { return User.FindFirst(ClaimTypes.Role)?.Value ?? ""; }
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
            // ASP.NET Core'da [Authorize] attribute'u otomatik olarak authentication kontrolü yapar
            // Bu yüzden manuel kontrol gerekmez
            
            ViewBag.CurrentUserId = CurrentUserId;
            ViewBag.CurrentUsername = CurrentUsername;
            ViewBag.CurrentUserFullName = CurrentUserFullName;
            ViewBag.CurrentUserRole = CurrentUserRole;
            ViewBag.IsAdmin = IsAdmin;

            base.OnActionExecuting(context);
        }
    }
}
