using System.ComponentModel.DataAnnotations;

namespace BookStoreKAP.Models.DTO
{
    public class ReqCreateRole
    {
        [Required]
        public required string Name { get; set; }
    }

    public class ReqModifyRole
    {
        [Required]
        public required Guid Id { get; set; }
        [Required]
        public required string Name { get; set; }

        public List<Guid> PolicyIDs { get; set; }

    }


}
