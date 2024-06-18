using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace BookStoreKAP.Models.Entities
{
    public class Book : BaseEntity
    {

        public string Images { get; set; }
        [AllowNull]
        public string? Thumbnail { get; set; }
        [StringLength(50)]
        public string Title { get; set; }
        [StringLength(50)]
        public string Publisher { get; set; }
        [StringLength(50)]
        public string Author { get; set; }
        public int PublicationYear { get; set; } = 0;
        public double Price { get; set; } = 0;
        public double Discount { get; set; } = 0;
        public int Quantity { get; set; } = 0;

        [Column(TypeName = "text")]
        [AllowNull]
        public string? Synopsis { get; set; }
        public string Feedback { get; set; }
        public int ViewCount { get; set; } = 0;

        [ForeignKey(nameof(TagID))]
        public Guid TagID { get; set; }
        public ICollection<Tag> Tags { get; set; }

        [ForeignKey(nameof(SeriesID))]
        public Guid SeriesID { get; set; }
        public ICollection<Series> Series { get; set; }

        public ICollection<OrderDetail> OrderDetails { get; set; }

        public ICollection<Sale> Sales { get; set; }

        public ICollection<BookGenre> BookGenres { get; set; }

        public ICollection<Favorite> Favorites { get; set; }
    }
}
