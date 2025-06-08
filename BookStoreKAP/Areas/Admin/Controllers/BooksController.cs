using BookStoreKAP.Common.Constants;
using BookStoreKAP.Data;
using BookStoreKAP.Filters;
using BookStoreKAP.Models;
using BookStoreKAP.Models.DTO;
using BookStoreKAP.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookStoreKAP.Areas.Admin.Controllers
{
    [Area(AreasConstant.ADMIN)]
    public class BooksController : Controller
    {
        private readonly BookStoreKAPDBContext _context;

        // Định nghĩa ID tags dưới dạng constants - giữ lại để tham khảo nhưng sẽ không sử dụng chọn tag
        private static readonly Guid BEST_SELLER_TAG_ID = new Guid("3E5CAC9B-6E5A-416D-86F8-52F044D6994E");
        private static readonly Guid NEW_RELEASE_TAG_ID = new Guid("CA038048-95D2-4BFD-86D8-740FB2ECE1AF");

        public BooksController(BookStoreKAPDBContext context)
        {
            _context = context;
        }

        // Phương thức cập nhật tags tự động - sẽ tự gán tag dựa trên điều kiện
        private async Task UpdateTagsAutomatically()
        {
            try
            {
                var oneMonthAgo = DateTime.Now.AddMonths(-1);

                var newReleaseBooks = await _context.Books
                    .Where(b => b.CreatedAt >= oneMonthAgo)
                    .ToListAsync();

                foreach (var book in newReleaseBooks)
                {
                    book.TagID = NEW_RELEASE_TAG_ID;
                }
                await _context.SaveChangesAsync();

                var bestSellerBooks = await _context.Books
                    .Where(b => b.BuyCount > 10)
                    .OrderByDescending(b => b.BuyCount)
                    .Take(6)
                    .ToListAsync();

                foreach (var book in bestSellerBooks)
                {
                    if (book.TagID == null)
                    {
                        book.TagID = BEST_SELLER_TAG_ID;
                    }
                }
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating book tags: {ex.Message}");
                throw;
            }
        }

        [PermissionFilter(Name = "CanView")]
        public IActionResult Index([FromQuery] ReqQuerySearchBook q)
        {
            var booksQuery = _context.Books.Include(x => x.Series).Include(x => x.BookGenres).ThenInclude(x => x.Genre).AsQueryable();

            if (!string.IsNullOrEmpty(q.Title))
            {
                booksQuery = booksQuery.Where(x => x.Title.ToUpper().Trim().Contains(q.Title.ToUpper().Trim()));
            }
            if (!string.IsNullOrEmpty(q.Publisher))
            {
                booksQuery = booksQuery.Where(x => x.Publisher.ToUpper().Trim().Contains(q.Publisher.ToUpper().Trim()));
            }
            if (q.PublicationYear != null)
            {
                booksQuery = booksQuery.Where(x => x.PublicationYear == q.PublicationYear);
            }
            if (!string.IsNullOrEmpty(q.Author))
            {
                booksQuery = booksQuery.Where(x => x.Author.ToUpper().Trim().Contains(q.Author.ToUpper().Trim()));
            }
            if (q.SeriesID != null)
            {
                booksQuery = booksQuery.Where(x => x.SeriesID == q.SeriesID);
            }
            if (q.TagID != null)
            {
                booksQuery = booksQuery.Where(x => x.TagID == q.TagID);
            }

            var totalItems = booksQuery.Count();
            var pagedBooks = booksQuery
                .OrderBy(x => x.UpdatedAt)
                .Skip((q.Page - 1) * q.PageSize)
                .Take(q.PageSize)
                .ToList();

            ViewBag.SearchValue = q;
            ViewBag.Pagination = new PaginationDTO()
            {
                TotalItems = totalItems,
                CurrentPage = q.Page,
                PageSize = q.PageSize,
                SearchParams = q,
                Action = "Index",
                Controller = "Books"
            };

            return View(pagedBooks);
        }

        [PermissionFilter(Name = "CanCreate")]
        public IActionResult Create()
        {
            // Đã bỏ phần chọn tag, chỉ giữ lại series và genres
            var series = _context.Series.ToList();
            var genres = _context.Genres.ToList();

            ViewBag.Series = series;
            ViewBag.Genres = genres;
            return View();
        }

        [PermissionFilter(Name = "CanSaveCreate")]
        [HttpPost]
        public async Task<IActionResult> Create(ReqCreateBook req, IFormFile Thumbnail)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Validate the model
                if (!ModelState.IsValid)
                {
                    TempData[ToastrConstant.ERROR_MSG] = "Invalid data.";
                    ViewBag.Series = _context.Series.ToList();
                    ViewBag.Genres = _context.Genres.ToList();
                    return View(req);
                }

                // Verify that required related entities exist
                if (req.SeriesID != null && !await _context.Series.AnyAsync(s => s.ID == req.SeriesID))
                {
                    throw new Exception("Selected series does not exist");
                }

                // Removed tag validation since tags are now automatic

                foreach (var genreId in req.GenreIds)
                {
                    if (!await _context.Genres.AnyAsync(g => g.ID == genreId))
                    {
                        throw new Exception($"Selected genre with ID {genreId} does not exist");
                    }
                }

                string thumbnailPath = null;

                if (Thumbnail != null && Thumbnail.Length > 0)
                {
                    var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "images", "products");
                    if (!Directory.Exists(uploads))
                    {
                        Directory.CreateDirectory(uploads);
                    }
                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(Thumbnail.FileName)}";
                    var filePath = Path.Combine(uploads, fileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await Thumbnail.CopyToAsync(fileStream);
                    }
                    thumbnailPath = $"/uploads/images/products/{fileName}";
                }

                var book = new Book
                {
                    ID = Guid.NewGuid(), // Explicitly set ID to avoid potential conflicts
                    Title = req.Title,
                    Publisher = req.Publisher,
                    Author = req.Author,
                    PublicationYear = req.PublicationYear,
                    Price = req.Price,
                    Discount = req.Discount,
                    Quantity = req.Quantity,
                    Synopsis = req.Synopsis,
                    // TagID is now null and will be set by UpdateTagsAutomatically
                    //TagID = null,
                    SeriesID = req.SeriesID,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    Thumbnail = thumbnailPath,
                    BuyCount = req.BuyCount
                };

                _context.Books.Add(book);
                await _context.SaveChangesAsync();

                // Add book genres separately
                if (req.GenreIds != null && req.GenreIds.Any())
                {
                    foreach (var genreId in req.GenreIds)
                    {
                        var bookGenre = new BookGenre
                        {
                            BookID = book.ID,
                            GenreID = genreId
                        };
                        _context.BookGenres.Add(bookGenre);
                    }
                    await _context.SaveChangesAsync();
                }

                // Cập nhật tags tự động sau khi thêm sách
                try
                {
                    await UpdateTagsAutomatically();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Tag update failed, but book was created: {ex.Message}");
                    // Continue execution, don't roll back the transaction
                }

                await transaction.CommitAsync();

                TempData[ToastrConstant.SUCCESS_MSG] = "Book created successfully.";
                return Redirect($"{RouteConstant.ADMIN_BOOKS}?menuKey=BM");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData[ToastrConstant.ERROR_MSG] = ex.Message;
                Console.WriteLine($"Error creating book: {ex.Message}");
                Console.WriteLine($"Inner Exception: {ex.InnerException?.Message}");

                ViewBag.Series = _context.Series.ToList();
                ViewBag.Genres = _context.Genres.ToList();
                return View(req);
            }
        }

        [PermissionFilter(Name = "CanViewEdit")]
        public async Task<IActionResult> Modify(Guid bookID)
        {
            try
            {
                var book = await _context.Books
                    .Include(b => b.BookGenres)
                    .ThenInclude(bg => bg.Genre)
                    .FirstOrDefaultAsync(x => x.ID == bookID);

                if (book == null)
                {
                    throw new Exception("Book is not exists!");
                }

                ViewBag.Series = _context.Series.ToList();
                // Bỏ Tags khỏi ViewBag
                ViewBag.Genres = _context.Genres.ToList();
                ViewBag.SelectedGenres = book.BookGenres.Select(bg => bg.GenreID).ToList();
                return View(book);
            }
            catch (Exception ex)
            {
                TempData[ToastrConstant.ERROR_MSG] = ex.Message;
                return View();
            }
        }

        [PermissionFilter(Name = "CanSaveEdit")]
        [HttpPost]
        public async Task<IActionResult> Modify(ReqModifyBook req, IFormFile? Thumbnail)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (!ModelState.IsValid)
                {
                    throw new Exception("Values invalid");
                }

                var book = await _context.Books
                    .Include(b => b.BookGenres)
                    .FirstOrDefaultAsync(x => x.ID == req.ID);

                if (book == null)
                {
                    throw new Exception("Book not found");
                }

                // Verify that required related entities exist
                if (req.SeriesID != null && !await _context.Series.AnyAsync(s => s.ID == req.SeriesID))
                {
                    throw new Exception("Selected series does not exist");
                }

                // Đã loại bỏ phần kiểm tra TagID

                if (req.GenreIds != null)
                {
                    foreach (var genreId in req.GenreIds)
                    {
                        if (!await _context.Genres.AnyAsync(g => g.ID == genreId))
                        {
                            throw new Exception($"Selected genre with ID {genreId} does not exist");
                        }
                    }
                }

                book.Title = req.Title;
                book.Publisher = req.Publisher;
                book.PublicationYear = req.PublicationYear;
                book.Author = req.Author;
                book.Price = req.Price;
                book.Discount = req.Discount;
                book.Quantity = req.Quantity;
                book.SeriesID = req.SeriesID;
                // Giữ nguyên TagID hiện tại, sẽ được cập nhật tự động sau
                book.Synopsis = req.Synopsis;
                book.UpdatedAt = DateTime.Now;

                string oldThumbnailPath = string.Empty;

                if (Thumbnail != null && Thumbnail.Length > 0)
                {
                    if (!string.IsNullOrEmpty(book.Thumbnail))
                    {
                        oldThumbnailPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", book.Thumbnail.TrimStart('/'));
                    }

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
                    book.Thumbnail = $"/uploads/images/products/{fileName}";
                }

                // Handle BookGenres updates
                if (req.GenreIds != null)
                {
                    var existingBookGenres = book.BookGenres.Select(bg => bg.GenreID).ToList();
                    var newBookGenres = req.GenreIds.Except(existingBookGenres).ToList();
                    var removedBookGenres = existingBookGenres.Except(req.GenreIds).ToList();

                    // Remove first
                    if (removedBookGenres.Any())
                    {
                        var bookGenresToRemove = _context.BookGenres
                            .Where(bg => bg.BookID == req.ID && removedBookGenres.Contains(bg.GenreID))
                            .ToList();
                        _context.BookGenres.RemoveRange(bookGenresToRemove);
                        await _context.SaveChangesAsync();
                    }

                    // Then add new ones
                    if (newBookGenres.Any())
                    {
                        foreach (var genreId in newBookGenres)
                        {
                            var bookGenre = new BookGenre
                            {
                                BookID = req.ID,
                                GenreID = genreId
                            };
                            _context.BookGenres.Add(bookGenre);
                        }
                        await _context.SaveChangesAsync();
                    }
                }

                _context.Books.Update(book);
                await _context.SaveChangesAsync();

                // Cập nhật tags tự động sau khi sửa sách
                try
                {
                    await UpdateTagsAutomatically();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Tag update failed, but book was updated: {ex.Message}");
                    // Continue execution, don't roll back the transaction
                }

                await transaction.CommitAsync();

                if (!string.IsNullOrEmpty(oldThumbnailPath) && System.IO.File.Exists(oldThumbnailPath))
                {
                    System.IO.File.Delete(oldThumbnailPath);
                }

                TempData[ToastrConstant.SUCCESS_MSG] = "Modify Success";
                return Redirect($"{RouteConstant.ADMIN_BOOKS}?menuKey=BM");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData[ToastrConstant.ERROR_MSG] = ex.Message;
                Console.WriteLine($"Error modifying book: {ex.Message}");
                Console.WriteLine($"Inner Exception: {ex.InnerException?.Message}");

                var book = await _context.Books
                    .Include(b => b.BookGenres)
                    .ThenInclude(bg => bg.Genre)
                    .FirstOrDefaultAsync(x => x.ID == req.ID);

                ViewBag.Series = _context.Series.ToList();
                // Bỏ Tags khỏi ViewBag
                ViewBag.Genres = _context.Genres.ToList();
                if (book != null)
                {
                    ViewBag.SelectedGenres = book.BookGenres.Select(bg => bg.GenreID).ToList();
                }

                return View(book);
            }
        }

        [PermissionFilter(Name = "CanDelete")]
        [HttpDelete]
        public async Task<IActionResult> RemoveBookByIDAPI(Guid bookID)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // First check if the book exists
                var book = await _context.Books
                    .Include(b => b.BookGenres)
                    .Include(b => b.OrderDetails)
                    .FirstOrDefaultAsync(b => b.ID == bookID);

                if (book == null)
                {
                    throw new Exception("Book not found");
                }

                // Process deletions in order to avoid constraint violations
                if (book.BookGenres != null && book.BookGenres.Any())
                {
                    _context.BookGenres.RemoveRange(book.BookGenres);
                    await _context.SaveChangesAsync();
                }

                if (book.OrderDetails != null && book.OrderDetails.Any())
                {
                    _context.OrderDetails.RemoveRange(book.OrderDetails);
                    await _context.SaveChangesAsync();
                }

                _context.Books.Remove(book);
                await _context.SaveChangesAsync();

                // Cập nhật tags tự động sau khi xóa sách (BestSeller có thể thay đổi)
                try
                {
                    await UpdateTagsAutomatically();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Tag update failed, but book was deleted: {ex.Message}");
                    // Continue execution, don't roll back the transaction
                }

                await transaction.CommitAsync();

                // Delete thumbnail file if exists
                if (!string.IsNullOrEmpty(book.Thumbnail))
                {
                    var thumbnailPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", book.Thumbnail.TrimStart('/'));
                    if (System.IO.File.Exists(thumbnailPath))
                    {
                        System.IO.File.Delete(thumbnailPath);
                    }
                }

                return Ok(new ResponseAPI<string>() { Success = true, Message = "Remove Success" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"Error removing book: {ex.Message}");
                Console.WriteLine($"Inner Exception: {ex.InnerException?.Message}");
                return Ok(new ResponseAPI<string>() { Success = false, Message = ex.Message });
            }
        }

        [PermissionFilter(Name = "CanView")]
        public async Task<IActionResult> UpdateTags()
        {
            try
            {
                await UpdateTagsAutomatically();
                TempData[ToastrConstant.SUCCESS_MSG] = "Tags updated successfully.";
            }
            catch (Exception ex)
            {
                TempData[ToastrConstant.ERROR_MSG] = $"Error updating tags: {ex.Message}";
                Console.WriteLine($"Error in UpdateTags action: {ex.Message}");
                Console.WriteLine($"Inner Exception: {ex.InnerException?.Message}");
            }

            return Redirect($"{RouteConstant.ADMIN_BOOKS}?menuKey=BM");
        }
    }
}