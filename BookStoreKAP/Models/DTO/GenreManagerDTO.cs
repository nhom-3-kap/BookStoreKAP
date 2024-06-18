using System.ComponentModel.DataAnnotations;

namespace BookStoreKAP.Models.DTO
{
    public class ReqGenreCreate
    {
        [Required]
        public string Name { get; set; }
    }

    public class ReqQuerySearchGenre : BaseRequestQueryManagerDTO
    {
        public string Name { get; set; }
    }

    public class ReqGenreModify
    {
        public Guid GenreID { get; set; }
        public string? Name { get; set; }
    }
}
