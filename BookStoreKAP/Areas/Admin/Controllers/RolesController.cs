using BookStoreKAP.Common.Constants;
using BookStoreKAP.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookStoreKAP.Areas.Admin.Controllers
{
    [Area(AreasConstant.ADMIN)]
    [Authorize(Roles = RolesConstant.ADMIN)]
    public class RolesController : Controller
    {
        private readonly BookStoreKAPDBContext _context;

        public RolesController(BookStoreKAPDBContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var roles = _context.Roles.ToList();
            return View(roles);
        }

        public IActionResult Create()
        {
            return View();
        }
    }
}
