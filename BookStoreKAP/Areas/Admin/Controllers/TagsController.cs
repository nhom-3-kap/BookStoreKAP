using BookStoreKAP.Database;
using Microsoft.AspNetCore.Mvc;

namespace BookStoreKAP.Areas.Admin.Controllers
{
    public class TagsController : Controller
    {
        private readonly BookStoreKAPDBContext _context;

        public TagsController(BookStoreKAPDBContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var tags = _context.Tags.ToList();
            return View(tags);
        }
    }
}
