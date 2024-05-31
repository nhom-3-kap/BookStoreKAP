using Microsoft.AspNetCore.Mvc;

namespace BookStoreKAP.Controllers
{
    public class NewReleasesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
