using BookStoreKAP.Common.Constants;
using BookStoreKAP.Data;
using BookStoreKAP.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace BookStoreKAP.Controllers
{
    public class HomeController : Controller
    {
        private readonly BookStoreKAPDBContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, BookStoreKAPDBContext context)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult Index()
        {
            var tags = _context.Tags.Include(x=>x.Books).ToList();
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
    }
}
