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

        public HomeController(ILogger<HomeController> logger, BookStoreKAPDBContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            // Xác ??nh và c?p nh?t sách theo Tags tr??c khi hi?n th?
            UpdateBookTags();

            // L?y danh sách tags cùng v?i các sách ?ã ???c gán tag
            var tags = _context.Tags.Include(x => x.Books).ToList();
            return View(tags);
        }

        private void UpdateBookTags()
        {
            var BEST_SELLER_TAG_ID = new Guid("3E5CAC9B-6E5A-416D-86F8-52F044D6994E");
            var NEW_RELEASE_TAG_ID = new Guid("CA038048-95D2-4BFD-86D8-740FB2ECE1AF");

            // Reset tags tr??c khi c?p nh?t
            var allBooks = _context.Books.ToList();
            foreach (var book in allBooks)
            {
                book.TagID = null;
            }
            _context.SaveChanges();

            // 1. Xác ??nh sách New Release: sách có CreatedAt trong vòng 1 tháng
            var oneMonthAgo = DateTime.Now.AddMonths(-1);
            var newReleaseBooks = _context.Books
                .Where(b => b.CreatedAt >= oneMonthAgo)
                .ToList();

            foreach (var book in newReleaseBooks)
            {
                book.TagID = NEW_RELEASE_TAG_ID;
            }
            _context.SaveChanges();

            // 2. Xác ??nh Best Seller: sách có BuyCount > 10
            var bestSellerBooks = _context.Books
                .Where(b => b.BuyCount > 10)
                .ToList();

            foreach (var book in bestSellerBooks)
            {
                book.TagID = BEST_SELLER_TAG_ID;
            }
            _context.SaveChanges();
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