using BookStoreKAP.Common.Constants;
using BookStoreKAP.Data;
using BookStoreKAP.Models;
using BookStoreKAP.Models.DTO;
using BookStoreKAP.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BookStoreKAP.Areas.Admin.Controllers
{
    [Area(AreasConstant.ADMIN)]
    public class SeriesController : Controller
    {
        private readonly BookStoreKAPDBContext _context;
        public SeriesController(BookStoreKAPDBContext context)
        {
            _context = context;
        }

        [Authorize(Policy = "CanView")]
        public IActionResult Index([FromQuery] ReqQuerySearchSeries q)
        {
            var series = _context.Series.Where(x =>
                                                  (string.IsNullOrEmpty(q.Name) || x.Name.ToUpper().Trim().Contains(q.Name.ToUpper().Trim())) &&
                                                  (q.Volumns == -1 || x.Volumns == q.Volumns)).ToList();
            ViewBag.SearchValue = q;

            return View(series);
        }

        [Authorize(Policy = "CanViewCreate")]
        public IActionResult Create()
        {
            return View();
        }

        [Authorize(Policy = "CanSaveCreate")]
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

        [Authorize(Policy = "CanDelete")]
        [HttpDelete]
        public async Task<IActionResult> DeleteSeriesByIdAPI(Guid seriesID)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (seriesID.Equals(Guid.Empty))
                {
                    throw new Exception("Series is not exsits");
                }

                var series = await _context.Series.FindAsync(seriesID) ?? throw new Exception("Series is not exsits");

                _context.Remove(series);
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

        [Authorize(Policy = "CanViewModify")]
        public IActionResult Modify(Guid seriesID)
        {
            var series = _context.Series.Find(seriesID) ?? new Series();

            return View(series);
        }

        [Authorize(Policy = "CanSaveModify")]
        [HttpPost]
        public async Task<IActionResult> Modify(ReqModifySeries req)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var series = await _context.Series.FindAsync(req.SeriesID);
                if (series == null)
                {
                    return NotFound();
                }

                // Cập nhật thông tin series
                if (!string.IsNullOrEmpty(req.Name))
                {
                    series.Name = req.Name;
                }
                series.Volumns = req.Volumns;
                series.UpdatedAt = DateTime.UtcNow;

                _context.Series.Update(series);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                TempData[ToastrConstant.SUCCESS_MSG] = "Series updated successfully.";
                return Redirect($"{RouteConstant.ADMIN_SERIES}?menuKey=CM");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError("", ex.Message);
                TempData[ToastrConstant.ERROR_MSG] = "An error occurred while updating the series.";
                var series = await _context.Series.FindAsync(req.SeriesID) ?? new Series();
                return View(series);
            }
        }

    }
}
