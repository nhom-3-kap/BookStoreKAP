namespace BookStoreKAP.Models.DTO
{
    public class ReqAddCart
    {
        public Guid BookID { get; set; }
        public int? Quantity { get; set; }
    }
}
