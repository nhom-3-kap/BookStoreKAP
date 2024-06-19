using BookStoreKAP.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace BookStoreKAP.Models.DTO
{
    public class UserRolesViewModel
    {
        public User User { get; set; }
        public List<string> Roles { get; set; }
    }

    public class ReqQuerySearchUser : BaseRequestQueryManagerDTO
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime? BOD { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? Username { get; set; }
        public List<Guid>? RoleIds { get; set; }
    }

    public class ReqCreateUser
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
        public List<Guid> RoleIds { get; set; }
    }

    public class ReqDeleteUser
    {
        public Guid UserID { get; set; }
    }
    public class ReqModifyUser
    {
        public Guid Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime BOD { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public List<Guid>? RoleIds { get; set; }
        public string? Avatar { get; set; }
    }
}
