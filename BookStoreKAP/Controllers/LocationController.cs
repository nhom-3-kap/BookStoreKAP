using Microsoft.AspNetCore.Mvc;

namespace BookStoreKAP.Controllers
{
    public class LocationController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
