using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BookStoreKAP.Models.Entities
{
    [Table("PromotionBook")] // Thêm thuộc tính Table với tên đúng của bảng trong CSDL
    public class PromotionBook
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid PromotionId { get; set; }

        [Required]
        public Guid BookId { get; set; }

        [ForeignKey("PromotionId")]
        public Promotion Promotion { get; set; }

        [ForeignKey("BookId")]
        public Book Book { get; set; }
    }
}
