using BookStoreKAP.Data;
using BookStoreKAP.Models.Entities;
using BookStoreKAP.ViewModels;
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
        public IActionResult Index(Guid productId)
        {
            var book = _context.Books.Include(x => x.BookGenres).ThenInclude(x => x.Genre).Include(x => x.Series).FirstOrDefault(x => x.ID == productId);
            if (book == null)
            {
                return NotFound();
            }

            var viewModel = new BookDetailVM() { Book = book };

            return View(viewModel);
        }
    }
}
