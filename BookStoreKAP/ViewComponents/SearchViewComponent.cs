using BookStoreKAP.Data;
using Microsoft.AspNetCore.Mvc;

namespace BookStoreKAP.ViewComponents
{
    public class SearchViewComponent : ViewComponent
    {
        private readonly BookStoreKAPDBContext _context;

        public SearchViewComponent(BookStoreKAPDBContext context)
        {
            _context = context;
        }

        public IViewComponentResult Invoke()
        {
            
            return View();
        }
    }
   
}
