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

        public async Task<IActionResult> Index(ReqSearchUserDTO req)
        {
            ViewBag.ReqSearch = req;
            ViewBag.Roles = _roleManager.Roles.ToList();


            var users = new List<User>();
            if (req != null && req.RoleId != Guid.Empty)
            {
                users = _userManager.Users.Where(u => u.UserName == req.Username || (u.UserRoles != null && u.UserRoles.Any(ur => ur.RoleId == req.RoleId))).ToList();
            }
            else
            {
                users = _userManager.Users.ToList();
            }

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

            return View(userRolesViewModel);
        }
    }
}
