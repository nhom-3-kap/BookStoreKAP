using BookStoreKAP.Common.Constants;
using BookStoreKAP.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BookStoreKAP.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
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
