using BookStoreKAP.Common.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookStoreKAP.Areas.Admin.Controllers
{
    [Area(AreasConstant.ADMIN)]
    public class HomeAdminController : Controller
    {
        //[Authorize(Roles = "Admin", Policy = "CanView")]
        public IActionResult Index()
        {
            TempData[ToastrConstant.SUCCESS_MSG] = "Welcome Back 🙋‍";
            return View();
        }
    }
}
