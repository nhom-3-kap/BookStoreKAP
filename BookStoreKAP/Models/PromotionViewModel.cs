using System;
using System.ComponentModel.DataAnnotations;

namespace BookStoreKAP.Models
{
    public class PromotionViewModel
    {
        public Guid ID { get; set; }

        [Required(ErrorMessage = "Tên khuyến mãi không được để trống")]
        [StringLength(100, ErrorMessage = "Tên khuyến mãi không được vượt quá 100 ký tự")]
        public string Name { get; set; }

        [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Thể loại sách không được để trống")]
        public Guid TagID { get; set; }

        public string TagName { get; set; }

        [Required(ErrorMessage = "Ngày bắt đầu không được để trống")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Ngày kết thúc không được để trống")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [Required(ErrorMessage = "Phần trăm giảm giá không được để trống")]
        [Range(1, 100, ErrorMessage = "Phần trăm giảm giá phải từ 1% đến 100%")]
        public double DiscountPercent { get; set; }

        // Keep this property for backward compatibility but we'll use tags instead
        public bool ApplyToBestSeller { get; set; }

        // Helper property to determine if this is for best sellers
        public bool IsBestSellerPromotion => TagID == new Guid("3E5CAC9B-6E5A-416D-86F8-52F044D6994E");

        // Helper property to determine if this is for new releases
        public bool IsNewReleasePromotion => TagID == new Guid("CA038048-95D2-4BFD-86D8-740FB2ECE1AF");
    }
}