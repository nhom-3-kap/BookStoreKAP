using BookStoreKAP.Common.Constants;
using BookStoreKAP.Data;
using BookStoreKAP.Filters;
using BookStoreKAP.Models;
using BookStoreKAP.Models.DTO;
using BookStoreKAP.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookStoreKAP.Areas.Admin.Controllers
{
    [Area(AreasConstant.ADMIN)]
    public class PromotionsController : Controller
    {
        private readonly BookStoreKAPDBContext _context;
        private const int PAGE_SIZE = 10;

        // Định nghĩa các constant cho Tag ID
        private static readonly Guid BEST_SELLER_TAG_ID = new Guid("3E5CAC9B-6E5A-416D-86F8-52F044D6994E");
        private static readonly Guid NEW_RELEASE_TAG_ID = new Guid("CA038048-95D2-4BFD-86D8-740FB2ECE1AF");

        public PromotionsController(BookStoreKAPDBContext context)
        {
            _context = context;
        }

        [PermissionFilter(Name = "CanView")]
        public IActionResult Index(ReqQuerySearchPromotion searchModel, int page = 1)
        {
            // Set dữ liệu cho form tìm kiếm
            ViewBag.Tags = _context.Tags.ToList();
            ViewBag.SearchValue = searchModel;
            // Truy vấn chiến dịch khuyến mãi
            var query = _context.Promotions.AsQueryable();
            // Áp dụng các điều kiện tìm kiếm
            if (!string.IsNullOrEmpty(searchModel.Name))
            {
                query = query.Where(p => p.Name.Contains(searchModel.Name));
            }
            if (!string.IsNullOrEmpty(searchModel.TagID))
            {
                var tagId = Guid.Parse(searchModel.TagID);
                query = query.Where(p => p.TagID == tagId);
            }
            if (!string.IsNullOrEmpty(searchModel.Status))
            {
                var currentDate = DateTime.Now;
                switch (searchModel.Status)
                {
                    case "Active":
                        query = query.Where(p => p.StartDate <= currentDate && p.EndDate >= currentDate);
                        break;
                    case "Upcoming":
                        query = query.Where(p => p.StartDate > currentDate);
                        break;
                    case "Expired":
                        query = query.Where(p => p.EndDate < currentDate);
                        break;
                }
            }
            // Sắp xếp theo ngày tạo mới nhất
            query = query.OrderByDescending(p => p.StartDate);
            // Tính toán thông tin phân trang
            int totalItems = query.Count();
            var pagination = new PaginationDTO
            {
                TotalItems = totalItems,
                CurrentPage = page,
                PageSize = PAGE_SIZE,
                SearchParams = searchModel,
                menuKey = searchModel.menuKey,
                Action = "Index",
                Controller = "Promotions"
            };
            // Lấy dữ liệu cho trang hiện tại và chuyển sang ViewModel
            var promotions = query
                .Skip((page - 1) * PAGE_SIZE)
                .Take(PAGE_SIZE)
                .Include(p => p.Tag)
                .ToList()
                .Select(p => new PromotionViewModel
                {
                    ID = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    TagID = p.TagID,
                    TagName = p.Tag.Name,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    DiscountPercent = p.DiscountPercent
                })
                .ToList();
            ViewBag.Pagination = pagination;
            return View(promotions);
        }

        [PermissionFilter(Name = "CanCreate")]
        public IActionResult Create()
        {
            ViewBag.Tags = _context.Tags.ToList();
            ViewBag.BestSellerTagId = BEST_SELLER_TAG_ID;
            ViewBag.NewReleaseTagId = NEW_RELEASE_TAG_ID;

            return View(new PromotionViewModel
            {
                StartDate = DateTime.Now.AddDays(0),
                EndDate = DateTime.Now.AddMonths(1),
                DiscountPercent = 10
            });
        }

        [PermissionFilter(Name = "CanSaveCreate")]
        [HttpPost]
        public async Task<IActionResult> Create(PromotionViewModel model)
        {
            //if (ModelState.IsValid)
            //{
                try
                {
                    // Kiểm tra xem đã có promotion nào đang hoạt động với cùng tag không 
                    var currentDate = DateTime.Now;
                    var existingActivePromotion = await _context.Promotions
                        .FirstOrDefaultAsync(p => p.TagID == model.TagID &&
                                                p.StartDate <= model.EndDate &&
                                                p.EndDate >= model.StartDate);

                    if (existingActivePromotion != null)
                    {
                        TempData[ToastrConstant.ERROR_MSG] = "Đã tồn tại một khuyến mãi cho thể loại này trong khoảng thời gian đã chọn!";
                        ViewBag.Tags = _context.Tags.ToList();
                        ViewBag.BestSellerTagId = BEST_SELLER_TAG_ID;
                        ViewBag.NewReleaseTagId = NEW_RELEASE_TAG_ID;
                        return View(model);
                    }

                    var promotion = new Promotion
                    {
                        Id = Guid.NewGuid(), // Tạo ID mới cho promotion
                        Name = model.Name,
                        Description = model.Description,
                        DiscountPercent = model.DiscountPercent,
                        StartDate = model.StartDate,
                        EndDate = model.EndDate,
                        TagID = model.TagID,
                        CreatedAt = DateTime.Now,
                        IsActive = true // Mặc định promotion là active
                    };

                    _context.Promotions.Add(promotion);
                    await _context.SaveChangesAsync();

                    // Áp dụng giảm giá cho các sách thuộc Tag nếu khuyến mãi đang hoạt động
                    await UpdateBooksDiscount(promotion);

                    TempData[ToastrConstant.SUCCESS_MSG] = "Khuyến mãi đã được tạo thành công!";
                    return RedirectToAction(nameof(Index), new { menuKey = "PM" });
                }
                catch (Exception ex)
                {
                    TempData[ToastrConstant.ERROR_MSG] = $"Lỗi khi tạo khuyến mãi: {ex.Message}";
                }
            //}

            // Nếu ModelState không hợp lệ, trả về view
            ViewBag.Tags = _context.Tags.ToList();
            ViewBag.BestSellerTagId = BEST_SELLER_TAG_ID;
            ViewBag.NewReleaseTagId = NEW_RELEASE_TAG_ID;
            return View(model);
        }

        [PermissionFilter(Name = "CanViewEdit")]
        public async Task<IActionResult> Modify(Guid id)
        {
            var promotion = await _context.Promotions
                .Include(p => p.Tag)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (promotion == null)
            {
                TempData[ToastrConstant.ERROR_MSG] = "Không tìm thấy khuyến mãi!";
                return RedirectToAction(nameof(Index), new { menuKey = "PM" });
            }

            var viewModel = new PromotionViewModel
            {
                ID = promotion.Id,
                Name = promotion.Name,
                Description = promotion.Description,
                DiscountPercent = promotion.DiscountPercent,
                StartDate = promotion.StartDate,
                EndDate = promotion.EndDate,
                TagID = promotion.TagID,
                TagName = promotion.Tag?.Name
            };

            ViewBag.Tags = _context.Tags.ToList();
            ViewBag.BestSellerTagId = BEST_SELLER_TAG_ID;
            ViewBag.NewReleaseTagId = NEW_RELEASE_TAG_ID;
            return View("Create", viewModel);
        }

        [PermissionFilter(Name = "CanSaveEdit")]
        [HttpPost]
        public async Task<IActionResult> Modify(PromotionViewModel model)
        {
            //if (!ModelState.IsValid)
            //{
            //    ViewBag.Tags = _context.Tags.ToList();
            //    ViewBag.BestSellerTagId = BEST_SELLER_TAG_ID;
            //    ViewBag.NewReleaseTagId = NEW_RELEASE_TAG_ID;
            //    return View("Create", model);
            //}

            try
            {
                var promotion = await _context.Promotions.FindAsync(model.ID);
                if (promotion == null)
                {
                    TempData[ToastrConstant.ERROR_MSG] = "Không tìm thấy khuyến mãi!";
                    return RedirectToAction(nameof(Index), new { menuKey = "PM" });
                }

                // Kiểm tra xem đã có promotion nào đang hoạt động với cùng tag không (ngoại trừ promotion hiện tại)
                var currentDate = DateTime.Now;
                var existingActivePromotion = await _context.Promotions
                    .FirstOrDefaultAsync(p => p.Id != model.ID &&
                                            p.TagID == model.TagID &&
                                            p.StartDate <= model.EndDate &&
                                            p.EndDate >= model.StartDate);

                if (existingActivePromotion != null)
                {
                    TempData[ToastrConstant.ERROR_MSG] = "Đã tồn tại một khuyến mãi cho thể loại này trong khoảng thời gian đã chọn!";
                    ViewBag.Tags = _context.Tags.ToList();
                    ViewBag.BestSellerTagId = BEST_SELLER_TAG_ID;
                    ViewBag.NewReleaseTagId = NEW_RELEASE_TAG_ID;
                    return View("Create", model);
                }

                // Lưu TagID cũ để kiểm tra xem có thay đổi không
                var oldTagID = promotion.TagID;

                promotion.Name = model.Name;
                promotion.Description = model.Description;
                promotion.DiscountPercent = model.DiscountPercent;
                promotion.StartDate = model.StartDate.Date;
                promotion.EndDate = model.EndDate.Date.AddDays(1).AddTicks(-1);
                promotion.TagID = model.TagID;

                await _context.SaveChangesAsync();

                // Xử lý cập nhật giá sách
                if (oldTagID != promotion.TagID)
                {
                    // Reset giá sách ở tag cũ
                    var oldTagBooks = await _context.Books.Where(b => b.TagID == oldTagID).ToListAsync();
                    foreach (var book in oldTagBooks)
                    {
                        book.Discount = book.Price;
                    }
                    await _context.SaveChangesAsync();

                    // Kiểm tra xem có promotion nào khác đang active cho tag cũ không
                    var activePromotionForOldTag = await _context.Promotions
                        .FirstOrDefaultAsync(p => p.TagID == oldTagID &&
                                              p.StartDate <= currentDate &&
                                              p.EndDate >= currentDate);

                    if (activePromotionForOldTag != null)
                    {
                        // Áp dụng lại promotion đang active cho tag cũ
                        await UpdateBooksDiscount(activePromotionForOldTag);
                    }
                }

                // Cập nhật giá sách cho tag mới
                await UpdateBooksDiscount(promotion);

                TempData[ToastrConstant.SUCCESS_MSG] = "Khuyến mãi đã được cập nhật thành công!";
                return RedirectToAction(nameof(Index), new { menuKey = "PM" });
            }
            catch (Exception ex)
            {
                TempData[ToastrConstant.ERROR_MSG] = $"Lỗi khi cập nhật khuyến mãi: {ex.Message}";
                ViewBag.Tags = _context.Tags.ToList();
                ViewBag.BestSellerTagId = BEST_SELLER_TAG_ID;
                ViewBag.NewReleaseTagId = NEW_RELEASE_TAG_ID;
                return View("Create", model);
            }
        }

        [PermissionFilter(Name = "CanDelete")]
        [HttpPost]
        public async Task<IActionResult> RemovePromotionByIDAPI(Guid id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var promotion = await _context.Promotions.FindAsync(id);
                if (promotion == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy khuyến mãi!" });
                }

                // Lưu TagID để cập nhật giá sách
                var tagID = promotion.TagID;
                _context.Promotions.Remove(promotion);
                await _context.SaveChangesAsync();

                // Kiểm tra xem có promotion nào khác cho tag này không
                var currentDate = DateTime.Now;
                var activePromotion = await _context.Promotions
                    .FirstOrDefaultAsync(p => p.TagID == tagID &&
                                          p.StartDate <= currentDate &&
                                          p.EndDate >= currentDate);

                if (activePromotion != null)
                {
                    // Nếu có promotion khác, áp dụng promotion đó
                    await UpdateBooksDiscount(activePromotion);
                }
                else
                {
                    // Nếu không có promotion nào, reset giá sách
                    var books = await _context.Books.Where(b => b.TagID == tagID).ToListAsync();
                    foreach (var book in books)
                    {
                        book.Discount = book.Price;
                    }
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();
                return Json(new { success = true, message = "Khuyến mãi đã được xóa thành công!" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = $"Lỗi khi xóa khuyến mãi: {ex.Message}" });
            }
        }

        [PermissionFilter(Name = "CanSaveEdit")]
        [HttpPost]
        public async Task<IActionResult> EndPromotionAPI(Guid id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var promotion = await _context.Promotions.FindAsync(id);
                if (promotion == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy khuyến mãi!" });
                }

                var tagID = promotion.TagID;
                promotion.EndDate = DateTime.Now.AddSeconds(-1);
                await _context.SaveChangesAsync();

                // Kiểm tra xem có promotion nào khác đang active cho tag này không
                var currentDate = DateTime.Now;
                var activePromotion = await _context.Promotions
                    .FirstOrDefaultAsync(p => p.Id != id &&
                                          p.TagID == tagID &&
                                          p.StartDate <= currentDate &&
                                          p.EndDate >= currentDate);

                if (activePromotion != null)
                {
                    // Nếu có promotion khác, áp dụng promotion đó
                    await UpdateBooksDiscount(activePromotion);
                }
                else
                {
                    // Nếu không có promotion nào, reset giá sách
                    var books = await _context.Books.Where(b => b.TagID == tagID).ToListAsync();
                    foreach (var book in books)
                    {
                        book.Discount = book.Price;
                    }
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();
                return Json(new { success = true, message = "Khuyến mãi đã kết thúc thành công!" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = $"Lỗi khi kết thúc khuyến mãi: {ex.Message}" });
            }
        }

        // Helper method để cập nhật giá sách khi thêm/sửa promotion
        private async Task UpdateBooksDiscount(Promotion promotion)
        {
            var currentDate = DateTime.Now;
            var isActive = promotion.StartDate <= currentDate && promotion.EndDate >= currentDate && promotion.IsActive;

            // Xác định danh sách sách cần cập nhật
            var booksQuery = _context.Books.Where(b => b.TagID == promotion.TagID);
            var books = await booksQuery.ToListAsync();

            // Cập nhật giá sách
            foreach (var book in books)
            {
                if (isActive)
                {
                    // Áp dụng giảm giá nếu promotion đang active
                    book.Discount = Math.Round(book.Price * (1 - promotion.DiscountPercent / 100), 0);
                }
                else
                {
                    // Kiểm tra xem có promotion nào khác đang active cho tag này không
                    var activePromotion = await _context.Promotions
                        .Where(p => p.TagID == promotion.TagID &&
                               p.Id != promotion.Id &&
                               p.StartDate <= currentDate &&
                               p.EndDate >= currentDate &&
                               p.IsActive)
                        .OrderByDescending(p => p.DiscountPercent) // Lấy khuyến mãi có % giảm giá cao nhất
                        .FirstOrDefaultAsync();

                    if (activePromotion != null)
                    {
                        // Áp dụng khuyến mãi khác nếu có
                        book.Discount = Math.Round(book.Price * (1 - activePromotion.DiscountPercent / 100), 0);
                    }
                    else
                    {
                        // Đặt lại giá gốc nếu không có khuyến mãi nào khác
                        book.Discount = book.Price;
                    }
                }
            }

            await _context.SaveChangesAsync();
        }

        private bool PromotionExists(Guid id)
        {
            return _context.Promotions.Any(e => e.Id == id);
        }
    }
}