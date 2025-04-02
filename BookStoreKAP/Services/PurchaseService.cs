using BookStoreKAP.Data;
using BookStoreKAP.Models;
using BookStoreKAP.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BookStoreKAP.Services
{
    public class PurchaseService : IPurchaseService
    {
        private readonly BookStoreKAPDBContext _context;

        public PurchaseService(BookStoreKAPDBContext context)
        {
            _context = context;
        }

        public List<PurchaseViewModel> GetAllPurchases()
        {
            return _context.OrderDetails
                .Select(od => new PurchaseViewModel
                {
                    ID = od.OrderID,
                    Title = od.Book.Title,
                    Author = od.Book.Author,
                    Price = od.Price,
                    Quantity = od.Quantity,
                    UserName = od.Order.Customer.UserName,
                    BoughtDate = od.Order.OrderDate
                }).ToList();
        }

        public PurchaseViewModel GetPurchaseById(Guid id)
        {
            var orderDetail = _context.OrderDetails
                .FirstOrDefault(od => od.OrderID == id);

            if (orderDetail == null) return null;

            return new PurchaseViewModel
            {
                ID = orderDetail.OrderID,
                Title = orderDetail.Book.Title,
                Author = orderDetail.Book.Author,
                Price = orderDetail.Price,
                Quantity = orderDetail.Quantity,
                UserName = orderDetail.Order.Customer.UserName,
                BoughtDate = orderDetail.Order.OrderDate
            };
        }

        public void UpdatePurchase(PurchaseViewModel model)
        {
            var orderDetail = _context.OrderDetails.FirstOrDefault(od => od.OrderID == model.ID);
            if (orderDetail != null)
            {
                orderDetail.Price = model.Price;
                orderDetail.Quantity = model.Quantity;
                _context.OrderDetails.Update(orderDetail);
                _context.SaveChanges();
            }
        }

        public void DeletePurchase(Guid id)
        {
            var orderDetail = _context.OrderDetails.FirstOrDefault(od => od.OrderID == id);
            if (orderDetail != null)
            {
                _context.OrderDetails.Remove(orderDetail);
                _context.SaveChanges();
            }
        }
    }
}
