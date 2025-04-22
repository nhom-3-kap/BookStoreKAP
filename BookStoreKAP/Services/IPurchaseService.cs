using BookStoreKAP.Models;
using BookStoreKAP.Models.Entities;
using System.Collections.Generic;

namespace BookStoreKAP.Services
{
    public interface IPurchaseService
    {
        List<PurchaseViewModel> GetAllPurchases();
        PurchaseViewModel GetPurchaseById(Guid id);
        void UpdatePurchase(PurchaseViewModel model);
        void DeletePurchase(Guid id); 
        List<PurchaseViewModel> GetPurchasesByUserId(Guid userId); 

    }

}
