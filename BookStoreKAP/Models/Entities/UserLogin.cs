using Microsoft.AspNetCore.Identity;

namespace BookStoreKAP.Models.Entities
{
    public class UserLogin : IdentityUserLogin<Guid>
    {
        public User User { get; set; }
    }
}
