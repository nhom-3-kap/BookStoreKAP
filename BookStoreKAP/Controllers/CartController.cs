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

            int quantity = req.Quantity.HasValue ? req.Quantity.Value : 1; // Để hỗ trợ thêm nhiều số lượng từ trang chi tiết

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
                    Quantity = quantity,
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
                    cartItemExist.Quantity += quantity;
                    _context.CartItems.Update(cartItemExist);
                }
                else
                {
                    var cartItem = new CartItem()
                    {
                        BookID = book.ID,
                        CartID = cart.ID,
                        Price = book.Discount, // Sửa từ Price thành Discount để phù hợp với hiển thị
                        Quantity = quantity,
                    };
                    _context.CartItems.Add(cartItem);
                }

                if (book.Quantity < quantity)
                {
                    return Ok(new ResponseAPI<string>() { Success = false, Message = "Không đủ số lượng sách trong kho" });
                }

                book.Quantity -= quantity;
                _context.Books.Update(book);
                _context.SaveChanges();
            }
            return Ok(new ResponseAPI<string>() { Success = true, Message = "Add Book To Cart Successfully" });
        }

        [HttpPost]
        public IActionResult UpdateCartQuantityAPI(Guid cartItemId, int quantity)
        {
            var user = _context.Users.FirstOrDefault(x => x.UserName == this.User.Identity.Name);
            if (user == null)
            {
                return Ok(new ResponseAPI<object>() { Success = false, Message = "UnAuthenticated" });
            }

            var cartItem = _context.CartItems.Include(x => x.Book).Include(x => x.Cart)
                .FirstOrDefault(x => x.ID == cartItemId && x.Cart.UserID == user.Id);

            if (cartItem == null)
            {
                return Ok(new ResponseAPI<object>() { Success = false, Message = "Cart item not found" });
            }

            var book = cartItem.Book;

            // Kiểm tra nếu là tăng số lượng, phải đảm bảo còn hàng trong kho
            if (quantity > cartItem.Quantity)
            {
                int additionalQuantity = quantity - cartItem.Quantity;
                if (book.Quantity < additionalQuantity)
                {
                    return Ok(new ResponseAPI<object>() { Success = false, Message = "Không đủ số lượng sách trong kho" });
                }
                book.Quantity -= additionalQuantity;
            }
            else if (quantity < cartItem.Quantity)
            {
                // Trả lại số lượng vào kho nếu giảm
                int returnedQuantity = cartItem.Quantity - quantity;
                book.Quantity += returnedQuantity;
            }

            if (quantity <= 0)
            {
                _context.CartItems.Remove(cartItem);
            }
            else
            {
                cartItem.Quantity = quantity;
                _context.CartItems.Update(cartItem);
            }

            _context.Books.Update(book);
            _context.SaveChanges();

            // Tính toán tổng giá trị giỏ hàng sau khi cập nhật
            var cart = _context.Carts.FirstOrDefault(x => x.ID == cartItem.CartID);
            var cartItems = _context.CartItems.Include(x => x.Book).Where(x => x.CartID == cart.ID).ToList();
            double totalPrice = cartItems.Sum(item => item.Quantity * item.Book.Discount);
            double total = totalPrice + (totalPrice * 0.1); // Thêm thuế 10%

            return Ok(new ResponseAPI<object>()
            {
                Success = true,
                Message = "Updated Successfully",
                Data = new
                {
                    itemSubtotal = cartItem.Quantity * book.Discount,
                    subtotal = totalPrice,
                    total = total,
                    cartCount = cartItems.Count,
                    itemCount = cartItems.Sum(x => x.Quantity)
                }
            });
        }

        [HttpPost]
        public IActionResult RemoveFromCartAPI(Guid cartItemId)
        {
            var user = _context.Users.FirstOrDefault(x => x.UserName == this.User.Identity.Name);
            if (user == null)
            {
                return Ok(new ResponseAPI<object>() { Success = false, Message = "UnAuthenticated" });
            }

            var cartItem = _context.CartItems.Include(x => x.Book).Include(x => x.Cart)
                .FirstOrDefault(x => x.ID == cartItemId && x.Cart.UserID == user.Id);

            if (cartItem == null)
            {
                return Ok(new ResponseAPI<object>() { Success = false, Message = "Cart item not found" });
            }

            // Trả lại số lượng vào kho
            var book = cartItem.Book;
            book.Quantity += cartItem.Quantity;
            _context.Books.Update(book);

            _context.CartItems.Remove(cartItem);
            _context.SaveChanges();

            // Tính toán tổng giá trị giỏ hàng sau khi xóa
            var cart = _context.Carts.FirstOrDefault(x => x.ID == cartItem.CartID);
            var cartItems = _context.CartItems.Include(x => x.Book).Where(x => x.CartID == cart.ID).ToList();
            double totalPrice = cartItems.Sum(item => item.Quantity * item.Book.Discount);
            double total = totalPrice + (totalPrice * 0.1); // Thêm thuế 10%

            return Ok(new ResponseAPI<object>()
            {
                Success = true,
                Message = "Removed Successfully",
                Data = new
                {
                    subtotal = totalPrice,
                    total = total,
                    cartCount = cartItems.Count,
                    itemCount = cartItems.Sum(x => x.Quantity)
                }
            });
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
            var cartItemLength = _context.CartItems.Where(x => x.CartID == cart.ID).Sum(x => x.Quantity);
            return Ok(new ResponseAPI<int>() { Success = true, Message = "", Data = cartItemLength });
        }
    }
}

