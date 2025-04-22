using Microsoft.AspNetCore.Mvc;

namespace YimYimDental.Controllers
{
    public class DentistController : Controller
    {
        public IActionResult Dashboard()
        {
            var username = HttpContext.Session.GetString("Username");
            var role = HttpContext.Session.GetString("Role");

            if (string.IsNullOrEmpty(role) || role != "Dentist")
                return RedirectToAction("AccessDenied", "Account");

            ViewBag.Username = username;
            ViewBag.Role = role;

            return View();
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
