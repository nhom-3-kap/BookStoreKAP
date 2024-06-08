using Microsoft.AspNetCore.Mvc;

namespace BookStoreKAP.Controllers
{
    public class BookRequestController : Controller
    {
        [Route("/BookRequest")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
