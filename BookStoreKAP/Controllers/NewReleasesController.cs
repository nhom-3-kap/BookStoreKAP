using BookStoreKAP.Data;
using BookStoreKAP.Models;
using BookStoreKAP.Models.DTO;
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
        public async Task<IActionResult> IndexAsync(string Service, Guid? genresId, string input)
        {
            if (Service == null)
            {
                return NotFound();
            }

            ViewBag.Genres = _context.Genres.ToList();
            ViewBag.Service = Service;
            ViewBag.GenresID = genresId;
            var books = new List<Book>();
            if (!string.IsNullOrEmpty(Service) && genresId == null)
            {
                books = _context.Books
                                .Where(b => b.Tag.Name == Service)
                                .OrderBy(b => b.Title)
                                .ToList();
            }
            else
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
            
            
            ViewBag.Books = books;
             
            return View();

        }
        [HttpPost("/List/SortPriceBook")]
        public IActionResult SortPriceBook(ReqBookByDK req)
        {
            var books = new List<Book>();
            if (req.GenreID != Guid.Empty)
            { 
            books = _context.BookGenres
                                .Where(b => b.Book.Discount <= req.MaxPrice && b.Book.Discount >= req.MinPrice && b.Book.Tag.Name == req.Service && b.GenreID == req.GenreID)
                                .Select(a => a.Book).Distinct()
                                .OrderBy(b => b.Title)
                                .ToList();
            }
            else
            {
                books = _context.BookGenres
                                .Where(b => b.Book.Discount <= req.MaxPrice && b.Book.Discount >= req.MinPrice && b.Book.Tag.Name == req.Service )
                                .Select(a => a.Book).Distinct()
                                .OrderBy(b => b.Title)
                                .ToList();
            }    
            return Ok(new ResponseAPI<List<Book>>() { Success=true, Message = "Success", Data=books});
        }
    }
}