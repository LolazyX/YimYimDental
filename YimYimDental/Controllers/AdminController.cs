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
            // เช็ค session Role ว่าเป็น Admin เท่านั้น
            var role = HttpContext.Session.GetString("Role");
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(role) || role != "Admin")
                return RedirectToAction("AccessDenied", "Account");

            // เริ่มด้วย query ผู้ใช้ที่ไม่ใช่ Admin
            var usersQuery = _db.Users
                .Where(u => u.Role != "Admin")
                .AsQueryable();

            // กรองด้วย search (FullName หรือ Email)
            if (!string.IsNullOrEmpty(search))
            {
                usersQuery = usersQuery
                    .Where(u => u.FullName.Contains(search) ||
                                u.Email.Contains(search));
            }

            // กรองด้วยบทบาทที่เลือก (Dentist หรือ Officer)
            if (!string.IsNullOrEmpty(roleFilter))
            {
                usersQuery = usersQuery
                    .Where(u => u.Role == roleFilter);
            }

            // สร้าง ViewModel และสั่ง Order ตามชื่อ
            var model = usersQuery
                .OrderBy(u => u.FullName)
                .Select(u => new UserViewModel
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email,
                    Phone = u.Phone,
                    DateOfBirth = u.DateOfBirth,
                    Position = u.Position,
                    Address = u.Address,
                    Password = u.Password,
                    Role = u.Role
                })
                .ToList();

            // เก็บค่าที่กรองไว้ใน ViewBag เพื่อใช้แสดงในฟิลด์ฟอร์ม
            ViewBag.Username = username;
            ViewBag.Role = role;
            ViewBag.Search = search;
            ViewBag.RoleFilter = roleFilter;

            return View(model);
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
            var role = HttpContext.Session.GetString("Role");
            if (string.IsNullOrEmpty(role) || role != "Admin")
                return RedirectToAction("AccessDenied", "Account");

            var userInDb = _db.Users.FirstOrDefault(u => u.Id == id);
            if (userInDb == null)
                return NotFound();

            if (ModelState.IsValid)
            {
                // อัปเดตข้อมูล
                userInDb.FullName = updatedUser.FullName;
                userInDb.Email = updatedUser.Email;
                userInDb.Phone = updatedUser.Phone;
                userInDb.DateOfBirth = updatedUser.DateOfBirth;
                userInDb.Position = updatedUser.Position;
                userInDb.Address = updatedUser.Address;
                userInDb.Password = updatedUser.Password;
                userInDb.Role = updatedUser.Role;

                _db.SaveChanges();

                return RedirectToAction("SystemUser");
            }

            return View("SystemUser", _db.Users.Where(u => u.Role == "Dentist").ToList());
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
