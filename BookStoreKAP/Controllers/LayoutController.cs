using BookStoreKAP.Data;
using Microsoft.AspNetCore.Mvc;
using SQLitePCL;

namespace BookStoreKAP.Controllers
{
    public class LayoutController : Controller
    {
        private readonly BookStoreKAPDBContext _context;

        public LayoutController(BookStoreKAPDBContext context)
        {
            _context = context;
        }

        public IActionResult GetTagNavigation()
        {
            var tags = _context.Tags.ToList();
            return PartialView("_GetTagNavigate",tags);
        }
    }
}
