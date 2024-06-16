using BookStoreKAP.Common.Constants;
using BookStoreKAP.Models.DTO;
using BookStoreKAP.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace BookStoreKAP.Areas.Admin.Controllers
{
    [Area(AreasConstant.ADMIN)]
    public class UsersController : Controller
    {
        private readonly RoleManager<Role> _roleManager;
        private readonly UserManager<User> _userManager;
        public UsersController(RoleManager<Role> roleManager, UserManager<User> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(ReqSearchUserDTO req, int page = 1, int pageSize = 10)
        {
            var users = _userManager.Users
                                   .Where(u =>
                                                (string.IsNullOrEmpty(req.FirstName) || u.FirstName.Contains(req.FirstName)) &&
                                                (string.IsNullOrEmpty(req.LastName) || u.LastName.Contains(req.LastName)) &&
                                                (string.IsNullOrEmpty(req.PhoneNumber) || u.PhoneNumber.Contains(req.PhoneNumber)) &&
                                                (string.IsNullOrEmpty(req.Email) || u.Email.Contains(req.Email)) &&
                                                (string.IsNullOrEmpty(req.Username) || u.UserName.Contains(req.Username)) &&
                                                (req.RoleId == Guid.Empty || (u.UserRoles != null && u.UserRoles.Any(ur => ur.RoleId == req.RoleId)))
                                         )
                                   .OrderBy(u => u.LastName)
                                   .ToList();

            var totalItems = users.Count;
            var pagedUsers = users.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            var userRolesViewModel = new List<UserRolesViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userRolesViewModel.Add(new UserRolesViewModel
                {
                    User = user,
                    Roles = roles.ToList()
                });
            }

            ViewBag.ReqSearch = req;
            ViewBag.Roles = _roleManager.Roles.ToList();
            ViewBag.Pagination = new PaginationModel()
            {
                TotalItems = totalItems,
                CurrentPage = page,
                PageSize = pageSize,
                Action = "Index",
                Controller = "Users"
            };

            return View(userRolesViewModel);
        }
    }
}
