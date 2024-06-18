using BookStoreKAP.Common.Constants;
using BookStoreKAP.Database;
using BookStoreKAP.Models;
using BookStoreKAP.Models.DTO;
using BookStoreKAP.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Framework;
using Microsoft.EntityFrameworkCore;

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

        public async Task<IActionResult> Modify(Guid genreID)
        {
            var genre = await _context.Genres.FindAsync(genreID);
            if (genre == null)
            {
                return NotFound();
            }

            var model = new ReqGenreModify
            {
                GenreID = genre.ID,
                Name = genre.Name
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Modify(ReqGenreModify req)
        {
            if (!ModelState.IsValid)
            {
                TempData[ToastrConstant.ERROR_MSG] = "Invalid data.";
                return View(req);
            }

            try
            {
                var genre = await _context.Genres.FindAsync(req.GenreID);
                if (genre == null)
                {
                    return NotFound();
                }

                if (!string.IsNullOrEmpty(req.Name))
                {
                    genre.Name = req.Name;
                }

                genre.UpdatedAt = DateTime.UtcNow;
                _context.Genres.Update(genre);
                await _context.SaveChangesAsync();

                TempData[ToastrConstant.SUCCESS_MSG] = "Genre updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData[ToastrConstant.ERROR_MSG] = ex.Message;
                return View(req);
            }
        }

        // Xử lý yêu cầu xóa Genre theo ID
        [HttpDelete]
        public async Task<IActionResult> DeleteGenreByIdAPI(Guid genreID)
        {
            try
            {
                // Tìm Genre theo ID
                var genre = await _context.Genres
                    .Include(g => g.BookGenres) // Bao gồm các liên kết BookGenres
                    .FirstOrDefaultAsync(g => g.ID == genreID) ?? throw new Exception("Genre is not exsits!");

                // Xóa các liên kết BookGenres trước khi xóa Genre
                _context.BookGenres.RemoveRange(genre.BookGenres);
                _context.Genres.Remove(genre);
                await _context.SaveChangesAsync();

                return Ok(new ResponseAPI<string>() { Success = true, Message = "Deleted successfully" });
            }
            catch (Exception ex)
            {
                return Ok(new ResponseAPI<string>() { Success = false, Message = ex.Message });
            }
        }
    }
}
