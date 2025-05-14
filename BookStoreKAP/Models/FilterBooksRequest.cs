namespace BookStoreKAP.Models
{
    public class FilterRequest
    {
        public int MinPrice { get; set; } = 10000;
        public int MaxPrice { get; set; } = 500000;
        public string Service { get; set; }
        public List<Guid> GenreIDs { get; set; }
        public List<string> MagazineIDs { get; set; }
        public string SortOrder { get; set; }
        public List<string> Availability { get; set; }
        public int Page { get; set; } = 1;
        public string Hambuger { get; set; }
    }
    public class FilterBooksRequest
    {
        public FilterRequest req { get; set; }


    }
}
