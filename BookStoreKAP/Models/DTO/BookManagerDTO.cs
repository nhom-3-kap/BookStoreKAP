using Newtonsoft.Json;

namespace BookStoreKAP.Models.DTO
{
    public class ReqCreateBook
    {
        public string Synopsis { get; set; }
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
}
