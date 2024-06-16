using Microsoft.AspNetCore.Identity;

namespace BookStoreKAP.Models.Entities
{
    public class UserToken : IdentityUserToken<Guid>
    {
        public User User { get; set; }
    }
}
