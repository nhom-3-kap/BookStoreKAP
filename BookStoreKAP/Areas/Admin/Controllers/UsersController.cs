using BookStoreKAP.Common.Constants;
using BookStoreKAP.Database;
using BookStoreKAP.Models;
using BookStoreKAP.Models.DTO;
using BookStoreKAP.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookStoreKAP.Areas.Admin.Controllers
{
    [Area(AreasConstant.ADMIN)]
    public class UsersController : Controller
    {
        private readonly RoleManager<Role> _roleManager;
        private readonly UserManager<User> _userManager;
        private readonly BookStoreKAPDBContext _context;
        public UsersController(RoleManager<Role> roleManager, UserManager<User> userManager, BookStoreKAPDBContext context)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index([FromQuery] ReqQuerySearchUserDTO req)
        {

            req.menuKey ??= req.menuKey;
            var users = _userManager.Users
                                   .Where(u =>
                                                (string.IsNullOrEmpty(req.FirstName) || u.FirstName.ToUpper().Contains(req.FirstName.ToUpper())) &&
                                                (string.IsNullOrEmpty(req.LastName) || u.LastName.ToUpper().Contains(req.LastName.ToUpper())) &&
                                                (string.IsNullOrEmpty(req.PhoneNumber) || u.PhoneNumber.ToUpper().Contains(req.PhoneNumber.ToUpper())) &&
                                                (string.IsNullOrEmpty(req.Email) || u.Email.ToUpper().Contains(req.Email.ToUpper())) &&
                                                (string.IsNullOrEmpty(req.Username) || u.UserName.ToUpper().Contains(req.Username.ToUpper())) &&
                                                (req.RoleIds == null || req.RoleIds.Count == 0 || (u.UserRoles != null && u.UserRoles.Any(ur => req.RoleIds.Contains(ur.RoleId))))
                                         )
                                   .OrderBy(u => u.LastName)
                                   .ToList();

            var totalItems = users.Count;
            var pagedUsers = users.Skip((req.page - 1) * req.pageSize).Take(req.pageSize).ToList();

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
                CurrentPage = req.page,
                PageSize = req.pageSize,
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

        public async Task<IActionResult> Modify(Guid userID)
        {
            var user = _userManager.Users.Where(x => x.Id.Equals(userID)).FirstOrDefault();
            var roles = _roleManager.Roles.ToList();
            var rolesNames = new List<string>();
            if (user != null)
            {
                var resRoles = await _userManager.GetRolesAsync(user);
                rolesNames = resRoles.ToList();
            }
            var roleGuids = _roleManager.Roles.Where(x => rolesNames.Contains(x.NormalizedName)).Select(x => x.Id).ToList();

            user ??= new User();
            ViewBag.RoleGuids = roleGuids;
            ViewBag.Roles = roles;
            return View(user);
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

        [HttpDelete]
        public async Task<IActionResult> RemoveUserByIDAPI(Guid userID)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var user = await _userManager.FindByIdAsync(userID.ToString()) ?? throw new Exception("User Is Not Found");

                // Lấy tất cả các UserRoles liên quan đến người dùng
                var userRoles = await _context.UserRoles.Where(ur => ur.UserId == userID).ToListAsync();

                // Xóa tất cả các UserRoles liên quan
                _context.UserRoles.RemoveRange(userRoles);
                await _context.SaveChangesAsync();

                // Xóa người dùng
                var result = await _userManager.DeleteAsync(user);

                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description).ToArray();
                    throw new Exception("Delete user failed! Errors: " + string.Join(", ", errors));
                }

                // Commit transaction
                await transaction.CommitAsync();

                return Ok(new ResponseAPI<string>() { Success = true, Message = "Remove Success" });
            }
            catch (Exception ex)
            {
                // Rollback transaction nếu có lỗi
                await transaction.RollbackAsync();
                return Ok(new ResponseAPI<string>() { Success = false, Message = ex.Message });
            }
        }

    }
}
