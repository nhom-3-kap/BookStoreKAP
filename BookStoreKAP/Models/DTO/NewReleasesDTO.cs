namespace BookStoreKAP.Models.DTO
{
    public class ReqBookByDK
    {
        public int MinPrice { get; set; }
        public int MaxPrice { get; set; }
        
        public string Service { get; set; }
        public Guid GenreID { get; set; }

        public string input {get; set; }


    }
}
