using Microsoft.AspNetCore.Mvc;
namespace BookStoreKAP.Controllers
{
    public class LoginController : Controller
    {
        [Route("/Login")]
        public IActionResult Index(string Service)
        {
            ViewBag.Service = "Login";
            return View();
        }

        [Route("/Register")]
        public IActionResult Register()
        {
            ViewBag.Service = "Register";
            return View();
        }


        [Route("/ForgotPassword")]
        public IActionResult ForgotPassword()
        {
            ViewBag.Service = "Forgot Password";
            return View();
        }
    }
}
