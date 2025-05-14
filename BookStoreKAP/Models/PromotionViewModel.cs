using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

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

        // Promotion type: 1 = Tag, 2 = Series, 3 = Specific Books
        [Required(ErrorMessage = "Vui lòng chọn loại khuyến mãi")]
        public int PromotionType { get; set; } = 1;

        // For Tag-based promotions
        public Guid? TagID { get; set; }
        public string TagName { get; set; }

        // For Series-based promotions
        public Guid? SeriesID { get; set; }
        public string SeriesName { get; set; }

        // For Specific Book promotions
        public List<Guid> SelectedBookIds { get; set; } = new List<Guid>();
        public List<BookBasicViewModel> SelectedBooks { get; set; } = new List<BookBasicViewModel>();

        [Required(ErrorMessage = "Ngày bắt đầu không được để trống")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Ngày kết thúc không được để trống")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [Required(ErrorMessage = "Phần trăm giảm giá không được để trống")]
        [Range(1, 100, ErrorMessage = "Phần trăm giảm giá phải từ 1% đến 100%")]
        public double DiscountPercent { get; set; }

        // Helper constants for TagIDs
        public static readonly Guid BEST_SELLER_TAG_ID = new Guid("3E5CAC9B-6E5A-416D-86F8-52F044D6994E");
        public static readonly Guid NEW_RELEASE_TAG_ID = new Guid("CA038048-95D2-4BFD-86D8-740FB2ECE1AF");

        // Helper property to determine if this is for best sellers
        public bool IsBestSellerPromotion => TagID == BEST_SELLER_TAG_ID;

        // Helper property to determine if this is for new releases
        public bool IsNewReleasePromotion => TagID == NEW_RELEASE_TAG_ID;
    }

    // Simple view model for books in the selection list
    public class BookBasicViewModel
    {
        public Guid Id { get; set; }
        public Guid SeriesID { get; set; }

        public string Title { get; set; }
        public string Author { get; set; }
        public double Price { get; set; }
        public string Thumbnail { get; set; }
    }
}