using System.Data;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YimYimDental.Data;
using YimYimDental.Models;

namespace YimYimDental.Controllers
{
    public class CustomerController : Controller
    {
        private readonly ApplicationDBContext _db;

        public CustomerController(ApplicationDBContext db)
        {
            _db = db;
        }

        // GET: Customer/Index?search=...
        public IActionResult Index(string search)
        {
            var username = HttpContext.Session.GetString("Username");
            var role = HttpContext.Session.GetString("Role");

            if (string.IsNullOrEmpty(role))
                return RedirectToAction("AccessDenied", "Account");

            ViewBag.Username = username;
            ViewBag.Role = role;
            var customers = _db.Customers.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                customers = customers.Where(c => c.HN.Contains(search));
            }

            customers = customers.OrderBy(c => c.HN);

            return View(customers.ToList());
        }

        // POST: Customer/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Customer customer)
        {
            // สร้าง HN อัตโนมัติ
            var lastCustomer = _db.Customers
                .OrderByDescending(c => c.Id)
                .FirstOrDefault();

            int lastHNNumber = 0;
            if (lastCustomer != null && int.TryParse(lastCustomer.HN, out int parsedHN))
            {
                lastHNNumber = parsedHN;
            }

            int newHNNumber = lastHNNumber + 1;
            customer.HN = newHNNumber.ToString("D6"); // เติม 0 ข้างหน้า เช่น 000001

            // ลบ validation error ของ HN เพราะกำหนดเอง
            ModelState.Remove(nameof(Customer.HN));

            if (ModelState.IsValid)
            {
                _db.Customers.Add(customer);
                _db.SaveChanges();
                TempData["Success"] = true;
                return RedirectToAction("Index");
            }

            // ถ้ามี error ให้แสดงในฟอร์ม
            TempData["Error"] = true;
            return View(customer);
        }

        // GET: Customer/Edit/{id}
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var customer = _db.Customers.Find(id);
            if (customer == null)
            {
                return NotFound();
            }
            return View(customer);
        }

        // POST: Customer/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Customer customer)
        {
            if (ModelState.IsValid)
            {
                _db.Customers.Update(customer);
                _db.SaveChanges();
                return RedirectToAction("Details", new { id = customer.Id });
            }
            return View(customer);
        }

        // GET: Customer/Delete/{id}
        [HttpGet]
        public IActionResult Delete(int id)
        {
            var customer = _db.Customers.Find(id);
            if (customer == null)
            {
                return NotFound();
            }
            return View(customer);
        }

        // POST: Customer/Delete/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var customer = _db.Customers.Find(id);
            if (customer == null)
            {
                return NotFound();
            }
            _db.Customers.Remove(customer);
            _db.SaveChanges();
            return RedirectToAction("Index");
        }

        // GET: Customer/TreatmentHistory/{id}
        // หน้าแสดงประวัติการรักษาของลูกค้าแต่ละราย
        public IActionResult TreatmentHistory(int id)
        {
            var customer = _db.Customers
                .Include(c => c.TreatmentHistories)
                .FirstOrDefault(c => c.Id == id);
            if (customer == null)
            {
                return NotFound();
            }
            return View(customer);
        }

        public async Task<IActionResult> Details(int? id, int? queue)
        {
            var username = HttpContext.Session.GetString("Username");
            var role = HttpContext.Session.GetString("Role");
            var dentistName = HttpContext.Session.GetString("FullName");

            if (string.IsNullOrEmpty(role))
                return RedirectToAction("AccessDenied", "Account");

            if (id == null)
            {
                return NotFound();
            }

            var customer = await _db.Customers
                .Include(c => c.TreatmentHistories)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (customer == null)
            {
                return NotFound();
            }

            var xrays = _db.Xrays
                   .Where(x => x.CustomerId == id)
                   .OrderByDescending(x => x.CreatedAt)
                   .ToList();

            // คำนวณอายุ
            int age = 0;
            if (customer.DateOfBirth != null)
            {
                var today = DateTime.Today;
                age = today.Year - customer.DateOfBirth.Value.Year;
                if (customer.DateOfBirth.Value.Date > today.AddYears(-age)) age--;
            }

            ViewBag.Username = username;
            ViewBag.Role = role;
            ViewBag.DentistName = dentistName;
            ViewBag.Xrays = xrays;
            ViewBag.Age = age;
            ViewBag.Queue = queue;

            return View(customer);
        }

        // POST: Customer/SaveHistory
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveHistory(
            int id,                             // รหัสลูกค้า (Customer.Id)
            string TreatmentDetails,           // ผลการตรวจ
            string? EquipmentDetails,           // อุปกรณ์เพิ่มเติม
            string TreatmentRight,             // สิทธิ์การรักษา
            string DentistName,
            int queue// แพทย์ผู้ตรวจ
        )
        {
            // ตรวจสอบว่ามี session Role หรือไม่ (ถ้ายังไม่ได้ login จะพาไปหน้า AccessDenied)
            var role = HttpContext.Session.GetString("Role");
            if (string.IsNullOrEmpty(role))
                return RedirectToAction("AccessDenied", "Account");

            // สร้าง record ใหม่
            var history = new TreatmentHistory
            {
                CustomerId = id,
                TreatmentDetails = TreatmentDetails,
                EquipmentDetails = EquipmentDetails ?? "",
                TreatmentRights = TreatmentRight,
                DentistName = DentistName,
                TreatmentDate = DateTime.Now
            };

            // ตรวจสอบความถูกต้องของข้อมูลเบื้องต้น
            if (!ModelState.IsValid)
            {
                // กรณีข้อมูลไม่ครบ ให้กลับไปที่หน้า Details
                return RedirectToAction("Details", new { id });
            }

            // บันทึกลงฐานข้อมูล
            _db.TreatmentHistories.Add(history);
            // หา TreatmentQueue ที่มี Id ตรงกับ queue แล้วอัปเดตสถานะ
            var treatmentQueue = _db.TreatmentQueues.FirstOrDefault(q => q.Id == queue);
            if (treatmentQueue != null)
            {
                treatmentQueue.Status = "รักษาเสร็จแล้ว";
                _db.TreatmentQueues.Update(treatmentQueue);
            }
            _db.SaveChanges();
            TempData["Success"] = "บันทึกประวัติการรักษาแล้ว";
            return RedirectToAction("Index", "Queue");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteHistory(int id)
        {
            var history = _db.TreatmentHistories.FirstOrDefault(h => h.Id == id);
            if (history == null)
                return NotFound();

            var customerId = history.CustomerId;

            _db.TreatmentHistories.Remove(history);
            _db.SaveChanges();

            return RedirectToAction("Details", new { id = customerId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditTreatmen(TreatmentHistory updated)
        {
            var existing = _db.TreatmentHistories.FirstOrDefault(t => t.Id == updated.Id);
            if (existing == null)
                return NotFound();

            existing.TreatmentDate = updated.TreatmentDate;
            existing.TreatmentDetails = updated.TreatmentDetails;
            existing.EquipmentDetails = updated.EquipmentDetails;
            existing.TreatmentRights = updated.TreatmentRights;
            existing.DentistName = updated.DentistName;

            _db.SaveChanges();

            return RedirectToAction("Details", "Customer", new { id = existing.CustomerId });
        }

    }
}
