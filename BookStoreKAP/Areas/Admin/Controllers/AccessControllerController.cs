using BookStoreKAP.Data;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using BookStoreKAP.Models.Entities;
using BookStoreKAP.Common.Constants;
using Microsoft.AspNetCore.Authorization;

namespace BookStoreKAP.Areas.Admin.Controllers
{
    [Area(AreasConstant.ADMIN), Authorize(Roles = RolesConstant.ADMIN)]
    public class AccessControllerController : Controller
    {
        private readonly BookStoreKAPDBContext _context;

        public AccessControllerController(BookStoreKAPDBContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var controllerList = _context.AccessControllers.ToList();

            return View(controllerList);
        }


        public IActionResult RefreshList()
        {
            var controllerListOnProject = Assembly.GetExecutingAssembly()
                                                    .GetTypes()
                                                    .Where(type => typeof(Controller)
                                                    .IsAssignableFrom(type) || typeof(ControllerBase)
                                                    .IsAssignableFrom(type))
                                                    .ToDictionary(x => x.Name[..x.Name.LastIndexOf("Controller")]);
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
                });
            }

            _context.SaveChanges();


            return RedirectToAction("Index");
        }
    }
}
