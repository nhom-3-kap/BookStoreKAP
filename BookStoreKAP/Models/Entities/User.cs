using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace BookStoreKAP.Models.Entities
{
    public class User : IdentityUser<Guid>
    {
        [StringLength(50)]
        [AllowNull]
        public string? FirstName { get; set; }

        [StringLength(50)]
        [AllowNull]
        public string? LastName { get; set; }

        [AllowNull]
        public DateTime? BOD { get; set; }

        [StringLength(100)]
        [AllowNull]
        public string? ShipAddress { get; set; }

        [AllowNull]
        public string? Avatar { get; set; }

        public ICollection<Favorite>? Favorites { get; set; }

        #region Custom Identity
        public ICollection<UserClaim>? UserClaims { get; set; }
        public ICollection<UserRole>? UserRoles { get; set; }
        public ICollection<UserLogin>? UserLogins { get; set; }
        public ICollection<UserToken>? UserTokens { get; set; }
        #endregion
    }
}
