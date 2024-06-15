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
        public string Username { get; set; }
        public Guid RoleId { get; set; }
    }
}
