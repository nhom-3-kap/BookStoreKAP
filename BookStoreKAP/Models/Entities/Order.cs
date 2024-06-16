using System.ComponentModel.DataAnnotations.Schema;

namespace BookStoreKAP.Models.Entities
{
    public class Order : BaseEntity
    {
        public DateTime OrderDate { get; set; }
        public int SubTotal { get; set; }
        public double Total { get; set; }
        public string Address { get; set; }
        public StatusType Status { get; set; }

        [ForeignKey(nameof(CustomerID))]
        public Guid CustomerID { get; set; }
        public ICollection<User> Customers { get; set; }

        public ICollection<OrderDetail> OrderDetails { get; set; }
    }

    public enum StatusType
    {
        WAITING_FOR_PROGRESSING,
        APPROVED,
        DENIED
    }
}
