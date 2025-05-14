namespace BookStoreKAP.Models.DTO
{
    public class ReqBookByDK
    {
        public double MinPrice { get; set; }
        public double MaxPrice { get; set; }
        public string Service { get; set; }
        public List<Guid> GenreID { get; set; }
        public string Input { get; set; }
        public string SortBy { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }

}