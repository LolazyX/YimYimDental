using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
            // ดึงข้อมูล session ของผู้ใช้ (username, role) สำหรับแสดงหรือเช็คสิทธิ์
            var username = HttpContext.Session.GetString("Username");
            var role = HttpContext.Session.GetString("Role");

            if (string.IsNullOrEmpty(role))
                return RedirectToAction("AccessDenied", "Account");

            ViewBag.Username = username;
            ViewBag.Role = role;

            // เริ่มต้น Queryable ของลูกค้าทั้งหมด
            var query = _db.Customers.AsQueryable();

            // ถ้ามีคำค้นหา (search) ให้กรอง HN ที่มีคำค้นหาอยู่ภายใน (Contains)
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(c => c.HN.Contains(search));
            }

            // สั่งเรียงผลลัพธ์ตาม HN จากมากไปน้อย (Descending)
            var list = query
                .OrderByDescending(c => c.HN)
                .ToList();

            return View(list);
        }

        // POST: Customer/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Customer customer)
        {
            var lastCustomer = _db.Customers
                .OrderByDescending(c => c.Id)
                .FirstOrDefault();

            int lastHNNumber = 0;
            if (lastCustomer != null && int.TryParse(lastCustomer.HN, out int parsedHN))
            {
                lastHNNumber = parsedHN;
            }

            int newHNNumber = lastHNNumber + 1;
            customer.HN = newHNNumber.ToString("D6");

            ModelState.Remove(nameof(Customer.HN));

            if (ModelState.IsValid)
            {
                _db.Customers.Add(customer);
                _db.SaveChanges();
                TempData["Success"] = true;
                return RedirectToAction("Index");
            }

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
            Console.WriteLine("===== Received Customer data =====");
            Console.WriteLine($"Id               : {customer.Id}");
            Console.WriteLine($"Prefix           : {customer.Prefix}");
            Console.WriteLine($"FullName         : {customer.FullName}");
            Console.WriteLine($"HN               : {customer.HN}");
            Console.WriteLine($"NationalId       : {customer.NationalId}");
            Console.WriteLine($"DateOfBirth      : {customer.DateOfBirth?.ToString("yyyy-MM-dd")}");
            Console.WriteLine($"Gender           : {customer.Gender}");
            Console.WriteLine($"Phone            : {customer.Phone}");
            Console.WriteLine($"Email            : {customer.Email}");
            Console.WriteLine($"Address          : {customer.Address}");
            Console.WriteLine($"ChronicDiseases  : {customer.ChronicDiseases}");
            Console.WriteLine($"Allergies        : {customer.Allergies}");
            Console.WriteLine("====================================");
            if (ModelState.IsValid)
            {
                _db.Customers.Update(customer);
                _db.SaveChanges();
                return RedirectToAction("Details", new { id = customer.Id });
            }
            return RedirectToAction("Index");
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

        // GET: Customer/Details/{id}
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
                .ThenInclude(th => th.Billings)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (customer == null)
            {
                return NotFound();
            }

            var xrays = _db.Xrays
                .Where(x => x.CustomerId == id)
                .OrderByDescending(x => x.CreatedAt)
                .ToList();

            int age = 0;
            if (customer.DateOfBirth.HasValue)
            {
                var today = DateTime.Today;
                age = today.Year - customer.DateOfBirth.Value.Year;
                if (customer.DateOfBirth.Value.Date > today.AddYears(-age)) age--;
            }

            ViewBag.Services = await _db.Services.ToListAsync() ?? new List<Service>();
            ViewBag.Equipments = await _db.Equipments.ToListAsync() ?? new List<Equipment>();
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
            int id,
            string TreatmentDetails,
            string TreatmentRight,
            string DentistName,
            int queue,
            [FromForm] List<BillingItem> BillingItems)
        {
            var role = HttpContext.Session.GetString("Role");
            if (string.IsNullOrEmpty(role))
                return RedirectToAction("AccessDenied", "Account");

            if (string.IsNullOrEmpty(TreatmentDetails) || string.IsNullOrEmpty(TreatmentRight) || string.IsNullOrEmpty(DentistName))
            {
                TempData["Error"] = "กรุณากรอกข้อมูลให้ครบถ้วน";
                return RedirectToAction("Details", new { id });
            }

            var history = new TreatmentHistory
            {
                CustomerId = id,
                TreatmentDetails = TreatmentDetails,
                TreatmentRights = TreatmentRight,
                DentistName = DentistName,
                TreatmentDate = DateTime.Now,
                IsPaid = false
            };

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "ข้อมูลไม่ถูกต้อง";
                return RedirectToAction("Details", new { id });
            }

            _db.TreatmentHistories.Add(history);
            _db.SaveChanges();

            if (BillingItems != null && BillingItems.Any())
            {
                foreach (var item in BillingItems)
                {
                    if (item == null || item.ItemId == 0 || string.IsNullOrEmpty(item.ItemType) || item.Quantity <= 0)
                    {
                        continue;
                    }

                    var billing = new Billing
                    {
                        TreatmentHistoryId = history.Id,
                        ItemId = item.ItemId,
                        ItemType = item.ItemType,
                        Quantity = item.Quantity,
                        Price = item.Price,
                        PaymentStatus = "ยังไม่ชำระ"
                    };
                    _db.Billings.Add(billing);
                }
            }

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

        // POST: Customer/DeleteHistory/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteHistory(int id)
        {
            var history = _db.TreatmentHistories
                .Include(th => th.Billings)
                .FirstOrDefault(h => h.Id == id);

            if (history == null)
                return NotFound();

            var customerId = history.CustomerId;

            if (history.Billings != null)
            {
                _db.Billings.RemoveRange(history.Billings);
            }

            _db.TreatmentHistories.Remove(history);
            _db.SaveChanges();

            return RedirectToAction("Details", new { id = customerId });
        }

        // POST: Customer/EditTreatmen
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditTreatmen(TreatmentHistory updated)
        {
            var existing = _db.TreatmentHistories.FirstOrDefault(t => t.Id == updated.Id);
            if (existing == null)
                return NotFound();

            existing.TreatmentDate = updated.TreatmentDate;
            existing.TreatmentDetails = updated.TreatmentDetails;
            existing.TreatmentRights = updated.TreatmentRights;
            existing.DentistName = updated.DentistName;

            _db.SaveChanges();

            return RedirectToAction("Details", new { id = existing.CustomerId });
        }

        public class UpdatePaymentStatusModel
        {
            public int TreatmentId { get; set; }
            public bool IsPaid { get; set; }
        }

        // POST: Customer/UpdatePaymentStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdatePaymentStatus([FromBody] UpdatePaymentStatusModel model)
        {
            var history = _db.TreatmentHistories.FirstOrDefault(th => th.Id == model.TreatmentId);
            if (history == null)
                return Json(new { success = false });

            history.IsPaid = model.IsPaid;
            _db.SaveChanges();

            return Json(new { success = true });
        }
    }
}