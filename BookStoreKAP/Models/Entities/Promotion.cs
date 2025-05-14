using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookStoreKAP.Models.Entities
{
    public class Promotion
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        [Required]
        [Range(0, 100)]
        public double DiscountPercent { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Promotion type: 1 = Tag, 2 = Series, 3 = Specific Books
        public int PromotionType { get; set; } = 1;

        // For Tag-based promotions
        public Guid? TagID { get; set; }
        public Tag? Tag { get; set; }

        // For Series-based promotions
        public Guid? SeriesID { get; set; }
        public Series? Series { get; set; }

        // For Specific Book promotions - will use a junction table
        public ICollection<PromotionBook>? PromotionBooks { get; set; }
    }


}