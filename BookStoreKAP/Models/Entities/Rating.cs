using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookStoreKAP.Models.Entities
{
    public class Rating : BaseEntity
    {
        [ForeignKey(nameof(ReviewerID))]
        public Guid ReviewerID { get; set; }
        public ICollection<User> Reviewers { get; set; }

        [ForeignKey(nameof(BookID))]
        public Guid BookID { get; set; }
        public ICollection<Book> Books { get; set; }

        public int RatingCount { get; set; }

        [StringLength(100)]
        public string Detail { get; set; }
    }
}
