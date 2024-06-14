using System.ComponentModel.DataAnnotations.Schema;

namespace BookStoreKAP.Models.Entities
{
    public class Favorite
    {
        public Guid BookID { get; set; }
        public Book Book { get; set; }

        public Guid CustomersID { get; set; }
        public User Customer { get; set; }
    }
}
