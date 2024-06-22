using BookStoreKAP.Common.Constants;
using BookStoreKAP.Data;
using BookStoreKAP.Models;
using BookStoreKAP.Models.Entities;
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
            
            List<Book> lst = _context.Books.Include(x=>x.Sales).ToList();
            return View(lst);
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
