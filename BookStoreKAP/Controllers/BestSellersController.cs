using BookStoreKAP.Data;
using BookStoreKAP.Models;
using BookStoreKAP.Models.DTO;
using BookStoreKAP.Models.Entities;
using BookStoreKAP.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BookStoreKAP.Controllers
{
    public class BestSellersController : Controller
    {
        private readonly BookStoreKAPDBContext _context;
        private readonly IPromotionService _promotionService;

        public BestSellersController(BookStoreKAPDBContext context, IPromotionService promotionService)
        {
            _context = context;
            _promotionService = promotionService;
        }

        [Route("/BestSellers")]
        public IActionResult Index(string Service, Guid? genresId, string input, string Hambuger, string sortBy = "")
        {
            ViewBag.Genres = _context.Genres.ToList();
            ViewBag.Service = Service;
            ViewBag.GenresID = genresId;
            ViewBag.Hambuger = Hambuger;
            ViewBag.SortBy = sortBy;

            var books = new List<Book>();
            if (input == null)
            {
                // ĐẶC BIỆT QUAN TRỌNG: Sử dụng Include để tải dữ liệu sách trực tiếp
                if (genresId != null && genresId != Guid.Empty)
                {
                    books = _context.BookGenres
                            .Include(bg => bg.Book)
                            .Where(bg => bg.GenreID == genresId && bg.Book.Tag.Name == Service)
                            .Select(bg => bg.Book)
                            .ToList();
                }
                else
                {
                    books = _context.Books
                            .Include(b => b.Tag)
                            .Where(b => b.Tag.Name == Service)
                            .ToList();
                }

                // Luôn sắp xếp Best Seller theo BuyCount giảm dần
                books = books.OrderByDescending(b => b.BuyCount).ToList();

                if (books == null || !books.Any())
                {
                    books ??= new List<Book>();
                }
            }
            else
            {
                // Tìm kiếm
                if (genresId == null)
                {
                    books = _context.BookGenres
                                    .Where(b => b.Book.Title.Contains(input) || b.Book.Author.Contains(input) || b.Book.Publisher.Contains(input) || b.Genre.Name.Contains(input))
                                    .Select(B => B.Book).Distinct()
                                    .ToList();
                }
                else
                {
                    books = _context.BookGenres
                                     .Where(b => (b.Book.Title.Contains(input) || b.Book.Author.Contains(input) || b.Book.Publisher.Contains(input)
                                     || b.Genre.Name.Contains(input)) && b.GenreID == genresId)
                                     .Select(B => B.Book).Distinct()
                                     .ToList();
                }

                // Luôn sắp xếp kết quả tìm kiếm theo BuyCount
                books = books.OrderByDescending(b => b.BuyCount).ToList();
            }

            // Áp dụng sắp xếp dựa trên tham số sortBy nếu được chỉ định
            if (!string.IsNullOrEmpty(sortBy))
            {
                switch (sortBy)
                {
                    case "buycount":
                        books = books.OrderByDescending(b => b.BuyCount).ToList();
                        break;
                    case "newest":
                        books = books.OrderByDescending(b => b.CreatedAt).ToList();
                        break;
                    case "pricelow":
                        books = books.OrderBy(b => b.Discount).ToList();
                        break;
                    case "pricehigh":
                        books = books.OrderByDescending(b => b.Discount).ToList();
                        break;
                    default:
                        // Mặc định cho Best Seller là sắp xếp theo BuyCount
                        books = books.OrderByDescending(b => b.BuyCount).ToList();
                        break;
                }
            }

            // Lấy thông tin khuyến mãi cho best seller
            var bestSellerPromotions = _promotionService.GetBestSellerPromotions();
            ViewBag.BestSellerPromotions = bestSellerPromotions;

            ViewBag.Books = books;
            return View();
        }

        [HttpPost("/BestSellers/SortPriceBook")]
        public IActionResult SortPriceBook(ReqBookByDK req)
        {
            var books = new List<Book>();
            if (!string.IsNullOrEmpty(req.input))
            {
                books = _context.BookGenres
                        .Where(b => b.Book.Title.Contains(req.input ?? "") || b.Book.Author.Contains(req.input ?? "") || b.Book.Publisher.Contains(req.input ?? "") || b.Genre.Name.Contains(req.input ?? "")
                            && b.Book.Discount <= req.MaxPrice && b.Book.Discount >= req.MinPrice && b.Book.Tag.Name.Contains(req.Service ?? "") && (req.GenreID == Guid.Empty || b.GenreID == req.GenreID))
                        .Select(B => B.Book).Distinct()
                        .ToList();
            }
            else
            {
                books = _context.BookGenres
                        .Where(b => b.Book.Discount <= req.MaxPrice && b.Book.Discount >= req.MinPrice && b.Book.Tag.Name.Contains(req.Service ?? "") && (req.GenreID == Guid.Empty || b.GenreID == req.GenreID))
                        .Select(B => B.Book).Distinct()
                        .ToList();
            }

            // Áp dụng sắp xếp
            if (!string.IsNullOrEmpty(req.SortBy))
            {
                switch (req.SortBy)
                {
                    case "buycount":
                        books = books.OrderByDescending(b => b.BuyCount).ToList();
                        break;
                    case "newest":
                        books = books.OrderByDescending(b => b.CreatedAt).ToList();
                        break;
                    case "pricelow":
                        books = books.OrderBy(b => b.Discount).ToList();
                        break;
                    case "pricehigh":
                        books = books.OrderByDescending(b => b.Discount).ToList();
                        break;
                    default:
                        // Mặc định luôn sắp xếp theo BuyCount
                        books = books.OrderByDescending(b => b.BuyCount).ToList();
                        break;
                }
            }
            else
            {
                // Nếu không có SortBy, luôn sắp xếp theo BuyCount
                books = books.OrderByDescending(b => b.BuyCount).ToList();
            }

            return Ok(new ResponseAPI<List<Book>>() { Success = true, Message = "Success", Data = books });
        }

        [HttpGet("/BestSellers/BestSellerPromotions")]
        public IActionResult GetBestSellerPromotions()
        {
            var bestSellerPromotions = _promotionService.GetBestSellerPromotions();
            return Ok(new ResponseAPI<List<Promotion>>() { Success = true, Message = "Success", Data = bestSellerPromotions });
        }
    }
}