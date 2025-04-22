// Models/ServiceItem.cs
using System.ComponentModel.DataAnnotations;

namespace YimYimDental.Models
{
    public class BillingItem
    {
        public string ItemId { get; set; }           // เช่น S001 หรือ E012
        public string ItemName { get; set; }         // เช่น ขูดหินปูน หรือ ชุดอุปกรณ์ปลอดเชื้อ
        public string ItemType { get; set; }         // "บริการ" หรือ "อุปกรณ์"

        [Range(1, int.MaxValue, ErrorMessage = "กรุณาระบุจำนวนอย่างน้อย 1")]
        public int Quantity { get; set; }

        [DataType(DataType.Currency)]
        public decimal Price { get; set; }
    }
}