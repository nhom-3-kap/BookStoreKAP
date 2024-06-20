using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace BookStoreKAP.Models.DTO
{
    public class ReqCreateBook
    {
        [Required]
        public string Title { get; set; }
        [Required]
        public string Publisher { get; set; }
        [Required]
        public int PublicationYear { get; set; }
        [Required]
        public string Author { get; set; }
        [Required]
        public double Price { get; set; }
        [Required]
        public double Discount { get; set; }
        [Required]
        public int Quantity { get; set; }

        [Required]
        public Guid SeriesID { get; set; }
        [Required]
        public Guid TagID { get; set; }
        [Required]
        public List<Guid> GenreIds { get; set; }

        public string? Synopsis { get; set; }
    }

    public class ReqQuerySearchBook : BaseRequestQueryManagerDTO
    {
        public string? Title { get; set; }
        public string? Publisher { get; set; }
        public int? PublicationYear { get; set; }
        public string? Author { get; set; }
        public Guid? SeriesID { get; set; }
        public Guid? TagID { get; set; }
    }

    public class ReqModifyBook
    {
        [Required]
        public Guid ID { get; set; }

        [Required]
        public string Title { get; set; }

        public string? Publisher { get; set; }
        public int PublicationYear { get; set; }
        public string? Author { get; set; }
        public double Price { get; set; }
        public double Discount { get; set; }
        public int Quantity { get; set; }
        public Guid SeriesID { get; set; }
        public Guid TagID { get; set; }
        public List<Guid> GenreIds { get; set; }
        public string? Synopsis { get; set; }
    }
}
