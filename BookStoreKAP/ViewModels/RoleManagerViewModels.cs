using BookStoreKAP.Models.Entities;

namespace BookStoreKAP.ViewModels
{
    public class ModifyRoleVM
    {
        public Role Role { get; set; }
        public List<AccessController> AccessControllers { get; set; }
    }
}
