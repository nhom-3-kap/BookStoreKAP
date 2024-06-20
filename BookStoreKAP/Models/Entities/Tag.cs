using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace BookStoreKAP.Models.Entities
{
    public class Tag : BaseEntity
    {
        [Required]
        [StringLength(150)]
        public string Name { get; set; }

        [AllowNull]
        public string? Thumbnail { get; set; }
    }
}
