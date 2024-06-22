using Microsoft.AspNetCore.Mvc;

namespace BookStoreKAP.Controllers
{
    public class ProductDetailsController : Controller
    {
        [Route("/ProductDetails/{ProductId}")]
        public IActionResult Index(string ProductId)
        {
            ViewBag.ProductId = ProductId;
            return View();
        }
    }
}
