using BookStoreKAP.Common.Constants;
using BookStoreKAP.Database;
using BookStoreKAP.Models.DTO;
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
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(ReqCreateBook req)
        {
            return View();
        }
    }
}
