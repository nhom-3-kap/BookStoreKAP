using BookStoreKAP.Data;
using BookStoreKAP.Models;
using BookStoreKAP.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BookStoreKAP.Services
{
    public interface IPromotionService
    {
        IEnumerable<Promotion> GetAllPromotions();
        List<Promotion> GetActivePromotions();
        Promotion GetActivePromotionByTag(Guid tagId);
        Promotion GetPromotionById(Guid id);
        void CreatePromotion(PromotionViewModel model);
        void UpdatePromotion(PromotionViewModel model);
        void DeletePromotion(Guid id);
        List<Book> GetBooksWithPromotion();
        List<Promotion> GetBestSellerPromotions();
        bool EndPromotion(Guid id);
        bool HasOverlappingPromotion(PromotionViewModel model, bool isUpdate = false);
        decimal ApplyPromotionDiscount(Book book);
    }

    public class PromotionService : IPromotionService
    {
        private readonly BookStoreKAPDBContext _context;
        private static readonly Guid BEST_SELLER_TAG_ID = new Guid("3E5CAC9B-6E5A-416D-86F8-52F044D6994E");
        private static readonly Guid NEW_RELEASE_TAG_ID = new Guid("CA038048-95D2-4BFD-86D8-740FB2ECE1AF");

        public PromotionService(BookStoreKAPDBContext context)
        {
            _context = context;
        }

        public IEnumerable<Promotion> GetAllPromotions()
        {
            return _context.Promotions.Include(p => p.Tag);
        }

        public List<Promotion> GetActivePromotions()
        {
            var currentDate = DateTime.Now;
            return _context.Promotions
                .Where(p => p.StartDate <= currentDate && p.EndDate >= currentDate && p.IsActive)
                .ToList();
        }

        public Promotion GetActivePromotionByTag(Guid tagId)
        {
            var currentDate = DateTime.Now;
            return _context.Promotions
                .Where(p => p.TagID != null && p.TagID == tagId &&
                            p.StartDate <= currentDate &&
                            p.EndDate >= currentDate &&
                            p.IsActive)
                .OrderByDescending(p => p.DiscountPercent)
                .FirstOrDefault();
        }

        public Promotion GetPromotionById(Guid id)
        {
            return _context.Promotions.Find(id);
        }

        public void CreatePromotion(PromotionViewModel model)
        {
            var promotion = new Promotion
            {
                Id = Guid.NewGuid(),
                Name = model.Name,
                Description = model.Description,
                TagID = model.TagID,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                DiscountPercent = model.DiscountPercent,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            _context.Promotions.Add(promotion);
            _context.SaveChanges();
        }

        public void UpdatePromotion(PromotionViewModel model)
        {
            var promotion = _context.Promotions.Find(model.ID);
            if (promotion == null)
            {
                throw new Exception("Promotion not found");
            }

            promotion.Name = model.Name;
            promotion.Description = model.Description;
            promotion.TagID = model.TagID;
            promotion.StartDate = model.StartDate;
            promotion.EndDate = model.EndDate;
            promotion.DiscountPercent = model.DiscountPercent;

            _context.Update(promotion);
            _context.SaveChanges();
        }

        public void DeletePromotion(Guid id)
        {
            var promotion = _context.Promotions.Find(id);
            if (promotion != null)
            {
                _context.Promotions.Remove(promotion);
                _context.SaveChanges();
            }
        }

        public bool EndPromotion(Guid id)
        {
            var promotion = _context.Promotions.Find(id);
            if (promotion == null)
            {
                throw new Exception("Promotion not found");
            }

            if (!promotion.IsActive)
            {
                return false; // Promotion already ended
            }

            promotion.IsActive = false;
            promotion.EndDate = DateTime.Now;
            _context.SaveChanges();
            return true;
        }

        public List<Book> GetBooksWithPromotion()
        {
            var currentDate = DateTime.Now;
            
            // Get active promotions
            var activePromotions = GetActivePromotions();
            var tagIds = activePromotions.Select(p => p.TagID).ToList();
            
            if (!tagIds.Any())
                return new List<Book>();

            var books = _context.Books
                .Where(b => tagIds.Contains((Guid)b.TagID))
                .Include(b => b.Tag)
                .ToList();

            // Apply discounts to books
            foreach (var book in books)
            {
                var promo = activePromotions.FirstOrDefault(p => p.TagID == book.TagID);
                if (promo != null)
                {
                    book.Discount = Math.Round(book.Price - (book.Price * promo.DiscountPercent / 100), 0);
                }
                else
                {
                    book.Discount = book.Price;
                }
            }

            return books;
        }

        public List<Promotion> GetBestSellerPromotions()
        {
            var currentDate = DateTime.Now;

            // Lấy khuyến mãi dành cho sách best seller đang hoạt động
            return _context.Promotions
                .Where(p =>
                    p.IsActive &&
                    p.TagID == BEST_SELLER_TAG_ID &&
                    p.StartDate <= currentDate &&
                    p.EndDate >= currentDate)
                .OrderBy(p => p.StartDate)
                .ToList();
        }

        public bool HasOverlappingPromotion(PromotionViewModel model, bool isUpdate = false)
        {
            var query = _context.Promotions.Where(p => p.TagID == model.TagID && p.IsActive);

            // If updating, exclude the current promotion
            if (isUpdate)
            {
                query = query.Where(p => p.Id != model.ID);
            }

            // Check for date overlaps
            var existingPromo = query.FirstOrDefault(p =>
                (p.StartDate <= model.StartDate && p.EndDate >= model.StartDate) ||
                (p.StartDate <= model.EndDate && p.EndDate >= model.EndDate) ||
                (model.StartDate <= p.StartDate && model.EndDate >= p.EndDate));

            return existingPromo != null;
        }

        public decimal ApplyPromotionDiscount(Book book)
        {
            if (book == null)
                return 0;

            var promotion = GetActivePromotionByTag((Guid)book.TagID);
            if (promotion != null)
            {
                return (decimal)Math.Round(book.Price - (book.Price * promotion.DiscountPercent / 100), 0);
            }

            return (decimal)book.Price;
        }
    }
}