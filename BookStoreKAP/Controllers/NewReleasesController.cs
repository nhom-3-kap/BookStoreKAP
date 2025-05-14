using BookStoreKAP.Data;
using BookStoreKAP.Models;
using BookStoreKAP.Models.DTO;
using BookStoreKAP.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BookStoreKAP.Controllers
{
    public class NewReleasesController : Controller
    {
        private readonly BookStoreKAPDBContext _context;
        private static readonly Guid BEST_SELLER_TAG_ID = new Guid("3E5CAC9B-6E5A-416D-86F8-52F044D6994E");
        private static readonly Guid NEW_RELEASE_TAG_ID = new Guid("CA038048-95D2-4BFD-86D8-740FB2ECE1AF");
        private const int PAGE_SIZE = 4; // Số sách trên mỗi trang

        public NewReleasesController(BookStoreKAPDBContext context)
        {
            _context = context;
        }

        [Route("/List")]
        public IActionResult Index(string Service, Guid? genresId, string input, string Hambuger, int page = 1)
        {
            ViewBag.Genres = _context.Genres.ToList();
            ViewBag.Service = Service;
            ViewBag.GenresID = genresId;
            ViewBag.Hambuger = Hambuger;
            ViewBag.CurrentPage = page;
            ViewBag.Input = input;

            var tag = _context.Tags.FirstOrDefault(t => t.Name.ToLower() == (Service ?? "").ToLower());
            var tagId = tag?.ID;

            var currentDate = DateTime.Now;
            var activePromotions = _context.Promotions
                .Where(p => p.StartDate <= currentDate && p.EndDate >= currentDate && p.IsActive)
                .OrderByDescending(p => p.StartDate)
                .ToList();

            var bestSellerPromo = activePromotions.FirstOrDefault(p => p.TagID == BEST_SELLER_TAG_ID);
            var newReleasePromo = activePromotions.FirstOrDefault(p => p.TagID == NEW_RELEASE_TAG_ID);

            ViewBag.BestSellerPromotion = bestSellerPromo;
            ViewBag.NewReleasePromotion = newReleasePromo;
            ViewBag.CurrentPromotion = null;

            if (tagId == BEST_SELLER_TAG_ID)
                ViewBag.CurrentPromotion = bestSellerPromo;
            else if (tagId == NEW_RELEASE_TAG_ID)
                ViewBag.CurrentPromotion = newReleasePromo;

            if (Service == "Best Seller")
            {
                ViewBag.BestSellerPromotions = _context.Promotions
                    .Where(p => p.TagID == BEST_SELLER_TAG_ID && p.StartDate <= currentDate && p.EndDate >= currentDate && p.IsActive)
                    .ToList();
            }

            IQueryable<Book> booksQuery;
            if (string.IsNullOrEmpty(input))
            {
                if (tagId == BEST_SELLER_TAG_ID)
                {
                    booksQuery = _context.Books
                        .Where(b => b.TagID == BEST_SELLER_TAG_ID)
                        .OrderByDescending(b => b.BuyCount);
                    if (genresId != null && genresId != Guid.Empty)
                    {
                        booksQuery = _context.BookGenres
                            .Where(bg => bg.GenreID == genresId && bg.Book.TagID == BEST_SELLER_TAG_ID)
                            .Select(bg => bg.Book)
                            .OrderByDescending(b => b.BuyCount);
                    }
                }
                else
                {
                    booksQuery = _context.Books
                        .Where(b => b.TagID == tagId)
                        .OrderByDescending(b => b.CreatedAt);
                    if (tagId != null && genresId != null && genresId != Guid.Empty)
                    {
                        booksQuery = _context.BookGenres
                            .Where(bg => bg.GenreID == genresId && bg.Book.TagID == tagId)
                            .Select(bg => bg.Book)
                            .OrderByDescending(b => b.CreatedAt);
                    }
                }
            }
            else
            {
                var query = _context.BookGenres.AsQueryable();
                query = query.Where(bg =>
                    (bg.Book.Title.Contains(input) ||
                     bg.Book.Author.Contains(input) ||
                     bg.Book.Publisher.Contains(input) ||
                     bg.Genre.Name.Contains(input)));

                if (genresId != null && genresId != Guid.Empty)
                    query = query.Where(bg => bg.GenreID == genresId);

                if (tagId != null)
                    query = query.Where(bg => bg.Book.TagID == tagId);

                booksQuery = query.Select(bg => bg.Book).Distinct();

                if (tagId == BEST_SELLER_TAG_ID)
                    booksQuery = booksQuery.OrderByDescending(b => b.BuyCount);
                else
                    booksQuery = booksQuery.OrderByDescending(b => b.CreatedAt);
            }

            var totalItems = booksQuery.Count();
            var totalPages = (int)Math.Ceiling(totalItems / (double)PAGE_SIZE);
            page = Math.Max(1, Math.Min(page, totalPages > 0 ? totalPages : 1));

            var booksForCurrentPage = booksQuery
                .Skip((page - 1) * PAGE_SIZE)
                .Take(PAGE_SIZE)
                .ToList();

            ViewBag.Books = booksForCurrentPage;
            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = page;
            ViewBag.TotalItems = totalItems;

            return View();
        }

        [HttpPost("/List/SortPriceBook")]
        public IActionResult SortPriceBook([FromBody] ReqBookByDK req)
        {
            var currentDate = DateTime.Now;

            var activePromotions = _context.Promotions
                .Where(p => p.IsActive)
                .ToList();

            var bestSellerPromo = activePromotions.FirstOrDefault(p => p.TagID == BEST_SELLER_TAG_ID);
            var newReleasePromo = activePromotions.FirstOrDefault(p => p.TagID == NEW_RELEASE_TAG_ID);

            var tag = _context.Tags.FirstOrDefault(t => t.Name.ToLower() == req.Service.ToLower());
            var tagId = tag?.ID;
            if (req.GenreID == null) req.GenreID = new List<Guid>();

            var query = _context.BookGenres
                .Include(bg => bg.Book)
                .Where(bg =>
                    bg.Book.Price >= req.MinPrice &&
                    bg.Book.Price <= req.MaxPrice &&
                    (tagId == null || bg.Book.TagID == tagId) &&
                    (string.IsNullOrEmpty(req.Input) ||
                        bg.Book.Title.Contains(req.Input) ||
                        bg.Book.Author.Contains(req.Input) ||
                        bg.Book.Publisher.Contains(req.Input) ||
                        bg.Genre.Name.Contains(req.Input)) &&
                    (req.GenreID == null || req.GenreID.Count == 0 || req.GenreID.Contains(bg.GenreID))
                )
                .Select(bg => bg.Book)
                .Distinct();

            if (tagId == BEST_SELLER_TAG_ID)
                query = query.OrderByDescending(b => b.BuyCount);
            else
                query = query.OrderByDescending(b => b.CreatedAt);

            int page = req.Page > 0 ? req.Page : 1;
            int pageSize = req.PageSize > 0 ? req.PageSize : PAGE_SIZE;
            int totalItems = query.Count();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var books = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            foreach (var book in books)
            {
                book.Discount = book.Price;
                if (book.TagID == BEST_SELLER_TAG_ID && bestSellerPromo != null)
                    book.Discount = Math.Round(book.Price - (book.Price * bestSellerPromo.DiscountPercent / 100), 0);
                else if (book.TagID == NEW_RELEASE_TAG_ID && newReleasePromo != null)
                    book.Discount = Math.Round(book.Price - (book.Price * newReleasePromo.DiscountPercent / 100), 0);
            }

            return Ok(new ResponseAPI<object>()
            {
                Success = true,
                Message = "Success",
                Data = new
                {
                    Books = books,
                    Promotions = new
                    {
                        BestSeller = bestSellerPromo,
                        NewRelease = newReleasePromo
                    },
                    Pagination = new
                    {
                        CurrentPage = page,
                        TotalPages = totalPages,
                        TotalItems = totalItems,
                        PageSize = pageSize,
                        Service = req.Service,
                        GenreID = req.GenreID,
                        Input = req.Input
                    }
                }
            });
        }

        [HttpPost("/Promotion/Create")]
        public IActionResult CreatePromotion(Promotion promotion)
        {
            if (ModelState.IsValid)
            {
                var existingPromo = _context.Promotions
                    .FirstOrDefault(p => p.TagID == promotion.TagID && p.IsActive &&
                                   ((p.StartDate <= promotion.StartDate && p.EndDate >= promotion.StartDate) ||
                                    (p.StartDate <= promotion.EndDate && p.EndDate >= promotion.EndDate)));

                if (existingPromo != null)
                {
                    existingPromo.Name = promotion.Name;
                    existingPromo.Description = promotion.Description;
                    existingPromo.StartDate = promotion.StartDate;
                    existingPromo.EndDate = promotion.EndDate;
                    existingPromo.DiscountPercent = promotion.DiscountPercent;
                    existingPromo.IsActive = true;
                    _context.Update(existingPromo);
                }
                else
                {
                    if (promotion.EndDate == DateTime.MinValue || promotion.EndDate == null)
                    {
                        promotion.StartDate = DateTime.Now;
                        promotion.EndDate = DateTime.Now.AddMonths(1);
                    }

                    promotion.Id = Guid.NewGuid();
                    promotion.IsActive = true;
                    promotion.CreatedAt = DateTime.Now;

                    _context.Promotions.Add(promotion);
                }

                _context.SaveChanges();
                return Ok(new ResponseAPI<Promotion>() { Success = true, Message = "Promotion created successfully", Data = promotion });
            }

            return BadRequest(new ResponseAPI<string>() { Success = false, Message = "Invalid model data", Data = null });
        }

        [HttpPost("/Promotion/End")]
        public IActionResult EndPromotion(Guid promotionId)
        {
            var promotion = _context.Promotions.Find(promotionId);
            if (promotion != null)
            {
                promotion.IsActive = false;
                promotion.EndDate = DateTime.Now;
                _context.SaveChanges();
                return Ok(new ResponseAPI<Promotion>() { Success = true, Message = "Promotion ended successfully", Data = promotion });
            }

            return NotFound(new ResponseAPI<string>() { Success = false, Message = "Promotion not found", Data = null });
        }
    }
}
