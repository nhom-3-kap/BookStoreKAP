namespace BookStoreKAP.Models.Entities
{
    public class PurchaseViewModel
    {
        public Guid ID { get; set; }  // Thêm ID

        public string Title { get; set; }
        public string Author { get; set; }
        public double Price { get; set; }
        public string UserName { get; set; }
        public DateTime BoughtDate { get; set; }
        public int Quantity { get; set; } 

    }
}
