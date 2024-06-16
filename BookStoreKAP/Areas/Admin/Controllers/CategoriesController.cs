using BookStoreKAP.Common.Constants;
using BookStoreKAP.Database;
using BookStoreKAP.Models.DTO;
using BookStoreKAP.Models.Entities;
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

        public IActionResult Index()
        {
            var series = _context.Series.ToList();
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
    }
}
