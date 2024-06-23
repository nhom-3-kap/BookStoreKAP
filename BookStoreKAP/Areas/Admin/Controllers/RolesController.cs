using BookStoreKAP.Common.Constants;
using BookStoreKAP.Common.Extensions;
using BookStoreKAP.Data;
using BookStoreKAP.Filters;
using BookStoreKAP.Models;
using BookStoreKAP.Models.DTO;
using BookStoreKAP.Models.Entities;
using BookStoreKAP.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace BookStoreKAP.Areas.Admin.Controllers
{
    [Area(AreasConstant.ADMIN)]
    public class RolesController : Controller
    {
        private readonly BookStoreKAPDBContext _context;
        private readonly RoleManager<Role> _roleManager;

        public RolesController(BookStoreKAPDBContext context, RoleManager<Role> roleManager)
        {
            _context = context;
            _roleManager = roleManager;
        }

        [PermissionFilter(Name = "CanView")]
        public IActionResult Index()
        {
            var roles = _context.Roles.Where(x => x.NormalizedName != "ROOT").ToList();
            return View(roles);
        }

        [PermissionFilter(Name = "CanViewCreate")]
        public IActionResult Create()
        {
            return View();
        }

        [PermissionFilter(Name = "CanSaveCreate")]
        [HttpPost]
        public async Task<IActionResult> Create(ReqCreateRole req)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    throw new Exception("Values invalid");
                }

                var role = new Role() { Name = req.Name };

                await _roleManager.CreateAsync(role);

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

        [PermissionFilter(Name = "CanViewEdit")]
        public IActionResult Modify(Guid roleID)
        {
            var role = _context.Roles.Where(x => x.Id == roleID).SingleOrDefault();
            if (role == null)
            {
                return NotFound();
            }

            var accessControllers = _context.AccessControllers.Include(x => x.Policies)
                .Where(x => x.AreaName == "Admin" || x.AreaName == "Api")
                .OrderByDescending(x => x.AreaName)
                .ThenByDescending(x => x.Policies.Count)
                .ToList();

            var currentPolicy = _context.RoleClaims
                .Where(rc => rc.RoleId == roleID)
                .ToList();

            var roleModifyVM = new ModifyRoleVM()
            {
                Role = role,
                AccessControllers = accessControllers,
                CurrentPolicy = currentPolicy
            };

            return View(roleModifyVM);
        }


        [PermissionFilter(Name = "CanSaveEdit")]
        [HttpPost]
        public IActionResult Modify(ReqModifyRole req)
        {
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                var role = _context.Roles.Include(x => x.UserRoles).SingleOrDefault(x => x.Id == req.Id);
                if (role == null)
                {
                    return NotFound();
                }

                // Xóa các RoleClaim hiện tại của Role
                var existingRoleClaims = _context.RoleClaims.Where(rc => rc.RoleId == req.Id).ToList();
                _context.RoleClaims.RemoveRange(existingRoleClaims);

                if (req.PolicyIDs != null && req.PolicyIDs.Count > 0)
                {
                    var accessControllerIds = _context.Policies.Include(x => x.AccessController).Where(x => req.PolicyIDs.Contains(x.ID)).GroupBy(x => x.AccessControllerID).Select(x => x.Key).ToList();
                    var domains = _context.Domains.Where(x => accessControllerIds.Contains(x.AccessControllerID) && x.RoleID == req.Id).ToList();

                    foreach (var accessControllerId in accessControllerIds)
                    {
                        if (domains.Any(x => x.AccessControllerID == accessControllerId))
                        {
                            continue;
                        }

                        var newDomain = new Domain()
                        {
                            AccessControllerID = accessControllerId,
                            RoleID = req.Id,
                        };

                        _context.Domains.Add(newDomain);
                    }

                    // Thêm các RoleClaim mới dựa trên PolicyIDs từ request
                    foreach (var policyID in req.PolicyIDs)
                    {
                        var policy = _context.Policies.Include(x => x.AccessController).FirstOrDefault(x => x.ID == policyID);
                        if (policy != null)
                        {
                            var roleClaim = new RoleClaim()
                            {
                                RoleId = req.Id,
                                ClaimType = "Permission",
                                ClaimValue = policy.Name,
                                ActionName = policy.ActionName,
                                ControllerName = policy.AccessController.Name
                            };



                            _context.RoleClaims.Add(roleClaim);
                        }
                    }
                }

                _context.SaveChanges();
                transaction.Commit();

                TempData[ToastrConstant.SUCCESS_MSG] = "Modify Successfully";
                return RedirectToAction("Index", new { menuKey = "RM" });
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                TempData[ToastrConstant.ERROR_MSG] = ex.Message;
                return View(req);
            }
        }

        [PermissionFilter(Name = "CanRefreshController")]
        public IActionResult RefreshListController(Guid roleID)
        {

            RefreshListController();
            TempData[ToastrConstant.SUCCESS_MSG] = "Refresh Successfully";
            return RedirectToAction("Modify", new { roleID, menuKey = "RM" });
        }

        [PermissionFilter(Name = "CanRefreshPermission")]
        public IActionResult RefreshListPermissions(Guid roleID)
        {
            RefreshListPermissions();
            TempData["SUCCESS_MSG"] = "Refresh Successfully";
            return RedirectToAction("Modify", new { roleID, menuKey = "RM" });
        }

        [PermissionFilter(Name = "CanDelete")]
        [HttpDelete]
        public IActionResult RemoveRoleByRoleIdAPI(Guid roleID)
        {
            try
            {
                //var accessControllerExists = _context.AccessControllers.Where(x => x.)

                var domainsExists = _context.Domains.Where(x => x.RoleID == roleID);
                _context.Domains.RemoveRange(domainsExists);

                var roleClaimsExists = _context.RoleClaims.Where(x => x.RoleId == roleID);
                _context.RoleClaims.RemoveRange(roleClaimsExists);

                var userRoleExists = _context.UserRoles.Where(x => x.RoleId == roleID);
                _context.UserRoles.RemoveRange(userRoleExists);

                var roleExists = _context.Roles.First(x => x.Id == roleID);
                _context.Roles.Remove(roleExists);

                _context.SaveChanges();
                return Ok(new ResponseAPI<string>() { Success = true, Message = "Deleted Successfully" });
            }
            catch (Exception ex)
            {
                return Ok(new ResponseAPI<string>() { Success = false, Message = ex.Message });
            }
        }

        public void RefreshListController()
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

                var accessController = new AccessController()
                {
                    Name = controller.Key,
                    AreaName = controller.Value.AreaName
                };

                if (string.IsNullOrEmpty(controller.Value.AreaName))
                {
                    accessController.Status = AccessControllerStatusConstant.PUBLIC;
                }
                else
                {
                    accessController.Status = AccessControllerStatusConstant.PRIVATE;
                }

                _context.AccessControllers.Add(accessController);
            }

            _context.SaveChanges();
        }

        public void RefreshListPermissions()
        {
            var controllerActionPermissionList = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(type => typeof(Controller).IsAssignableFrom(type) || typeof(ControllerBase).IsAssignableFrom(type))
                .SelectMany(type => type.GetMethods(BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public)
                    .Where(m => !m.IsDefined(typeof(NonActionAttribute)) && m.IsPublic)
                    .Select(m => new
                    {
                        ControllerName = type.Name[..type.Name.LastIndexOf("Controller")],
                        ActionName = m.Name,
                        Permissions = m.GetCustomAttributes<PermissionFilter>()
                                       .Where(attr => !string.IsNullOrEmpty(attr.Name))
                                       .Select(attr => attr.Name)
                                       .ToList()
                    }))
                .GroupBy(x => x.ControllerName)
                .ToDictionary(g => g.Key, g => g.Select(x => new { x.ActionName, x.Permissions }).ToList());

            var permissionListOnDB = _context.Policies.Include(p => p.AccessController).ToList();

            // Remove permissions from database if they do not exist in the project
            foreach (var permission in permissionListOnDB)
            {
                if (!controllerActionPermissionList.TryGetValue(permission.AccessController.Name, out var actions) ||
                    !actions.Any(a => a.ActionName == permission.ActionName && a.Permissions.Contains(permission.Name)))
                {
                    _context.Policies.Remove(permission);
                }
            }

            var permissionNewList = _context.Policies.Include(p => p.AccessController).ToList();

            // Add new permissions to database if they do not exist for the same controller
            foreach (var controller in controllerActionPermissionList)
            {
                var accessController = _context.AccessControllers.FirstOrDefault(ac => ac.Name == controller.Key);
                if (accessController == null)
                {
                    continue;
                }

                foreach (var action in controller.Value)
                {
                    foreach (var permission in action.Permissions)
                    {
                        if (permissionNewList.Any(p => p.AccessController.Name == controller.Key && p.ActionName == action.ActionName && p.Name == permission))
                        {
                            continue;
                        }

                        _context.Policies.Add(new Policy()
                        {
                            Name = permission,
                            ActionName = action.ActionName,
                            AccessControllerID = accessController.ID
                        });
                    }
                }
            }

            _context.SaveChanges();
        }
    }
}
