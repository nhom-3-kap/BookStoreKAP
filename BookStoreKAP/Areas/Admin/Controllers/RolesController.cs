using BookStoreKAP.Common.Constants;
using BookStoreKAP.Common.Extensions;
using BookStoreKAP.Data;
using BookStoreKAP.Models.DTO;
using BookStoreKAP.Models.Entities;
using BookStoreKAP.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace BookStoreKAP.Areas.Admin.Controllers
{
    [Area(AreasConstant.ADMIN)]
    public class RolesController : Controller
    {
        private readonly BookStoreKAPDBContext _context;

        public RolesController(BookStoreKAPDBContext context)
        {
            _context = context;
        }

        //[Authorize(Policy = "CanView")]
        public IActionResult Index()
        {
            var roles = _context.Roles.ToList();
            return View(roles);
        }

        //[Authorize(Policy = "CanCreate")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(ReqCreateRole req)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    throw new Exception("Values invalid");
                }

                var role = new Role() { Name = req.Name, NormalizedName = req.Name.ToSnakeCase() };

                _context.Roles.Add(role);
                _context.SaveChanges();

                TempData[ToastrConstant.SUCCESS_MSG] = "Create Successfully";
                return RedirectToAction("Index", new { menuKey = "RM" });
            }
            catch (Exception ex)
            {
                TempData[ToastrConstant.ERROR_MSG] = ex.Message;
                return View();
            }


        }

        public IActionResult Modify(Guid roleID)
        {
            var role = _context.Roles.Where(x => x.Id == roleID).SingleOrDefault();
            if (role == null)
            {
                return NotFound();
            }
            var accessControllers = _context.AccessControllers.Include(x => x.Policies).ToList();


            var roleModifyVM = new ModifyRoleVM() { Role = role, AccessControllers = accessControllers };
            return View(roleModifyVM);
        }

        [HttpPost]
        public IActionResult Modify(ReqModifyRole req)
        {
            return View();
        }

        public IActionResult RefreshListController(Guid roleID)
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
            return RedirectToAction("Modify", new { roleID, menuKey = "RM" });
        }

        public IActionResult RefreshListPolicy(Guid roleID)
        {
            var controllerActionPolicyListOnProject = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(type => typeof(Controller).IsAssignableFrom(type) || typeof(ControllerBase).IsAssignableFrom(type))
                .SelectMany(type => type.GetMethods(BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public)
                    .Where(m => !m.IsDefined(typeof(NonActionAttribute)) && m.IsPublic)
                    .Select(m => new
                    {
                        ControllerName = type.Name[..type.Name.LastIndexOf("Controller")],
                        ActionName = m.Name,
                        Policies = m.GetCustomAttributes<AuthorizeAttribute>()
                                    .Where(attr => !string.IsNullOrEmpty(attr.Policy))
                                    .Select(attr => attr.Policy)
                                    .ToList()
                    }))
                .GroupBy(x => x.ControllerName)
                .ToDictionary(g => g.Key, g => g.Select(x => new { x.ActionName, x.Policies }).ToList());

            var policyListOnDB = _context.Policies.Include(p => p.AccessController).ToList();

            // Remove policies from database if they do not exist in the project
            foreach (var policy in policyListOnDB)
            {
                if (!controllerActionPolicyListOnProject.TryGetValue(policy.AccessController.Name, out var actions) ||
                    !actions.Any(a => a.ActionName == policy.ActionName && a.Policies.Contains(policy.Name)))
                {
                    _context.Policies.Remove(policy);
                }
            }

            var policyNewList = _context.Policies.Include(p => p.AccessController).ToList();

            // Add new policies to database if they do not exist for the same controller
            foreach (var controller in controllerActionPolicyListOnProject)
            {
                var accessController = _context.AccessControllers.FirstOrDefault(ac => ac.Name == controller.Key);
                if (accessController == null)
                {
                    continue;
                }

                var existingPoliciesForController = policyNewList
                    .Where(p => p.AccessController.Name == controller.Key)
                    .Select(p => p.Name)
                    .ToHashSet();

                foreach (var action in controller.Value)
                {
                    foreach (var policy in action.Policies)
                    {
                        if (existingPoliciesForController.Contains(policy))
                        {
                            continue;
                        }

                        _context.Policies.Add(new Policy()
                        {
                            Name = policy,
                            ActionName = action.ActionName,
                            AccessControllerID = accessController.ID
                        });

                        existingPoliciesForController.Add(policy);
                    }
                }
            }

            _context.SaveChanges();

            TempData[ToastrConstant.SUCCESS_MSG] = "Refresh Successfully";
            return RedirectToAction("Modify", new { menuKey = "RM", roleID });
        }

    }
}
