namespace BookStoreKAP.Models.Entities
{
    public class AccessController : BaseEntity
    {
        public string Name { get; set; }
        public ICollection<Domain> Domains { get; set; }
    }
}