using BookStoreKAP.Data;
using BookStoreKAP.Models.DTO;
using BookStoreKAP.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookStoreKAP.Controllers
{
	public class PayController : Controller
	{
		private readonly BookStoreKAPDBContext _context;

		public PayController(BookStoreKAPDBContext context)
		{
			_context = context;
		}

		public IActionResult Index()
		{
			return View();
		}
		[HttpPost]
		public IActionResult Payment(PaymentDTO req)
		{
			var user = _context.Users.FirstOrDefault(x => x.UserName == this.User.Identity.Name);
			var order = new Order()
			{
				Address = req.Address,
				CustomerID = user.Id,
				PaymentMethod = req.PaymentMethod,
				OrderDate = new DateTime(),


			};
			_context.Orders.Add(order);
			_context.SaveChanges();
			var cart = _context.Carts.FirstOrDefault(x => x.UserID == user.Id && x.Status == StatusCart.PENDING);

			var cartItems = _context.CartItems.Include(x => x.Book).Where(x => x.CartID == cart.ID).ToList();

			var orderDetails = new List<OrderDetail>();
			var total = 0.0;
			cartItems.ForEach(cartItem =>
			{
				total += cartItem.Price * cartItem.Quantity;
				orderDetails.Add(new OrderDetail
				{
					BookID = cartItem.BookID,
					OrderID = order.ID,
					Price = cartItem.Price,
					Quantity = cartItem.Quantity,
				});
			});
			order.Total = total;
			cart.Status = StatusCart.DONE;
			_context.Carts.Update(cart);
			_context.Orders.Update(order);
			_context.OrderDetails.AddRange(orderDetails);
			_context.SaveChanges();
			return View();
		}
	}
}
