using BookStoreKAP.Common.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookStoreKAP.Areas.Admin.Controllers
{
    [Area(AreasConstant.ADMIN), Authorize(Roles = RolesConstant.ADMIN)]
    public class HomeAdminController : Controller
    {
        public IActionResult Index()
        {
            TempData[ToastrConstant.SUCCESS_MSG] = "Welcome Back 🙋‍";
            return View();
        }
    }
}
