using Microsoft.AspNetCore.Mvc;
namespace BookStoreKAP.Controllers
{
    public class ForgotPasswordController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
