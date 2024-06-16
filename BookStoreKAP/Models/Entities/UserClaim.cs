using Microsoft.AspNetCore.Identity;

namespace BookStoreKAP.Models.Entities
{
    public class UserClaim : IdentityUserClaim<Guid>
    {
        public User User { get; set; }
    }
}
