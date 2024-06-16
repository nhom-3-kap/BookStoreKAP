using Microsoft.AspNetCore.Identity;

namespace BookStoreKAP.Models.Entities
{
    public class UserRole : IdentityUserRole<Guid>
    {
        public User User { get; set; }
    }
}
