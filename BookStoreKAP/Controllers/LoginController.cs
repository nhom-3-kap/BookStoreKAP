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

        // Action method for /Login/Register
        [Route("/Login/Register")]
        public IActionResult Register()
        {
            ViewBag.Service = "Register";
            return View();
        }

        // Action method for /Login/ForgotPassword
        [Route("/Login/ForgotPassword")]
        public IActionResult ForgotPassword()
        {
            ViewBag.Service = "Forgot Password";
            return View();
        }
    }
}
