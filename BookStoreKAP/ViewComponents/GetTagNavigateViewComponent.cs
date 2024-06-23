using BookStoreKAP.Data;
using BookStoreKAP.Models.DTO;
using Microsoft.AspNetCore.Mvc;

namespace BookStoreKAP.ViewComponents
{
    public class GetTagNavigateViewComponent : ViewComponent
    {
        private readonly BookStoreKAPDBContext _context;

        public GetTagNavigateViewComponent(BookStoreKAPDBContext context)
        {
            _context = context;
        }

        public IViewComponentResult Invoke()
        {
            var tags = _context.Tags.ToList();
            return View(tags);
        }
    }
}
