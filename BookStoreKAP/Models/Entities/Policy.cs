using System.ComponentModel.DataAnnotations.Schema;

namespace BookStoreKAP.Models.Entities
{
    public class Policy : BaseEntity
    {
        public string Name { get; set; }
        public string ActionName { get; set; }

        [ForeignKey(nameof(AccessControllerID))]
        public Guid AccessControllerID { get; set; }
        public AccessController AccessController { get; set; }
    }
}
