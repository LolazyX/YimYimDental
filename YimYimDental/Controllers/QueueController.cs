using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YimYimDental.Data;
using YimYimDental.Models;

namespace YimYimDental.Controllers
{
    public class QueueController : Controller
    {
        private readonly ApplicationDBContext _db;

        public QueueController(ApplicationDBContext db)
        {
            _db = db;
        }

        // GET: Queue/Index
        public IActionResult Index(string? date)
        {
            var username = HttpContext.Session.GetString("Username");
            var role = HttpContext.Session.GetString("Role");

            if (string.IsNullOrEmpty(role))
                return RedirectToAction("AccessDenied", "Account");

            ViewBag.Username = username;
            ViewBag.Role = role;

            DateTime selectedDate;
            if (!DateTime.TryParse(date, out selectedDate))
            {
                selectedDate = DateTime.Now;
            }

            ViewBag.SelectedDate = selectedDate.ToString("yyyy-MM-dd");

            var queues = _db.TreatmentQueues
                .Include(q => q.Customer) // โหลดข้อมูล Customer พร้อมกัน
                .Where(q => q.AppointmentTime.Date == selectedDate.Date)
                .OrderBy(q => q.AppointmentTime)
                .ToList();

            return View(queues);
        }

        // GET: Queue/Create
        public IActionResult Create(string? customerHN)
        {
            var username = HttpContext.Session.GetString("Username");
            var role = HttpContext.Session.GetString("Role");

            if (string.IsNullOrEmpty(role))
                return RedirectToAction("AccessDenied", "Account");

            ViewBag.Username = username;
            ViewBag.Role = role;
            // หากต้องการเลือกลูกค้า คุณสามารถส่ง ViewBag.Customers เพื่อเลือกลูกค้า
            ViewBag.Customers = _db.Customers.ToList();
            ViewBag.CustomerHN = customerHN;
            return View();
        }

        // POST: Queue/Create
        [HttpPost]
        public IActionResult Create(TreatmentQueue queue)
        {
            var username = HttpContext.Session.GetString("Username");
            var role = HttpContext.Session.GetString("Role");

            if (string.IsNullOrEmpty(role))
                return RedirectToAction("AccessDenied", "Account");

            ViewBag.Username = username;
            ViewBag.Role = role;

            if (ModelState.IsValid)
            {

                var maxQueue = _db.TreatmentQueues.Any() ? _db.TreatmentQueues.Max(q => q.QueueNumber) : 0;
                queue.QueueNumber = maxQueue + 1;

                _db.TreatmentQueues.Add(queue);
                _db.SaveChanges();

                TempData["Success"] = "เพิ่มคิวการรักษาสำเร็จแล้ว";
                return RedirectToAction("Index");
            }

            // กรณี ModelState ไม่ถูกต้อง ให้โหลดข้อมูลลูกค้าใหม่และแสดงฟอร์มพร้อมข้อผิดพลาด
            ViewBag.Customers = _db.Customers.ToList();
            return View(queue); // เปลี่ยนจาก RedirectToAction เป็น return View
        }

        // GET: Queue/Edit/{id}
        public IActionResult Edit(int id)
        {
            var queue = _db.TreatmentQueues.Find(id);
            if (queue == null)
            {
                return NotFound();
            }
            ViewBag.Customers = _db.Customers.ToList();
            return View(queue);
        }

        // POST: Queue/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(TreatmentQueue queue)
        {
            if (ModelState.IsValid)
            {
                _db.TreatmentQueues.Update(queue);
                _db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.Customers = _db.Customers.ToList();
            return View(queue);
        }

        // POST: Queue/UpdateStatus/{id}
        // ใช้เปลี่ยนสถานะของคิว เช่น จาก "รอรักษา" เป็น "กำลังรักษา" หรือ "รักษาเสร็จแล้ว"
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateStatus(int id, string status)
        {
            var queue = _db.TreatmentQueues.Find(id);
            if (queue == null)
            {
                return NotFound();
            }
            queue.Status = status;
            _db.SaveChanges();
            return RedirectToAction("Index");
        }

        // GET: Queue/Delete/{id}
        public IActionResult Delete(int id)
        {
            var queue = _db.TreatmentQueues
                .Include(q => q.Customer)
                .FirstOrDefault(q => q.Id == id);
            if (queue == null)
            {
                return NotFound();
            }
            return View(queue);
        }

        // POST: Queue/Delete/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var queue = _db.TreatmentQueues.Find(id);
            if (queue == null)
            {
                return NotFound();
            }
            _db.TreatmentQueues.Remove(queue);
            _db.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult Timeline(string date)
        {
            // ดึงข้อมูล session สำหรับ username และ role
            var username = HttpContext.Session.GetString("Username");
            var role = HttpContext.Session.GetString("Role");

            // ตรวจสอบสิทธิ์การเข้าถึง (ถ้า role เป็น null หรือว่างจะ redirect ไปหน้า AccessDenied)
            if (string.IsNullOrEmpty(role))
                return RedirectToAction("AccessDenied", "Account");

            ViewBag.Username = username;
            ViewBag.Role = role;

            // แปลงค่า date ที่รับเข้ามาจาก URL เป็น DateTime
            // ถ้าไม่สามารถแปลงได้ (หรือไม่มีการส่งพารามิเตอร์มา) จะใช้วันที่ปัจจุบันแทน
            DateTime selectedDate;
            if (!DateTime.TryParse(date, out selectedDate))
            {
                selectedDate = DateTime.Now;
            }

            // ดึงข้อมูลคิวการรักษา กรองเฉพาะวันที่ที่เลือก และเรียงลำดับตามเวลา
            var queues = _db.TreatmentQueues
                 .Include(q => q.Customer) // โหลดข้อมูล Customer พร้อมกัน
                 .Where(q => q.AppointmentTime.Date == selectedDate.Date)
                 .OrderBy(q => q.AppointmentTime)
                 .ToList();

            // ส่งค่า selectedDate ไปยัง View ด้วย ViewBag เพื่อนำไปแสดงใน date picker
            ViewBag.SelectedDate = selectedDate.ToString("yyyy-MM-dd");

            return View(queues);
        }

    }
}


