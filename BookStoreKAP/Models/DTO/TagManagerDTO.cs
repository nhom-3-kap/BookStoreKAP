namespace BookStoreKAP.Models.DTO
{
    public class ReqQuerySearchTag : BaseRequestQueryManagerDTO
    {
        public string? Name { get; set; }
    }

    public class ReqCreateTag
    {
        public string Name { get; set; }
    }

    public class ReqModifyTag
    {
        public Guid ID { get; set; }
        public string Name { get; set; }
        public string? Thumbnail { get; set; }
    }
}
