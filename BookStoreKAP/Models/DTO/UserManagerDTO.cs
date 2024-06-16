using BookStoreKAP.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace BookStoreKAP.Models.DTO
{
    public class UserRolesViewModel
    {
        public User User { get; set; }
        public List<string> Roles { get; set; }
    }

    public class ReqSearchUserDTO
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime BOD { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public Guid RoleId { get; set; } = Guid.Empty;

        public string menuKey { get; set; }

    }

    public class ReqQuerySearchUserDTO
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime? BOD { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? Username { get; set; }
        public List<Guid>? RoleIds { get; set; }

        public string? menuKey { get; set; }
        public int page { get; set; } = 1;
        public int pageSize { get; set; } = 10;
    }

    public class ReqCreateUserDTO
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime? BOD { get; set; }
        public string Email { get; set; }
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập trường này")]
        public string Username { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập trường này")]
        public string Password { get; set; }
        public Guid RoleId { get; set; }
    }

    public class ReqDeleteUserDTO
    {
        public Guid UserID { get; set; }
    }
}
