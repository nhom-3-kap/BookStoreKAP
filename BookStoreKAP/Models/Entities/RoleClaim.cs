using Microsoft.AspNetCore.Identity;

namespace BookStoreKAP.Models.Entities
{
    public class RoleClaim : IdentityRoleClaim<Guid>
    {
        public Role Role { get; set; }
    }
}
