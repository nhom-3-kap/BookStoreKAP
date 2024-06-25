using BookStoreKAP.Data;
using BookStoreKAP.Models;
using BookStoreKAP.Models.DTO;
using BookStoreKAP.Models.Entities;
using Microsoft.AspNetCore.Mvc;

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
        public IActionResult Index(string Service, Guid? genresId, string input, string Hambuger)
        {
            ViewBag.Genres = _context.Genres.ToList();
            ViewBag.Service = Service;
            ViewBag.GenresID = genresId;
            ViewBag.Hambuger = Hambuger;
            var books = new List<Book>();
            if (input == null)
            {
                books = _context.Books
                                    .Where(b => b.Tag.Name.Contains(Service ?? ""))
                                    .OrderBy(b => b.Title)
                                    .ToList();
                if (!string.IsNullOrEmpty(Service) && genresId != null && genresId != Guid.Empty)
                {
                    books = _context.BookGenres
                                    .Where(b => b.GenreID == genresId && b.Book.Tag.Name == Service)
                                    .Select(B => B.Book)
                                    .OrderBy(b => b.Title)
                                    .ToList();
                }
                if (books == null || !books.Any())
                {
                    books ??= new List<Book>();

                }
            }
            else
            {
                if (genresId == null)
                {
                    books = _context.BookGenres
                                    .Where(b => b.Book.Title.Contains(input) || b.Book.Author.Contains(input) || b.Book.Publisher.Contains(input) || b.Genre.Name.Contains(input))
                                    .Select(B => B.Book).Distinct()
                                    .OrderBy(b => b.Title)
                                    .ToList();
                }
                else
                {
                    books = _context.BookGenres
                                     .Where(b => (b.Book.Title.Contains(input) || b.Book.Author.Contains(input) || b.Book.Publisher.Contains(input)
                                     || b.Genre.Name.Contains(input)) && b.GenreID == genresId)
                                     .Select(B => B.Book).Distinct()
                                     .OrderBy(b => b.Title)
                                     .ToList();
                }
            }

            ViewBag.Books = books;
            return View();

        }
        [HttpPost("/List/SortPriceBook")]
        public IActionResult SortPriceBook(ReqBookByDK req)
        {
            var books = new List<Book>();
            if (!string.IsNullOrEmpty(req.input))
            {
                books = _context.BookGenres
                        .Where(b => b.Book.Title.Contains(req.input ?? "") || b.Book.Author.Contains(req.input ?? "") || b.Book.Publisher.Contains(req.input ?? "") || b.Genre.Name.Contains(req.input ?? "")
                            && b.Book.Discount <= req.MaxPrice && b.Book.Discount >= req.MinPrice && b.Book.Tag.Name.Contains(req.Service ?? "") && (req.GenreID == Guid.Empty || b.GenreID == req.GenreID))
                        .Select(B => B.Book).Distinct()
                        .OrderBy(b => b.Title)
                        .ToList();
            }
            else
            {
                books = _context.BookGenres
                        .Where(b => b.Book.Discount <= req.MaxPrice && b.Book.Discount >= req.MinPrice && b.Book.Tag.Name.Contains(req.Service ?? "") && (req.GenreID == Guid.Empty || b.GenreID == req.GenreID))
                        .Select(B => B.Book).Distinct()
                        .OrderBy(b => b.Title)
                        .ToList();
            }
            return Ok(new ResponseAPI<List<Book>>() { Success = true, Message = "Success", Data = books });
        }
    }
}