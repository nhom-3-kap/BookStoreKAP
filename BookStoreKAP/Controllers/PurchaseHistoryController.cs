using BookStoreKAP.Common.Constants;
using BookStoreKAP.Models;
using BookStoreKAP.Models.DTO;
using BookStoreKAP.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;

namespace BookStoreKAP.Controllers
{
    [Authorize]
    public class PurchaseHistoryController : Controller
    {
        private readonly IPurchaseService _purchaseService;

        public PurchaseHistoryController(IPurchaseService purchaseService)
        {
            _purchaseService = purchaseService;
        }

        public IActionResult Index()
        {
            // Lấy UserId của người dùng hiện tại từ Claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var userPurchases = _purchaseService.GetPurchasesByUserId(userId);
            return View(userPurchases);
        }

        public IActionResult Details(Guid id)
        {
            // Lấy UserId của người dùng hiện tại từ Claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var purchase = _purchaseService.GetPurchaseById(id);
            if (purchase == null || purchase.ID != userId)
            {
                TempData[ToastrConstant.ERROR_MSG] = "Purchase not found or you don't have permission to view it";
                return RedirectToAction("Index");
            }

            return View(purchase);
        }
    }
}
