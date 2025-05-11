using Microsoft.AspNetCore.Mvc;

using Microsoft.EntityFrameworkCore;
using YimYimDental.Data;
using YimYimDental.Models;

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
                HttpContext.Session.SetString("Id", user.Id.ToString());
                HttpContext.Session.SetString("Username", user.Email);
                HttpContext.Session.SetString("Role", user.Role);
                HttpContext.Session.SetString("FullName", user.FullName);
                HttpContext.Session.SetString("Address", user.Address);
                HttpContext.Session.SetString("Phone", user.Phone);

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Account/Edit/{id}")]
        public async Task<IActionResult> Edit(
           int id,
           [Bind("FullName,Address,Phone,Email")] UserViewModel posted,
           string Password,
           string ConfirmPassword)
        {
            Console.WriteLine("1");
            // 1) load the existing user
            var user = await _db.Users.FindAsync(id);
            if (user == null) return NotFound();

            // 2) check for duplicate email (other than current)
            bool emailTaken = await _db.Users
                .AnyAsync(u => u.Email == posted.Email && u.Id != id);
            if (emailTaken)
            {
                
                TempData["DuplicateEmail"] = true;
                return Redirect(Request.Headers["Referer"].ToString());
            }

            // 3) apply changes
            user.FullName = posted.FullName;
            user.Address = posted.Address;
            user.Phone = posted.Phone;
            user.Email = posted.Email;

            // only update password if the modal enabled it and they entered a new one
            if (!string.IsNullOrWhiteSpace(Password))
            {
                if (Password != ConfirmPassword)
                {
                    
                    // you could also set a TempData flag for mismatch if you like
                    TempData["PasswordMismatch"] = true;
                    return Redirect(Request.Headers["Referer"].ToString());
                }
                user.Password = Password;
            }

            _db.Users.Update(user);
            await _db.SaveChangesAsync();

            // 4) refresh session so sidebar & navbar show updated info
            HttpContext.Session.SetString("FullName", user.FullName);
            HttpContext.Session.SetString("Address", user.Address ?? "");
            HttpContext.Session.SetString("Phone", user.Phone ?? "");
            HttpContext.Session.SetString("Username", user.Email);

            // 5) fire your “saved” alert
            TempData["Success"] = true;

            Console.WriteLine("success");

            // 6) go back to wherever they were
            return Redirect(Request.Headers["Referer"].ToString());
        }
    }
}
