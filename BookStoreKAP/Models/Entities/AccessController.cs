using System.Diagnostics.CodeAnalysis;

namespace BookStoreKAP.Models.Entities
{
    public class AccessController : BaseEntity
    {
        public string Name { get; set; }

        [AllowNull]
        public string? AreaName { get; set; } // Thêm trường AreaName
        public ICollection<Domain> Domains { get; set; }
        public ICollection<Policy> Policies { get; set; }
    }
}