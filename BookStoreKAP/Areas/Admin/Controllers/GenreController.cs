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
    public class GenreController : Controller
    {
        private readonly BookStoreKAPDBContext _context;
        public GenreController(BookStoreKAPDBContext context)
        {
            _context = context;
        }

        public IActionResult Index([FromQuery] ReqQuerySearchGenre q)
        {
            var genres = _context.Genres.Where(x => (string.IsNullOrEmpty(q.Name) || x.Name.Trim().ToUpper().Contains(q.Name.Trim().ToUpper()))).OrderBy(x => x.UpdatedAt).ToList();
            var totalItems = genres.Count;
            var paged = genres.Skip((q.page - 1) * q.pageSize).Take(q.pageSize).ToList();

            ViewBag.SearchValue = q;
            ViewBag.Pagination = new PaginationModel()
            {
                TotalItems = totalItems,
                CurrentPage = q.page,
                PageSize = q.pageSize,
                SearchParams = q,
                Action = "Index",
                Controller = "Users"
            };
            return View(genres);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(ReqGenreCreate req)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    throw new Exception("Invalid Values");
                }

                var genre = new Genre
                {
                    Name = req.Name,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Genres.Add(genre);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Genre created successfully.";
                return RedirectToAction(nameof(Index)); // Redirect to the index or any other page after success
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View(req);
            }
        }

        public IActionResult Modify(Guid genreID)
        {
            return View();
        }
    }
}
