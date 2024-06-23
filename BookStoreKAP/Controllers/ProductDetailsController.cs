using BookStoreKAP.Data;
using BookStoreKAP.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookStoreKAP.Controllers
{
    public class ProductDetailsController : Controller
    {
        private readonly BookStoreKAPDBContext _context;

        public ProductDetailsController(BookStoreKAPDBContext context)
        {
            _context = context;
        }
        [Route("/ProductDetails/{ProductId}")]
        public IActionResult Index(String ProductId)
        {
            ViewBag.ProductId = ProductId;
            Guid guid = new Guid(ProductId);
            var item = _context.Books.Where(x => x.ID.Equals(guid)).FirstOrDefault();
            ViewBag.Title = item.Title;
            ViewBag.Publisher = item.Publisher;
            ViewBag.Author = item.Author;
            var genres = _context.Genres
                .Where(g => g.BookGenres.Any(bg => bg.BookID == guid))
                .Select(a => a.Name)
                .ToList();
            ViewBag.Genres = genres;
            var seriesName = _context.Series
                .Where(g => g.Books.Any(x => x.ID == guid))
                .Select(a => a.Name)
                .FirstOrDefault();
            ViewBag.Series = seriesName;
            ViewBag.Price = item.Price;
            ViewBag.Discount = item.Discount;
            ViewBag.Quantity = item.Quantity;
            ViewBag.Description = item.Synopsis;
            ViewBag.Image = item.Thumbnail;
            return View();
        }
    }
}
