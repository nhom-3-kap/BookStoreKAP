using BookStoreKAP.Models.Entities;

namespace BookStoreKAP.Models.DTO
{
    public class ReqCreateDomain
    {
        public Guid AccessControllerID { get; set; }
        public Guid RoleID { get; set; }
    }

    public class ReqModifyDomain
    {
        public Guid AccessControllerID { get; set; }
        public Guid RoleID { get; set; }

        public string AccessControllerForTitle { get; set; }
        public List<Role> Roles { get; set; }
    }
}
