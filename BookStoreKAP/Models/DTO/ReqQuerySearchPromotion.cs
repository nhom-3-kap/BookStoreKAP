namespace BookStoreKAP.Models.DTO
{
    public class ReqQuerySearchPromotion
    {
        public string Name { get; set; }
        public string TagID { get; set; }
        public string Status { get; set; } // "Active", "Upcoming", "Expired"
        public string SortBy { get; set; }
        public string SortDirection { get; set; }
        public string menuKey { get; set; }
        public string PromotionType { get; set; }
        public string SeriesID { get; set; }


    }
}