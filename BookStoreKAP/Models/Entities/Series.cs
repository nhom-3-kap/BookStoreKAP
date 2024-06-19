using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookStoreKAP.Models.Entities
{
    public class Series : BaseEntity
    {
        [StringLength(50)]
        public string Name { get; set; }

        public int Volumns { get; set; }

        public ICollection<Book> Books { get; set; }

    }
}
