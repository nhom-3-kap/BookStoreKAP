using Microsoft.AspNetCore.Identity;

namespace BookStoreKAP.Models.Entities
{
    public class RoleClaim : IdentityRoleClaim<Guid>
    {
        public Role Role { get; set; }
        public string ControllerName { get; set; }
        public string ActionName { get; set; }
    }
}
