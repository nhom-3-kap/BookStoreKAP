using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookStoreKAP.Models.Entities
{
    public class Book : BaseEntity
    {

        public string Images { get; set; }
        [Required]
        public string Thumbnail { get; set; }
        [MaxLength(50)]
        public string Title { get; set; }
        [MaxLength(50)]
        public string Publisher { get; set; }
        [MaxLength(50)]
        public string Author { get; set; }
        [Required]
        public int PublicationYear { get; set; }
        [Required]
        public double Price { get; set; }
        [Required]
        public double Discount { get; set; }
        [Required]
        public int Quantity { get; set; }
        public string Synopsis { get; set; }
        public string Feedback { get; set; }

        [ForeignKey(nameof(SeriesID))]
        public Guid SeriesID { get; set; }
        public ICollection<Series> Series { get; set; }

        public ICollection<OrderDetail> OrderDetails { get; set; }

        public ICollection<Sale> Sales { get; set; }

        public ICollection<BookGenre> BookGenres { get; set; }

        public ICollection<Favorite> Favorites { get; set; }
    }
}
