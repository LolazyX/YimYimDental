namespace YimYimDental.Models
{
    public class BillingItem
    {
        public int ItemId { get; set; }
        public string ItemType { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}