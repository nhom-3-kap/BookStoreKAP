using System.ComponentModel.DataAnnotations;

namespace BookStoreKAP.Models.DTO
{
    public class ExternalLoginConfirmationDTO
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
