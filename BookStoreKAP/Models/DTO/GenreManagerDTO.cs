using System.ComponentModel.DataAnnotations;

namespace BookStoreKAP.Models.DTO
{
    public class ReqGenreCreate
    {
        [Required]
        public string Name { get; set; }
    }

    public class ReqQuerySearchGenre
    {
        public string Name { get; set; }

        public int page { get; set; } = 1;
        public int pageSize { get; set; } = 10;
        public string menuKey { get; set; }
    }

    public class ReqGenreModify
    {
        public Guid GenreID { get; set; }
        public string? Name { get; set; }
    }
}
