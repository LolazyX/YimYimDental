using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YimYimDental.Data;
using YimYimDental.Models;

namespace YimYimDental.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDBContext _db;

        public AdminController(ApplicationDBContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> DashboardAsync()
        {
            var username = HttpContext.Session.GetString("Username");
            var role = HttpContext.Session.GetString("Role");

            if (string.IsNullOrEmpty(role) || role != "Admin")
                return RedirectToAction("AccessDenied", "Account");

            // Count the total number of users
            var totalUsersCount = await _db.Users.CountAsync();

            var adminCount = await _db.Users.CountAsync(u => u.Role == "Admin");

            // Count the number of users with "dentist" role
            var dentistCount = await _db.Users.CountAsync(u => u.Role == "Dentist");

            // Count the number of users with "officer" role
            var officerCount = await _db.Users.CountAsync(u => u.Role == "Officer");

            var customerCount = await _db.Customers.CountAsync();
            

            // Pass the counts to the view
            ViewBag.Username = username;
            ViewBag.Role = role;
            ViewBag.TotalUsersCount = totalUsersCount + customerCount;
            ViewBag.AdminCount = adminCount;
            ViewBag.DentistCount = dentistCount;
            ViewBag.OfficerCount = officerCount;
            ViewBag.CustomerCount = customerCount;

            return View();
        }

        public IActionResult Create(string? roleType)
        {
            var username = HttpContext.Session.GetString("Username");
            var role = HttpContext.Session.GetString("Role");
            if (string.IsNullOrEmpty(role) || role != "Admin")
                return RedirectToAction("AccessDenied", "Account");

            ViewBag.Username = username;
            ViewBag.Role = role;
            ViewBag.NewUserRole = roleType;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(UserViewModel user)
        {
            var role = HttpContext.Session.GetString("Role");
            if (string.IsNullOrEmpty(role) || role != "Admin")
                return RedirectToAction("AccessDenied", "Account");

            // ตรวจสอบอีเมลซ้ำ
            if (_db.Users.Any(u => u.Email == user.Email))
            {
                TempData["DuplicateEmail"] = true;
                return View(user);
            }


            if (ModelState.IsValid)
            {
                _db.Users.Add(user);
                _db.SaveChanges();

                TempData["Success"] = true;
                return RedirectToAction("Dashboard");
            }

            return View(user);
        }
    }
}
