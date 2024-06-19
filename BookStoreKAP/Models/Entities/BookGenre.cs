using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookStoreKAP.Models.Entities
{
    public class BookGenre
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int ID { get; set; }

        public Guid GenreID { get; set; }
        public Genre Genre { get; set; }

        public Guid BookID { get; set; }
        public Book Book { get; set; }
    }
}
