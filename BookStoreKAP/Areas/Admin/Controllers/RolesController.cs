using BookStoreKAP.Common.Constants;
using BookStoreKAP.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookStoreKAP.Areas.Admin.Controllers
{
    [Area(AreasConstant.ADMIN)]
    public class RolesController : Controller
    {
        private readonly BookStoreKAPDBContext _context;

        public RolesController(BookStoreKAPDBContext context)
        {
            _context = context;
        }

        [Authorize(Policy = "CanView")]
        public IActionResult Index()
        {
            var roles = _context.Roles.ToList();
            return View(roles);
        }

        [Authorize(Policy = "CanCreate")]
        public IActionResult Create()
        {
            return View();
        }
    }
}
