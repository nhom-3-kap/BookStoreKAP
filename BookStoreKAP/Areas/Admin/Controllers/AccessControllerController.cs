using BookStoreKAP.Data;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using BookStoreKAP.Models.Entities;
using BookStoreKAP.Common.Constants;
using Microsoft.AspNetCore.Authorization;

namespace BookStoreKAP.Areas.Admin.Controllers
{
    [Area(AreasConstant.ADMIN)]
    public class AccessControllerController : Controller
    {
        private readonly BookStoreKAPDBContext _context;

        public AccessControllerController(BookStoreKAPDBContext context)
        {
            _context = context;
        }

        [Authorize(Policy = "CanView")]
        public IActionResult Index()
        {
            var controllerList = _context.AccessControllers.OrderByDescending(x => x.AreaName).ToList();

            return View(controllerList);
        }

        [Authorize(Policy = "CanView")]
        public IActionResult RefreshList()
        {
            var controllerListOnProject = Assembly.GetExecutingAssembly()
                                                    .GetTypes()
                                                    .Where(type => typeof(Controller).IsAssignableFrom(type) || typeof(ControllerBase).IsAssignableFrom(type))
                                                    .Select(type => new
                                                    {
                                                        Name = type.Name[..type.Name.LastIndexOf("Controller")],
                                                        AreaName = type.GetCustomAttribute<AreaAttribute>()?.RouteValue
                                                    })
                                                    .ToDictionary(x => x.Name);

            var accessControllerListOnDB = _context.AccessControllers.ToList();

            foreach (var accessController in accessControllerListOnDB)
            {
                if (!controllerListOnProject.TryGetValue(accessController.Name, out var type))
                {
                    _context.AccessControllers.Remove(accessController);
                }
            }

            var accessControllerNewList = _context.AccessControllers.ToList();

            foreach (var controller in controllerListOnProject)
            {
                if (accessControllerNewList.Any(x => x.Name == controller.Key))
                {
                    continue;
                }
                _context.AccessControllers.Add(new AccessController()
                {
                    Name = controller.Key,
                    AreaName = controller.Value.AreaName
                });
            }

            _context.SaveChanges();

            TempData[ToastrConstant.SUCCESS_MSG] = "Refresh Successfully";
            return RedirectToAction("Index");
        }
    }
}
