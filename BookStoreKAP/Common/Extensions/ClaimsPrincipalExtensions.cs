using System.Security.Claims;

namespace BookStoreKAP.Common.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static string GetAvatar(this ClaimsPrincipal principal)
        {
            if (principal == null)
                throw new ArgumentNullException(nameof(principal));

            var avatarClaim = principal.FindFirst("Avatar");
            return avatarClaim != null ? avatarClaim.Value : "https://placehold.co/200x200";
        }
    }
}
