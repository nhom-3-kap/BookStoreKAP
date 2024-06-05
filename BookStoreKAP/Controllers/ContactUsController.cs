using Microsoft.AspNetCore.Mvc;

namespace BookStoreKAP.Controllers
{
    public class ContactUsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
