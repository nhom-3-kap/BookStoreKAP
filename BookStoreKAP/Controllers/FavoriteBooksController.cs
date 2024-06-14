using Microsoft.AspNetCore.Mvc;

namespace BookStoreKAP.Controllers
{
    public class FavoriteBooksController : Controller
    {
        public IActionResult Index()

        {
            return View();
        }
    }
}
