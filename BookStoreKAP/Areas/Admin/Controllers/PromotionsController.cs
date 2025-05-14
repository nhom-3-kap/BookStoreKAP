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
            // Set data for search form
            ViewBag.Series = _context.Series.ToList();
            ViewBag.SearchValue = searchModel;

            // Query promotions
            var query = _context.Promotions
                .Include(p => p.Series)
                .AsQueryable();

            // Apply search conditions
            if (!string.IsNullOrEmpty(searchModel.Name))
            {
                query = query.Where(p => p.Name.Contains(searchModel.Name));
            }

            if (!string.IsNullOrEmpty(searchModel.PromotionType))
            {
                int promotionType = int.Parse(searchModel.PromotionType);
                query = query.Where(p => p.PromotionType == promotionType);
            }

            if (!string.IsNullOrEmpty(searchModel.SeriesID))
            {
                var seriesId = Guid.Parse(searchModel.SeriesID);
                query = query.Where(p => p.SeriesID == seriesId);
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

            // Sort by most recent start date
            query = query.OrderByDescending(p => p.StartDate);

            // Calculate pagination info
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

            // Get data for current page and convert to ViewModel
            var promotionsData = query
                .Skip((page - 1) * PAGE_SIZE)
                .Take(PAGE_SIZE)
                .ToList();

            var promotions = promotionsData.Select(p => new PromotionViewModel
            {
                ID = p.Id,
                Name = p.Name ?? "",
                Description = p.Description ?? "",
                PromotionType = p.PromotionType,
                SeriesID = p.SeriesID,
                SeriesName = p.Series?.Name ?? "",
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                DiscountPercent = p.DiscountPercent
            }).ToList();

            // Load book data for specific book promotions
            foreach (var promo in promotions.Where(p => p.PromotionType == 2))
            {
                var promotionBooks = _context.PromotionBooks
                    .Where(pb => pb.PromotionId == promo.ID)
                    .Include(pb => pb.Book)
                    .ToList();

                promo.SelectedBooks = promotionBooks.Select(pb => new BookBasicViewModel
                {
                    Id = pb.BookId,
                    Title = pb.Book.Title,
                    Author = pb.Book.Author,
                    Price = pb.Book.Discount,
                    Thumbnail = pb.Book.Thumbnail
                }).ToList();

                promo.SelectedBookIds = promotionBooks.Select(pb => pb.BookId).ToList();
            }

            ViewBag.Pagination = pagination;
            return View(promotions);
        }

        [PermissionFilter(Name = "CanCreate")]
        public IActionResult Create()
        {
            ViewBag.Series = _context.Series.ToList();
            ViewBag.Books = _context.Books
                .Select(b => new BookBasicViewModel
                {
                    Id = b.ID,
                    Title = b.Title,
                    Author = b.Author,
                    Price = b.Price,
                    Thumbnail = b.Thumbnail
                })
                .ToList();

            return View(new PromotionViewModel
            {
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddMonths(1),
                DiscountPercent = 10,
                PromotionType = 1 // Default to Series-based promotion
            });
        }

        [PermissionFilter(Name = "CanSaveCreate")]
        [HttpPost]
        public async Task<IActionResult> Create(PromotionViewModel model)
        {
            try
            {
                // Validate based on promotion type
                bool isValid = ValidatePromotionModel(model);

                if (!isValid)
                {
                    PrepareViewBagForForm();
                    return View(model);
                }

                // Check for overlapping promotions
                if (!await ValidateOverlappingPromotions(model))
                {
                    PrepareViewBagForForm();
                    return View(model);
                }

                var promotion = new Promotion
                {
                    Id = Guid.NewGuid(),
                    Name = model.Name,
                    Description = model.Description,
                    DiscountPercent = model.DiscountPercent,
                    StartDate = model.StartDate,
                    EndDate = model.EndDate.Date.AddDays(1).AddTicks(-1), // Set to end of day
                    PromotionType = model.PromotionType,
                    CreatedAt = DateTime.Now,
                    IsActive = true
                };

                // Set appropriate IDs based on promotion type
                switch (model.PromotionType)
                {
                    case 1: // Series-based
                        promotion.SeriesID = model.SeriesID;
                        break;
                    case 2: // Books-based
                        promotion.SeriesID = null;
                        break;
                }

                _context.Promotions.Add(promotion);
                await _context.SaveChangesAsync();

                // For specific books promotion, add entries to junction table
                if (model.PromotionType == 2 && model.SelectedBookIds != null && model.SelectedBookIds.Any())
                {
                    foreach (var bookId in model.SelectedBookIds)
                    {
                        _context.PromotionBooks.Add(new PromotionBook
                        {
                            Id = Guid.NewGuid(),
                            PromotionId = promotion.Id,
                            BookId = bookId
                        });
                    }
                    await _context.SaveChangesAsync();
                }

                // Apply discount to books
                await UpdateBooksDiscount(promotion);

                TempData[ToastrConstant.SUCCESS_MSG] = "Khuyến mãi đã được tạo thành công!";
                return RedirectToAction(nameof(Index), new { menuKey = "PM" });
            }
            catch (Exception ex)
            {
                TempData[ToastrConstant.ERROR_MSG] = $"Lỗi khi tạo khuyến mãi: {ex.Message}";
                PrepareViewBagForForm();
                return View(model);
            }
        }


        [PermissionFilter(Name = "CanViewEdit")]
        public async Task<IActionResult> Modify(Guid id)
        {
            var promotion = await _context.Promotions
                .Include(p => p.Tag)
                .Include(p => p.Series)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (promotion == null)
            {
                TempData[ToastrConstant.ERROR_MSG] = "Không tìm thấy khuyến mãi!";
                return RedirectToAction(nameof(Index), new { menuKey = "PM" });
            }

            var viewModel = new PromotionViewModel
            {
                ID = promotion.Id,
                Name = promotion.Name ?? "", // Handle null name
                Description = promotion.Description ?? "", // Handle null description
                PromotionType = promotion.PromotionType,
                DiscountPercent = promotion.DiscountPercent,
                StartDate = promotion.StartDate,
                EndDate = promotion.EndDate,
                TagID = promotion.TagID, // Already nullable
                TagName = promotion.Tag?.Name ?? "", // Null conditional
                SeriesID = promotion.SeriesID, // Already nullable
                SeriesName = promotion.Series?.Name ?? "" // Null conditional
            };

            // Load selected books for specific books promotion
            if (promotion.PromotionType == 3)
            {
                var promotionBooks = await _context.PromotionBooks
                    .Where(pb => pb.PromotionId == promotion.Id)
                    .Include(pb => pb.Book)
                    .ToListAsync();

                viewModel.SelectedBookIds = promotionBooks
                    .Where(pb => pb.Book != null) // Filter out any null books
                    .Select(pb => pb.BookId)
                    .ToList();

                viewModel.SelectedBooks = promotionBooks
                    .Where(pb => pb.Book != null) // Filter out any null books
                    .Select(pb => new BookBasicViewModel
                    {
                        Id = pb.BookId,
                        Title = pb.Book.Title ?? "", // Handle null title
                        Author = pb.Book.Author ?? "", // Handle null author
                        Price = pb.Book.Price,
                        Thumbnail = pb.Book.Thumbnail ?? "" // Handle null thumbnail
                    })
                    .ToList();
            }

            PrepareViewBagForForm();
            return View("Create", viewModel);
        }


        [PermissionFilter(Name = "CanSaveEdit")]
        [HttpPost]
        public async Task<IActionResult> Modify(PromotionViewModel model)
        {
            try
            {
                // Validate based on promotion type
                bool isValid = ValidatePromotionModel(model);

                if (!isValid)
                {
                    PrepareViewBagForForm();
                    return View("Create", model);
                }

                var promotion = await _context.Promotions.FindAsync(model.ID);
                if (promotion == null)
                {
                    TempData[ToastrConstant.ERROR_MSG] = "Không tìm thấy khuyến mãi!";
                    return RedirectToAction(nameof(Index), new { menuKey = "PM" });
                }

                // Store original values for comparison
                var oldPromotionType = promotion.PromotionType;
                var oldSeriesID = promotion.SeriesID;

                // Check for overlapping promotions
                if (!await ValidateOverlappingPromotions(model, promotion.Id))
                {
                    PrepareViewBagForForm();
                    return View("Create", model);
                }

                // Update promotion details
                promotion.Name = model.Name;
                promotion.Description = model.Description;
                promotion.DiscountPercent = model.DiscountPercent;
                promotion.StartDate = model.StartDate;
                promotion.EndDate = model.EndDate.Date.AddDays(1).AddTicks(-1); // End of day
                promotion.PromotionType = model.PromotionType;

                // Update appropriate IDs based on promotion type
                switch (model.PromotionType)
                {
                    case 1: // Series-based
                        promotion.SeriesID = model.SeriesID;
                        break;
                    case 2: // Books-based
                        promotion.SeriesID = null;
                        break;
                }

                await _context.SaveChangesAsync();

                // Handle specific books if promotion type is 2
                if (model.PromotionType == 2)
                {
                    // Remove existing entries
                    var existingPromotionBooks = _context.PromotionBooks.Where(pb => pb.PromotionId == promotion.Id);
                    _context.PromotionBooks.RemoveRange(existingPromotionBooks);
                    await _context.SaveChangesAsync();

                    // Add new entries
                    if (model.SelectedBookIds != null && model.SelectedBookIds.Any())
                    {
                        foreach (var bookId in model.SelectedBookIds)
                        {
                            _context.PromotionBooks.Add(new PromotionBook
                            {
                                Id = Guid.NewGuid(),
                                PromotionId = promotion.Id,
                                BookId = bookId
                            });
                        }
                        await _context.SaveChangesAsync();
                    }
                }

                // Reset discounts on previously affected books if promotion type/target changed
                if (oldPromotionType != promotion.PromotionType ||
                    (oldPromotionType == 1 && oldSeriesID != promotion.SeriesID))
                {
                    await ResetDiscountsForPreviousTarget(oldPromotionType, null, oldSeriesID);

                    // If promotion type was 2 (specific books) and there were selected books, reset discount for those books
                    if (oldPromotionType == 2)
                    {
                        var oldPromotionBookIds = await _context.PromotionBooks
                            .Where(pb => pb.PromotionId == promotion.Id)
                            .Select(pb => pb.BookId)
                            .ToListAsync();

                        foreach (var bookId in oldPromotionBookIds)
                        {
                            var book = await _context.Books.FindAsync(bookId);
                            if (book != null)
                            {
                                book.Discount = book.Price;
                            }
                        }
                        await _context.SaveChangesAsync();
                    }
                }

                // Apply new discounts
                await UpdateBooksDiscount(promotion);

                TempData[ToastrConstant.SUCCESS_MSG] = "Khuyến mãi đã được cập nhật thành công!";
                return RedirectToAction(nameof(Index), new { menuKey = "PM" });
            }
            catch (Exception ex)
            {
                TempData[ToastrConstant.ERROR_MSG] = $"Lỗi khi cập nhật khuyến mãi: {ex.Message}";
                PrepareViewBagForForm();
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

                // Store info to reset books later
                var promotionType = promotion.PromotionType;
                var tagID = promotion.TagID;
                var seriesID = promotion.SeriesID;

                // Remove promotion books junction entries if it's a specific books promotion
                if (promotionType == 3)
                {
                    var promotionBooks = _context.PromotionBooks.Where(pb => pb.PromotionId == id);
                    _context.PromotionBooks.RemoveRange(promotionBooks);
                }

                // Remove the promotion
                _context.Promotions.Remove(promotion);
                await _context.SaveChangesAsync();

                // Reset book discounts and check for other active promotions
                await ResetDiscountsAndApplyActivePromotions(promotionType, tagID, seriesID);

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

                // Store info to reset books later
                var promotionType = promotion.PromotionType;
                var tagID = promotion.TagID;
                var seriesID = promotion.SeriesID;

                // End the promotion
                promotion.EndDate = DateTime.Now.AddSeconds(-1);
                await _context.SaveChangesAsync();

                // Reset book discounts and check for other active promotions
                await ResetDiscountsAndApplyActivePromotions(promotionType, tagID, seriesID);

                await transaction.CommitAsync();
                return Json(new { success = true, message = "Khuyến mãi đã kết thúc thành công!" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = $"Lỗi khi kết thúc khuyến mãi: {ex.Message}" });
            }
        }

        // Helper method to populate ViewBag for form
        private void PrepareViewBagForForm()
        {
            ViewBag.Series = _context.Series.ToList();
            ViewBag.Books = _context.Books
                .Select(b => new BookBasicViewModel
                {
                    Id = b.ID,
                    Title = b.Title,
                    Author = b.Author,
                    Price = b.Price,
                    Thumbnail = b.Thumbnail
                })
                .ToList();
        }

        // Helper method to validate promotion model based on type
        private bool ValidatePromotionModel(PromotionViewModel model)
        {
            bool isValid = true;

            // Validate end date is after start date
            if (model.EndDate <= model.StartDate)
            {
                TempData[ToastrConstant.ERROR_MSG] = "Ngày kết thúc phải sau ngày bắt đầu!";
                return false;
            }

            // Validate fields based on promotion type
            switch (model.PromotionType)
            {
                case 1: // Series-based
                    if (model.SeriesID == null || model.SeriesID == Guid.Empty)
                    {
                        TempData[ToastrConstant.ERROR_MSG] = "Vui lòng chọn series sách!";
                        isValid = false;
                    }
                    break;

                case 2: // Specific books
                    if (model.SelectedBookIds == null || !model.SelectedBookIds.Any())
                    {
                        TempData[ToastrConstant.ERROR_MSG] = "Vui lòng chọn ít nhất một cuốn sách!";
                        isValid = false;
                    }
                    break;

                default:
                    TempData[ToastrConstant.ERROR_MSG] = "Loại khuyến mãi không hợp lệ!";
                    isValid = false;
                    break;
            }

            return isValid;
        }

        // Helper method to validate overlapping promotions
        private async Task<bool> ValidateOverlappingPromotions(PromotionViewModel model, Guid? currentPromotionId = null)
        {
            var currentDate = DateTime.Now;
            var query = _context.Promotions.AsQueryable();

            // Exclude current promotion if editing
            if (currentPromotionId.HasValue)
            {
                query = query.Where(p => p.Id != currentPromotionId.Value);
            }

            // Check for overlapping time periods based on promotion type
            switch (model.PromotionType)
            {
                case 1: // Series-based
                    if (model.SeriesID.HasValue) // Verify SeriesID has value
                    {
                        var existingSeriesPromotion = await query
                            .FirstOrDefaultAsync(p => p.PromotionType == 1 &&
                                                 p.SeriesID.HasValue && // Verify p.SeriesID has value
                                                 p.SeriesID == model.SeriesID &&
                                                 p.StartDate <= model.EndDate &&
                                                 p.EndDate >= model.StartDate);

                        if (existingSeriesPromotion != null)
                        {
                            TempData[ToastrConstant.ERROR_MSG] = "Đã tồn tại một khuyến mãi cho series này trong khoảng thời gian đã chọn!";
                            return false;
                        }
                    }
                    break;

                case 2: // Specific books
                        // For specific books, we need to check if any of the selected books are in another promotion
                    if (model.SelectedBookIds != null && model.SelectedBookIds.Any())
                    {
                        foreach (var bookId in model.SelectedBookIds)
                        {
                            var existingBookPromotion = await _context.PromotionBooks
                                .Where(pb => pb.BookId == bookId)
                                .Join(query.Where(p => p.PromotionType == 2 &&
                                                    p.StartDate <= model.EndDate &&
                                                    p.EndDate >= model.StartDate),
                                      pb => pb.PromotionId,
                                      p => p.Id,
                                      (pb, p) => new { Book = pb, Promotion = p })
                                .FirstOrDefaultAsync();

                            if (existingBookPromotion != null)
                            {
                                var book = await _context.Books.FindAsync(bookId);
                                if (book != null) // Add null check
                                {
                                    TempData[ToastrConstant.ERROR_MSG] = $"Sách \"{book.Title}\" đã có trong một khuyến mãi khác trong khoảng thời gian đã chọn!";
                                }
                                else
                                {
                                    TempData[ToastrConstant.ERROR_MSG] = "Một sách đã chọn đã có trong một khuyến mãi khác trong khoảng thời gian đã chọn!";
                                }
                                return false;
                            }
                        }
                    }
                    break;
            }

            return true;
        }

        [HttpGet]
        public async Task<IActionResult> GetBooksBySeries(Guid seriesId)
        {
            try
            {
                var books = await _context.Books
                    .Where(b => b.SeriesID == seriesId)
                    .Select(b => new BookBasicViewModel
                    {
                        Id = b.ID,
                        Title = b.Title,
                        Author = b.Author,
                        Price = b.Price,
                        Thumbnail = b.Thumbnail
                    })
                    .ToListAsync();

                return Json(books);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }
        // Helper method to update book discounts when adding/modifying promotion
        private async Task UpdateBooksDiscount(Promotion promotion)
        {
            var currentDate = DateTime.Now;
            var isActive = promotion.StartDate <= currentDate && promotion.EndDate >= currentDate && promotion.IsActive;

            // Define query to get books based on promotion type
            IQueryable<Book> booksQuery;
            switch (promotion.PromotionType)
            {
                case 1: // Series-based
                    if (promotion.SeriesID.HasValue) // Verify SeriesID has value
                    {
                        booksQuery = _context.Books.Where(b => b.SeriesID == promotion.SeriesID);
                    }
                    else
                    {
                        return; // No SeriesID, nothing to update
                    }
                    break;

                case 2: // Specific books
                    var bookIds = _context.PromotionBooks
                        .Where(pb => pb.PromotionId == promotion.Id)
                        .Select(pb => pb.BookId);

                    if (!bookIds.Any())
                    {
                        return; // No books selected, nothing to update
                    }

                    booksQuery = _context.Books.Where(b => bookIds.Contains(b.ID));
                    break;

                default:
                    return; // Invalid promotion type
            }

            var books = await booksQuery.ToListAsync();
            if (books == null || !books.Any())
            {
                return; // No books found, nothing to update
            }

            // Update book discounts
            foreach (var book in books)
            {
                if (book == null) continue; // Skip null books

                if (isActive)
                {
                    // Apply discount if promotion is active
                    book.Discount = Math.Round(book.Price * (1 - promotion.DiscountPercent / 100), 0);
                }
                else
                {
                    // Reset to original price if not active
                    book.Discount = book.Price;
                }
            }

            await _context.SaveChangesAsync();
        }

        // Helper method to reset discounts when promotion type changes
        private async Task ResetDiscountsForPreviousTarget(int oldPromotionType, Guid? oldTagID, Guid? oldSeriesID)
        {
            IQueryable<Book> booksQuery = null;

            switch (oldPromotionType)
            {
                case 1: // Series-based
                    if (oldSeriesID.HasValue)
                    {
                        booksQuery = _context.Books.Where(b => b.SeriesID == oldSeriesID);
                        var seriesBooks = await booksQuery.ToListAsync();

                        foreach (var book in seriesBooks)
                        {
                            if (book == null) continue; // Skip null books
                            book.Discount = book.Price;
                        }
                    }
                    break;

                case 2: // Specific books - handled separately in Modify method
                    break;
            }

            await _context.SaveChangesAsync();
        }

        // Helper method to reset discounts and apply active promotions
        private async Task ResetDiscountsAndApplyActivePromotions(int promotionType, Guid? tagID, Guid? seriesID)
        {
            var currentDate = DateTime.Now;
            IQueryable<Book> booksQuery;

            switch (promotionType)
            {
                case 1: // Series-based
                    if (seriesID.HasValue)
                    {
                        // Get books for this series
                        booksQuery = _context.Books.Where(b => b.SeriesID == seriesID);
                        var seriesBooks = await booksQuery.ToListAsync();

                        // Reset discounts
                        foreach (var book in seriesBooks)
                        {
                            book.Discount = book.Price;
                        }
                        await _context.SaveChangesAsync();

                        // Find another active promotion for this series
                        var activeSeriesPromotion = await _context.Promotions
                            .Where(p => p.PromotionType == 1 &&
                                   p.SeriesID == seriesID &&
                                   p.StartDate <= currentDate &&
                                   p.EndDate >= currentDate &&
                                   p.IsActive)
                            .OrderByDescending(p => p.DiscountPercent)
                            .FirstOrDefaultAsync();

                        if (activeSeriesPromotion != null)
                        {
                            await UpdateBooksDiscount(activeSeriesPromotion);
                        }
                    }
                    break;

                case 2: // Specific books
                        // Get list of books from PromotionBooks table
                    var bookIds = await _context.PromotionBooks
                        .Where(pb => pb.PromotionId == seriesID) // seriesID here is actually promotionId
                        .Select(pb => pb.BookId)
                        .ToListAsync();

                    if (bookIds.Any())
                    {
                        // Reset discount for these books
                        foreach (var bookId in bookIds)
                        {
                            var book = await _context.Books.FindAsync(bookId);
                            if (book != null)
                            {
                                book.Discount = book.Price;

                                // Find other active promotions for this book
                                var activeBookPromotion = await FindActivePromotionForBook(bookId, currentDate);
                                if (activeBookPromotion != null)
                                {
                                    // Apply discount from active promotion
                                    book.Discount = Math.Round(book.Price * (1 - activeBookPromotion.DiscountPercent / 100), 0);
                                }
                            }
                        }
                        await _context.SaveChangesAsync();
                    }
                    break;
            }
        }
        private async Task<Promotion> FindActivePromotionForBook(Guid bookId, DateTime currentDate)
        {
            // Find promotion type 2 (specific books)
            var bookSpecificPromotion = await _context.PromotionBooks
                .Where(pb => pb.BookId == bookId)
                .Join(_context.Promotions.Where(p =>
                    p.PromotionType == 2 &&
                    p.StartDate <= currentDate &&
                    p.EndDate >= currentDate &&
                    p.IsActive),
                      pb => pb.PromotionId,
                      p => p.Id,
                      (pb, p) => p)
                .OrderByDescending(p => p.DiscountPercent)
                .FirstOrDefaultAsync();

            if (bookSpecificPromotion != null)
            {
                return bookSpecificPromotion;
            }

            // Find promotion type 1 (series-based)
            var book = await _context.Books.FindAsync(bookId);
            if (book != null && book.SeriesID.HasValue)
            {
                var seriesPromotion = await _context.Promotions
                    .Where(p => p.PromotionType == 1 &&
                           p.SeriesID == book.SeriesID &&
                           p.StartDate <= currentDate &&
                           p.EndDate >= currentDate &&
                           p.IsActive)
                    .OrderByDescending(p => p.DiscountPercent)
                    .FirstOrDefaultAsync();

                if (seriesPromotion != null)
                {
                    return seriesPromotion;
                }
            }

            return null;
        }
        private bool PromotionExists(Guid id)
        {
            return _context.Promotions.Any(e => e.Id == id);
        }
    }
}