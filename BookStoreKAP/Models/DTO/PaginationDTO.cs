namespace BookStoreKAP.Models.DTO
{
    public class PaginationModel
    {
        public int TotalItems { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public string Action { get; set; }
        public string Controller { get; set; }
        public dynamic SearchParams { get; set; }
    }
}
