using BookStoreKAP.Data;
using BookStoreKAP.Models;
using BookStoreKAP.Models.DTO;
using BookStoreKAP.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
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
        public IActionResult Index(string Service, List<string> genresId, string input, string Hambuger, int? maxPrice, int page = 1)
        {
            // Log the incoming parameters for debugging
            Console.WriteLine($"Service: {Service}, Genres: {(genresId != null ? string.Join(", ", genresId) : "null")}, Page: {page}");

            ViewBag.Genres = _context.Genres.ToList();
            ViewBag.Service = Service;
            ViewBag.GenresID = genresId;
            ViewBag.Hambuger = Hambuger;
            ViewBag.CurrentPage = page;
            ViewBag.Input = input;
            ViewBag.MaxPrice = maxPrice ?? 500000;

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

            // Parse genresId to valid Guids
            var genreGuidList = new List<Guid>();
            if (genresId != null && genresId.Any())
            {
                foreach (var id in genresId)
                {
                    if (Guid.TryParse(id, out var guid))
                    {
                        genreGuidList.Add(guid);
                    }
                }
                Console.WriteLine($"Parsed {genreGuidList.Count} valid genre GUIDs");
            }

            // Start query with all books
            IQueryable<Book> booksQuery = _context.Books;

            // Filter by tag
            if (tagId != null)
            {
                booksQuery = booksQuery.Where(b => b.TagID == tagId);
            }

            // Filter by keyword
            if (!string.IsNullOrEmpty(input))
            {
                booksQuery = booksQuery.Where(b =>
                    b.Title.Contains(input) ||
                    b.Author.Contains(input) ||
                    b.Publisher.Contains(input));
            }
            if (maxPrice.HasValue && maxPrice.Value < 500000)
            {
                booksQuery = booksQuery.Where(b => b.Discount <= maxPrice.Value);
            }

            // Filter by selected genres
            if (genreGuidList.Any())
            {
                var bookIdsWithAllGenres = _context.BookGenres
                    .Where(bg => genreGuidList.Contains(bg.GenreID))
                    .GroupBy(bg => bg.BookID)
                    .Where(g => g.Select(x => x.GenreID).Distinct().Count() == genreGuidList.Count)
                    .Select(g => g.Key)
                    .ToList();

                booksQuery = booksQuery.Where(b => bookIdsWithAllGenres.Contains(b.ID));
            }


            // Sort depending on tag
            if (tagId == BEST_SELLER_TAG_ID)
                booksQuery = booksQuery.OrderByDescending(b => b.BuyCount);
            else
                booksQuery = booksQuery.OrderByDescending(b => b.CreatedAt);

            // Pagination
            int totalItems = booksQuery.Count();
            int totalPages = (int)Math.Ceiling(totalItems / (double)PAGE_SIZE);
            page = Math.Max(1, Math.Min(page, totalPages > 0 ? totalPages : 1));

            // Get books for the current page
            var booksForCurrentPage = booksQuery
                .Skip((page - 1) * PAGE_SIZE)
                .Take(PAGE_SIZE)
                .ToList();

            // Apply promotions and discounts
            foreach (var book in booksForCurrentPage)
            {
                // Default discount is same as price (no discount)
                if (book.Discount == 0)
                {
                    book.Discount = book.Price;
                }

                // Apply discounts based on tag promotions
                if (book.TagID == BEST_SELLER_TAG_ID && bestSellerPromo != null)
                {
                    book.Discount = Math.Round(book.Price - (book.Price * bestSellerPromo.DiscountPercent / 100), 0);
                }
                else if (book.TagID == NEW_RELEASE_TAG_ID && newReleasePromo != null)
                {
                    book.Discount = Math.Round(book.Price - (book.Price * newReleasePromo.DiscountPercent / 100), 0);
                }
            }

            Console.WriteLine($"Returning {booksForCurrentPage.Count} books for page {page}");

            ViewBag.Books = booksForCurrentPage;
            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = page;
            ViewBag.TotalItems = totalItems;

            return View();
        }


        [HttpPost("/List/SortPriceBook")]
        public IActionResult SortPriceBook([FromBody] RequestDTO<ReqBookByDK> requestDTO)
        {
            try
            {
                // Extract the actual request from the wrapper object
                var req = requestDTO?.req;

                // Log request data for debugging
                Console.WriteLine($"Received filter request: {(req != null ? "Not null" : "Null")}");

                if (req != null)
                {
                    Console.WriteLine($"Service: {req.Service ?? "null"}");
                    Console.WriteLine($"Price Range: {req.MinPrice} - {req.MaxPrice}");
                    Console.WriteLine($"GenreID count: {(req.GenreID?.Count ?? 0)}");
                    if (req.GenreID != null && req.GenreID.Any())
                    {
                        Console.WriteLine($"GenreIDs: {string.Join(", ", req.GenreID)}");
                    }
                }

                var currentDate = DateTime.Now;

                // Check if req is null
                if (req == null)
                {
                    return BadRequest(new ResponseAPI<string>
                    {
                        Success = false,
                        Message = "Request data is null",
                        Data = null
                    });
                }

                // Handle null Service
                string serviceNameToUse = !string.IsNullOrEmpty(req.Service) ? req.Service : "";

                // Handle null GenreID - initialize to empty list if null
                List<Guid> genreIds = req.GenreID ?? new List<Guid>();

                var activePromotions = _context.Promotions
                    .Where(p => p.StartDate <= currentDate && p.EndDate >= currentDate && p.IsActive)
                    .ToList();

                var bestSellerPromo = activePromotions.FirstOrDefault(p => p.TagID == BEST_SELLER_TAG_ID);
                var newReleasePromo = activePromotions.FirstOrDefault(p => p.TagID == NEW_RELEASE_TAG_ID);

                // Safely find tag using null-conditional operator
                var tag = _context.Tags.FirstOrDefault(t => t.Name.ToLower() == serviceNameToUse.ToLower());
                var tagId = tag?.ID;

                // Start with base query of all books
                IQueryable<Book> baseQuery = _context.Books;

                // Apply price filter
                baseQuery = baseQuery.Where(b =>
                    b.Price >= req.MinPrice &&
                    b.Price <= req.MaxPrice);

                // Apply tag filter if specified
                if (tagId != null)
                {
                    baseQuery = baseQuery.Where(b => b.TagID == tagId);
                }

                // Apply search term filter if provided
                if (!string.IsNullOrEmpty(req.Input))
                {
                    baseQuery = baseQuery.Where(b =>
                        b.Title.Contains(req.Input) ||
                        b.Author.Contains(req.Input) ||
                        b.Publisher.Contains(req.Input));
                }

                // Apply genre filter if any genres are selected
                if (genreIds.Count > 0)
                {
                    // We need to handle books with multiple genres that match ANY of the selected genres
                    var bookIdsWithSelectedGenres = _context.BookGenres
                        .Where(bg => genreIds.Contains(bg.GenreID))
                        .Select(bg => bg.BookID)
                        .Distinct()
                        .ToList();

                    baseQuery = baseQuery.Where(b => bookIdsWithSelectedGenres.Contains(b.ID));
                }

                // Apply sorting based on tag
                if (tagId == BEST_SELLER_TAG_ID)
                    baseQuery = baseQuery.OrderByDescending(b => b.BuyCount);
                else
                    baseQuery = baseQuery.OrderByDescending(b => b.CreatedAt);

                // Apply pagination
                int page = req.Page > 0 ? req.Page : 1;
                int pageSize = req.PageSize > 0 ? req.PageSize : PAGE_SIZE;
                int totalItems = baseQuery.Count();
                int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

                // Get the books for the current page
                var books = baseQuery
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                // Calculate discounts for books
                foreach (var book in books)
                {
                    // Default discount is same as price (no discount)
                    book.Discount = book.Price;

                    // Apply discounts based on promotions if applicable
                    if (book.TagID == BEST_SELLER_TAG_ID && bestSellerPromo != null)
                        book.Discount = Math.Round(book.Price - (book.Price * bestSellerPromo.DiscountPercent / 100), 0);
                    else if (book.TagID == NEW_RELEASE_TAG_ID && newReleasePromo != null)
                        book.Discount = Math.Round(book.Price - (book.Price * newReleasePromo.DiscountPercent / 100), 0);
                }

                // Log the response
                Console.WriteLine($"Found {books.Count} books matching the criteria");

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
                            GenreID = genreIds,
                            Input = req.Input
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                // Enhanced exception logging
                Console.WriteLine($"Exception in SortPriceBook: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                    Console.WriteLine($"Inner exception stack trace: {ex.InnerException.StackTrace}");
                }

                return StatusCode(500, new ResponseAPI<string>
                {
                    Success = false,
                    Message = "An error occurred while processing your request: " + ex.Message,
                    Data = null
                });
            }
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

// Add this class if it doesn't exist elsewhere
namespace BookStoreKAP.Models.DTO
{
    public class RequestDTO<T>
    {
        public T req { get; set; }
    }
}