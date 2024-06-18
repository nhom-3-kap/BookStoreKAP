namespace BookStoreKAP.Models.DTO
{
    public class BaseRequestQueryManagerDTO
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string menuKey { get; set; }
    }
}
