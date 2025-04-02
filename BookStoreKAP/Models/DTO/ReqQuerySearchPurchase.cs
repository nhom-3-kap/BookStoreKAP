
namespace BookStoreKAP.Models.DTO
{
    public class ReqQuerySearchPurchase : PaginationModel
    {
        public string Title { get; set; }
        public string Author { get; set; }
        public string UserName { get; set; }
        public int Page { get; set; } = 1; // Default to 1
        public int PageSize { get; set; } = 10; // Default page size
        public string menuKey { get; set; } = "PM"; // Thêm menuKey với giá trị mặc định "PM"


    }
}
