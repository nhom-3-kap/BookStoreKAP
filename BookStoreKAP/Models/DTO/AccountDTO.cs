using System.ComponentModel.DataAnnotations;

namespace BookStoreKAP.Models.DTO
{
    public class ExternalLoginConfirmationDTO
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }

    public class ReqLoginDTO
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
