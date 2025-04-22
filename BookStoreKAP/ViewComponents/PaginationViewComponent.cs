using BookStoreKAP.Models.DTO;
using Microsoft.AspNetCore.Mvc;

namespace BookStoreKAP.ViewComponents
{
    public class PaginationViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(PaginationDTO paginationViewModel)
        {
            return View(paginationViewModel);
        }
    }
}
