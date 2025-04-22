using Microsoft.AspNetCore.Mvc;

using Microsoft.EntityFrameworkCore;
using YimYimDental.Data;

namespace YimYimDental.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDBContext _db;

        public AccountController(ApplicationDBContext db)
        {
            _db = db;
        }

        // GET: Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            var role = HttpContext.Session.GetString("Role");

            if (!string.IsNullOrEmpty(role))
            {
                return role switch
                {
                    "Admin" => RedirectToAction("Dashboard", "Admin"),
                    "Officer" => RedirectToAction("Dashboard", "Officer"),
                    "Dentist" => RedirectToAction("Dashboard", "Dentist"),
                    _ => RedirectToAction("AccessDenied")
                };
            }

            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.Email == email && u.Password == password);

            if (user != null)
            {
                HttpContext.Session.SetString("Username", user.Email);
                HttpContext.Session.SetString("Role", user.Role);
                HttpContext.Session.SetString("FullName", user.FullName);

                return user.Role switch
                {
                    "Admin" => RedirectToAction("Dashboard", "Admin"),
                    "Officer" => RedirectToAction("Dashboard", "Officer"),
                    "Dentist" => RedirectToAction("Index", "Queue"),
                    _ => RedirectToAction("Login")
                };
            }

            ModelState.AddModelError("", "ชื่อผู้ใช้หรือรหัสผ่านไม่ถูกต้อง");
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
