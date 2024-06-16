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

        public async Task<IActionResult> Index(ReqSearchUserDTO req, string menuKey, int page = 1, int pageSize = 2)
        {
            req.menuKey ??= menuKey;
            var users = _userManager.Users
                                   .Where(u =>
                                                (string.IsNullOrEmpty(req.FirstName) || u.FirstName.ToUpper().Contains(req.FirstName.ToUpper())) &&
                                                (string.IsNullOrEmpty(req.LastName) || u.LastName.ToUpper().Contains(req.LastName.ToUpper())) &&
                                                (string.IsNullOrEmpty(req.PhoneNumber) || u.PhoneNumber.ToUpper().Contains(req.PhoneNumber.ToUpper())) &&
                                                (string.IsNullOrEmpty(req.Email) || u.Email.ToUpper().Contains(req.Email.ToUpper())) &&
                                                (string.IsNullOrEmpty(req.Username) || u.UserName.ToUpper().Contains(req.Username.ToUpper())) &&
                                                (req.RoleId == Guid.Empty || (u.UserRoles != null && u.UserRoles.Any(ur => ur.RoleId == req.RoleId)))
                                         )
                                   .OrderBy(u => u.LastName)
                                   .ToList();

            var totalItems = users.Count;
            var pagedUsers = users.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            var userRolesViewModel = new List<UserRolesViewModel>();

            foreach (var user in pagedUsers)
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
                SearchParams = req,
                Action = "Index",
                Controller = "Users"
            };

            return View(userRolesViewModel);
        }

        public IActionResult Create()
        {
            var roles = _roleManager.Roles.ToList();
            ViewBag.Roles = roles;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(ReqCreateUserDTO req)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ErrorMessage = "Tạo user thất bại";
                var roles = _roleManager.Roles.ToList();
                ViewBag.Roles = roles;
                return View();
            }

            var user = new User()
            {
                FirstName = req.FirstName,
                LastName = req.LastName,
                Email = req.Email,
                BOD = req.BOD,
                PhoneNumber = req.PhoneNumber,
                UserName = req.Username,

            };

            var userCreated = await _userManager.CreateAsync(user, req.Password);

            if (userCreated.Succeeded)
            {
                var role = await _roleManager.FindByIdAsync(req.RoleId.ToString());
                if (role != null)
                {
                    await _userManager.AddToRoleAsync(user, role.Name);
                }
            }

            return View();
        }
    }
}
