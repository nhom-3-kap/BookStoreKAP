using BookStoreKAP.Data;
using BookStoreKAP.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BookStoreKAP.Services
{
    public class PromotionPriceUpdateService : BackgroundService
    {
        private readonly IServiceProvider _services;
        private static readonly TimeSpan _interval = TimeSpan.FromMinutes(10); // Chạy mỗi 10 phút

        public PromotionPriceUpdateService(IServiceProvider services)
        {
            _services = services;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await UpdateBookPricesBasedOnPromotions();
                await Task.Delay(_interval, stoppingToken);
            }
        }

        private async Task UpdateBookPricesBasedOnPromotions()
        {
            using var scope = _services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<BookStoreKAPDBContext>();
            var currentDate = DateTime.Now;

            // Lấy tất cả tag trong hệ thống
            var tags = await dbContext.Tags.ToListAsync();

            foreach (var tag in tags)
            {
                // Lấy chiến dịch khuyến mãi đang hoạt động cho tag này
                var activePromotion = await dbContext.Promotions
                    .Where(p => p.TagID == tag.ID &&
                           p.StartDate <= currentDate &&
                           p.EndDate >= currentDate &&
                           p.IsActive)
                    .OrderByDescending(p => p.DiscountPercent) // Lấy khuyến mãi có % giảm giá cao nhất
                    .FirstOrDefaultAsync();

                // Lấy tất cả sách thuộc tag này
                var books = await dbContext.Books
                    .Where(b => b.TagID == tag.ID)
                    .ToListAsync();

                // Cập nhật giá sách
                foreach (var book in books)
                {
                    if (activePromotion != null)
                    {
                        // Áp dụng giảm giá
                        book.Discount = Math.Round(book.Price * (1 - activePromotion.DiscountPercent / 100), 0);
                    }
                    else
                    {
                        // Đặt lại giá gốc
                        book.Discount = book.Price;
                    }
                }
            }

            await dbContext.SaveChangesAsync();
        }
    }
}