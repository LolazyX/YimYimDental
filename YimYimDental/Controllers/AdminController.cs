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

        public IActionResult SystemUser(string? search, string? roleFilter)
        {
            var role = HttpContext.Session.GetString("Role");
            if (string.IsNullOrEmpty(role) || role != "Admin")
                return RedirectToAction("AccessDenied", "Account");

            ViewBag.Search = search;
            ViewBag.RoleFilter = roleFilter;
            ViewBag.Username = HttpContext.Session.GetString("Username");
            ViewBag.Role = role;

            var users = _db.Users
                .Where(u => u.Role != "Admin")
                .Where(u => string.IsNullOrEmpty(search) || u.FullName.Contains(search) || u.Email.Contains(search))
                .Where(u => string.IsNullOrEmpty(roleFilter) || u.Role == roleFilter)
                .OrderBy(u => u.FullName)
                .ToList();

            return View(users);
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
                return RedirectToAction("SystemUser");
            }

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, UserViewModel updatedUser)
        {
            Console.WriteLine("id = " + id);
            // 1. ตรวจสิทธิ์เหมือนเดิม…
            var userInDb = _db.Users.Find(id);
            if (userInDb == null) return NotFound();

            // 2. ถ้า Password field ว่าง ให้ลบ Validation นี้ออก
            if (string.IsNullOrWhiteSpace(updatedUser.Password))
            {
                ModelState.Remove(nameof(updatedUser.Password));
                ModelState.Remove(nameof(updatedUser.ConfirmPassword));
            }

            // 3. เช็ค ModelState อีกครั้ง
            if (ModelState.IsValid)
            {
                // อัปเดตข้อมูลที่แก้ได้
                userInDb.FullName = updatedUser.FullName;
                userInDb.Email = updatedUser.Email;
                userInDb.Phone = updatedUser.Phone;
                userInDb.Address = updatedUser.Address;
                userInDb.Role = updatedUser.Role;

                // 4. ถ้ามีการกรอกรหัสผ่านใหม่ จึงค่อยอัปเดต
                if (!string.IsNullOrWhiteSpace(updatedUser.Password))
                {
                    userInDb.Password = updatedUser.Password;
                }

                _db.SaveChanges();
                TempData["Success"] = true;
                return RedirectToAction("SystemUser");
            }

            // 5. ถ้าไม่ผ่าน ให้เก็บ error ไว้แจ้งผู้ใช้ (หรือ redirect กลับพร้อม ModelState)
            TempData["EditError"] = ModelState.Values
                                              .SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();
            return View(updatedUser);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var role = HttpContext.Session.GetString("Role");
            if (string.IsNullOrEmpty(role) || role != "Admin")
                return RedirectToAction("AccessDenied", "Account");

            var user = _db.Users.FirstOrDefault(u => u.Id == id);
            if (user == null)
                return NotFound();

            _db.Users.Remove(user);
            _db.SaveChanges();

            return RedirectToAction("SystemUser");
        }
    }
}
