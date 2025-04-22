using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YimYimDental.Data;
using YimYimDental.Models;

namespace YimYimDental.Controllers
{
    public class OfficerController : Controller
    {
        private readonly ApplicationDBContext _db;

        public OfficerController(ApplicationDBContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> DashboardAsync()
        {
            var username = HttpContext.Session.GetString("Username");
            var role = HttpContext.Session.GetString("Role");

            if (string.IsNullOrEmpty(role) || role != "Officer")
                return RedirectToAction("AccessDenied", "Account");

            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            // ดึงข้อมูลที่ต้องใช้
            var totalCustomers = await _db.Customers.CountAsync();
            var todayQueueCount = await _db.TreatmentQueues.CountAsync(q => q.AppointmentTime.Date == today);
            var tomorrowQueueCount = await _db.TreatmentQueues.CountAsync(q => q.AppointmentTime == tomorrow);

            var todayDentists = await _db.WorkingTimes
                .Where(w => w.Start.Date == today)
                .OrderBy(w => w.Start)
                .ToListAsync();

            ViewBag.Username = username;
            ViewBag.Role = role;
            ViewBag.TotalCustomers = totalCustomers;
            ViewBag.TodayQueue = todayQueueCount;
            ViewBag.TomorrowQueue = tomorrowQueueCount;
            ViewBag.TodayDentists = todayDentists;

            return View();
        }

        public IActionResult CustomerCreate()
        {
            var username = HttpContext.Session.GetString("Username");
            var role = HttpContext.Session.GetString("Role");
            if (string.IsNullOrEmpty(role) || role == "Dentist")
                return RedirectToAction("AccessDenied", "Account");
            ViewBag.Username = username;
            ViewBag.Role = role;
            return View();
        }

        public IActionResult Billing()
        {
            // ตรวจสอบสิทธิ์การเข้าถึง
            var username = HttpContext.Session.GetString("Username");
            var role = HttpContext.Session.GetString("Role");

            if (string.IsNullOrEmpty(role))
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            // ดึงข้อมูลบริการ (Service) และอุปกรณ์ (Equipment) จากฐานข้อมูล
            var services = _db.Services.ToList();
            var equipments = _db.Equipments.ToList();

            // ส่งข้อมูลไปยัง View
            ViewBag.Username = username;
            ViewBag.Role = role;
            ViewBag.Services = services;
            ViewBag.Equipments = equipments;  // ส่งข้อมูล Equipment

            return View();
        }

        [HttpPost]
        public IActionResult Billing(List<ServiceItem> selectedServices)
        {
            if (selectedServices == null || !selectedServices.Any())
            {
                ModelState.AddModelError("", "กรุณาเลือกบริการอย่างน้อย 1 รายการ");
            }

            if (ModelState.IsValid)
            {
                // คำนวณราคารวม (ตัวอย่างการคำนวณ)
                decimal totalPrice = selectedServices!.Sum(s => s.Quantity * s.Price);

                // นำไปใช้งานต่อ เช่น บันทึกฐานข้อมูล หรือแสดงผล
                ViewBag.TotalPrice = totalPrice;
                return View("CalculationResult");
            }

            ViewBag.Services = _db.Services.ToList();
            return View();
        }

        public class ServiceItem
        {
            public int ServiceId { get; set; }
            public string ServiceName { get; set; }
            public int Quantity { get; set; }
            public decimal Price { get; set; }
        }

        // ✅ หน้าแสดงบริการทั้งหมด
        public IActionResult Service()
        {
            var username = HttpContext.Session.GetString("Username");
            var role = HttpContext.Session.GetString("Role");

            if (string.IsNullOrEmpty(role))
                return RedirectToAction("AccessDenied", "Account");

            ViewBag.Username = username;
            ViewBag.Role = role;
            var services = _db.Services.ToList();
            return View(services);
        }

        // ✅ GET: เพิ่มบริการ
        public IActionResult ServiceCreate()
        {
            var username = HttpContext.Session.GetString("Username");
            var role = HttpContext.Session.GetString("Role");

            if (string.IsNullOrEmpty(role))
                return RedirectToAction("AccessDenied", "Account");

            ViewBag.Username = username;
            ViewBag.Role = role;
            return View();
        }

        // ✅ POST: เพิ่มบริการ
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ServiceCreate(Service service)
        {
            if (ModelState.IsValid)
            {
                _db.Services.Add(service);
                _db.SaveChanges();
                TempData["Success"] = true;
                return RedirectToAction(nameof(Service));
            }

            return View(service);
        }

        // ✅ GET: แก้ไขบริการ
        public IActionResult ServiceEdit(int id)
        {
            var service = _db.Services.Find(id);
            if (service == null)
                return NotFound();

            return View(service);
        }

        // Action สำหรับการแก้ไขบริการ
        [HttpPost]
        public IActionResult ServiceEdit(int id, Service service)
        {
            if (ModelState.IsValid)
            {
                var existingService = _db.Services.FirstOrDefault(s => s.Id == id);
                if (existingService != null)
                {
                    existingService.Name = service.Name;
                    existingService.Price = service.Price;

                    _db.Update(existingService);
                    _db.SaveChanges();
                }

                return RedirectToAction("Service"); // เปลี่ยนไปที่หน้าแสดงบริการ
            }

            return View(service);
        }

        // ✅ POST: ลบบริการ
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ServiceDelete(int id)
        {
            var service = _db.Services.Find(id);
            if (service == null)
                return NotFound();

            _db.Services.Remove(service);
            _db.SaveChanges();
            return RedirectToAction(nameof(Service));
        }

        // ✅ หน้าแสดงอุปกรณ์ทั้งหมด
        public IActionResult Equipment()
        {
            var username = HttpContext.Session.GetString("Username");
            var role = HttpContext.Session.GetString("Role");

            if (string.IsNullOrEmpty(role))
                return RedirectToAction("AccessDenied", "Account");

            ViewBag.Username = username;
            ViewBag.Role = role;
            var equipments = _db.Equipments.ToList(); // แก้ชื่อ DbSet ด้วย
            return View(equipments);
        }

        // ✅ GET: เพิ่มอุปกรณ์
        public IActionResult EquipmentCreate()
        {
            var username = HttpContext.Session.GetString("Username");
            var role = HttpContext.Session.GetString("Role");

            if (string.IsNullOrEmpty(role))
                return RedirectToAction("AccessDenied", "Account");

            ViewBag.Username = username;
            ViewBag.Role = role;
            return View();
        }

        // ✅ POST: เพิ่มอุปกรณ์
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EquipmentCreate(Equipment equipment)
        {
            if (ModelState.IsValid)
            {
                _db.Equipments.Add(equipment);
                _db.SaveChanges();
                TempData["Success"] = true;
                return RedirectToAction(nameof(Equipment));
            }

            return View(equipment);
        }

        // ✅ GET: แก้ไขอุปกรณ์
        public IActionResult EquipmentEdit(int id)
        {
            var equipment = _db.Equipments.Find(id);
            if (equipment == null)
                return NotFound();

            return View(equipment);
        }

        // ✅ POST: แก้ไขอุปกรณ์
        [HttpPost]
        public IActionResult EquipmentEdit(int id, Equipment equipment)
        {
            if (ModelState.IsValid)
            {
                var existing = _db.Equipments.FirstOrDefault(e => e.Id == id);
                if (existing != null)
                {
                    existing.Name = equipment.Name;
                    existing.Price = equipment.Price;

                    _db.Update(existing);
                    _db.SaveChanges();
                }

                return RedirectToAction("Equipment");
            }

            return View(equipment);
        }

        // ✅ POST: ลบอุปกรณ์
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EquipmentDelete(int id)
        {
            var equipment = _db.Equipments.Find(id);
            if (equipment == null)
                return NotFound();

            _db.Equipments.Remove(equipment);
            _db.SaveChanges();
            return RedirectToAction(nameof(Equipment));
        }


        public IActionResult WorkingTime()
        {
            var username = HttpContext.Session.GetString("Username");
            var role = HttpContext.Session.GetString("Role");
            if (string.IsNullOrEmpty(role))
                return RedirectToAction("AccessDenied", "Account");
            ViewBag.Username = username;
            ViewBag.Role = role;

            // โหลดรายชื่อทันตแพทย์สำหรับ dropdown ใน modal แก้ไข
            ViewBag.Dentists = _db.Users
                .Where(u => u.Role == "dentist")
                .OrderBy(u => u.FullName)
                .ToList();

            return View();
        }

        public IActionResult WorkingTimeCreate()
        {
            var username = HttpContext.Session.GetString("Username");
            var role = HttpContext.Session.GetString("Role");
            if (string.IsNullOrEmpty(role))
                return RedirectToAction("AccessDenied", "Account");
            var dentists = _db.Users
                .Where(c => c.Role == "dentist")
                .OrderBy(c => c.FullName)
                .ToList();

            ViewBag.Dentists = dentists;
            ViewBag.Username = username;
            ViewBag.Role = role;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult WorkingTimeCreate(WorkingTime model)
        {
            var username = HttpContext.Session.GetString("Username");
            var role = HttpContext.Session.GetString("Role");
            if (string.IsNullOrEmpty(role))
                return RedirectToAction("AccessDenied", "Account");

            if (ModelState.IsValid)
            {
                _db.WorkingTimes.Add(model);
                _db.SaveChanges();
                TempData["Success"] = true;
                return RedirectToAction("WorkingTime"); // เปลี่ยนตามชื่อหน้ารายการถ้ามี
            }

            // โหลดทันตแพทย์ใหม่ในกรณีฟอร์มไม่ผ่าน validation
            var dentists = _db.Users
                .Where(c => c.Role == "dentist")
                .OrderBy(c => c.FullName)
                .ToList();

            ViewBag.Dentists = dentists;
            ViewBag.Username = username;
            ViewBag.Role = role;

            return View(model);
        }

        public JsonResult GetWorkingTimes()
        {
            var events = _db.WorkingTimes.Select(wt => new
            {
                id = wt.Id,
                title = wt.DentistName,
                // รูปแบบ ISO 8601 เพื่อให้ FullCalendar เข้าใจ
                start = wt.Start.ToString("s"),
                end = wt.End.ToString("s")
            }).ToList();

            return Json(events);
        }

        // POST: ลบเวลาการทำงาน (WorkingTimeDelete)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult WorkingTimeDelete(int id)
        {
            var workingTime = _db.WorkingTimes.Find(id);
            if (workingTime == null)
                return NotFound();

            _db.WorkingTimes.Remove(workingTime);
            _db.SaveChanges();

            return RedirectToAction("WorkingTime");
        }
    }
}
