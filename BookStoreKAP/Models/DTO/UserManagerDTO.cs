using BookStoreKAP.Models.Entities;

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

    }
}
