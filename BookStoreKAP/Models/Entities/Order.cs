using System.ComponentModel.DataAnnotations.Schema;

namespace BookStoreKAP.Models.Entities
{
    public class Order : BaseEntity
    {
        public DateTime OrderDate { get; set; }
        public int SubTotal { get; set; }
        public double Total { get; set; }
        public string Address { get; set; }
        public int Status { get; set; }
        public string PaymentMethod { get; set; }

        [ForeignKey(nameof(Customer))]
        public Guid CustomerID { get; set; }
        public User Customer { get; set; }  // Sửa từ ICollection<User> thành User

        public ICollection<OrderDetail> OrderDetails { get; set; }
    }

 //   public enum StatusType
	//{
	//	WAITING_FOR_PROGRESSING,
	//	APPROVED,
	//	DENIED
	//}
}
