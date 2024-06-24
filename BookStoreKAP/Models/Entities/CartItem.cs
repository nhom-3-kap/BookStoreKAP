using System.ComponentModel.DataAnnotations.Schema;

namespace BookStoreKAP.Models.Entities
{
    public class CartItem : BaseEntity
    {
        [ForeignKey(nameof(CartID))]
        public Guid CartID { get; set; }
        public Cart Cart { get; set; }

        [ForeignKey(nameof(BookID))]
        public Guid BookID { get; set; }
        public Book Book { get; set; }

        public int Quantity { get; set; }
        public double Price { get; set; }
    }
}
