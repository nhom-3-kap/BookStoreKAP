using BookStoreKAP.Data;
using BookStoreKAP.Models;
using BookStoreKAP.Models.Entities;
using Microsoft.AspNetCore.Mvc;
namespace BookStoreKAP.Controllers
{
    public class CartController : Controller
    {
        private readonly BookStoreKAPDBContext _context;

        public CartController(BookStoreKAPDBContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult AddToCartAPI()
        {
            var user = _context.Users.FirstOrDefault(x => x.UserName == this.User.Identity.Name);

            if (user == null)
            {
                return Ok(new ResponseAPI<string>() { Success = false, Message = "UnAuthenticated" });
            }

            var cart = _context.Carts.FirstOrDefault(x => x.UserID == user.Id);

            if (cart == null)
            {
                var newCart = new Cart()
                {
                    UserID = user.Id,
                    Status = 0,
                };
            }


            return Ok(new ResponseAPI<string>() { Success = true, Message = "Add Book To Cart Successfully" });
        }
    }
}
