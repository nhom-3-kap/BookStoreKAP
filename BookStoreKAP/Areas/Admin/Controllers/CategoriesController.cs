using BookStoreKAP.Common.Constants;
using BookStoreKAP.Database;
using BookStoreKAP.Models;
using BookStoreKAP.Models.DTO;
using BookStoreKAP.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BookStoreKAP.Areas.Admin.Controllers
{
    [Area(AreasConstant.ADMIN)]
    public class CategoriesController : Controller
    {
        private readonly BookStoreKAPDBContext _context;
        public CategoriesController(BookStoreKAPDBContext context)
        {
            _context = context;
        }

        public IActionResult Index([FromQuery] ReqQuerySearchSeries q)
        {
            var series = _context.Series.Where(x =>
                                                  (string.IsNullOrEmpty(q.Name) || x.Name.ToUpper().Trim().Contains(q.Name.ToUpper().Trim())) &&
                                                  (q.Volumns == -1 || x.Volumns == q.Volumns)).ToList();
            ViewBag.SearchValue = q;

            return View(series);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(ReqCreateSeries req)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (!ModelState.IsValid)
                {
                    throw new Exception("Invalid input values");
                }

                var newSeries = new Series
                {
                    Name = req.Name,
                    Volumns = req.Volumns
                };

                _context.Series.Add(newSeries);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError(string.Empty, "An error occurred while creating the series.");
                return View(req);
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteCategoryByIdAPI(Guid categoryId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (categoryId.Equals(Guid.Empty))
                {
                    throw new Exception("Category is not exsits");
                }

                var category = await _context.Series.FindAsync(categoryId) ?? throw new Exception("Category is not exsits");

                _context.Remove(category);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new ResponseAPI<string>() { Success = true, Message = "Delete Successfully" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Ok(new ResponseAPI<string>() { Success = false, Message = ex.Message });
            }

        }

        public IActionResult Modify(Guid seriesID)
        {
            var category = _context.Series.Find(seriesID) ?? new Series();

            return View(category);
        }

        [HttpPost]
        public async Task<IActionResult> Modify(ReqModifyCategory req)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var category = await _context.Series.FindAsync(req.SeriesID);
                if (category == null)
                {
                    return NotFound();
                }

                // Cập nhật thông tin category
                if (!string.IsNullOrEmpty(req.Name))
                {
                    category.Name = req.Name;
                }
                category.Volumns = req.Volumns;
                category.UpdatedAt = DateTime.UtcNow;

                _context.Series.Update(category);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                TempData[ToastrConstant.SUCCESS_MSG] = "Category updated successfully.";
                return Redirect($"{RouteConstant.ADMIN_CATEGORIES}?menuKey=CM");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError("", ex.Message);
                TempData[ToastrConstant.ERROR_MSG] = "An error occurred while updating the category.";
                var category = await _context.Series.FindAsync(req.SeriesID) ?? new Series();
                return View(category);
            }
        }

    }
}
