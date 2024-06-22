using System.Diagnostics.CodeAnalysis;

namespace BookStoreKAP.Models.Entities
{
    public class AccessController : BaseEntity
    {
        public string Name { get; set; }

        [AllowNull]
        public string? AreaName { get; set; } // Thêm trường AreaName

        [AllowNull]
        public string? Status { get; set; }

        public ICollection<Domain> Domains { get; set; }
        public ICollection<Policy> Policies { get; set; }
    }
}