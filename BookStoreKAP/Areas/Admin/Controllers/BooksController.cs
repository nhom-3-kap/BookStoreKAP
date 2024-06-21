using BookStoreKAP.Common.Constants;
using BookStoreKAP.Data;
using BookStoreKAP.Models;
using BookStoreKAP.Models.DTO;
using BookStoreKAP.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol;

namespace BookStoreKAP.Areas.Admin.Controllers
{
    [Area(AreasConstant.ADMIN)]
    public class BooksController : Controller
    {
        private readonly BookStoreKAPDBContext _context;
        public BooksController(BookStoreKAPDBContext context)
        {
            _context = context;
        }

        [Authorize(Policy = "CanView")]
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
            ViewBag.Pagination = new PaginationModel()
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

        [Authorize(Policy = "CanCreate")]
        public IActionResult Create()
        {
            var tags = _context.Tags.ToList();
            var series = _context.Series.ToList();
            var genres = _context.Genres.ToList();

            ViewBag.Tags = tags;
            ViewBag.Series = series;
            ViewBag.Genres = genres;
            return View();
        }


        [Authorize(Policy = "CanCreate")]
        [HttpPost]
        public async Task<IActionResult> Create(ReqCreateBook req, IFormFile Thumbnail)
        {
            if (!ModelState.IsValid) // Kiểm tra tính hợp lệ của dữ liệu
            {
                TempData[ToastrConstant.ERROR_MSG] = "Invalid data."; // Thông báo lỗi nếu dữ liệu không hợp lệ
                ViewBag.Series = _context.Series.ToList(); // Truyền lại danh sách Series, Tags và Genres vào ViewBag
                ViewBag.Tags = _context.Tags.ToList();
                ViewBag.Genres = _context.Genres.ToList();
                return View(req); // Trả về view với dữ liệu không hợp lệ
            }

            string thumbnailPath = null; // Biến lưu đường dẫn thumbnail

            if (Thumbnail != null && Thumbnail.Length > 0) // Kiểm tra nếu người dùng đã chọn file ảnh
            {
                var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "images", "products"); // Đường dẫn thư mục lưu trữ ảnh
                if (!Directory.Exists(uploads)) // Kiểm tra nếu thư mục chưa tồn tại
                {
                    Directory.CreateDirectory(uploads); // Tạo thư mục nếu chưa tồn tại
                }
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(Thumbnail.FileName)}"; // Tạo tên file mới sử dụng GUID
                var filePath = Path.Combine(uploads, fileName); // Đường dẫn đầy đủ của file ảnh
                using (var fileStream = new FileStream(filePath, FileMode.Create)) // Mở stream để lưu file
                {
                    await Thumbnail.CopyToAsync(fileStream); // Sao chép nội dung file vào stream
                }
                thumbnailPath = $"/uploads/images/products/{fileName}"; // Lưu đường dẫn đầy đủ vào biến thumbnailPath
            }

            // Tạo đối tượng Book từ ReqCreateBook
            var book = new Book
            {
                Title = req.Title,
                Publisher = req.Publisher,
                Author = req.Author,
                PublicationYear = req.PublicationYear,
                Price = req.Price,
                Discount = req.Discount,
                Quantity = req.Quantity,
                Synopsis = req.Synopsis,
                TagID = req.TagID,
                SeriesID = req.SeriesID,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                Thumbnail = thumbnailPath // Lưu đường dẫn thumbnail vào đối tượng Book
            };

            _context.Books.Add(book); // Thêm đối tượng Book vào DbSet Books
            await _context.SaveChangesAsync(); // Lưu các thay đổi vào cơ sở dữ liệu

            // Thêm các liên kết BookGenres vào cơ sở dữ liệu
            foreach (var genreId in req.GenreIds)
            {
                var bookGenre = new BookGenre
                {
                    BookID = book.ID,
                    GenreID = genreId
                };
                _context.BookGenres.Add(bookGenre);
            }

            await _context.SaveChangesAsync(); // Lưu các thay đổi vào cơ sở dữ liệu

            TempData[ToastrConstant.SUCCESS_MSG] = "Book created successfully."; // Thông báo thành công
            return Redirect($"{RouteConstant.ADMIN_BOOKS}?menuKey=BM"); // Chuyển hướng về trang Index
        }

        [Authorize(Policy = "CanEdit")]
        public async Task<IActionResult> Modify(Guid bookID)
        {
            try
            {
                var book = await _context.Books.Include(b => b.BookGenres).ThenInclude(bg => bg.Genre).FirstOrDefaultAsync(x => x.ID == bookID) ?? throw new Exception("Book is not exists!");
                ViewBag.Series = _context.Series.ToList();
                ViewBag.Tags = _context.Tags.ToList();
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

        [Authorize(Policy = "CanEdit")]
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

                var book = await _context.Books.Include(b => b.BookGenres).ThenInclude(bg => bg.Genre).FirstOrDefaultAsync(x => x.ID == req.ID);


                if (book == null)
                {
                    throw new Exception("Book not found");
                }

                book.Title = req.Title;
                book.Publisher = req.Publisher;
                book.PublicationYear = req.PublicationYear;
                book.Author = req.Author;
                book.Price = req.Price;
                book.Discount = req.Discount;
                book.Quantity = req.Quantity;
                book.SeriesID = req.SeriesID;
                book.TagID = req.TagID;
                book.Synopsis = req.Synopsis;

                string oldThumbnailPath = string.Empty;

                // Cập nhật lưu ảnh nếu như có ảnh mới
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

                // Cập nhật BookGenres
                var existingBookGenres = book.BookGenres.Select(bg => bg.GenreID).ToList();
                var newBookGenres = req.GenreIds.Except(existingBookGenres).ToList();
                var removedBookGenres = existingBookGenres.Except(req.GenreIds).ToList();

                if (removedBookGenres.Any())
                {
                    var bookGenresToRemove = _context.BookGenres
                        .Where(bg => bg.BookID == req.ID && removedBookGenres.Contains(bg.GenreID))
                        .ToList();
                    _context.BookGenres.RemoveRange(bookGenresToRemove);
                }

                if (newBookGenres.Any())
                {
                    var bookGenresToAdd = newBookGenres.Select(genreId => new BookGenre
                    {
                        BookID = req.ID,
                        GenreID = genreId
                    }).ToList();
                    await _context.BookGenres.AddRangeAsync(bookGenresToAdd);
                }

                // Save changes
                _context.Books.Update(book);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                if (System.IO.File.Exists(oldThumbnailPath))
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
                var book = await _context.Books.Include(b => b.BookGenres).ThenInclude(bg => bg.Genre).FirstOrDefaultAsync(x => x.ID == req.ID);
                ViewBag.Series = _context.Series.ToList();
                ViewBag.Tags = _context.Tags.ToList();
                ViewBag.Genres = _context.Genres.ToList();

                return View(book);
            }
        }

        [Authorize(Policy = "CanDelete")]
        [HttpDelete]
        public async Task<IActionResult> RemoveBookByIDAPI(Guid bookID)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var book = await _context.Books
                    .Include(b => b.BookGenres)
                    .Include(b => b.OrderDetails)
                    .FirstOrDefaultAsync(b => b.ID == bookID);

                if (book == null)
                {
                    throw new Exception("Book not found");
                }

                // Xóa các liên kết BookGenres
                _context.BookGenres.RemoveRange(book.BookGenres);

                // Xóa các liên kết OrderDetails
                _context.OrderDetails.RemoveRange(book.OrderDetails);

                // Xóa sách
                _context.Books.Remove(book);

                // Lưu thay đổi
                await _context.SaveChangesAsync();
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
