using BookStoreKAP.Data;
using Microsoft.AspNetCore.Mvc;

namespace BookStoreKAP.Controllers
{
    public class BookRequestController : Controller
    {
        private readonly BookStoreKAPDBContext _context;

        public BookRequestController(BookStoreKAPDBContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var tags = _context.Tags.ToList(); // Lấy danh sách tags từ database
            var series = _context.Series.ToList();
            var genres = _context.Genres.ToList();

            ViewBag.Tags = tags;  // Truyền vào View
            ViewBag.Series = series;
            ViewBag.Genres = genres;

            return View();
        }
    }
}
