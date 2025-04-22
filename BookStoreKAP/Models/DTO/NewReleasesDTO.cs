namespace BookStoreKAP.Models.DTO
{
    public class ReqBookByDK
    {
        public int MinPrice { get; set; }
        public int MaxPrice { get; set; }
        
        public string Service { get; set; }
        public Guid GenreID { get; set; }

        public string input {get; set; }
        public string SortBy { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; } = 30; // Hiển thị 30 cuốn sách mỗi trang


    }
}
