using Microsoft.AspNetCore.Mvc;
using BookStoreKAP.Models;
using BookStoreKAP.Services;
using System;
using System.Linq;
using System.Collections.Generic;
using BookStoreKAP.Models.Entities;
using BookStoreKAP.Common.Constants;
using BookStoreKAP.Filters;
using BookStoreKAP.Models.DTO;

namespace BookStoreKAP.Areas.Admin.Controllers
{
    [Area(AreasConstant.ADMIN)]
    public class PurchasesController : Controller
    {
        private readonly IPurchaseService _purchaseService;
        public PurchasesController(IPurchaseService purchaseService)
        {
            _purchaseService = purchaseService;
        }

        [PermissionFilter(Name = "CanView")]
        public IActionResult Index([FromQuery] ReqQuerySearchPurchase q)
        {
            var purchasesQuery = _purchaseService.GetAllPurchases().AsQueryable();
            if (!string.IsNullOrEmpty(q.Title))
            {
                purchasesQuery = purchasesQuery.Where(p => p.Title.ToUpper().Trim().Contains(q.Title.ToUpper().Trim()));
            }
            if (!string.IsNullOrEmpty(q.Author))
            {
                purchasesQuery = purchasesQuery.Where(p => p.Author.ToUpper().Trim().Contains(q.Author.ToUpper().Trim()));
            }
            if (!string.IsNullOrEmpty(q.UserName))
            {
                purchasesQuery = purchasesQuery.Where(p => p.UserName.ToUpper().Trim().Contains(q.UserName.ToUpper().Trim()));
            }
            var totalItems = purchasesQuery.Count();
            var pagedPurchases = purchasesQuery
                .OrderByDescending(x => x.BoughtDate)
                .Skip((q.Page - 1) * q.PageSize)
                .Take(q.PageSize)
                .ToList();
            ViewBag.SearchValue = q;
            ViewBag.Pagination = new PaginationModel()
            {
                TotalItems = totalItems,
                CurrentPage = q.Page,
                PageSize = q.PageSize,
                SearchParams = q,
                Action = "Index",
                Controller = "Purchases",
                menuKey = "PM"
            };
            return View(pagedPurchases);
        }

        [PermissionFilter(Name = "CanViewEdit")]
        public IActionResult Modify(Guid purchaseId)
        {
            var purchase = _purchaseService.GetPurchaseById(purchaseId);
            if (purchase == null)
            {
                TempData[ToastrConstant.ERROR_MSG] = "Purchase not found";
                return RedirectToAction("Index", new { menuKey = "PM" });
            }
            return View(purchase);
        }

        [PermissionFilter(Name = "CanSaveEdit")]
        [HttpPost]
        public IActionResult Modify(PurchaseViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _purchaseService.UpdatePurchase(model);
                    TempData[ToastrConstant.SUCCESS_MSG] = "Purchase updated successfully";
                    return RedirectToAction("Index", new { menuKey = "PM" });
                }
                catch (Exception ex)
                {
                    TempData[ToastrConstant.ERROR_MSG] = ex.Message;
                    return View(model);
                }
            }
            return View(model);
        }

        [PermissionFilter(Name = "CanDelete")]
        public IActionResult Delete(Guid id)
        {
            try
            {
                _purchaseService.DeletePurchase(id);
                TempData[ToastrConstant.SUCCESS_MSG] = "Purchase deleted successfully";
            }
            catch (Exception ex)
            {
                TempData[ToastrConstant.ERROR_MSG] = ex.Message;
            }
            return RedirectToAction("Index", new { menuKey = "PM" });
        }

        [PermissionFilter(Name = "CanDelete")]
        [HttpDelete]
        public IActionResult RemovePurchaseByIDAPI(Guid purchaseId)
        {
            try
            {
                _purchaseService.DeletePurchase(purchaseId);
                return Ok(new ResponseAPI<string>() { Success = true, Message = "Remove Success" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error removing purchase: {ex.Message}");
                Console.WriteLine($"Inner Exception: {ex.InnerException?.Message}");
                return Ok(new ResponseAPI<string>() { Success = false, Message = ex.Message });
            }
        }
    }
}