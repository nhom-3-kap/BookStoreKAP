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
            foreach (var book in allBooks)
            {
                book.TagID = null;
            }
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
        public IActionResult Index()
        {
            // Xác định và cập nhật sách theo Tags trước khi hiển thị
            UpdateBookTags();

            // Lấy danh sách tags cùng với các sách đã được gán tag
            var tags = _context.Tags
                        .Include(x => x.Books)
                        .ToList();

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
    }
}
