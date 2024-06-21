using BookStoreKAP.Common.Constants;
using BookStoreKAP.Data;
using BookStoreKAP.Models;
using BookStoreKAP.Models.DTO;
using BookStoreKAP.Models.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BookStoreKAP.Areas.Admin.Controllers
{
    [Area(AreasConstant.ADMIN)]
    public class UsersController : Controller
    {
        private readonly RoleManager<Role> _roleManager;
        private readonly UserManager<User> _userManager;
        private readonly BookStoreKAPDBContext _context;
        private readonly SignInManager<User> _signInManager;
        public UsersController(RoleManager<Role> roleManager, UserManager<User> userManager, BookStoreKAPDBContext context, SignInManager<User> signInManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _context = context;
            _signInManager = signInManager;
        }

        [Authorize(Policy = "CanView")]
        public async Task<IActionResult> Index([FromQuery] ReqQuerySearchUser q)
        {

            q.menuKey ??= q.menuKey;
            var users = _userManager.Users.Include(x => x.UserRoles)
                                   .Where(u =>
                                                (string.IsNullOrEmpty(q.FirstName) || u.FirstName.ToUpper().Contains(q.FirstName.ToUpper())) &&
                                                (string.IsNullOrEmpty(q.LastName) || u.LastName.ToUpper().Contains(q.LastName.ToUpper())) &&
                                                (string.IsNullOrEmpty(q.PhoneNumber) || u.PhoneNumber.ToUpper().Contains(q.PhoneNumber.ToUpper())) &&
                                                (string.IsNullOrEmpty(q.Email) || u.Email.ToUpper().Contains(q.Email.ToUpper())) &&
                                                (string.IsNullOrEmpty(q.Username) || u.UserName.ToUpper().Contains(q.Username.ToUpper()))
                                         )
                                   .OrderBy(u => u.LastName)
                                   .ToList().Where(u => (q.RoleIds == null || q.RoleIds.Count == 0 || (u.UserRoles != null && u.UserRoles.Count != 0 && q.RoleIds.All(rid => u.UserRoles.Any(ur => ur.RoleId == rid))))).ToList();

            var totalItems = users.Count;
            var pagedUsers = users.Skip((q.Page - 1) * q.PageSize).Take(q.PageSize).ToList();

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

            ViewBag.ReqSearch = q;
            ViewBag.Roles = _roleManager.Roles.ToList();
            ViewBag.Pagination = new PaginationModel()
            {
                TotalItems = totalItems,
                CurrentPage = q.Page,
                PageSize = q.PageSize,
                SearchParams = q,
                Action = "Index",
                Controller = "Users"
            };

            return View(userRolesViewModel);
        }

        [Authorize(Policy = "CanCreate")]
        public IActionResult Create()
        {
            var roles = _roleManager.Roles.ToList();
            ViewBag.Roles = roles;

            return View();
        }

        [Authorize(Policy = "CanEdit")]
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

        [Authorize(Policy = "CanEdit")]
        [HttpPost]
        public async Task<IActionResult> Modify(ReqModifyUser req, IFormFile Avatar)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var user = await _userManager.FindByIdAsync(req.Id.ToString());
                if (user == null)
                {
                    return NotFound();
                }

                if (!string.IsNullOrEmpty(req.FirstName)) user.FirstName = req.FirstName;
                if (!string.IsNullOrEmpty(req.LastName)) user.LastName = req.LastName;
                if (!string.IsNullOrEmpty(req.Email)) user.Email = req.Email;
                if (!string.IsNullOrEmpty(req.PhoneNumber)) user.PhoneNumber = req.PhoneNumber;
                if (req.BOD != default) user.BOD = req.BOD;
                if (!string.IsNullOrEmpty(req.Username)) user.UserName = req.Username;

                if (!string.IsNullOrEmpty(req.Password))
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var result = await _userManager.ResetPasswordAsync(user, token, req.Password);
                    if (!result.Succeeded)
                    {
                        throw new Exception("Password reset failed");
                    }
                }
                string? oldAvatarPath = null;
                if (Avatar != null && Avatar.Length > 0) // Nếu người dùng chọn file ảnh mới
                {
                    var fileNameRemove = !string.IsNullOrEmpty(user.Avatar) ? user.Avatar.Split("/").LastOrDefault() ?? "" : "";
                    // Lưu đường dẫn avatar cũ
                    oldAvatarPath = !string.IsNullOrEmpty(user.Avatar) ? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "images", "avatars", fileNameRemove) : null;

                    var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "images", "avatars");// Đường dẫn thư mục lưu trữ ảnh
                    if (!Directory.Exists(uploads)) // Kiểm tra nếu thư mục chưa tồn tại
                    {
                        Directory.CreateDirectory(uploads); // Tạo thư mục nếu chưa tồn tại
                    }
                    var fileExtension = Path.GetExtension(Avatar.FileName); // Lấy phần mở rộng của file ảnh
                    var fileName = $"{Guid.NewGuid()}{fileExtension}"; // Tạo tên file mới sử dụng GUID
                    var filePath = Path.Combine(uploads, fileName); // Đường dẫn đầy đủ của file ảnh
                    using (var fileStream = new FileStream(filePath, FileMode.Create)) // Mở stream để lưu file
                    {
                        await Avatar.CopyToAsync(fileStream); // Sao chép nội dung file vào stream
                    }
                    // Lấy thông tin protocol và host từ request
                    var request = HttpContext.Request;
                    user.Avatar = $"/uploads/images/avatars/{fileName}"; // Lưu đường dẫn đầy đủ vào thuộc tính Avatar của user
                }

                var resultUpdate = await _userManager.UpdateAsync(user); // Cập nhật thông tin user
                if (!resultUpdate.Succeeded)
                {
                    throw new Exception("User update failed");
                }

                if (!string.IsNullOrEmpty(oldAvatarPath) && System.IO.File.Exists(oldAvatarPath))
                {
                    System.IO.File.Delete(oldAvatarPath); // Xóa avatar cũ
                }

                var userRoles = await _userManager.GetRolesAsync(user);
                var rolesToRemove = userRoles.Where(r => req.RoleIds == null || !req.RoleIds.Any(g => _roleManager.Roles.Any(role => role.Id == g && role.Name == r))).ToList();
                var rolesToAdd = req.RoleIds?.Where(g => _roleManager.Roles.Any(role => role.Id == g && !userRoles.Contains(role.Name))).Select(g => _roleManager.Roles.First(role => role.Id == g).Name).ToList();

                if (rolesToRemove.Any())
                {
                    var resultRemove = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                    if (!resultRemove.Succeeded)
                    {
                        throw new Exception("Failed to remove roles");
                    }
                }

                if (rolesToAdd != null && rolesToAdd.Any())
                {
                    var resultAdd = await _userManager.AddToRolesAsync(user, rolesToAdd);
                    if (!resultAdd.Succeeded)
                    {
                        throw new Exception("Failed to add roles");
                    }
                }

                // Cập nhật claims của người dùng hiện tại nếu là người dùng hiện tại
                if (User.Identity?.Name == user.UserName)
                {
                    var additionalClaims = new List<Claim>
                    {
                        new("Avatar", user.Avatar ?? "https://placehold.co/200x200") // Thêm claim Avatar
                    };

                    var claimsIdentity = User.Identity as ClaimsIdentity;
                    if (claimsIdentity != null)
                    {
                        // Xóa claim cũ
                        var existingClaim = claimsIdentity.FindFirst("Avatar");
                        if (existingClaim != null)
                        {
                            claimsIdentity.RemoveClaim(existingClaim);
                        }
                        // Thêm claim mới
                        claimsIdentity.AddClaims(additionalClaims);
                    }
                    await _signInManager.SignOutAsync();
                    await _signInManager.Context.SignInAsync(IdentityConstants.ApplicationScheme, new ClaimsPrincipal(claimsIdentity));
                }

                await transaction.CommitAsync();
                TempData[ToastrConstant.SUCCESS_MSG] = "Modify user is successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();

                var user = await _userManager.FindByIdAsync(req.Id.ToString());
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
                TempData[ToastrConstant.ERROR_MSG] = "Modify user is error!!!";
                return View(user);
            }
        }

        [Authorize(Policy = "CanCreate")]
        [HttpPost]
        public async Task<IActionResult> Create(ReqCreateUser req)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    throw new Exception();
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

                if (!userCreated.Succeeded)
                {
                    throw new Exception();
                }

                foreach (var roleId in req.RoleIds)
                {
                    var role = await _roleManager.FindByIdAsync(roleId.ToString());
                    if (role != null)
                    {
                        await _userManager.AddToRoleAsync(user, role.Name);
                    }
                }

                TempData[ToastrConstant.SUCCESS_MSG] = "Create user successfully";
                return View();
            }
            catch (Exception)
            {
                TempData[ToastrConstant.ERROR_MSG] = "Create user is error!!!";
                var roles = _roleManager.Roles.ToList();
                ViewBag.Roles = roles;
                return View();
            }
        }


        [Authorize(Policy = "CanDelete")]
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
