using BookStoreKAP.Common.Constants;
using BookStoreKAP.Data;
using BookStoreKAP.Models;
using BookStoreKAP.Models.DTO;
using BookStoreKAP.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookStoreKAP.Areas.Admin.Controllers
{
    [Area(AreasConstant.ADMIN)]
    public class TagsController : Controller
    {
        private readonly BookStoreKAPDBContext _context;

        public TagsController(BookStoreKAPDBContext context)
        {
            _context = context;
        }

        [Authorize(Policy = "CanView")]
        public IActionResult Index([FromQuery] ReqQuerySearchTag q)
        {
            var tags = _context.Tags.Where(x => (string.IsNullOrEmpty(q.Name) || x.Name.ToUpper().Trim().Contains(q.Name.ToUpper().Trim()))).ToList();
            var totalItems = tags.Count;
            var paged = tags.Skip((q.Page - 1) * q.PageSize).Take(q.PageSize).ToList();

            ViewBag.SearchValue = q;
            ViewBag.Pagination = new PaginationModel()
            {
                TotalItems = totalItems,
                CurrentPage = q.Page,
                PageSize = q.PageSize,
                SearchParams = q,
                Action = "Index",
                Controller = "Tags"
            };
            return View(paged);
        }


        [Authorize(Policy = "CanViewCreate")]
        public IActionResult Create()
        {
            return View();
        }


        [Authorize(Policy = "CanSaveCreate")]
        [HttpPost]
        public async Task<IActionResult> Create(ReqCreateTag req, IFormFile? Thumbnail)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    throw new Exception("Invalid data.");
                }

                // Lưu trữ thumbnail
                string thumbnailPath = null;
                if (Thumbnail != null && Thumbnail.Length > 0)
                {
                    var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "images", "products");
                    if (!Directory.Exists(uploads))
                    {
                        Directory.CreateDirectory(uploads);
                    }

                    var fileExtension = Path.GetExtension(Thumbnail.FileName);
                    var fileName = $"{Guid.NewGuid()}{fileExtension}";
                    var filePath = Path.Combine(uploads, fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await Thumbnail.CopyToAsync(fileStream);
                    }

                    thumbnailPath = $"/uploads/images/products/{fileName}";
                }

                // Tạo mới tag
                var tag = new Tag
                {
                    Name = req.Name,
                    Thumbnail = thumbnailPath,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Tags.Add(tag);
                await _context.SaveChangesAsync();

                TempData[ToastrConstant.SUCCESS_MSG] = "Create Success";
                return Redirect($"{RouteConstant.ADMIN_TAGS}?menuKey=TM"); // Redirect to the index or any other page after success
            }
            catch (Exception ex)
            {
                TempData[ToastrConstant.ERROR_MSG] = ex.Message;
                return View(req);
            }
        }


        [Authorize(Policy = "CanViewModify")]
        public async Task<IActionResult> Modify(Guid tagID)
        {
            var tag = await _context.Tags.FindAsync(tagID);
            if (tag == null)
            {
                return NotFound();
            }

            var model = new ReqModifyTag
            {
                ID = tag.ID,
                Name = tag.Name,
                Thumbnail = tag.Thumbnail
            };

            return View(model);
        }


        [Authorize(Policy = "CanSaveModify")]
        [HttpPost]
        public async Task<IActionResult> Modify(ReqModifyTag req, IFormFile? Thumbnail)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    throw new Exception("Invalid data.");
                }

                var tag = await _context.Tags.FindAsync(req.ID);
                if (tag == null)
                {
                    return NotFound();
                }

                // Lưu trữ thumbnail nếu có tệp mới
                if (Thumbnail != null && Thumbnail.Length > 0)
                {
                    var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "images", "products");
                    if (!Directory.Exists(uploads))
                    {
                        Directory.CreateDirectory(uploads);
                    }

                    var fileExtension = Path.GetExtension(Thumbnail.FileName);
                    var fileName = $"{Guid.NewGuid()}{fileExtension}";
                    var filePath = Path.Combine(uploads, fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await Thumbnail.CopyToAsync(fileStream);
                    }

                    // Xóa thumbnail cũ nếu có
                    if (!string.IsNullOrEmpty(tag.Thumbnail))
                    {
                        var oldThumbnailPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", tag.Thumbnail.TrimStart('/'));
                        if (System.IO.File.Exists(oldThumbnailPath))
                        {
                            System.IO.File.Delete(oldThumbnailPath);
                        }
                    }

                    tag.Thumbnail = $"/uploads/images/products/{fileName}";
                }

                // Cập nhật thông tin tag
                tag.Name = req.Name;
                tag.UpdatedAt = DateTime.UtcNow;

                _context.Tags.Update(tag);
                await _context.SaveChangesAsync();

                TempData[ToastrConstant.SUCCESS_MSG] = "Updated Success";
                return Redirect($"{RouteConstant.ADMIN_TAGS}?menuKey=TM");
            }
            catch (Exception ex)
            {
                TempData[ToastrConstant.ERROR_MSG] = ex.Message;
                return View(req);
            }
        }

        [Authorize(Policy = "CanDelete")]
        [HttpDelete]
        public async Task<IActionResult> DeleteTagByIdAPI(Guid tagID)
        {
            try
            {
                // Tìm Tag theo ID
                var tag = await _context.Tags.FindAsync(tagID) ?? throw new Exception("Tag not found.");

                // Xóa thumbnail nếu có
                if (!string.IsNullOrEmpty(tag.Thumbnail))
                {
                    var thumbnailPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", tag.Thumbnail.TrimStart('/'));
                    if (System.IO.File.Exists(thumbnailPath))
                    {
                        System.IO.File.Delete(thumbnailPath);
                    }
                }

                _context.Tags.Remove(tag);
                await _context.SaveChangesAsync();

                return Ok(new ResponseAPI<string> { Success = true, Message = "Tag deleted successfully." });
            }
            catch (Exception ex)
            {
                return Ok(new ResponseAPI<string> { Success = false, Message = ex.Message });
            }
        }
    }
}
