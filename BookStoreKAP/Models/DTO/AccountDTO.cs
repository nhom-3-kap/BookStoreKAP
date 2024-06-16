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

    public class ReqRegisterDTO
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }

        [Required]
        public string Username { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
