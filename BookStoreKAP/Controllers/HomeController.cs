using BookStoreKAP.Common.Constants;
using BookStoreKAP.Data;
using BookStoreKAP.Models;
using BookStoreKAP.Models.Entities;
using BookStoreKAP.Services;
using BookStoreKAP.Models.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
namespace BookStoreKAP.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly BookStoreKAPDBContext _context;
        private readonly IPromotionService _promotionService;
        // GUID cho các tag
        private static readonly Guid BEST_SELLER_TAG_ID = new Guid("3E5CAC9B-6E5A-416D-86F8-52F044D6994E");
        private static readonly Guid NEW_RELEASE_TAG_ID = new Guid("CA038048-95D2-4BFD-86D8-740FB2ECE1AF");

        public HomeController(ILogger<HomeController> logger, BookStoreKAPDBContext context, IPromotionService promotionService)
        {
            _logger = logger;
            _context = context;
            _promotionService = promotionService;
        }

        private void UpdateBookTags()
        {
            // Reset tags trước khi cập nhật
            var allBooks = _context.Books.ToList();
            //foreach (var book in allBooks)
            //{
            //    book.TagID = null;
            //}
            _context.SaveChanges();

            // 1. Xác định sách New Release: sách có CreatedAt trong vòng 1 tháng
            var oneMonthAgo = DateTime.Now.AddMonths(-1);
            var newReleaseBooks = _context.Books
                .Where(b => b.CreatedAt >= oneMonthAgo)
                .OrderByDescending(b => b.CreatedAt)
                .ToList();

            foreach (var book in newReleaseBooks)
            {
                book.TagID = NEW_RELEASE_TAG_ID;
            }
            _context.SaveChanges();

            // 2. Xác định Best Seller: sách có BuyCount > 10
            var bestSellerBooks = _context.Books
                .Where(b => b.BuyCount > 10)
                .ToList();

            foreach (var book in bestSellerBooks)
            {
                book.TagID = BEST_SELLER_TAG_ID;
            }
            _context.SaveChanges();
        }

        // 4. Modify the Index method in HomeController to ensure sorting on the homepage
        // In HomeController.cs
        public IActionResult Index(string seriesIds = null, int minPrice = 10000, int maxPrice = 500000)
        {
            // Xác định và cập nhật sách theo Tags trước khi hiển thị
            UpdateBookTags();

            // Lấy danh sách genres cho bộ lọc
            var genres = _context.Genres.ToList();
            ViewBag.Genres = genres;

            // Thiết lập giá trị mặc định cho bộ lọc
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;

            // Lấy danh sách tags cùng với các sách đã được gán tag
            var tags = _context.Tags
                        .Include(x => x.Books.Where(b => b.Price >= minPrice && b.Price <= maxPrice))
                        .ToList();

            // Nếu có lọc theo series
            if (!string.IsNullOrEmpty(seriesIds))
            {
                var seriesIdsList = seriesIds.Split(',').Select(Guid.Parse).ToList();
                foreach (var tag in tags)
                {
                    if (tag.Books != null)
                    {
                        tag.Books = tag.Books.Where(b => b.SeriesID.HasValue && seriesIdsList.Contains(b.SeriesID.Value)).ToList();
                    }
                }
            }

            // ĐẢM BẢO sách Best Seller được sắp xếp theo BuyCount trong trang chủ
            var bestSellerTag = tags.FirstOrDefault(t => t.ID == BEST_SELLER_TAG_ID);
            if (bestSellerTag != null && bestSellerTag.Books != null && bestSellerTag.Books.Any())
            {
                // Sắp xếp sách Best Seller theo BuyCount giảm dần (cao đến thấp)
                bestSellerTag.Books = bestSellerTag.Books.OrderByDescending(b => b.BuyCount).ToList();
            }

            // Lấy thông tin khuyến mãi hiện tại
            ViewBag.CurrentPromotion = _promotionService.GetActivePromotions().FirstOrDefault();

            // Lấy đợt khuyến mãi cho sách best seller trong 1 tháng
            var bestSellerPromotions = _promotionService.GetBestSellerPromotions();
            ViewBag.BestSellerPromotions = bestSellerPromotions;

            return View(tags);
        }

        // API endpoint để lọc sách theo nhiều điều kiện
        [HttpPost]
        public IActionResult FilterBooks(string seriesIds, int minPrice = 10000, int maxPrice = 500000)
        {
            try
            {
                UpdateBookTags();

                var tags = _context.Tags
                    .Include(x => x.Books.Where(b => b.Price >= minPrice && b.Price <= maxPrice))
                    .ToList();

                // Nếu có lọc theo series
                if (!string.IsNullOrEmpty(seriesIds))
                {
                    var seriesIdsList = seriesIds.Split(',').Select(Guid.Parse).ToList();
                    foreach (var tag in tags)
                    {
                        if (tag.Books != null)
                        {
                            tag.Books = tag.Books.Where(b => b.SeriesID.HasValue && seriesIdsList.Contains(b.SeriesID.Value)).ToList();
                        }
                    }
                }

                // Sắp xếp Best Seller
                var bestSellerTag = tags.FirstOrDefault(t => t.ID == BEST_SELLER_TAG_ID);
                if (bestSellerTag != null && bestSellerTag.Books != null && bestSellerTag.Books.Any())
                {
                    bestSellerTag.Books = bestSellerTag.Books.OrderByDescending(b => b.BuyCount).ToList();
                }

                return Json(new { success = true, data = tags });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(int statusCode)
        {
            if (statusCode == HttpStatusCodeConstant.NOT_FOUND)
            {
                return View("NotFound");
            }
            else if (statusCode == HttpStatusCodeConstant.INTERNAL_ERROR)
            {
                return View("InternalError");
            }
            else
            {
                return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }

        [HttpPost]
        public IActionResult HandleSearch(SearchDTO req)
        {
            return Redirect($"{RouteConstant.LIST}?input={req.KeySearch}&Hambuger=Search");
        }
        // API để kết thúc chiến dịch khuyến mãi
        [HttpPost("/Promotion/End")]
        public IActionResult EndPromotion(Guid promotionId)
        {
            var promotion = _context.Promotions.Find(promotionId);
            if (promotion != null)
            {
                // Kết thúc chiến dịch bằng cách đặt IsActive = false
                promotion.IsActive = false;
                promotion.EndDate = DateTime.Now;
                _context.SaveChanges();
                return Ok(new ResponseAPI<Promotion>() { Success = true, Message = "Promotion ended successfully", Data = promotion });
            }

            return NotFound(new ResponseAPI<string>() { Success = false, Message = "Promotion not found", Data = null });
        }
    }
}