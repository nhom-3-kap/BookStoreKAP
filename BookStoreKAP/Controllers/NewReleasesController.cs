using Microsoft.AspNetCore.Mvc;

namespace BookStoreKAP.Controllers
{

    public class NewReleasesController : Controller

    {
        [Route("/List")]
        public IActionResult Index(string Service)
        {
            if (Service == null)
            {
                return NotFound();
            } else if(Service=="NewReleases") {
                ViewBag.Service = "New Releases";
                return View();

            }else if (Service == "BestSellers")
            {
                ViewBag.Service = "Best Sellers";
                return View();
            }
            return View();

        }
    }
}
