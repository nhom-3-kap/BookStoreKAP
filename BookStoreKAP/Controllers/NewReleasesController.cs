using BookStoreKAP.Data;
using BookStoreKAP.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
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

            ViewBag.Series = _context.Series.ToList();

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
                Guid guid = new Guid("9a4b304f-ed75-4e77-867f-2f941f330b0c");
                ViewBag.Service = "New Releases";
                var lst = _context.Books
                                 .Where(c => c.TagID.Equals(guid))
                                .OrderBy(b => b.Title)
                                .ToList();
                ViewBag.Books = lst;
                return View();

            }
            else if (Service == "BestSellers")
            {
                Guid guid = new Guid("a2e58fe7-0850-414e-b683-d365cc3166ad");
                ViewBag.Service = "BestSellers";
                var lst = _context.Books
                                 .Where(c => c.TagID.Equals(guid))
                                .OrderBy(b => b.Title)
                                .ToList();
                ViewBag.Books = lst;
                return View();
            }
            return View();

        }
    }
}