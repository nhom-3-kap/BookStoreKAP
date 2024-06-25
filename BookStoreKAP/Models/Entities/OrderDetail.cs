namespace BookStoreKAP.Models.Entities
{
	public class OrderDetail : BaseEntity
	{
		public Guid OrderID { get; set; }
		public Order Order { get; set; }

		public Guid BookID { get; set; }
		public Book Book { get; set; }

		public int Quantity { get; set; }
		public double Price { get; set; }
	}
}
