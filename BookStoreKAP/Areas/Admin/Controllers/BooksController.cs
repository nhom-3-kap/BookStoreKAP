using BookStoreKAP.Common.Constants;
using BookStoreKAP.Database;
using BookStoreKAP.Models.DTO;
using BookStoreKAP.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookStoreKAP.Areas.Admin.Controllers
{
    [Area(AreasConstant.ADMIN)]
    [Authorize(Roles = RolesConstant.ADMIN)]
    public class BooksController : Controller
    {
        private readonly BookStoreKAPDBContext _context;
        public BooksController(BookStoreKAPDBContext context)
        {
            _context = context;
        }

        // GET: Books/Index
        public IActionResult Index([FromQuery] ReqQuerySearchBook q)
        {
            var booksQuery = _context.Books.AsQueryable();

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

        // POST: Books/Create
        // Phương thức POST để xử lý việc lưu Book mới vào cơ sở dữ liệu
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

        public IActionResult Modify(Guid bookID)
        {
            return View();
        }
    }
}
