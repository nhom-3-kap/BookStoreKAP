using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookStoreKAP.Models.Entities
{
    public class Books: BaseEntity
    {
        
        public string Images { get; set; }
        [Required]
        public string Thumbnail { get; set; }
        [MaxLength(50)]
        public string Title { get; set; }
        [Required,MaxLength(10)]
        public string Genre { get; set; }
        [MaxLength(50)]
        public string Publisher  { get; set; }
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
        public Guid SeriesID { get; set; }
        public  string Synopsis { get; set; }
        public string Feedback { get; set; }

    }
}
