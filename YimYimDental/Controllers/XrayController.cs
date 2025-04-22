using Microsoft.AspNetCore.Mvc;
using YimYimDental.Data;
using YimYimDental.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace YimYimDental.Controllers
{
    public class XrayController : Controller
    {
        private readonly ApplicationDBContext _db;
        private readonly IWebHostEnvironment _env;

        public XrayController(ApplicationDBContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        // GET: Xray/Index
        public IActionResult Index(string search)
        {
            var role = HttpContext.Session.GetString("Role");

            if (string.IsNullOrEmpty(role))
                return RedirectToAction("AccessDenied", "Account");

            var xrays = _db.Xrays.Include(x => x.Customer)
                     .OrderByDescending(x => x.CreatedAt)
                     .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                xrays = xrays.Where(x => x.Customer!.HN.Contains(search));
            }

            return View(xrays.ToList());
        }

        // GET: Xray/Create
        public IActionResult Create()
        {
            var role = HttpContext.Session.GetString("Role");

            if (string.IsNullOrEmpty(role))
                return RedirectToAction("AccessDenied", "Account");
            // เตรียม dropdown ลูกค้า
            ViewBag.Customers = _db.Customers.OrderBy(c => c.HN).ToList();
            return View(new Xray());
        }

        // POST: Xray/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Xray model, IFormFile ImagePath)
        {
            foreach (var key in ModelState.Keys)
            {
                var state = ModelState[key];
                foreach (var error in state!.Errors)
                {
                    Console.WriteLine($"[ModelState Error] {key} => {error.ErrorMessage}");
                }
            }

            // เช็คก่อนแปลง
            if (ImagePath == null || ImagePath.Length == 0)
            {
                ViewBag.Customers = _db.Customers.OrderBy(c => c.HN).ToList();
                ModelState.AddModelError("ImagePath", "กรุณาเลือกไฟล์ภาพเอ็กซ์เรย์");
                return View(model);
            }


            // อัปโหลดและเซ็ต path
            var uploads = Path.Combine(_env.WebRootPath, "uploads", "xrays");
            if (!Directory.Exists(uploads))
                Directory.CreateDirectory(uploads);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(ImagePath.FileName)}";
            var filePath = Path.Combine(uploads, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                ImagePath.CopyTo(stream);
            }

            model.ImagePath = $"/uploads/xrays/{fileName}";
            model.CreatedAt = DateTime.Now;

            // จากนั้น validate Model อีกครั้ง (ถ้าอยากใช้)
            TryValidateModel(model);

            if (!ModelState.IsValid)
            {
                ViewBag.Customers = _db.Customers.OrderBy(c => c.HN).ToList();
                return View(model);
            }

            _db.Xrays.Add(model);
            _db.SaveChanges();
            TempData["Success"] = "บันทึกภาพสำเร็จแล้ว";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var xray = _db.Xrays.FirstOrDefault(x => x.Id == id);
            if (xray == null)
            {
                return NotFound();
            }

            // ลบไฟล์จริงออกจากระบบด้วย
            var fullPath = Path.Combine(_env.WebRootPath, xray.ImagePath.TrimStart('/'));
            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }

            _db.Xrays.Remove(xray);
            _db.SaveChanges();

            return RedirectToAction("Index");
        }

    }
}
