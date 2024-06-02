using Microsoft.AspNetCore.Mvc;

namespace BookStoreKAP.Controllers
{
    public class BlogController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
