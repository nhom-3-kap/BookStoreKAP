using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace BookStoreKAP.Models.Entities
{
    public class User : IdentityUser
    {
        [StringLength(100)]
        [Required]
        public string FullName { get; set; }
    }
}
