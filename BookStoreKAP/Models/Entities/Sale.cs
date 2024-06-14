using System.ComponentModel.DataAnnotations.Schema;

namespace BookStoreKAP.Models.Entities
{
    public class Sale : BaseEntity
    {
        public DateTime SaleBegin { get; set; }
        public DateTime SaleEnd { get; set; }
        public double Price { get; set; }

        [ForeignKey(nameof(BookID))]
        public Guid BookID { get; set; }
        public Book Book { get; set; }

    }
}
