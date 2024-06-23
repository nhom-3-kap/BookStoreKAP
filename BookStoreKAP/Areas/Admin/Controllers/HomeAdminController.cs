using BookStoreKAP.Common.Constants;
using BookStoreKAP.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookStoreKAP.Areas.Admin.Controllers
{
    [Area(AreasConstant.ADMIN)]
    public class HomeAdminController : Controller
    {
        [PermissionFilter(Name = "CanView")]
        public IActionResult Index()
        {
            TempData[ToastrConstant.SUCCESS_MSG] = "Welcome Back 🙋‍";
            return View();
        }
    }
}
