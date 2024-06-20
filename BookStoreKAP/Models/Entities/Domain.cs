using System.ComponentModel.DataAnnotations.Schema;

namespace BookStoreKAP.Models.Entities
{
    public class Domain : BaseEntity
    {
        [ForeignKey(nameof(AccessControllerID))]
        public Guid AccessControllerID { get; set; }
        public AccessController AccessController { get; set; }

        [ForeignKey(nameof(RoleID))]
        public Guid RoleID { get; set; }
        public Role Role { get; set; }
    }
}
