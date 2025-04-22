using Microsoft.AspNetCore.Mvc;
using YimYimDental.Data;
using YimYimDental.Models;

[Route("Admin/Officer")]
public class AdminOfficerController : Controller
{
    private readonly ApplicationDBContext _db;

    public AdminOfficerController(ApplicationDBContext db)
    {
        _db = db;
    }

    public IActionResult Index()
    {
        var role = HttpContext.Session.GetString("Role");
        var username = HttpContext.Session.GetString("Username");

        if (string.IsNullOrEmpty(role) || role != "Admin")
            return RedirectToAction("AccessDenied", "Account");

        IEnumerable<UserViewModel> allDentist = _db.Users.Where(u => u.Role == "Officer");

        ViewBag.Username = username;
        ViewBag.Role = role;

        return View(allDentist);
    }

    [HttpGet("Create")]
    public IActionResult OfficerCreate()
    {
        var role = HttpContext.Session.GetString("Role");
        var username = HttpContext.Session.GetString("Username");

        if (string.IsNullOrEmpty(role) || role != "Admin")
            return RedirectToAction("AccessDenied", "Account");

        ViewBag.Username = username;
        ViewBag.Role = role;

        return View(); // มองหา Views/AdminOfficer/OfficerCreate.cshtml
    }

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public IActionResult OfficerCreate(UserViewModel user)
    {
        var role = HttpContext.Session.GetString("Role");
        if (string.IsNullOrEmpty(role) || role != "Admin")
            return RedirectToAction("AccessDenied", "Account");

        if (ModelState.IsValid)
        {
            user.Role = "Officer"; // กำหนดบทบาทเป็นเจ้าหน้าที่
            _db.Users.Add(user);
            _db.SaveChanges();

            return RedirectToAction("Index"); // กลับไปหน้าแดชบอร์ด
        }

        return View(user);
    }

    [HttpPost("Edit/{id}")]
    [ValidateAntiForgeryToken]
    public IActionResult DentistEdit(int id, UserViewModel updatedUser)
    {
        var role = HttpContext.Session.GetString("Role");
        if (string.IsNullOrEmpty(role) || role != "Admin")
            return RedirectToAction("AccessDenied", "Account");

        var userInDb = _db.Users.FirstOrDefault(u => u.Id == id && u.Role == "Officer");
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

            return RedirectToAction("Index");
        }

        return View("Index", _db.Users.Where(u => u.Role == "Dentist").ToList());
    }

    [HttpPost("Delete/{id}")]
    [ValidateAntiForgeryToken]
    public IActionResult DentistDelete(int id)
    {
        var role = HttpContext.Session.GetString("Role");
        if (string.IsNullOrEmpty(role) || role != "Admin")
            return RedirectToAction("AccessDenied", "Account");

        var user = _db.Users.FirstOrDefault(u => u.Id == id && u.Role == "Officer");
        if (user == null)
            return NotFound();

        _db.Users.Remove(user);
        _db.SaveChanges();

        return RedirectToAction("Index");
    }
}
