using Microsoft.AspNetCore.Mvc;
namespace BookStoreKAP.Controllers
{
    public class LoginController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
