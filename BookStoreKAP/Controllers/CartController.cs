using BookStoreKAP.Common.Constants;
using BookStoreKAP.Data;
using BookStoreKAP.Models;
using BookStoreKAP.Models.DTO;
using BookStoreKAP.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
            var user = _context.Users.FirstOrDefault(x => x.UserName == this.User.Identity.Name);

            if (user == null)
            {
                return Redirect($"{RouteConstant.LOGIN}");
            }

            var cart = _context.Carts.FirstOrDefault(x => x.UserID == user.Id && x.Status != StatusCart.DONE);

            if (cart == null)
            {
                return View(new List<CartItem>());
            }

            var cartItems = _context.CartItems.Include(x => x.Book).Where(x => x.CartID == cart.ID).ToList();

            return View(cartItems);
        }

        [HttpPost]
        public IActionResult AddToCartAPI(ReqAddCart req)
        {
            var user = _context.Users.FirstOrDefault(x => x.UserName == this.User.Identity.Name);

            if (user == null)
            {
                return Ok(new ResponseAPI<string>() { Success = false, Message = "UnAuthenticated" });
            }

            var book = _context.Books.FirstOrDefault(x => x.ID == req.BookID && x.Quantity > 0);
            if (book == null)
            {
                return Ok(new ResponseAPI<string>() { Success = false, Message = "Book error" });
            }

            var cart = _context.Carts.FirstOrDefault(x => x.UserID == user.Id && x.Status != StatusCart.DONE);

            if (cart == null)
            {
                var newCart = new Cart()
                {
                    UserID = user.Id,
                    Status = StatusCart.PENDING,
                };
                _context.Carts.Add(newCart);
                _context.SaveChanges();

                var newCartItem = new CartItem()
                {
                    BookID = book.ID,
                    Quantity = 1,
                    CartID = newCart.ID,
                    Price = book.Discount,
                };
                _context.CartItems.Add(newCartItem);
                _context.SaveChanges();
            }
            else
            {
                var cartItemExist = _context.CartItems.FirstOrDefault(x => x.BookID == book.ID && x.CartID == cart.ID);
                if (cartItemExist != null)
                {
                    cartItemExist.Quantity += 1;
                    _context.CartItems.Update(cartItemExist);
                }
                else
                {
                    var cartItem = new CartItem()
                    {
                        BookID = book.ID,
                        CartID = cart.ID,
                        Price = book.Price,
                        Quantity = 1,
                    };
                    _context.CartItems.Add(cartItem);
                }

                book.Quantity -= 1;
                _context.Books.Update(book);
                _context.SaveChanges();
            }

            return Ok(new ResponseAPI<string>() { Success = true, Message = "Add Book To Cart Successfully" });
        }

        public IActionResult GetCountCartAPI()
        {
            var user = _context.Users.FirstOrDefault(x => x.UserName == this.User.Identity.Name);

            if (user == null)
            {
                return Ok(new ResponseAPI<int>() { Success = false, Message = "Cart is null", Data = 0 });
            }


            var cart = _context.Carts.FirstOrDefault(x => x.UserID == user.Id && x.Status != StatusCart.DONE);

            if (cart == null)
            {
                return Ok(new ResponseAPI<int>() { Success = false, Message = "Cart is null", Data = 0 });
            }

            var cartItemLength = _context.CartItems.Where(x => x.CartID == cart.ID).ToList().Count;

            return Ok(new ResponseAPI<int>() { Success = true, Message = "", Data = cartItemLength });
        }
    }
}
