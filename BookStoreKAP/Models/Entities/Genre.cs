using System.ComponentModel.DataAnnotations;

namespace BookStoreKAP.Models.Entities
{
    public class Genre : BaseEntity
    {
        [StringLength(20)]
        public string Name { get; set; }

        //Thumbnail

        public ICollection<BookGenre> BookGenres { get; set; }
    }
}
