using Microsoft.AspNetCore.Mvc;

namespace BookStoreKAP.Controllers
{
    public class AboutUsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
