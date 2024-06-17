using System.ComponentModel.DataAnnotations;

namespace BookStoreKAP.Models.DTO
{
    public class ReqCreateSeries
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public int Volumns { get; set; }
    }

    public class ReqQuerySearchSeries
    {
        public string? Name { get; set; }
        public int Volumns { get; set; } = -1;
    }

    public class ReqModifyCategory
    {
        public Guid SeriesID { get; set; }
        public string? Name { get; set; }
        public int Volumns { get; set; }
    }
}
