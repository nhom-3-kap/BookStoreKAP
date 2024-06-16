using Microsoft.AspNetCore.Identity;

namespace BookStoreKAP.Models.Entities
{
    public class Role : IdentityRole<Guid>
    {
        public ICollection<UserRole> UserRoles { get; set; }
    }
}
