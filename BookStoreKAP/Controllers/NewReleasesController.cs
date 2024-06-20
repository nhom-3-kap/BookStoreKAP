using BookStoreKAP.Data;
using BookStoreKAP.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace BookStoreKAP.Controllers
{

    public class NewReleasesController : Controller

    {
        private readonly BookStoreKAPDBContext _context;

        public NewReleasesController(BookStoreKAPDBContext context)
        {
            _context = context;
        }
        [Route("/List")]
        public async Task<IActionResult> IndexAsync(string Service, Guid? seriesId)
        {
            if (Service == null)
            {
                return NotFound();
            }

            var series = _context.Series.ToList();
            ViewBag.Series = series.Select(s => s.Name).ToList();

            var books = _context.Books
                                .Where(b => b.SeriesID == seriesId)
                                .OrderBy(b => b.Title)
                                .ToList();

            if (books == null || !books.Any())
            {
                books ??= new List<Book>();

            }
            ViewBag.Books = books;

            if (Service == "NewReleases")
            {
                ViewBag.Service = "New Releases";
                return View();

            }
            else if (Service == "BestSellers")
            {
                ViewBag.Service = "Best Sellers";
                return View();
            }
            return View();

        }
    }
}
